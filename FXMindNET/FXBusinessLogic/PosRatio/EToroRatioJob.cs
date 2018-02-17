using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Xpo;
using FXBusinessLogic.eToro;
using FXBusinessLogic.fx_mind;
using FXBusinessLogic.Scheduler;
using log4net;
using Quartz;
using Quartz.Collection;

namespace FXBusinessLogic.PosRatio
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class EToroRatioJob : GenericJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (EToroRatioJob));

        //public static string URL = "https://openbook.etoro.com/API/markets";  // https://openbook.etoro.com/markets/currencies/";
        public static string URL = "https://openbook.etoro.com/API/Markets/Symbol/InstrumentRate/?name=";
        private HashSet<string> set;

        public EToroRatioJob() : base(log)
        {
            SetNonSupportedSymbols();
        }

        private void SetNonSupportedSymbols()
        {
            set = new HashSet<string>();
            set.Add("GBP/CHF");
            set.Add("AUD/NZD");
            set.Add("AUD/CAD");
            set.Add("AUD/CHF");
            set.Add("CAD/CHF");
            set.Add("EUR/NZD");
            set.Add("GBP/AUD");
            set.Add("GBP/CAD");
            set.Add("GBP/NZD");
            set.Add("NZD/CAD");
            set.Add("NZD/CHF");
            set.Add("NZD/JPY");
        }

        public override void Execute(IJobExecutionContext context)
        {
            try
            {
                if (Begin(context))
                {
                    SetMessage("Job Locked");
                    Exit(context);
                    return;
                }
                JobKey jobKey = context.JobDetail.Key;
                log.InfoFormat("EToroRatioJob started parsing: {0} executing at {1}", jobKey, DateTime.Now.ToString("r"));
                Session session = FXConnectionHelper.GetNewSession();
                
                IQueryable<DBSymbol> symbols = FXMindHelpers.getTechSymbols(session);
                foreach (DBSymbol symbol in symbols)
                {
                    if (set.Contains(symbol.Name))
                        continue;
                    string URLSuffix = symbol.Name.Replace('/', '-');
                    string _urlAction = URL + URLSuffix.ToLower();
                    InstrumentRate result = null;
                    try
                    {
                        Task<string> data = GenericParser.GetDataRequest(_urlAction, true);
                        while (!data.IsCompleted)
                        {
                            Thread.Sleep(10);
                        }
                        if (data == null)
                            continue;
                        if (data.Result == null)
                            continue;
                        var parser = new EToroJsonInstrumentParseProvider();
                        result = parser.Parse(data.Result);
                    }
                    catch (Exception)
                    {
                        log.InfoFormat("Parsing failed for Symbol: " + _urlAction);
                        continue;
                    }
                    if (result != null)
                    {
                        var posRatio = new DBOpenPosRatio(session);
                        posRatio.SiteID = 2;
                        posRatio.ParseTime = DateTime.UtcNow;

                        posRatio.SymbolID = symbol.ID;
                        bool hasBuy = false;
                        bool hasSell = false;
                        if (result.SentimentPercent.HasValue && result.SentimentType.Contains("Buy"))
                        {
                            hasBuy = true;
                            posRatio.LongRatio = (float) result.SentimentPercent.Value;
                        }
                        if (result.SentimentPercent.HasValue && result.SentimentType.Contains("Sell"))
                        {
                            hasSell = true;
                            posRatio.ShortRatio = (float) result.SentimentPercent.Value;
                        }

                        if (hasBuy)
                        {
                            posRatio.ShortRatio = (float) 100.0 - (float) result.SentimentPercent.Value;
                        }
                        if (hasSell)
                        {
                            posRatio.LongRatio = (float) 100.0 - (float) result.SentimentPercent.Value;
                        }

                        session.Save(posRatio);
                    }
                }

                session.Dispose();

                SetMessage("Succeeded");
            }
            catch (Exception ex)
            {
                SetMessage("ERROR: " + ex);
            }
            Exit(context);
        }
    }
}