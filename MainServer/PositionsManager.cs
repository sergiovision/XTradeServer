using BusinessObjects;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autofac;

namespace XTrade.MainServer
{
    public class PositionsManager : ITerminalEvents
    {
        private static readonly object lockObject = new object();
        private readonly ConcurrentDictionary<long, PositionInfo> positions;
        private readonly Dictionary<long, Terminal> terminals;
        private readonly Dictionary<long, DealInfo> todayDeals;
        private TodayStat todayStat;
        private readonly IMainService xtrade;
        private IHubConnectionContext<dynamic> Clients { get; }

        public PositionsManager()
        {
            positions = new ConcurrentDictionary<long, PositionInfo>();
            Clients = GlobalHost.ConnectionManager.GetHubContext<TerminalsHub>().Clients;
            xtrade = Program.Container.Resolve<IMainService>();
            todayDeals = new Dictionary<long, DealInfo>();
            terminals = new Dictionary<long, Terminal>();
            foreach (var term in xtrade.GetTerminals()) terminals.Add(term.AccountNumber, term);
            todayStat = new TodayStat();
        }


        public List<PositionInfo> GetAllPositions()
        {
            List<PositionInfo> result = new List<PositionInfo>();
            lock (lockObject)
            {
                foreach (var posTerm in positions) result.Add(posTerm.Value);
            }

            return result;
        }

        public void UpdatePositions(long magicId, long AccountNumber, IEnumerable<PositionInfo> posMagic)
        {
            lock (lockObject)
            {
                Dictionary<long, PositionInfo> positionsToAdd = new Dictionary<long, PositionInfo>();
                List<long> positionsToDelete = new List<long>();
                foreach (var notcontains in posMagic)
                    if (!positionsToAdd.ContainsKey(notcontains.Ticket))
                    {
                        notcontains.AccountName = terminals[AccountNumber].Broker;
                        notcontains.Profit = (double)xtrade.ConvertToUSD(new decimal(notcontains.Profit), terminals[AccountNumber].Currency);
                        positionsToAdd.Add(notcontains.Ticket, notcontains);
                    }

                foreach (var pos in positions.Where(x => x.Value.Account.Equals(AccountNumber)))
                {
                    var contains = posMagic.Where(x => x.Ticket == pos.Key && x.Account == AccountNumber);
                    if (contains != null && contains.Count() > 0)
                    {
                        positionsToAdd.Remove(pos.Key);
                        var newvalue = contains.FirstOrDefault();
                        //newvalue.ProfitStopsPercent = pos.Value.ProfitStopsPercent;
                        newvalue.AccountName = terminals[AccountNumber].Broker;
                        if (positions.TryUpdate(pos.Key, newvalue, pos.Value))
                            UpdatePosition(newvalue);
                    }
                    else
                    {
                        //if (pos.Value.Account == AccountNumber)  (pos.Value.Account == AccountNumber) && (pos.Value.Ticket > 0)
                        if ((pos.Value.Account == AccountNumber) && (pos.Value.Ticket > 0))
                            positionsToDelete.Add(pos.Key);
                    }

                    foreach (var notcontains in posMagic.Where(x => x.Ticket != pos.Key))
                        if (!positionsToAdd.ContainsKey(notcontains.Ticket))
                            positionsToAdd.Add(notcontains.Ticket, notcontains);
                }

                foreach (var toremove in positionsToDelete)
                {
                    PositionInfo todel = null;
                    if (positions.TryRemove(toremove, out todel))
                        RemovePosition(toremove);
                }

                foreach (var toadd in positionsToAdd)
                {
                    toadd.Value.AccountName = terminals[AccountNumber].Broker;
                    if (positions.TryAdd(toadd.Key, toadd.Value))
                        InsertPosition(toadd.Value);
                }
            }
        }

