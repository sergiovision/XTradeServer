using BusinessObjects;
using NHibernate;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Repo
{
    public class DataService : IDataService
    {
        private static IWebLog log;
        private IRepository<DBSettings> settings;
        private IRepository<DBSymbol> symbols;
        private IRepository<DBCurrency> currencies;
        private IRepository<DBJobs> jobs;
        private IRepository<DBAccountstate> accstates;
        private IRepository<DBNewsevent> newsevents;
        private IRepository<DBDeals> deals;
        private ExpertsRepository experts;
        private AuthRepository persons;
        private AccountsRepository accounts;
        private WalletsRepository wallets;

        public DataService(IWebLog l)
        {
            symbols = new BaseRepository<DBSymbol>();
            currencies = new BaseRepository<DBCurrency>();
            settings = new BaseRepository<DBSettings>();
            jobs = new BaseRepository<DBJobs>();
            accstates = new BaseRepository<DBAccountstate>();
            newsevents = new BaseRepository<DBNewsevent>();
            accounts = new AccountsRepository();
            experts = new ExpertsRepository();
            persons = new AuthRepository();
            wallets = new WalletsRepository(this);
            deals = new BaseRepository<DBDeals>();
            log = l;
        }
        public List<CurrencyInfo> GetCurrencies()
        {
            List<CurrencyInfo> result = new List<CurrencyInfo>();
            try
            {
                currencies.GetAll().ForEach(currency =>
                {
                    var curr = new CurrencyInfo();
                    curr.Id = (short)currency.Id;
                    curr.Name = currency.Name;
                    curr.Retired = (currency.Enabled.Value > 0)?false:true;
                    result.Add(curr);
                });
            }
            catch (Exception e)
            {
                log.Error("Error: GetCurrencies: " + e.ToString());
            }
            return result;
        }

        public string GetGlobalProp(string name)
        {
            try
            {
                var gvars = settings.GetAll().Where(x => x.Propertyname.Equals(name));
                if (gvars.Count() > 0)
                {
                    DBSettings gvar = gvars.First();
                    return gvar.Value;
                }
            }
            catch (Exception e)
            {
                log.Error("Error: GetGlobalProp: " + e.ToString());
            }
            return "";
        }
        public void SetGlobalProp(string name, string value)
        {
            var gvars = settings.GetAll().Where(x => x.Propertyname == name);
            if (gvars.Count() > 0)
            {
                DBSettings gvar = gvars.First();
                gvar.Value = value;
                settings.Update(gvar);
            }
            else
            {
                var gvar = new DBSettings();
                gvar.Propertyname = name;
                gvar.Value = value;
                settings.Insert(gvar);
            }
        }

        public decimal ConvertToUSD(decimal value, string valueCurrency, IEnumerable<DBRates> rates)
        {
            decimal result = value;
            if ((rates == null) || valueCurrency.Equals("USD"))
                return result;
            DBRates rate = rates.Where(x => x.Metasymbol.C1.Equals(valueCurrency)).FirstOrDefault();
            if (rate != null && (rate.Rateask > 0))
            {
                result = result / rate.Rateask;
            }
            return result;
        }

        public IEnumerable<MetaSymbolStat> MetaSymbolStatistics(bool IsDemoAccount)
        {
            List<MetaSymbolStat> result = new List<MetaSymbolStat>();
            try
            {
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    var rateList = Session.Query<DBRates>().Where(x => x.Retired == false).ToList();
                    var symbols = Session.Query<DBMetasymbol>().Where(x => x.Retired == false).ToList();
                    foreach (var sym in symbols)
                    {
                        var deals  = Session.Query<DBDeals>().Where(x => (x.Symbol.Metasymbol.Id == sym.Id) && (x.Terminal.Demo == IsDemoAccount));
                        decimal sumProfit = 0;
                        int countTrades = 0;
                        foreach (var deal in deals)
                        {
                            sumProfit += ConvertToUSD(deal.Profit, deal.Terminal.Account.Currency.Name, rateList);
                            countTrades++;
                        }
                        if (countTrades <= 10)
                            continue;
                        MetaSymbolStat mss = new MetaSymbolStat();
                        mss.MetaId = sym.Id;
                        mss.Name = sym.Name;
                        mss.Description = sym.Description;
                        mss.TotalProfit = sumProfit;
                        mss.NumOfTrades = countTrades;
                        mss.ProfitPerTrade = sumProfit / countTrades;
                        mss.Date = DateTime.Now;
                        result.Add(mss);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Error in MetaSymbolStat : " + e.ToString());
            }
            return result.OrderByDescending(x=>x.ProfitPerTrade);
        }


        public List<Terminal> GetTerminals()
        {
            List<Terminal> result = new List<Terminal>();
            try
            {
                result = accounts.GetTerminals();
            }
            catch (Exception e)
            {
                log.Error("Error: GetTerminals: " + e.ToString());
            }
            return result;
        }

        public List<DealInfo> GetDeals()
        {
            List<DealInfo> result = new List<DealInfo>();
            try
            {
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    var deals = Session.Query<DBDeals>().OrderByDescending(x=>x.Closetime);
                    foreach (var dbd in deals)
                    {
                        result.Add(toDTO(dbd));
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Error: GetDeals: " + e.ToString());
            }
            return result;
        }

        public DealInfo toDTO(DBDeals deal)
        {
            DealInfo result = new DealInfo();
            //result.ClosePrice = deal.;
            if (deal.Closetime.HasValue)
                result.CloseTime = deal.Closetime.Value.ToString(xtradeConstants.MTDATETIMEFORMAT);
            result.Comment = deal.Comment;
            result.Commission = (double)deal.Commission;
            result.Lots = (double)deal.Volume;
            if (deal.Adviser != null)
            {
                result.Magic = deal.Adviser.Id;
            }
            result.OpenPrice = (double)deal.Price;
            result.OpenTime = deal.Opentime.ToString(xtradeConstants.MTDATETIMEFORMAT);
            result.Profit = (double)deal.Profit;
            if (deal.Terminal != null)
            {
                result.Account = deal.Terminal.Accountnumber.Value;
                result.AccountName = deal.Terminal.Broker;
            }
            result.SwapValue = (double)deal.Swap;
            if (deal.Symbol != null)
                result.Symbol = deal.Symbol.Name;
            if (deal.Orderid.HasValue)
                result.Ticket = deal.Orderid.Value;
            result.Type = (sbyte)deal.Typ;
            return result;
        }


        public bool UpdateTerminals(Terminal t)
        {
            try
            {
                return accounts.UpdateTerminals(t);
            }
            catch (Exception e)
            {
                log.Error("Error: UpdateTerminals: " + e.ToString());
            }
            return false;
        }

        public void UpdateBalance(int TerminalId, decimal Balance, decimal Equity)
        {
            try
            {
                accounts.UpdateBalance(TerminalId, Balance, Equity);
            }
            catch (Exception e)
            {
                log.Error("Error: UpdateBalance: " + e.ToString());
            }
        }

        public bool UpdateAdviser(Adviser adviser)
        {
            try
            {
                return experts.UpdateAdviser(adviser);
            }
            catch (Exception e)
            {
                log.Error("Error: UpdateAdviser: " + e.ToString());
            }
            return false;
        }

        public List<Account> GetAccounts()
        {
            List<Account> result = new List<Account>();
            try
            {
                result = accounts.GetAccounts();
            }
            catch (Exception e)
            {
                log.Error("Error: GetAccounts: " + e.ToString());
            }
            return result;
        }

        public List<Adviser> GetAdvisers()
        {
            try
            {
                return experts.GetAdvisers();
            }
            catch (Exception e)
            {
                log.Error("Error: GetExperts: " + e.ToString());
            }
            return new List<Adviser>();
        }

        public List<ExpertsCluster> GetClusters()
        {
            try
            {
                return experts.GetClusters();
            }
            catch (Exception e)
            {
                log.Error("Error: GetExperts: " + e.ToString());
            }
            return new List<ExpertsCluster>();
        }

        public List<Wallet> GetWalletsState(DateTime date)
        {
            List<Wallet> result = new List<Wallet>();
            try
            {
                return wallets.GetWalletsState(date);
            }
            catch (Exception e)
            {
                log.Error("Error: GetCurrentWalletsState: " + e.ToString());
            }
            return result;
        }

        public void SaveDeals(List<DealInfo> deals)
        {
            if (deals == null)
                return;
            if (deals.Count() <= 0)
                return;
            try
            {
                int i = 0;
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    foreach (var deal in deals.OrderBy(x=>x.CloseTime))
                    {
                        var sym = getSymbolByName(deal.Symbol);
                        if (sym == null)
                            continue;
                        DBDeals dbDeal = Session.Get<DBDeals>((int)deal.Ticket);
                        if (dbDeal == null)
                        {
                            if (getDealById(Session, deal.Ticket) != null)
                                continue;
                            try
                            {
                                using (ITransaction Transaction = Session.BeginTransaction())
                                {
                                    dbDeal = new DBDeals();
                                    dbDeal.Dealid = (int)deal.Ticket;
                                    dbDeal.Symbol = getSymbolByName(deal.Symbol);
                                    dbDeal.Terminal = getBDTerminalByNumber(Session, deal.Account);
                                    dbDeal.Adviser = getAdviserByMagicNumber(Session, deal.Magic);
                                    dbDeal.Id = (int)deal.Ticket;
                                    DateTime closeTime;
                                    if (DateTime.TryParse(deal.CloseTime, out closeTime))
                                        dbDeal.Closetime = DateTime.Parse(deal.CloseTime);
                                    dbDeal.Comment = deal.Comment;
                                    dbDeal.Commission = (decimal)deal.Commission;
                                    DateTime openTime;
                                    if (DateTime.TryParse(deal.OpenTime, out openTime))
                                        dbDeal.Opentime = DateTime.Parse(deal.OpenTime);
                                    dbDeal.Orderid = (int)deal.OrderId;
                                    dbDeal.Profit = (decimal)deal.Profit;
                                    dbDeal.Price = (decimal)deal.ClosePrice;
                                    dbDeal.Swap = (decimal)deal.SwapValue;
                                    dbDeal.Typ = deal.Type;
                                    dbDeal.Volume = (decimal)deal.Lots;
                                    Session.Save(dbDeal);
                                    Transaction.Commit();
                                    i++;
                                }

                            } catch (Exception )
                            {
                                log.Log($"Deal {deal.Ticket}:{deal.Symbol} failed to be saved in database");
                            }
                        }
                    }
                }
                log.Log($"Saved {i} history deals in database");
            } catch (Exception e)
            {
                string message = "Error: DataService.SaveDeals: " + e.ToString();
                log.Error(message);
                log.Log(message);                
            }
        }

        #region LocalFuncs
        public IEnumerable<DBAccountstate> GetAccountStates()
        {
            try
            {
                var result = accstates.GetAll();
                return result;
            }
            catch (Exception e)
            {
                log.Error("Error: GetAccountStates: " + e.ToString());
            }
            return null;
        }

        public IEnumerable<DBJobs> GetDBActiveJobsList()
        {
            try
            {
                var result = jobs.GetAll().Where(x => x.Disabled == false);
                return result;
            }
            catch (Exception e)
            {
                log.Error("Error: GetDBActiveJobsList: " + e.ToString());
            }
            return null;
        }
        public DBAdviser getAdviserByMagicNumber(ISession Session, long magicNumber)
        {
            try
            {
                DBAdviser adviser = Session.Get<DBAdviser>((int)magicNumber);
                return adviser;
            }
            catch (Exception e)
            {
                log.Error("Error: getAdviserByMagicNumber: " + e.ToString());
            }
            return null;
        }

        public DBCurrency getCurrencyID(string currencyStr)
        {
            try
            {
                var result = currencies.GetAll().Where(x => x.Name.Equals(currencyStr));
                if (result.Any())
                    return result.First();
            }
            catch (Exception e)
            {
                log.Error("Error: getAdviserByMagicNumber: " + e.ToString());
            }
            return null;
        }
        public IEnumerable<Terminal> GetActiveTerminals()
        {
            try
            {
                var result = accounts.GetTerminals().Where(x => x.Disabled == false);
                return result;
            }
            catch (Exception e)
            {
                log.Error("Error: GetActiveTerminals: " + e.ToString());
            }
            return null;
        }

        public Terminal getTerminalByNumber(ISession Session, long AccountNumber)
        {
            try
            {
                var result = Session.Query<DBTerminal>().Where(x => x.Accountnumber == (int)AccountNumber);
                if (result.Any())
                {
                    var term = result.FirstOrDefault();
                    if (term != null)
                        return accounts.toDTO(term);
                }
            }
            catch (Exception e)
            {
                log.Error("Error: getTerminalByNumber: " + e.ToString());
            }
            return null;
        }

        public DBTerminal getBDTerminalByNumber(ISession Session, long AccountNumber)
        {
            try
            {
                var result = Session.Query<DBTerminal>().Where(x => x.Accountnumber == (int)AccountNumber);
                if (result.Any())
                {
                    return result.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                log.Error("Error: getTerminalByNumber: " + e.ToString());
            }
            return null;
        }

        public DBDeals getDealById(ISession Session, long DealId)
        {
            try
            {
                var result = Session.Query<DBDeals>().Where(x => x.Dealid == (int)DealId);
                if (result.Any())
                {
                    return result.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                log.Error("Error: getDealById: " + e.ToString());
            }
            return null;
        }


        public Terminal getTerminalById(int Id)
        {
            try
            {
                var result = accounts.GetTerminals().Where(x => x.Id.Equals(Id) && (x.Disabled == false));
                if (result.Any())
                    return result.FirstOrDefault();
            }
            catch (Exception e)
            {
                log.Error("Error: getTerminalById: " + e.ToString());
            }
            return null;
        }
        public DBSymbol getSymbolByName(string SymbolStr)
        {
            try
            {
                var result = symbols.GetAll().Where(x => x.Name.Equals(SymbolStr));
                if (result.Any())
                    return result.First();
            }
            catch (Exception e)
            {
                log.Error("Error: getSymbolByName: " + e.ToString());
            }
            return null;
        }

        public IEnumerable<DBSymbol> GetSymbols()
        {
            try
            {
                var result = symbols.GetAll().Where(x => x.Retired.Value == 0);
                return result;
            }
            catch (Exception e)
            {
                log.Error("Error: GetSymbols: " + e.ToString());
            }
            return null;
        }


        public DBAdviser getAdviser(ISession Session, int term_id, int sym_id, string ea)
        {
            try
            {
                var result = Session.Query<DBAdviser>().Where(x => (x.Terminal.Id == term_id) && (x.Symbol.Id == sym_id) && (x.Name == ea) && (x.Disabled == false));
                if ((result != null) && (result.Count() > 0))
                {                    
                    return result.OrderByDescending(x => x.Lastupdate).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                log.Error("Error: getSymbolByName: " + e.ToString());
            }
            return null;
        }

        public void SaveInsertAdviser(ISession Session, DBAdviser toAdd)
        {
            using (ITransaction Transaction = Session.BeginTransaction())
            {
                if (toAdd.Id == 0)
                    Session.Save(toAdd);
                else
                    Session.Update(toAdd);

                Transaction.Commit();
            }

        }



        public void SaveInsertWaletState(DBAccountstate toAdd)
        {
            try
            {
                var accState = accstates.Insert(toAdd);
                if (accState == null)
                    return;
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    using (ITransaction Transaction = Session.BeginTransaction())
                    {
                        if (accState.Account != null)
                        {
                            var account = Session.Get<DBAccount>(accState.Account.Id);
                            if (account != null)
                            {
                                account.Balance = accState.Balance;
                                account.Lastupdate = DateTime.UtcNow;
                                Session.Update(account);
                            }
                        }
                        Transaction.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Error: SaveInsertWaletState: " + e.ToString());
            }
        }

        public void SaveInsertNewsEvent(DBNewsevent toAdd)
        {
            newsevents.Insert(toAdd);
        }

        public Person LoginPerson(string mail, string password)
        {
            var result =  persons.FindUser(mail, password);
            return result;
        }

        public IList<T> ExecuteNativeQuery<T>(ISession session, string queryString, string entityParamName, Tuple<string, object, IType>[] parameters = null)
        {
            ISQLQuery query = session.CreateSQLQuery(queryString).AddEntity(entityParamName, typeof(T));
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    query.SetParameter(parameters[i].Item1, parameters[i].Item2, parameters[i].Item3);
                }
            }
            return query.List<T>();
        }

        #endregion
    }
}
