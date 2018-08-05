using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Autofac;
using BusinessObjects;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FXBusinessLogic.fx_mind;
using FXBusinessLogic.Scheduler;
using log4net;
using Quartz;

namespace FXBusinessLogic.BusinessObjects
{
    public class MainService : IMainService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainService));
        public static MainService thisGlobal;
        private static int isDebug = -1;
        private SchedulerService _gSchedulerService;
        private INotificationUi _ui;
        protected TimeZoneInfo BrokerTimeZoneInfo;
        private bool Initialized;
        public static char[] ParamsSeparator = fxmindConstants.PARAMS_SEPARATOR.ToCharArray();
        public  const int CHAR_BUFF_SIZE = 512;

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string RegistryInstallDir
        {
            get
            {
                string result = AssemblyDirectory;
                try
                {
                    result = thisGlobal.GetGlobalProp(fxmindConstants.SETTINGS_PROPERTY_INSTALLDIR);
                    //RegistryKey rk = Registry.LocalMachine.OpenSubKey(fxmindConstants.SETTINGS_APPREGKEY, false);
                    //if (rk == null)
                    //{
                    //    rk = Registry.LocalMachine.CreateSubKey(fxmindConstants.SETTINGS_APPREGKEY, true, RegistryOptions.None);
                    //    rk.SetValue("InstallDir", result);
                    //} else
                    //{
                    //    result = rk.GetValue("InstallDir")?.ToString();
                    //}
                }
                catch (Exception e)
                {
                    log.Error("ERROR FROM RegistryInstallDir: " + e);
                }
                return result;
            }
        }

        public static string MTTerminalUserName
        {
            get
            {
                string result = WindowsIdentity.GetCurrent().Name;
                try
                {
                    result = thisGlobal.GetGlobalProp(fxmindConstants.SETTINGS_PROPERTY_RUNTERMINALUSER);
                    //RegistryKey rk = Registry.LocalMachine.OpenSubKey(fxmindConstants.SETTINGS_APPREGKEY, false);
                    //if (rk == null)
                    //{
                    //    rk = Registry.LocalMachine.CreateSubKey(fxmindConstants.SETTINGS_APPREGKEY, true, RegistryOptions.None);
                    //    rk.SetValue("MTTerminalUserName", result);
                    //}
                    //else
                    //{
                    //    result = rk.GetValue("RunMTTerminalUserName")?.ToString();
                    //}
                }
                catch (Exception e)
                {
                    log.Error("ERROR FROM RunMTTerminalUserName: " + e);
                }
                return result;
            }
        }


        public MainService()
        {
            RegisterContainer();
            Initialized = false;
            thisGlobal = this;
            isDeploying = false;
        }

        public List<Currency> GetCurrencies()
        {
            Session session = FXConnectionHelper.GetNewSession();
            var currenciesDb = new XPCollection<DBCurrency>(session);
            List<Currency> list = new List<Currency>();
            foreach (var dbCurrency in currenciesDb)
            {
                var curr = new Currency();
                curr.ID = dbCurrency.ID;
                curr.Name = dbCurrency.Name;
                curr.Enabled = dbCurrency.Enabled;
                list.Add(curr);
            }
            session.Disconnect();
            return list;
        }

        public List<TechIndicator> GetIndicators()
        {
            Session session = FXConnectionHelper.GetNewSession();
            var techIndiDb = new XPCollection<DBTechIndicator>(session);
            List<TechIndicator> list = new List<TechIndicator>();
            foreach (var dbI in techIndiDb)
            {
                var ti = new TechIndicator();
                ti.ID = dbI.ID;
                ti.IndicatorName = dbI.IndicatorName;
                ti.Enabled = dbI.Enabled;
                list.Add(ti);
            }
            session.Disconnect();
            return list;
        }

        public void SaveCurrency(Currency currency)
        {
            Session session = FXConnectionHelper.GetNewSession();
            var cQuery = new XPQuery<DBCurrency>(session);
            IQueryable<DBCurrency> curs = from c in cQuery
                where c.ID == currency.ID
                select c;
            DBCurrency gvar = null;
            if (curs.Any())
            {
                gvar = curs.First();
                gvar.Enabled = currency.Enabled;
            }
            else
            {
                gvar = new DBCurrency(session);
                gvar.Name = currency.Name;
                gvar.Enabled = currency.Enabled;
            }

            gvar.Save();
            session.Disconnect();
        }

        public void SaveIndicator(TechIndicator i)
        {
            Session session = FXConnectionHelper.GetNewSession();
            var cQuery = new XPQuery<DBTechIndicator>(session);
            IQueryable<DBTechIndicator> curs = from c in cQuery
                where c.ID == i.ID
                select c;
            DBTechIndicator gvar = null;
            if (curs.Any())
            {
                gvar = curs.First();
                gvar.Enabled = i.Enabled;
            }
            else
            {
                gvar = new DBTechIndicator(session);
                gvar.IndicatorName = i.IndicatorName;
                gvar.Enabled = i.Enabled;
            }

            gvar.Save();
            session.Disconnect();
        }

        public IContainer Container { get; private set; }


        public INotificationUi GetUi()
        {
            return _ui;
        }

        protected void RegistryInit()
        {
            log.Info("Registry InstallDir: " + RegistryInstallDir);
        }

        public void Init(INotificationUi ui)
        {
            if (Initialized)
                return;
            _ui = ui;

            RegistryInit();

            //FXConnectionHelper.Connect(null);

            BrokerTimeZoneInfo = GetBrokerTimeZone();

            InitScheduler(true);

            Initialized = true;
        }

        public bool InitScheduler(bool serverMode /*unused*/)
        {
            if (_gSchedulerService == null)
                _gSchedulerService = Container.Resolve<SchedulerService>(new NamedParameter("ui", _ui));
            return _gSchedulerService.Initialize();
        }

        public TimeZoneInfo GetBrokerTimeZone()
        {
            if (BrokerTimeZoneInfo == null)
            {
                BrokerTimeZoneInfo = GetTimeZoneFromString(fxmindConstants.SETTINGS_PROPERTY_BROKERSERVERTIMEZONE);
            }
            return BrokerTimeZoneInfo;
        }

        public string GetGlobalProp(string name)
        {
            Session session = FXConnectionHelper.GetNewSession();
            var res = FXMindHelpers.GetGlobalVar(session, name);
            session.Disconnect();
            return res;
        }

        public void SetGlobalProp(string name, string value)
        {
            Session session = FXConnectionHelper.GetNewSession();
            FXMindHelpers.SetGlobalVar(session, name, value);
            session.Disconnect();
        }


        public List<double> iCurrencyStrengthAll(string currencyStr, List<string> brokerDates, int iTimeframe)
        {
            var resultDoubles = new List<double>();

            int dbTimeFrame = iTimeFrameToDBTimeframe(iTimeframe);
            if (dbTimeFrame == -1)
                return resultDoubles;

            // new session should be created for crossthread issues
            Session session = FXConnectionHelper.GetNewSession();

            const string queryStrInterval =
                @"SELECT TD.SummaryId, TD.IndicatorId, TD.Action, TD.Value, TS.ID, TS.SymbolId,  TS.Timeframe, TS.Date, S.Name, S.C1, S.C2
            FROM TechDetail TD
            INNER JOIN TechSummary TS ON TD.SummaryId = TS.ID
            INNER JOIN TechIndicator TI ON TD.IndicatorId = TI.ID
            INNER JOIN Symbol S ON TS.SymbolId = S.ID
            WHERE (TS.Type = 1) AND (TS.Date >= @fr_dt) AND (TS.Date <= @to_dt) AND (TS.Timeframe=@tf) AND (TI.Enabled=1) AND ((S.Name LIKE @curr) AND (S.Use4Tech=1))
            ORDER BY TS.Date DESC";

            foreach (string brokerDate in brokerDates)
            {
                DateTime from;
                DateTime.TryParseExact(brokerDate, fxmindConstants.MTDATETIMEFORMAT,
                    CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out from);

                from = TimeZoneInfo.ConvertTimeToUtc(from, BrokerTimeZoneInfo);
                DateTime to = from;
                switch (dbTimeFrame)
                {
                    case 1:
                        to = to.AddMinutes(1);
                        break;
                    case 2:
                        to = to.AddMinutes(5);
                        break;
                    case 3:
                        to = to.AddMinutes(15);
                        break;
                    case 4:
                        to = to.AddMinutes(30);
                        break;
                    case 5:
                        to = to.AddHours(1);
                        break;
                    case 6:
                        to = to.AddHours(5);
                        break;
                    case 7:
                        to = to.AddDays(1);
                        break;
                    case 8:
                        to = to.AddMonths(1);
                        break;
                }

                string[] paramnames = {"fr_dt", "to_dt", "tf", "curr"};
                object[] parameters =
                {
                    from.ToString(fxmindConstants.MYSQLDATETIMEFORMAT),
                    to.ToString(fxmindConstants.MYSQLDATETIMEFORMAT), dbTimeFrame, "%" + currencyStr + "%"
                };
                SelectedData data = session.ExecuteQuery(queryStrInterval, paramnames, parameters);
                int count = data.ResultSet[0].Rows.Count();
                int TFcnt = 0;
                var multiplier = new decimal(1.0);
                var result = new decimal(0.0);
                var ProcessedSymbols = new HashSet<int>();
                foreach (SelectStatementResultRow row in data.ResultSet[0].Rows)
                {
                    var symbol = (int) row.Values[5];
                    if (ProcessedSymbols.Contains(symbol))
                        continue;
                    ProcessedSymbols.Add(symbol);
                    if (row.Values[10].Equals(currencyStr))
                        multiplier = new decimal(-1.0);
                    else
                        multiplier = new decimal(1.0);
                    var actionValue = (short) row.Values[2];
                    //var tf = (Byte)row.Values[6];
                    result += actionValue * multiplier;
                    TFcnt++;
                }

                if (TFcnt > 0)
                {
                    result = 100 * result / TFcnt;
                    resultDoubles.Add((double) result);
                }
                else
                {
                    resultDoubles.Add(fxmindConstants.GAP_VALUE);
                }
            }

            session.Dispose();
            return resultDoubles;
        }

        /*
        public string GetActionString(int actionId)
        {
            switch (actionId)
            {
                case 2:
                    return "STRONG BUY";
                case 1:
                    return "BUY";
                case 0:
                    return "NEUTRAL";
                case -1:
                    return "SELL";
                case -2:
                    return "STRONG SELL";
                default:
                    return "ERROR";
            }
        }*/

        public List<ScheduledJob> GetAllJobsList()
        {
            return SchedulerService.GetAllJobsList();
        }

        public Dictionary<string, ScheduledJob> GetRunningJobs()
        {
            return SchedulerService.GetRunningJobs();
        }

        public DateTime? GetJobNextTime(string group, string name)
        {
            return SchedulerService.GetJobNextTime(group, name);
        }

        public DateTime? GetJobPrevTime(string group, string name)
        {
            return SchedulerService.GetJobPrevTime(group, name);
        }

        #region DBJobs
        public IEnumerable<DBJobs> GetDBActiveJobsList(Session session)
        {

            var jobsQuery = new XPQuery<DBJobs>(session);
            IQueryable<DBJobs> jobs = from c in jobsQuery
                                           where c.DISABLED == 0
                                           select c;

            return jobs;
        }

        public void UnsheduleJobs(IEnumerable<JobKey> jobs)
        {
            foreach (var job in jobs)
                SchedulerService.removeJobTriggers(job);
        }

        public bool DeleteJob(JobKey job)
        {
             return SchedulerService.sched.DeleteJob(job).Result;
        }

        #endregion

        public void Dispose()
        {
            if (_gSchedulerService != null)
                _gSchedulerService.Shutdown();
        }

        public void PauseScheduler()
        {
            SchedulerService.sched.PauseAll();
        }

        public void ResumeScheduler()
        {
            SchedulerService.sched.ResumeAll();
        }

        public bool GetNextNewsEvent(DateTime date, string symbolStr, byte minImportance, ref NewsEventInfo eventInfo)
        {
            List<NewsEventInfo> newsForToday = GetTodayNews(date, symbolStr, minImportance);
            if (newsForToday.Count() == 0)
                return false;
            eventInfo = newsForToday.FirstOrDefault();
            return newsForToday != null;
            /*
            Session session = FXConnectionHelper.GetNewSession();
            bool result = false;
            try
            {
                eventInfo = null;
                // new session should be created for crossthread issues

                BrokerTimeZoneInfo = GetBrokerTimeZone();

                const string queryStrInterval =
                    @"SELECT C.Name 
	                ,NE.HappenTime
                    ,NE.Name
                    ,NE.Raised
                    ,NE.Importance
                FROM NewsEvent NE
                INNER JOIN Currency C ON NE.CurrencyId = C.ID
                WHERE (C.Name=@c1 OR C.Name=@c2) AND (NE.HappenTime >= @fr_dt) AND (NE.HappenTime <= @to_dt) AND (NE.Importance >= @imp) ORDER BY NE.HappenTime ASC, NE.Importance DESC";

                string C1 = symbolStr.Substring(0, 3);
                string C2 = C1;
                if (symbolStr.Length == 6)
                    C2 = symbolStr.Substring(3, 3);

                DateTime from = TimeZoneInfo.ConvertTimeToUtc(date, BrokerTimeZoneInfo);
                // date.AddHours(-BrokerTimeZoneInfo.BaseUtcOffset.Hours);
                DateTime to = from.AddDays(1).AddSeconds(-1); //.AddMinutes(beforeIntervalMinutes);
                //to = to.AddHours(-BrokerTimeZoneInfo.BaseUtcOffset.Hours);

                string[] paramnames = {"c1", "c2", "fr_dt", "to_dt", "imp"};
                object[] parameters =
                {
                    C1, C2, from.ToString(fxmindConstants.MYSQLDATETIMEFORMAT),
                    to.ToString(fxmindConstants.MYSQLDATETIMEFORMAT), minImportance
                };
                SelectedData data = session.ExecuteQuery(queryStrInterval, paramnames, parameters);

                int count = data.ResultSet[0].Rows.Count();
                if (count <= 0)
                {
                    eventInfo = null;
                    return false;
                }

                foreach (SelectStatementResultRow row in data.ResultSet[0].Rows)
                {
                    eventInfo = new NewsEventInfo();
                    eventInfo.Currency = (string)row.Values[0];
                    DateTime raiseDT = (DateTime)row.Values[1];
                    raiseDT = TimeZoneInfo.ConvertTimeFromUtc(raiseDT,
                        BrokerTimeZoneInfo);
                    eventInfo.RaiseDateTime = raiseDT.ToString(fxmindConstants.MTDATETIMEFORMAT);
                    //eventInfo.RaiseDateTime.AddHours(BrokerTimeZoneInfo.BaseUtcOffset.Hours);
                    eventInfo.Name = (string)row.Values[2];
                    byte imp = (byte)row.Values[4];
                    eventInfo.Importance = (sbyte)imp;
                    break;
                }
                result = true;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
            return result;
            */
        }

        public List<NewsEventInfo> GetTodayNews(DateTime date, string symbolStr, byte minImportance)
        {
            // new session should be created for crossthread issues
            Session session = FXConnectionHelper.GetNewSession();
            List<NewsEventInfo> result = new List<NewsEventInfo>();
            try
            {
                BrokerTimeZoneInfo = GetBrokerTimeZone();

                const string queryStrInterval =
                    @"SELECT C.Name 
	                ,NE.HappenTime
                    ,NE.Name
                    ,NE.Raised
                    ,NE.Importance
                FROM NewsEvent NE
                INNER JOIN Currency C ON NE.CurrencyId = C.ID
                WHERE (C.Name=@c1 OR C.Name=@c2) AND (NE.HappenTime >= @fr_dt) AND (NE.HappenTime <= @to_dt) AND (NE.Importance >= @imp) ORDER BY NE.HappenTime ASC, NE.Importance DESC";

                string C1 = symbolStr.Substring(0, 3);
                string C2 = C1;
                if (symbolStr.Length == 6)
                    C2 = symbolStr.Substring(3, 3);

                //from current time bar
                DateTime from = TimeZoneInfo.ConvertTimeToUtc(date, BrokerTimeZoneInfo);
                //until midnight
                DateTime to = from.AddDays(1).AddSeconds(-1); 

                string[] paramnames = { "c1", "c2", "fr_dt", "to_dt", "imp" };
                object[] parameters =
                {
                    C1, C2, from.ToString(fxmindConstants.MYSQLDATETIMEFORMAT),
                    to.ToString(fxmindConstants.MYSQLDATETIMEFORMAT), minImportance
                };
                SelectedData data = session.ExecuteQuery(queryStrInterval, paramnames, parameters);

                int count = data.ResultSet[0].Rows.Count();
                if (count <= 0)
                {
                    return result;
                }

                NewsEventInfo eventInfo = null;
                foreach (SelectStatementResultRow row in data.ResultSet[0].Rows)
                {
                    eventInfo = new NewsEventInfo();
                    eventInfo.Currency = (string)row.Values[0];
                    DateTime raiseDT = (DateTime)row.Values[1];
                    raiseDT = TimeZoneInfo.ConvertTimeFromUtc(raiseDT,
                        BrokerTimeZoneInfo);
                    eventInfo.RaiseDateTime = raiseDT.ToString(fxmindConstants.MTDATETIMEFORMAT);
                    //eventInfo.RaiseDateTime.AddHours(BrokerTimeZoneInfo.BaseUtcOffset.Hours);
                    eventInfo.Name = (string)row.Values[2];
                    byte imp = (byte)row.Values[4];
                    eventInfo.Importance = (sbyte)imp;
                    result.Add(eventInfo);
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
            return result;
        }

        public void GetAverageLastGlobalSentiments(DateTime date, string symbolName, out double longPos,
            out double shortPos)
        {
            longPos = -1;
            shortPos = -1;
            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                BrokerTimeZoneInfo = GetBrokerTimeZone();
                if (symbolName.Length == 6)
                    symbolName = symbolName.Insert(3, "/");
                DBSymbol dbsym = FXMindHelpers.getSymbolID(session, symbolName);
                if (dbsym == null)
                    return;

                DateTime to = TimeZoneInfo.ConvertTimeToUtc(date, BrokerTimeZoneInfo);
                DateTime from = to.AddMinutes(-fxmindConstants.SENTIMENTS_FETCH_PERIOD);
                const string queryStrInterval = @"SELECT LongRatio, ShortRatio, SiteID FROM OpenPosRatio
                                                WHERE (SymbolID=@symID) AND (ParseTime >= @fr_dt) AND (ParseTime <= @to_dt) 
                                                ORDER BY ParseTime DESC";

                string[] paramnames = {"symID", "fr_dt", "to_dt"};
                object[] parameters =
                {
                    dbsym.ID, from.ToString(fxmindConstants.MYSQLDATETIMEFORMAT),
                    to.ToString(fxmindConstants.MYSQLDATETIMEFORMAT)
                };
                SelectedData data = session.ExecuteQuery(queryStrInterval, paramnames, parameters);

                int count = data.ResultSet[0].Rows.Count();
                if (count > 0)
                {
                    int cnt = 0;
                    double valLong = 0;
                    double valShort = 0;
                    foreach (SelectStatementResultRow row in data.ResultSet[0].Rows)
                    {
                        valLong += (double) row.Values[0];
                        valShort += (double) row.Values[1];
                        cnt++;
                    }

                    longPos = valLong / cnt;
                    shortPos = valShort / cnt;
                }

                //INotificationUi ui = GetUi();
                //if (ui != null)
                //    if (IsDebug())
                //        ui.LogStatus("GetLastAverageGlobalSentiments for " + symbolName + " : " + longPos + ", " +
                //                     shortPos);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
        }

        public bool IsDebug()
        {
            if (isDebug == -1)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                bool isDebugBuild = assembly.GetCustomAttributes(false).OfType<DebuggableAttribute>()
                    .Select(attr => attr.IsJITTrackingEnabled).FirstOrDefault();
                if (isDebugBuild)
                {
                    isDebug = 1;
                    return true;
                }

                isDebug = 0;
                return false;
            }

            return isDebug > 0 ? true : false;
        }

        public List<double> iGlobalSentimentsArray(string symbolName, List<string> brokerDates, int siteId)
        {
            var resList = new List<double>();
            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                if (symbolName.Length == 6)
                    symbolName = symbolName.Insert(3, "/");
                DBSymbol dbsym = FXMindHelpers.getSymbolID(session, symbolName);
                if (dbsym == null)
                    return resList;
                string queryStrInterval;
                var paramnames = new string[] { };
                if (siteId == 0)
                {
                    queryStrInterval = @"SELECT LongRatio, ShortRatio, SiteID FROM OpenPosRatio
                                                    WHERE (SymbolID=@symID) AND (ParseTime >= @fr_dt) AND (ParseTime <= @to_dt) 
                                                    ORDER BY ParseTime DESC";
                    paramnames = new[] { "symID", "fr_dt", "to_dt" };
                }
                else
                {
                    queryStrInterval = @"SELECT LongRatio, ShortRatio FROM OpenPosRatio
                                                WHERE (SymbolID=@symID) AND (ParseTime >= @fr_dt) AND (ParseTime <= @to_dt) AND (SiteID = @siteID)
                                                ORDER BY ParseTime ASC";
                    paramnames = new[] { "symID", "fr_dt", "to_dt", "siteID" };
                }

                foreach (string brokerDate in brokerDates)
                {
                    DateTime date;
                    DateTime.TryParseExact(brokerDate, fxmindConstants.MTDATETIMEFORMAT,
                        CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out date);

                    date = TimeZoneInfo.ConvertTimeToUtc(date, BrokerTimeZoneInfo);
                    DateTime hourPlus = date;
                    hourPlus = hourPlus.AddHours(1);
                    object[] parameters = { };
                    if (siteId == 0)
                        parameters = new object[]
                        {
                            dbsym.ID, date.ToString(fxmindConstants.MYSQLDATETIMEFORMAT),
                            hourPlus.ToString(fxmindConstants.MYSQLDATETIMEFORMAT)
                        };
                    else
                        parameters = new object[]
                        {
                            dbsym.ID, date.ToString(fxmindConstants.MYSQLDATETIMEFORMAT),
                            hourPlus.ToString(fxmindConstants.MYSQLDATETIMEFORMAT), siteId
                        };
                    SelectedData data = session.ExecuteQuery(queryStrInterval, paramnames, parameters);
                    int count = data.ResultSet[0].Rows.Count();
                    if (count == 0)
                    {
                        resList.Add(fxmindConstants.GAP_VALUE);
                    }
                    else
                    {
                        int cnt = 0;
                        double valLong = 0;
                        //double valShort = 0;
                        foreach (SelectStatementResultRow row in data.ResultSet[0].Rows)
                        {
                            valLong += (double)row.Values[0];
                            //valShort += (double) row.Values[1];
                            cnt++;
                        }

                        resList.Add(valLong / cnt);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            finally
            {

                session.Disconnect();
                session.Dispose();
            }
            return resList;
        }

        protected void testDate(string date)
        {
            DateTime from;
            DateTime.TryParseExact(date, fxmindConstants.MTDATETIMEFORMAT,
                CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out from);
            NewsEventInfo info = new NewsEventInfo();
            GetNextNewsEvent(from, "EURUSD", 1, ref info);
        }

        private void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<SchedulerService>().AsSelf().SingleInstance().WithParameter("ui", _ui);
            builder.RegisterType<MainService>().As<IMainService>().SingleInstance();
            Container = builder.Build();
        }

        protected TimeZoneInfo GetTimeZoneFromString(string propName)
        {
            string strTimeZone = GetGlobalProp(propName);
            ReadOnlyCollection<TimeZoneInfo> tz = TimeZoneInfo.GetSystemTimeZones();
            foreach (TimeZoneInfo tzi in tz)
                if (tzi.StandardName.Equals(strTimeZone))
                    return tzi;
            return null;
        }

        private double CalcCS4CurrencyLast(Session session, string currencyName, int Timeframe)
        {
            const string queryStrInterval =
                @"SELECT TOP 100 TD.SummaryId, TD.IndicatorId, TD.Action, TD.Value, TS.ID, TS.SymbolId,  TS.Timeframe, TS.Date, S.Name, S.C1, S.C2
                FROM TechDetail TD
                INNER JOIN TechSummary TS ON TD.SummaryId = TS.ID
                INNER JOIN TechIndicator TI ON TD.IndicatorId = TI.ID
                INNER JOIN Symbol S ON TS.SymbolId = S.ID
                WHERE (TS.Type = 1) AND (TS.Date >= '{0}') AND (TS.Date <= '{1}') AND (TS.Timeframe={2}) AND (TI.Enabled = 1) AND ((S.Name  LIKE '%{3}%') AND (S.Use4Tech = 1))
                ORDER BY TS.Date DESC";

            DateTime dayBefore = DateTime.UtcNow.AddDays(-3);
            DateTime dayToday = DateTime.UtcNow;
            string resultQuery = string.Format(queryStrInterval, dayBefore, dayToday, Timeframe, currencyName);
            SelectedData data = session.ExecuteQuery(resultQuery);

            int count = data.ResultSet[0].Rows.Count();
            int TFcnt = 0;
            double multiplier = 1.0;
            double result = 0.0;
            var ProcessedSymbols = new HashSet<int>();
            foreach (SelectStatementResultRow row in data.ResultSet[0].Rows)
            {
                var symbol = (int) row.Values[5];
                if (ProcessedSymbols.Contains(symbol))
                    continue;
                ProcessedSymbols.Add(symbol);
                if (row.Values[10].Equals(currencyName))
                    multiplier = -1.0;
                else
                    multiplier = 1.0;
                var actionValue = (short) row.Values[2];
                //var tf = (Byte)row.Values[6];
                result += actionValue * multiplier;
                TFcnt++;
            }

            if (TFcnt > 0)
                result = 100 * result / TFcnt;
            return result;
        }

        private int iTimeFrameToDBTimeframe(int iTimeframe)
        {
            // -1 means not supported
            switch (iTimeframe)
            {
                case 1: //Min1
                    return 1;
                case 5: //Min5
                    return 2;
                case 15: //Min15
                    return 3;
                case 30: //Min30
                    return 4;
                case 60: //Hour
                    return 5;
                case 240: // Hour4 Hour5
                    return 6;
                case 1440: // Daily
                    return 7;
                case 10080: // Weekly not supported
                    return -1;
                case 43200: //Monthly
                    return 8;
            }

            return -1;
        }

        List<WalletBalance> IMainService.GetWalletBalance()
        {
            List<WalletBalance> result = new List<WalletBalance>();
            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                
                var qLS = session.GetObjectsFromQuery<DBLaststate>("select * from laststate"); // new XPQuery<DBLaststate>(session);
                //qLS.All
                //IQueryable<DBLaststate> varQLS = from c in qLS
                //                                      select c;
                foreach (var ls in qLS)
                {
                    WalletBalance wb = new WalletBalance();
                    wb.WALLET_ID = ls.WALLET_ID;
                    wb.NAME = ls.NAME;
                    wb.BALANCE = ls.BALANCE;
                    wb.DATE = ls.DATE;
                    result.Add(wb);
                }
            }
            catch (Exception e)
            {
                log.Error("Error: GetWalletBalance: " + e.ToString());
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
            return result;
        }

        List<Account> IMainService.GetAccounts()
        {
            List<Account> result = new List<Account>();
            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                var qTerm = new XPQuery<DBTerminal>(session);
                IQueryable<DBTerminal> varQTerminal = from c in qTerm
                                                      // where c.DISABLED == 0
                                                      select c;
                foreach (var term in varQTerminal)
                {
                    Account acc = new Account();
                    acc.AccountNumber = term.ACCOUNTNUMBER;
                    acc.Broker = term.BROKER;
                    acc.CodeBase = term.CODEBASE;
                    acc.Disabled = (term.DISABLED > 0)?true:false;
                    acc.FullPath = term.FULLPATH;
                    acc.ID = term.ID;
                    result.Add(acc);
                }
            }
            catch (Exception e)
            {
                log.Error("Error: GetAccounts: " + e.ToString());
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
            return result;
        }

        List<Adviser> IMainService.GetExperts()
        {
            List<Adviser> result = new List<Adviser>();
            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                var qTerm = new XPQuery<DBAdviser>(session);
                IQueryable<DBAdviser> varQAdv = from c in qTerm
                                                      select c;
                foreach (var expert in varQAdv)
                {
                    Adviser adv = new Adviser();
                    adv.ID = expert.ID;
                    adv.Name = expert.NAME;
                    adv.Running = (expert.RUNNING > 0)?true:false;
                    adv.Disabled = (expert.DISABLED > 0) ? true : false;
                    adv.SYMBOL_ID = expert.SYMBOL_ID.ID;
                    adv.TERMINAL_ID = expert.TERMINAL_ID.ID;
                    adv.Symbol = expert.SYMBOL_ID.Name;
                    adv.Timeframe = expert.TIMEFRAME;
                    adv.LastUpdate = expert.LASTUPDATE;
                    adv.CloseReason = expert.CLOSE_REASON;
                    result.Add(adv);
                }
            }
            catch (Exception e)
            {
                log.Error("Error: GetExperts: " + e.ToString());
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
            return result;
        }

        public bool UpdateWallet(WalletBalance newbalance)
        {
            bool result = false;
            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                var newws = new DBWalletstate(session);
                newws.BALANCE = newbalance.BALANCE;
                newws.DATE = DateTime.UtcNow;
                newws.WALLET_ID = newbalance.WALLET_ID;
                newws.formula = newbalance.BALANCE.ToString();
                session.Save(newws);
                return true;
            }
            catch (Exception e)
            {
                log.Error("Error: UpdateWallet: " + e.ToString());
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
            return result;
        }

        #region Jobs

        public void RunJobNow(string group, string name)
        {
            SchedulerService.RunJobNow(new JobKey(name, group));
        }

        public string GetJobProp(string group, string name, string prop)
        {
            return SchedulerService.GetJobProp(group, name, prop);
        }

        public void SetJobCronSchedule(string group, string name, string cron)
        {
            SchedulerService.SetJobCronSchedule(group, name, cron);
        }

        [DllImport("kernel32.dll")]
        static extern int GetPrivateProfileString(int Section, string Key,
              string Value, [MarshalAs(UnmanagedType.LPArray)] byte[] Result,
              int Size, string FileName);

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringW", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileStringW(string lpApplicationName, string lpKeyName, string lpDefault,
                                                   [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] char[] lpReturnedString, int nSize, string Filename);

        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode)]
        static extern int WritePrivateProfileStringW(string lpApplicationName, int lpKeyName, int lpString,string lpFileName);

        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode)]
        static extern int WritePrivateProfileStringW2(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);

        // The Function called to obtain the SectionHeaders,
        // and returns them in an Dynamic Array.
        public string[] GetSectionNames(string path)
        {
            try
            {
                //    Sets the maxsize buffer to 500, if the more
                //    is required then doubles the size each time.
                for (int maxsize = CHAR_BUFF_SIZE; true; maxsize *= 2)
                {
                    //    Obtains the information in bytes and stores
                    //    them in the maxsize buffer (Bytes array)
                    byte[] bytes = new byte[maxsize];
                    int size = GetPrivateProfileString(0, "", "", bytes, maxsize, path);

                    // Check the information obtained is not bigger
                    // than the allocated maxsize buffer - 2 bytes.
                    // if it is, then skip over the next section
                    // so that the maxsize buffer can be doubled.
                    if (size < maxsize - 2)
                    {
                        // Converts the bytes value into an ASCII char. This is one long string.
                        string Selected = Encoding.ASCII.GetString(bytes, 0,
                                                   size - (size > 0 ? 1 : 0));
                        // Splits the Long string into an array based on the "\0"
                        // or null (Newline) value and returns the value(s) in an array
                        return Selected.Split(new char[] { '\0' });
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Failed to get Section Names from file: " + path + ". Error: " + e.ToString());
            }
            return new string[] { "" };
        }

        public static string GetPrivateProfileString(string fileName, string sectionName, string keyName)
        {
            char[] ret = new char[CHAR_BUFF_SIZE];

            while (true)
            {
                int length = GetPrivateProfileStringW(sectionName, keyName, null, ret, ret.Length, fileName);
                if (length == 0)
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                // This function behaves differently if both sectionName and keyName are null
                if (sectionName != null && keyName != null)
                {
                    if (length == ret.Length - 1)
                    {
                        // Double the buffer size and call again
                        ret = new char[ret.Length * 2];
                    }
                    else
                    {
                        // Return simple string
                        return new string(ret, 0, length);
                    }
                }
                else
                {
                    if (length == ret.Length - 2)
                    {
                        // Double the buffer size and call again
                        ret = new char[ret.Length * 2];
                    }
                    else
                    {
                        // Return multistring
                        return new string(ret, 0, length - 1);
                    }
                }
            }
        }

        public ExpertInfo InitExpert(ExpertInfo expert) 
        {
            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                DBTerminal terminal = FXMindHelpers.getTerminalID(session, expert.Account);
                if (terminal == null)
                {
                    log.Error("Unknown AccountNumber " + expert.Account + " ERROR");
                    expert.MagicNumber = 0;
                    return expert;
                }
                string strSymbol = expert.Symbol;
                if (strSymbol.Contains("_i"))
                    strSymbol = strSymbol.Substring(0, strSymbol.Length - 2); 
                if (strSymbol.Length == 6)
                    strSymbol = strSymbol.Insert(3, "/");

                DBSymbol symbol = FXMindHelpers.getSymbolID(session, strSymbol);
                if (symbol == null)
                {
                    log.Error("Unknown Symbol " + strSymbol + " ERROR");
                    return expert;
                }

                // from current time bar
                DateTime initTime = DateTime.UtcNow;

                DBAdviser adviser = FXMindHelpers.getAdviserID(session, terminal.ID, symbol.ID, expert.ChartTimeFrame, expert.EAName);
                if (adviser == null)
                {
                    adviser = new DBAdviser(session);
                    adviser.NAME = expert.EAName;
                    adviser.TIMEFRAME = expert.ChartTimeFrame;
                    adviser.DISABLED = 0;
                    adviser.TERMINAL_ID = terminal;
                    adviser.SYMBOL_ID = symbol;
                    
                }
                adviser.RUNNING = 1;
                adviser.LASTUPDATE = initTime;

                session.Save(adviser);

                GetOrdersListToLoad(adviser, ref expert);

                expert.MagicNumber = adviser.ID;
                log.Info($"Expert {expert.EAName} Magic={adviser.ID} On TF={expert.ChartTimeFrame} loaded successfully!");
                return expert;
            }
            catch (Exception e)
            {
                log.Error("Error: InitExpert: " + e.ToString());
                expert.MagicNumber = 0;
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
            return expert;
        }

        bool GetOrdersListToLoad(DBAdviser adviser, ref ExpertInfo expert)
        {
            if (expert.OrderTicketsToLoad == null)
                expert.OrderTicketsToLoad = new List<string>();
            string filePath = GetAdviserFilePath(adviser);
            if (!File.Exists(filePath))
                return false;
            string[] sections = GetSectionNames(filePath);
            if (sections.Length > 0)
            {
                List<string> sectionsList = sections.ToList();
                if ((sectionsList != null) && (sectionsList.Count() > 0))
                {
                    foreach (var order in sectionsList)
                    {
                        if (order.Equals(fxmindConstants.GLOBAL_SECTION_NAME) )
                            continue;
                        string roleString = GetPrivateProfileString(filePath, order, "role");
                        if (!String.IsNullOrEmpty(roleString))
                        {
                            ENUM_ORDERROLE role = (ENUM_ORDERROLE)Int32.Parse(roleString);
                            if (role != ENUM_ORDERROLE.History)
                            {
                                //string ticketString = GetPrivateProfileString(filePath, order, "ticket");
                                expert.OrderTicketsToLoad.Add(order);
                            }
                        }
                        //DeleteSection(order, filePath);
                        //if (!order.Equals(fxmindConstants.GLOBAL_SECTION_NAME))
                        //    WritePrivateProfileStringW(order, 0, 0, filePath);
                    }
                }
            }
            return true;
        }

        public void SaveExpert(long MagicNumber, string ActiveOrdersList)
        {
            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                DBAdviser adviser = FXMindHelpers.getAdviserByMagicNumber(session, MagicNumber);
                if (adviser == null)
                {
                    log.Error("Expert with MagicNumber=" + MagicNumber + " doesn't exist");
                }

                adviser.RUNNING = 1;
                adviser.LASTUPDATE = DateTime.UtcNow;

                string filePath = GetAdviserFilePath(adviser);
                string[] sections = GetSectionNames(filePath);
                if (sections.Length > 0)
                {
                    List<string> sectionsList = sections.ToList();
                    var activeOrdersList = ActiveOrdersList.Split(ParamsSeparator);
                    var ordersToDelete = sectionsList.Except(activeOrdersList);
                    if ((ordersToDelete != null) && (ordersToDelete.Count() > 0))
                    {
                        foreach (var order in ordersToDelete)
                        {
                            int histVal = (int)ENUM_ORDERROLE.History;
                            WritePrivateProfileStringW2(order, "role", histVal.ToString(), filePath);

                            //DeleteSection(order, filePath);
                            //if (!order.Equals(fxmindConstants.GLOBAL_SECTION_NAME))
                            //    WritePrivateProfileStringW(order, 0, 0, filePath);
                        }
                    }
                }

                session.Save(adviser);

            }
            catch (Exception e)
            {
                log.Error("SaveExpert: " + e.ToString());
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
        }

        public int DeleteHistoryOrders(string filePath)
        {
            int result = 0;
            string[] sections = GetSectionNames(filePath);
            if (sections.Length > 0)
            {
                List<string> sectionsList = sections.ToList();
                if ((sectionsList != null) && (sectionsList.Count() > 0))
                {
                    foreach (var sectionName in sectionsList)
                    {
                        if (sectionName.Equals(fxmindConstants.GLOBAL_SECTION_NAME))
                            continue;

                        string roleString = GetPrivateProfileString(filePath, sectionName, "role");
                        if (!String.IsNullOrEmpty(roleString))
                        {
                            ENUM_ORDERROLE role = (ENUM_ORDERROLE)Int32.Parse(roleString);
                            if (role.Equals(ENUM_ORDERROLE.History))
                            {
                                // Deletes section!!!
                                WritePrivateProfileStringW(sectionName, 0, 0, filePath);
                                result++;
                            }
                        }
                    }
                }
            }
            return result;
        }

        protected string GetAdviserFilePath(DBAdviser adviser)
        {
            string path = GetGlobalProp(fxmindConstants.SETTINGS_PROPERTY_MTCOMMONFILES);
            string sym = adviser.SYMBOL_ID.Name;
            if (sym.Length > 6)
                sym = sym.Remove(3, 1);
            string filePath = $"{path}\\{adviser.TERMINAL_ID.ACCOUNTNUMBER}_{sym}_{adviser.TIMEFRAME}_{adviser.ID}.set";
            return filePath;
        }

        public  bool FileLocked(string FileName)
        {
            FileStream fs = null;

            try
            {
                // NOTE: This doesn't handle situations where file is opened for writing by another process but put into write shared mode, it will not throw an exception and won't show it as write locked
                fs = File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None); // If we can't open file for reading and writing then it's locked by another process for writing
            }
            catch (UnauthorizedAccessException) // https://msdn.microsoft.com/en-us/library/y973b725(v=vs.110).aspx
            {
                // This is because the file is Read-Only and we tried to open in ReadWrite mode, now try to open in Read only mode
                try
                {
                    fs = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.None);
                }
                catch (Exception)
                {
                    return true; // This file has been locked, we can't even open it to read
                }
            }
            catch (Exception)
            {
                return true; // This file has been locked
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            return false;
        }

        protected bool SaveState(string filePath, DBAdviser adviser)
        {

            if (FileLocked(filePath))

                return false;
            adviser.STATE = File.ReadAllText(filePath);
            return true;
        }

        string ReasonToString(int Reason)
        {
            switch (Reason)
            {
                case 0: //0
                    return "0 REASON_PROGRAM - Эксперт прекратил свою работу, вызвав функцию ExpertRemove()";
                case 1: //1
                    return "1 REASON_REMOVE Программа удалена с графика";
                case 2: // 2
                    return "2 REASON_RECOMPILE Программа перекомпилирована";
                case 3: //3
                    return "3 REASON_CHARTCHANGE Символ или период графика был изменен";
                case 4:
                    return "4 REASON_CHARTCLOSE График закрыт";
                case 5:
                    return "5 REASON_PARAMETERS Входные параметры были изменены пользователем";
                case 6:
                    return "6 Активирован другой счет либо произошло переподключение к торговому серверу вследствие изменения настроек счета";
                case 7:
                    return "7 REASON_TEMPLATE Применен другой шаблон графика";
                case 8:
                    return "8 REASON_INITFAILED Признак того, что обработчик OnInit() вернул ненулевое значение";
                case 9:
                    return "9 REASON_CLOSE Терминал был закрыт";
            }
            return $"Unknown reason: {Reason}";
        }


        public void DeInitExpert(int Reason, long MagicNumber)
        {
            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                DBAdviser adviser = FXMindHelpers.getAdviserByMagicNumber(session, MagicNumber);
                if (adviser == null)
                {
                    log.Error("Expert with MagicNumber=" + MagicNumber + " doesn't exist");
                }

                adviser.RUNNING = 0;
                adviser.LASTUPDATE = DateTime.UtcNow;
                adviser.CLOSE_REASON = Reason;
                string filePath = GetAdviserFilePath(adviser);
                SaveState(filePath, adviser);

                
                session.Save(adviser);

                log.Info($"Expert MagicNumber: {MagicNumber} closed with reason {ReasonToString(Reason)}.");
            }
            catch (Exception e)
            {
                log.Error("DeInitExpert: " + e.ToString());
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
        }

        public void DeployToTerminals(string sourceFolder)
        {
            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                var qTerm = new XPQuery<DBTerminal>(session);
                IQueryable<DBTerminal> varQTerminal = from c in qTerm
                                                      where c.DISABLED == 0
                                                      select c;
                foreach ( var terminal in varQTerminal )
                {
                    DirectoryInfo sourceDir = new DirectoryInfo(sourceFolder);
                    string fileName = string.Format(@"deployto_{0}.bat", terminal.ACCOUNTNUMBER);
                    StreamWriter SW = new StreamWriter(fileName);
                    SW.Write(ProcessFolder("", terminal, sourceFolder, CopyFile));
                    SW.Write(ProcessFolder("", terminal, sourceFolder, CompileFile));
                    SW.Flush();
                    SW.Close();
                    SW.Dispose();
                    SW = null;
                    //Process.Start("deploy.bat");
                }
            }
            catch (Exception e)
            {
                log.Error("Error: Generate Deploy Scripts: " + e.ToString());
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
        }

        protected bool isDeploying;
        public void DeployToAccount(int id)
        {
            if (isDeploying)
            {
                log.Error("Application already deploying: Skip...");
                return;
            }
            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                isDeploying = true;
                var qTerm = new XPQuery<DBTerminal>(session);
                IQueryable<DBTerminal> varQTerminal = from c in qTerm
                                                      where (c.DISABLED == 0) && (c.ID == id)
                                                      select c;
                if (varQTerminal.Any())
                {
                    var terminal = varQTerminal.FirstOrDefault();
                    if (terminal != null)
                    {
                        ProcessImpersonation pi = new ProcessImpersonation(log);
                        string fileName = string.Format(@"deployto_{0}.bat", terminal.ACCOUNTNUMBER);
                        string logFile = string.Format(@"deployto_{0}.log", terminal.ACCOUNTNUMBER);
                        pi.StartProcessInNewThread(fileName, logFile, terminal.FULLPATH);
                    }
                } else
                {
                    log.Error($"Terminal {id} doesn't exist or disabled for deployment. Exiting...");
                }
            }
            catch (Exception e)
            {
                log.Error("Error: DeployToAccount: " + e.ToString());
            }
            finally
            {
                isDeploying = false;
                session.Disconnect();
                session.Dispose();
            }
        }

        public delegate string DeployFunc(string folder, DBTerminal terminal, string file, string targetFolder);

        public string CopyFile(string folder, DBTerminal terminal, string file, string targetFolder)
        {
            return string.Format(@"xcopy /y {0} {1}{2}", file, targetFolder, Environment.NewLine);
        }

        public bool IsMQL5(string path)
        {
            return path.Contains("MQL5");
        }

        public string CompileFile(string folder, DBTerminal terminal, string file, string targetFolder)
        {
            string ext = Path.GetExtension(file);
            if (ext.Contains("mq5") || ext.Contains("mq4"))
            {
                string compilerApp = "\\metaeditor";
                if (IsMQL5(targetFolder))
                {
                    compilerApp += "64.exe";
                }
                else
                    compilerApp += ".exe";

                string compilerPath = Path.GetDirectoryName(terminal.FULLPATH) + compilerApp;
                string targetFile = terminal.CODEBASE + folder + "\\" + Path.GetFileName(file);
                return string.Format(@"""{0}"" /compile:""{1}"" {2}", compilerPath, targetFile, Environment.NewLine);
            }
            else
                return "";
        }

        string ProcessFolder(string folder, DBTerminal terminal, string sourceFolder, DeployFunc func)
        {
            string result = "";
            string currentSourceFolder = sourceFolder;
            if (folder.Length > 0)
                currentSourceFolder +=  folder;
            if (!Directory.Exists(currentSourceFolder))
                return result;
            try
            {
                var folders = Directory.EnumerateDirectories(currentSourceFolder);
                foreach (var file in folders)
                {
                    if (Directory.Exists(file.ToString()))
                    {
                        string subF = folder + "\\" + Path.GetFileName(file);
                        result += ProcessFolder(subF, terminal, sourceFolder, func);
                    }
                }

                var files = Directory.EnumerateFiles(currentSourceFolder);
                foreach (var file in files)
                {
                    if (File.Exists(file))
                    {
                        string targetFolder = terminal.CODEBASE + folder;
                        // process file
                        result += func(folder, terminal, file, targetFolder);
                        // result += string.Format(@"xcopy /y {0} {1}{2}", file, targetFolder, Environment.NewLine);
                    }
                }
            } catch (Exception e )
            {
                log.Info(e.ToString());
            }
            return result;
        }

        #endregion
    }
}