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
        private readonly IMainService xtrade;

        public PositionsManager()
        {
            positions = new ConcurrentDictionary<long, PositionInfo>();
            Clients = GlobalHost.ConnectionManager.GetHubContext<TerminalsHub>().Clients;
            xtrade = Program.Container.Resolve<IMainService>();

            terminals = new Dictionary<long, Terminal>();
            foreach (var term in xtrade.GetTerminals()) terminals.Add(term.AccountNumber, term);
        }

        private IHubConnectionContext<dynamic> Clients { get; }

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
                        notcontains.Profit = xtrade.ConvertToUSD(notcontains.Profit, terminals[AccountNumber].Currency);
                        positionsToAdd.Add(notcontains.Ticket, notcontains);
                    }

                foreach (var pos in positions.Where(x => x.Value.Account.Equals(AccountNumber)))
                {
                    var contains = posMagic.Where(x => x.Ticket == pos.Key && x.Account == AccountNumber);
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
                        if (pos.Value.Magic == magicId)
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