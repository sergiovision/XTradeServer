using System;
using System.Threading.Tasks;
using DevExpress.Xpo;
using FXBusinessLogic;
using FXBusinessLogic.fx_mind;
using FXBusinessLogic.Scheduler;
using HtmlAgilityPack;
using log4net;
using Quartz;
using ScrapySharp.Network;

namespace com.fxmind.manager.jobs
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class MyFXBookRatioJob : GenericJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MyFXBookRatioJob));
        public static string URL;

        public MyFXBookRatioJob() : base(log)
        {
            URL = "https://www.myfxbook.com/community/outlook";
        }

        public override async Task Execute(IJobExecutionContext context)
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
                log.InfoFormat("MyFXBookRatioJob started parsing: {0} executing at {1}", jobKey,
                    DateTime.Now.ToString("r"));

                var browser = new ScrapingBrowser();
                WebPage homePage = browser.NavigateToPage(new Uri(URL));
                HtmlNode document = homePage.Html;
                int i = 0;
                HtmlNode nodeTooltip = document.SelectSingleNode("//*[@id=\"outlookTip" + i++ + "\"]");

                Session session = FXConnectionHelper.GetNewSession();

                char[] trimarr = {' ', '\n', '%', '&', 'n', 'b', 's', 'p', ';'};
                while (nodeTooltip != null)
                {
                    string HTMLTable = nodeTooltip.GetAttributeValue("value", "");
                    if (HTMLTable.Length > 0)
                    {
                        var docTable = new HtmlDocument();
                        docTable.LoadHtml(HTMLTable);
                        HtmlNode nodeSymbol = docTable.DocumentNode.SelectSingleNode("table/tr[1]/td");
                        string strSymbol = "";
                        if (nodeSymbol != null)
                            strSymbol = nodeSymbol.InnerText;
                        if (strSymbol.Length == 6)
                            strSymbol = strSymbol.Insert(3, "/");
                        DBSymbol dbsym = FXMindHelpers.getSymbolID(session, strSymbol);
                        if (dbsym != null)
                        {
                            var posRatio = new DBOpenPosRatio(session);
                            posRatio.SiteID = 4;
                            posRatio.ParseTime = DateTime.UtcNow;

                            posRatio.SymbolID = dbsym.ID;
                            HtmlNode nodeLong = docTable.DocumentNode.SelectSingleNode("table/tr[3]/td[2]");
                            if (nodeLong != null)
                            {
                                string trim = nodeLong.InnerText.Trim(trimarr);
                                if (trim.Length == 0)
                                    posRatio.LongRatio = 0;
                                else
                                    posRatio.LongRatio = float.Parse(trim);
                            }

                            HtmlNode nodeShort = docTable.DocumentNode.SelectSingleNode("table/tr[2]/td[2]");
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
                    }

                    nodeTooltip = document.SelectSingleNode("//*[@id=\"outlookTip" + i++ + "\"]");
                    //nodeTooltip = document.DocumentNode.SelectSingleNode("//*[@id=\"outlookTip" + (i++) + "\"]");
                }

                session.Dispose();

                SetMessage("Succeeded");
            }
            catch (Exception ex)
            {
                SetMessage("ERROR: " + ex);
            }

            Exit(context);
            await Task.CompletedTask;

        }
    }
}