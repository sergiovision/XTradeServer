using System;
using System.Linq;
using DevExpress.Xpo;
using FXBusinessLogic.fx_mind;
using FXBusinessLogic.Scheduler;
using HtmlAgilityPack;
using log4net;
using Quartz;
using ScrapySharp.Network;

namespace FXBusinessLogic.PosRatio
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class OandaRatioJob : GenericJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (OandaRatioJob));
        public static string URL;

        public OandaRatioJob()
            : base(log)
        {
            URL = "http://fxtrade.oanda.com/analysis/open-position-ratios";
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
                log.InfoFormat("OandaRatioJob started parsing: {0} executing at {1}", jobKey, DateTime.Now.ToString("r"));

                var browser = new ScrapingBrowser();
                WebPage homePage = browser.NavigateToPage(new Uri(URL));
                HtmlNodeCollection colSymbols =
                    homePage.Html.SelectNodes("/html/body/section/div/div[4]/div/div/div/div[3]/ol/li");
                if (colSymbols == null)
                {
                    SetMessage("Error parsing OANDA ratio page");
                    return;
                }
                Session session = FXConnectionHelper.GetNewSession();
                char[] trimarr = {' ', '\n', '%', '&', 'n', 'b', 's', 'p', ';'};
                foreach (HtmlNode symnode in colSymbols)
                {
                    var posRatio = new DBOpenPosRatio(session);
                    posRatio.SiteID = 5;
                    posRatio.ParseTime = DateTime.UtcNow;
                    string symbolname = symnode.Attributes["name"].Value;
                    DBSymbol dbsym = FXMindHelpers.getSymbolID(session, symbolname);
                    if (dbsym != null)
                        posRatio.SymbolID = dbsym.ID;
                    HtmlNode nodeLong = symnode.Descendants("div").First().Descendants("span").First();
                    if (nodeLong != null)
                    {
                        string trim = nodeLong.InnerText.Trim(trimarr);
                        if (trim.Length == 0)
                            posRatio.LongRatio = 0;
                        else
                            posRatio.LongRatio = float.Parse(trim);
                    }
                    HtmlNode nodeShort = symnode.Descendants("div").ElementAt(1).Descendants("span").ElementAt(1);
                    if (nodeShort != null)
                    {
                        string trim = nodeShort.InnerText.Trim(trimarr);
                        if (trim.Length == 0)
                            posRatio.ShortRatio = 0;
                        else
                            posRatio.ShortRatio = float.Parse(trim);
                    }
                    session.Save(posRatio);
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