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
        private static readonly object lockDeals = new object();
        private readonly AccountsRepository accounts;
        private readonly IRepository<DBAccountstate> accstates;
        private readonly IRepository<DBCurrency> currencies;
        private IRepository<DBDeals> deals;
        private readonly ExpertsRepository experts;
        private readonly IRepository<DBJobs> jobs;
        private readonly IRepository<DBNewsevent> newsevents;
        private readonly AuthRepository persons;
        private readonly List<Rates> rates;
        private readonly IRepository<DBSettings> settings;
        private readonly IRepository<DBSymbol> symbols;
        private readonly WalletsRepository wallets;

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
            rates = new List<Rates>();
        }

        public List<CurrencyInfo> GetCurrencies()
        {
            List<CurrencyInfo> result = new List<CurrencyInfo>();
            try
            {
                currencies.GetAll().ForEach(currency =>
                {
                    var curr = new CurrencyInfo();
                    curr.Id = (short) currency.Id;
                    curr.Name = currency.Name;
                    curr.Retired = currency.Enabled.Value > 0 ? false : true;
                    result.Add(curr);
                });
            }
            catch (Exception e)
            {
                log.Error("Error: GetCurrencies: " + e);
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
                log.Error("Error: GetGlobalProp: " + e);
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

        public decimal ConvertToUSD(decimal value, string valueCurrency)
        {
            decimal result = value;
            if (rates == null || valueCurrency.Equals("USD"))
                return result;
            Rates rate = GetRates(false).Where(x => x.C1.Equals(valueCurrency)).FirstOrDefault();
            if (rate != null && rate.Rateask > 0) result = result / rate.Rateask;

            return result;
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
                log.Error("Error: GetTerminals: " + e);
            }

            return result;
        }

        public bool isSameDay(DateTime d1, DateTime d2)
        {
            return (d1.DayOfYear == d2.DayOfYear) && (d2.Year == d1.Year);
        }

        public List<DealInfo> TodayDeals()
        {
            List<DealInfo> result = new List<DealInfo>();
            try
            {
                DateTime now = DateTime.Now;
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    var deals = Session.Query<DBDeals>().OrderByDescending(x => x.Closetime);
                    foreach (var dbd in deals)
                        if (isSameDay(dbd.Closetime.Value, now))
                            result.Add(toDTO(dbd));
                        
                }
            }
            catch (Exception e)
            {
                log.Error("Error: TodayDeals: " + e);
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
                    var deals = Session.Query<DBDeals>().OrderByDescending(x => x.Closetime);
                    foreach (var dbd in deals)
                        result.Add(toDTO(dbd));
                }
            }
            catch (Exception e)
            {
                log.Error("Error: GetDeals: " + e);
            }

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
                log.Error("Error: UpdateTerminals: " + e);
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
                log.Error("Error: UpdateBalance: " + e);
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
                log.Error("Error: UpdateAdviser: " + e);
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
                log.Error("Error: GetAccounts: " + e);
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
                log.Error("Error: GetExperts: " + e);
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
                log.Error("Error: GetExperts: " + e);
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
                log.Error("Error: GetCurrentWalletsState: " + e);
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
                lock (lockDeals)
                {
                    int i = 0;
                    using (ISession Session = ConnectionHelper.CreateNewSession())
                    {
                        foreach (var deal in deals.OrderBy(x => x.CloseTime))
                        {
                            var sym = getSymbolByName(deal.Symbol);
                            if (sym == null)
                                continue;
                            DBDeals dbDeal = Session.Get<DBDeals>((int) deal.Ticket);
                            if (dbDeal == null)
                            {
                                if (getDealById(Session, deal.Ticket) != null)
                                    continue;
                                try
                                {
                                    using (ITransaction Transaction = Session.BeginTransaction())
                                    {
                                        dbDeal = new DBDeals();
                                        dbDeal.Dealid = (int) deal.Ticket;
                                        dbDeal.Symbol = getSymbolByName(deal.Symbol);
                                        dbDeal.Terminal = getBDTerminalByNumber(Session, deal.Account);
                                        dbDeal.Adviser = getAdviserByMagicNumber(Session, deal.Magic);
                                        dbDeal.Id = (int) deal.Ticket;
                                        DateTime closeTime;
                                        if (DateTime.TryParse(deal.CloseTime, out closeTime))
                                            dbDeal.Closetime = DateTime.Parse(deal.CloseTime);
                                        dbDeal.Comment = deal.Comment;
                                        dbDeal.Commission = (decimal) deal.Commission;
                                        DateTime openTime;
                                        if (DateTime.TryParse(deal.OpenTime, out openTime))
                                            dbDeal.Opentime = DateTime.Parse(deal.OpenTime);
                                        dbDeal.Orderid = (int) deal.OrderId;
                                        dbDeal.Profit = (decimal) deal.Profit;
                                        dbDeal.Price = (decimal) deal.ClosePrice;
                                        dbDeal.Swap = (decimal) deal.SwapValue;
                                        dbDeal.Typ = deal.Type;
                                        dbDeal.Volume = (decimal) deal.Lots;
                                        Session.Save(dbDeal);
                                        Transaction.Commit();
                                        i++;
                                    }
                                }
                                catch (Exception)
                                {
                                    log.Log($"Deal {deal.Ticket}:{deal.Symbol} failed to be saved in database");
                                }
                            }
                        }
                    }

                    if (i > 0)
                        log.Log($"Saved {i} history deals in database");
                }
            }
            catch (Exception e)
            {
                string message = "Error: DataService.SaveDeals: " + e;
                log.Error(message);
                log.Log(message);
            }
        }

        public IEnumerable<MetaSymbolStat> MetaSymbolStatistics(int AccountType)
        {
            List<MetaSymbolStat> result = new List<MetaSymbolStat>();
            try
            {
                bool IsDemoAccount = AccountType > 0 ? true : false;
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    var symbols = Session.Query<DBMetasymbol>().Where(x => x.Retired == false).ToList();
                    foreach (var sym in symbols)
                    {
                        var deals = Session.Query<DBDeals>().Where(x =>
                            x.Symbol.Metasymbol.Id == sym.Id && x.Terminal.Demo == IsDemoAccount);
                        decimal sumProfit = 0;
                        int countTrades = 0;
                        foreach (var deal in deals)
                        {
                            sumProfit += ConvertToUSD(deal.Profit, deal.Terminal.Account.Currency.Name);
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
                log.Error("Error in MetaSymbolStat : " + e);
            }

            return result.OrderByDescending(x => x.ProfitPerTrade);
        }

        public List<TimeStat> Performance(int month, TimePeriod period)
        {
            List<TimeStat> result = new List<TimeStat>();
            try
            {
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    var rateList = Session.Query<DBRates>().Where(x => x.Retired == false).ToList();
                    DateTime now = DateTime.Now;
                    int dayFrom = 1;
                    int year = now.Year;
                    if (now.Month < month + 1) year--;

                    DateTime from = new DateTime(year, month + 1, dayFrom);
                    int dayTo = now.Month == month + 1 ? now.Day : DateTime.DaysInMonth(year, month + 1);
                    DateTime to = new DateTime(year, month + 1, dayTo);
                    var Accounts = Session.Query<DBAccount>(); // .Where(x => (x.Retired == false));
                    //var Deals = Session.Query<DBDeals>().Where(x => x.Terminal.Demo == false);
                    for (int i = dayFrom; i <= dayTo; i++)
                    {
                        DateTime forDate = new DateTime(year, month + 1, i);
                        DateTime forDateEnd = new DateTime(year, month + 1, i, 23, 50, 0);
                        TimeStat ts = new TimeStat();
                        ts.X = i;
                        ts.Date = forDate;
                        ts.Period = period;
                        foreach (var acc in Accounts)
                        {
                            if (acc.Terminal != null)
                                if (acc.Terminal.Demo)
                                    continue;
                            var accStateAll = Session.Query<DBAccountstate>().Where(x => x.Account.Id == acc.Id);
                            var accResultsStart = accStateAll.Where(x => x.Date <= forDate) //&& (x.Date >= from)
                                .OrderByDescending(x => x.Date);
                            var accResults = accStateAll.Where(x => x.Date <= forDateEnd) //&& (x.Date >= from)
                                .OrderByDescending(x => x.Date);

                            if (accResults == null || accResults.Count() == 0)
                                continue;
                            if (accResultsStart == null || accResultsStart.Count() == 0)
                                continue;

                            var accState = accResults.FirstOrDefault();
                            if (accState != null)
                            {
                                if (acc.Typ > 0)
                                    ts.InvestingValue += ConvertToUSD(accState.Balance, acc.Currency.Name);

                                ts.CheckingValue += ConvertToUSD(accState.Balance, acc.Currency.Name);
                            }

                            var accStateStart = accResultsStart.FirstOrDefault();
                            if (accStateStart != null)
                            {
                                if (acc.Typ > 0)
                                    ts.InvestingChange += ConvertToUSD(accStateStart.Balance, acc.Currency.Name);
                                ts.CheckingChange += ConvertToUSD(accStateStart.Balance, acc.Currency.Name);
                            }
                        }
                        /*
                        var deals = Deals.Where(x => (x.Closetime.HasValue) &&  (x.Closetime.Value >= forDate) && (x.Closetime.Value <= forDateEnd));
                        if (deals != null)
                        {
                            foreach (var deal in deals)
                            {
                                if (deal.Terminal == null)
                                    continue;
                                if (deal.Terminal.Account == null)
                                    continue;
                                ts.InvestingChange += ConvertToUSD(deal.Profit, deal.Terminal.Account.Currency.Name, rateList);
                            }
                        }
                        */

                        ts.CheckingChange = ts.CheckingValue - ts.CheckingChange;
                        ts.InvestingChange = ts.InvestingValue - ts.InvestingChange;

                        ts.CheckingChange = Math.Round(ts.CheckingChange, 2);
                        ts.InvestingChange = Math.Round(ts.InvestingChange, 2);
                        ts.CheckingValue = Math.Round(ts.CheckingValue, 2);
                        ts.InvestingValue = Math.Round(ts.InvestingValue, 2);
                        result.Add(ts);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Error in Performance : " + e);
            }

            return result;
        }


        public List<Rates> GetRates(bool IsReread)
        {
            try
            {
                if (rates.Count() > 0 && !IsReread)
                    return rates;
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    var dbrates = Session.Query<DBRates>();
                    foreach (var dbr in dbrates) rates.Add(toDTO(dbr));
                }
            }
            catch (Exception e)
            {
                log.Error("Error: GetRates: " + e);
            }

            return rates;
        }

        public DealInfo toDTO(DBDeals deal)
        {
            DealInfo result = new DealInfo();
            //result.ClosePrice = deal;
            if (deal.Closetime.HasValue)
                result.CloseTime = deal.Closetime.Value.ToString(xtradeConstants.MTDATETIMEFORMAT);
            result.Comment = deal.Comment;
            result.Commission = (double) deal.Commission;
            result.Lots = (double) deal.Volume;
            if (deal.Adviser != null) result.Magic = deal.Adviser.Id;

            result.OpenPrice = (double) deal.Price;
            result.OpenTime = deal.Opentime.ToString(xtradeConstants.MTDATETIMEFORMAT);
            result.Profit = (double) deal.Profit;
            if (deal.Terminal != null)
            {
                result.Account = deal.Terminal.Accountnumber.Value;
                result.AccountName = deal.Terminal.Broker;
            }

            result.SwapValue = (double) deal.Swap;
            if (deal.Symbol != null)
                result.Symbol = deal.Symbol.Name;
            if (deal.Orderid.HasValue)
                result.Ticket = deal.Orderid.Value;
            result.Type = (sbyte) deal.Typ;
            return result;
        }

        public Rates toDTO(DBRates rates)
        {
            Rates result = new Rates();
            result.MetaSymbol = rates.Metasymbol.Name;
            result.C1 = rates.Metasymbol.C1;
            result.C2 = rates.Metasymbol.C2;
            result.Ratebid = rates.Ratebid;
            result.Rateask = rates.Rateask;
            result.Retired = rates.Retired;
            if (rates.Lastupdate.HasValue)
                result.Lastupdate = rates.Lastupdate.Value;
            else
                result.Lastupdate = DateTime.UtcNow;
            return result;
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
                log.Error("Error: GetAccountStates: " + e);
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
                log.Error("Error: GetDBActiveJobsList: " + e);
            }

            return null;
        }

        public DBAdviser getAdviserByMagicNumber(ISession Session, long magicNumber)
        {
            try
            {
                DBAdviser adviser = Session.Get<DBAdviser>((int) magicNumber);
                return adviser;
            }
            catch (Exception e)
            {
                log.Error("Error: getAdviserByMagicNumber: " + e);
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
                log.Error("Error: getAdviserByMagicNumber: " + e);
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
                log.Error("Error: GetActiveTerminals: " + e);
            }

            return null;
        }

        public Terminal getTerminalByNumber(ISession Session, long AccountNumber)
        {
            try
            {
                var result = Session.Query<DBTerminal>().Where(x => x.Accountnumber == (int) AccountNumber);
                if (result.Any())
                {
                    var term = result.FirstOrDefault();
                    if (term != null)
                        return accounts.toDTO(term);
                }
            }
            catch (Exception e)
            {
                log.Error("Error: getTerminalByNumber: " + e);
            }

            return null;
        }

        public DBTerminal getBDTerminalByNumber(ISession Session, long AccountNumber)
        {
            try
            {
                var result = Session.Query<DBTerminal>().Where(x => x.Accountnumber == (int) AccountNumber);
                if (result.Any()) return result.FirstOrDefault();
            }
            catch (Exception e)
            {
                log.Error("Error: getTerminalByNumber: " + e);
            }

            return null;
        }

        public DBDeals getDealById(ISession Session, long DealId)
        {
            try
            {
                var result = Session.Query<DBDeals>().Where(x => x.Dealid == (int) DealId);
                if (result.Any()) return result.FirstOrDefault();
            }
            catch (Exception e)
            {
                log.Error("Error: getDealById: " + e);
            }

            return null;
        }


        public Terminal getTerminalById(int Id)
        {
            try
            {
                var result = accounts.GetTerminals().Where(x => x.Id.Equals(Id) && x.Disabled == false);
                if (result.Any())
                    return result.FirstOrDefault();
            }
            catch (Exception e)
            {
                log.Error("Error: getTerminalById: " + e);
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
                log.Error("Error: getSymbolByName: " + e);
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
                log.Error("Error: GetSymbols: " + e);
            }

            return null;
        }


        public DBAdviser getAdviser(ISession Session, int term_id, int sym_id, string ea)
        {
            try
            {
                var result = Session.Query<DBAdviser>().Where(x =>
                    x.Terminal.Id == term_id && x.Symbol.Id == sym_id && x.Name == ea && x.Disabled == false);
                if (result != null && result.Count() > 0)
                    return result.OrderByDescending(x => x.Lastupdate).FirstOrDefault();
            }
            catch (Exception e)
            {
                log.Error("Error: getSymbolByName: " + e);
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
                log.Error("Error: SaveInsertWaletState: " + e);
            }
        }

        public void SaveInsertNewsEvent(DBNewsevent toAdd)
        {
            newsevents.Insert(toAdd);
        }

        public Person LoginPerson(string mail, string password)
        {
            var result = persons.FindUser(mail, password);
            return result;
        }

        public IList<T> ExecuteNativeQuery<T>(ISession session, string queryString, string entityParamName,
            Tuple<string, object, IType>[] parameters = null)
        {
            ISQLQuery query = session.CreateSQLQuery(queryString).AddEntity(entityParamName, typeof(T));
            if (parameters != null)
                for (int i = 0; i < parameters.Length; i++)
                    query.SetParameter(parameters[i].Item1, parameters[i].Item2, parameters[i].Item3);

            return query.List<T>();
        }

        #endregion
    }
}