        public void UpdateSLTP(long magicId, long AccountNumber, IEnumerable<PositionInfo> UpdatePositions)
        {
            lock (lockObject)
            {
                Dictionary<long, PositionInfo> positionsToAdd = new Dictionary<long, PositionInfo>();
                List<long> positionsToDelete = new List<long>();
                foreach (var notcontains in UpdatePositions)
                    if (!positionsToAdd.ContainsKey(notcontains.Ticket))
                    {
                        notcontains.AccountName = terminals[AccountNumber].Broker;
                        notcontains.Profit = (double)xtrade.ConvertToUSD(new decimal(notcontains.Profit), terminals[AccountNumber].Currency);
                        positionsToAdd.Add(notcontains.Ticket, notcontains);
                    }

                foreach (var pos in positions.Where(x => x.Value.Account.Equals(AccountNumber)))
                {
                    var contains = UpdatePositions.Where(x => x.Ticket == pos.Key && x.Account == AccountNumber);
                    if (contains != null && contains.Count() > 0)
                    {
                        positionsToAdd.Remove(pos.Key);
                        var newvalue = contains.FirstOrDefault();
                        newvalue.AccountName = terminals[AccountNumber].Broker;
                        if (positions.TryUpdate(pos.Key, newvalue, pos.Value))
                            UpdatePosition(newvalue);
                    }
                    else
                    {
                        //if (pos.Value.Magic == magicId) (pos.Value.Account == AccountNumber)
                        if ((pos.Value.Magic == magicId))//&& (pos.Value.Ticket > 0)
                            positionsToDelete.Add(pos.Key);
                    }

                    foreach (var notcontains in UpdatePositions.Where(x => x.Ticket != pos.Key))
                        if (!positionsToAdd.ContainsKey(notcontains.Ticket))
                            positionsToAdd.Add(notcontains.Ticket, notcontains);
                }

                foreach (var toremove in positionsToDelete)
                {
                    PositionInfo todel = null;
                    if (positions.TryRemove(toremove, out todel))
                        RemovePosition(toremove);
                }

                foreach (var toadd in positionsToAdd)
                {
                    toadd.Value.AccountName = terminals[AccountNumber].Broker;
                    if (positions.TryAdd(toadd.Key, toadd.Value))
                        InsertPosition(toadd.Value);
                }

            }

        }

        public List<DealInfo> GetTodayDeals()
        {
            var xtrade = Program.Container.Resolve<IMainService>();
            if (xtrade == null)
                return null;
            var deals = xtrade.TodayDeals();
            if (deals != null)
            {
                foreach (var deal in deals)
                {
                    if (todayDeals.ContainsKey(deal.Ticket))
                        continue;
                    string currency = terminals[deal.Account].Currency;
                    deal.Profit = (double)xtrade.ConvertToUSD(new decimal(deal.Profit), currency);
                    bool IsDemo = terminals[deal.Account].Demo;
                    todayDeals.Add(deal.Ticket, deal);
                }
            }
            DateTime now = DateTime.UtcNow;
            List<long> toDelete = new List<long>();
            foreach( var val in todayDeals)
            {
                DateTime time = DateTime.Parse(val.Value.CloseTime);
                if (now.DayOfYear != time.DayOfYear)
                {
                    toDelete.Add(val.Key);
                }
            }
            foreach (var val in toDelete)
            {
                todayDeals.Remove(val);
            }
            return todayDeals.Values.OrderByDescending(x=>x.CloseTime).ToList();
        }

        public TodayStat GetTodayStat()
        {
            if (todayDeals.Count <= 0)
                GetTodayDeals();
            double sumReal = 0;
            double sumDemo = 0;
            foreach (var deal in todayDeals)
            {
                bool IsDemo = terminals[deal.Value.Account].Demo;
                if (IsDemo)
                    sumDemo += deal.Value.Profit;
                else
                    sumReal += deal.Value.Profit;

            }
            todayStat.TodayGainDemo = decimal.Round((decimal)sumDemo, 2);
            todayStat.TodayGainReal = decimal.Round((decimal)sumReal, 2);
            return todayStat;
        }

        public void DeletePosition(long Ticket)
        {
            lock (lockObject)
            {
                PositionInfo todel = null;
                positions.TryRemove(Ticket, out todel);
            }
        }

        #region Interface Imp

        public void InsertPosition(PositionInfo pos)
        {
            Clients.All.InsertPosition(pos);
        }

        public void UpdatePosition(PositionInfo pos)
        {
            Clients.All.UpdatePosition(pos);
        }

        public void RemovePosition(long Ticket)
        {
            Clients.All.RemovePosition(Ticket);
        }

        #endregion
    }
}