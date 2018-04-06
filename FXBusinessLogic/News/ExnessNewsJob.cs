using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FXBusinessLogic.fx_mind;
using FXBusinessLogic.Scheduler;
using log4net;
using Quartz;
using ScrapySharp.Network;
using HtmlAgilityPack;

namespace FXBusinessLogic.News
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class ExnessNewsJob : GenericJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ExnessNewsJob));
        public static string URL;

        public static string DATEFORMAT = "dd MMM yyyy";
        public static string TIMEFORMAT = "HH:mm";
        public static string SHORTDATETIMEFORMAT = "yyyy-M-d";
        protected string mCheckQuery;

        protected DateTime PARSEDATETIME;

        public ExnessNewsJob()
            : base(log)
        {
            URL = "https://www.exness.com/intl/en/tools/calendar";
/*            mCheckQuery = @"SELECT NE.ID
                          ,NE.HappenTime
                          ,NE.Name
                          ,NE.ParseTime
                          ,NE.Raised
                          ,NE.Importance
                          ,C.Name
                      FROM NewsEvent NE
                      INNER JOIN Currency C ON NE.CurrencyId = C.ID
                      WHERE C.Name='{0}' AND DATEPART(year, NE.HappenTime)={1} AND DATEPART(month, NE.HappenTime) = {2} AND DATEPART(day, NE.HappenTime)={3} AND DATEPART(hour, NE.HappenTime)={4} AND DATEPART(minute, NE.HappenTime)={5} AND NE.Importance={6}"; */
            mCheckQuery = @"SELECT NE.ID
                          ,NE.HappenTime
                          ,NE.Name
                          ,NE.ParseTime
                          ,NE.Raised
                          ,NE.Importance
                          ,C.Name
                      FROM NewsEvent NE
                      INNER JOIN Currency C ON NE.CurrencyId = C.ID
                      WHERE C.Name='{0}' AND YEAR(NE.HappenTime)={1} AND MONTH(NE.HappenTime) = {2} AND DAY(NE.HappenTime)={3} AND HOUR(NE.HappenTime)={4} AND MINUTE(NE.HappenTime)={5} AND NE.Importance={6}";
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
                log.InfoFormat("ExnessNewsJob started parsing: {0} executing at {1}", jobKey,
                    DateTime.Now.ToString("r"));

                PARSEDATETIME = DateTime.UtcNow;

                var browser = new ScrapingBrowser();
                WebPage homePage = null;
                Session session = FXConnectionHelper.GetNewSession();
                string strParseHistory = FXMindHelpers.GetGlobalVar(session, "NewsEvent.ParseHistory");

                bool parseAllHistory = false;
                DateTime curDateTime = DateTime.UtcNow;
                if (strParseHistory != null) parseAllHistory = bool.Parse(strParseHistory);
                if (parseAllHistory)
                {
                    string strParseHistoryStartDate = FXMindHelpers.GetGlobalVar(session, "NewsEvent.StartHistoryDate");
                    string strParseHistoryEndDate = FXMindHelpers.GetGlobalVar(session, "NewsEvent.EndHistoryDate");

                    if (strParseHistoryStartDate == null)
                        strParseHistoryStartDate = PARSEDATETIME.ToString(SHORTDATETIMEFORMAT);
                    if (strParseHistoryEndDate == null)
                        strParseHistoryEndDate = PARSEDATETIME.ToString(SHORTDATETIMEFORMAT);

                    string postData =
                        "Date=" + strParseHistoryStartDate +
                        "&SortField=Datetime&SortUpDown=Up&View_Period=Week1&EUR=on&USD=on&JPY=on&GBP=on&CHF=on&AUD=on&CAD=on&NZD=on&Important=All";
                    string postUrl = URL + "?" + postData;
                    homePage = browser.NavigateToPage(new Uri(postUrl), HttpVerb.Post, postData);

                    if (!DateTime.TryParseExact(strParseHistoryStartDate, SHORTDATETIMEFORMAT,
                        CultureInfo.InvariantCulture.DateTimeFormat,
                        DateTimeStyles.AdjustToUniversal, out curDateTime))
                    {
                        SetMessage("Error parsing History Start date: " + strParseHistoryStartDate);
                        log.Info(strMessage);
                        return;
                    }

                    DateTime endDate;
                    DateTime.TryParseExact(strParseHistoryEndDate, SHORTDATETIMEFORMAT,
                        CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal, out endDate);

                    DateTime nowDate = DateTime.UtcNow;
                    do
                    {
                        ParseOnePage(homePage, session, curDateTime);
                        curDateTime = curDateTime.AddDays(7);
                        postData = "Date=" + curDateTime.ToString(SHORTDATETIMEFORMAT) +
                                   "&SortField=Datetime&SortUpDown=Up&View_Period=Week1&EUR=on&USD=on&JPY=on&GBP=on&CHF=on&AUD=on&CAD=on&NZD=on&Important=All";
                        postUrl = URL + "?" + postData;
                        homePage = browser.NavigateToPage(new Uri(postUrl), HttpVerb.Post, postData);
                        if (homePage == null)
                            break;
                    } while (curDateTime <= nowDate && curDateTime <= endDate);
                }
                else
                {
                    homePage = browser.NavigateToPage(new Uri(URL));
                    ParseOnePage(homePage, session, curDateTime);
                }
                //PageWebForm form = homePage.FindForm("Calendar");
                //form["Date"] = "2013-3-3"; //Date
                //form["SortField"] = "Datetime"; //SortField
                //form["SortUpDown"] = "Up"; //SortUpDown
                //form["View_Period"] = "Week";
                //form["USD"] = "checked";
                //form.Method = HttpVerb.Post;
                //form.Action = "submit";
                //homePage = form.Submit(new Uri(postUrl), HttpVerb.Post);
                //HtmlNodeCollection colEvents = homePage.Html.SelectNodes("/html/body/div[5]/div/div[2]/div/table/tbody/tr");

                session.Disconnect();
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

        public bool ParseOnePage(WebPage homePage, Session session, DateTime curDateTime)
        {
            HtmlNodeCollection colEvents = homePage.Html.SelectNodes("//*[@class=\"row\"]");

            if (colEvents == null)
            {
                SetMessage("Error parsing Events page");
                return false;
            }

            //var eventscol = new XPCollection<DBNewsEvent>(session);
            char[] trimarr = {' ', '\n', '\t', '\"', '\''}; //, '\n', '%', '&', 'n', 'b', 's', 'p', ';' };

            foreach (HtmlNode eventNode in colEvents)
            {
                var eventRow = new DBNewsEvent(session);
                try
                {
                    eventRow.ParseTime = PARSEDATETIME;
                    IEnumerable<HtmlNode> htmlEventRows = eventNode.Descendants("td");
                    HtmlNode nodeDate = htmlEventRows.First();
                    if (nodeDate != null)
                    {
                        string trimDate = nodeDate.InnerText.Trim(trimarr);
                        if (trimDate.Length != 0)
                        {
                            trimDate += " " + curDateTime.Year;
                            if (
                                !DateTime.TryParseExact(trimDate, DATEFORMAT,
                                    CultureInfo.InvariantCulture.DateTimeFormat,
                                    DateTimeStyles.AdjustToUniversal, out curDateTime))
                                log.Info("Error parsing date column: " + trimDate);
                        }
                    }

                    HtmlNode nodeTime = htmlEventRows.ElementAt(1);
                    if (nodeTime != null)
                    {
                        DateTime currentTime;
                        string trimTime = nodeTime.InnerText; // .TrimStart(trimarr);
                        trimTime = trimTime.Trim(trimarr);
                        if (trimTime.Length != 0)
                            if (DateTime.TryParseExact(trimTime, TIMEFORMAT,
                                CultureInfo.InvariantCulture.DateTimeFormat,
                                DateTimeStyles.AdjustToUniversal, out currentTime))
                                curDateTime = new DateTime(curDateTime.Year, curDateTime.Month, curDateTime.Day,
                                    currentTime.Hour, currentTime.Minute, 0);
                            else
                                log.Info("Error parsing time event column: " + trimTime);
                    }

                    eventRow.HappenTime = curDateTime;
                    HtmlNode nodeCurrency = htmlEventRows.ElementAt(2);
                    if (nodeCurrency != null)
                    {
                        string trimCurr = nodeCurrency.InnerText;
                        trimCurr = trimCurr.Trim(trimarr);
                        if (trimCurr.Length != 0) eventRow.CurrencyId = FXMindHelpers.getCurrencyID(session, trimCurr);
                    }

                    if (eventRow.CurrencyId == null)
                        continue;

                    HtmlNode nodeName = htmlEventRows.ElementAt(3);
                    if (nodeName != null)
                    {
                        string eventName = nodeName.InnerText;
                        eventName = HttpUtility.HtmlDecode(eventName);
                        eventName = eventName.Trim(trimarr);
                        eventRow.Name = eventName;
                    }

                    HtmlNode nodeImportance = htmlEventRows.ElementAt(4);
                    if (nodeImportance != null)
                    {
                        string eventImportance = nodeImportance.InnerText;
                        eventImportance = eventImportance.Trim(trimarr).ToLower();
                        eventRow.Importance = 0;
                        switch (eventImportance)
                        {
                            case "low":
                                eventRow.Importance = 0;
                                break;
                            case "medium":
                                eventRow.Importance = 1;
                                break;
                            case "high":
                                eventRow.Importance = 2;
                                break;
                        }
                    }

                    HtmlNode nodeActual = htmlEventRows.ElementAt(5);
                    if (nodeActual != null)
                    {
                        string eventActual = nodeActual.InnerText;
                        eventActual = eventActual.Trim(trimarr);
                        eventRow.ActualVal = eventActual;
                    }

                    HtmlNode nodeForecast = htmlEventRows.ElementAt(6);
                    if (nodeForecast != null)
                    {
                        string eventForecast = nodeForecast.InnerText;
                        eventForecast = eventForecast.Trim(trimarr);
                        eventRow.ForecastVal = eventForecast;
                    }

                    HtmlNode nodePrevious = htmlEventRows.ElementAt(7);
                    if (nodePrevious != null)
                    {
                        string eventPrevious = nodePrevious.InnerText;
                        eventPrevious = eventPrevious.Trim(trimarr);
                        eventRow.PreviousVal = eventPrevious;
                    }
                }
                catch (Exception e)
                {
                    log.Info("Error parsing news event: " + e);
                    continue;
                }

                string resultQuery = string.Format(mCheckQuery, eventRow.CurrencyId.Name, curDateTime.Year,
                    curDateTime.Month, curDateTime.Day, curDateTime.Hour, curDateTime.Minute, eventRow.Importance);
                SelectedData data = session.ExecuteQuery(resultQuery);
                int count = data.ResultSet[0].Rows.Count();
                if (count > 0)
                    continue;
                if (eventRow.ParseTime >= eventRow.HappenTime)
                    eventRow.Raised = true;
                else
                    eventRow.Raised = false;

                session.Save(eventRow);
                //eventscol.Add(eventRow);
            }

            //session.Save(eventscol);
            SetMessage("Events for start Date: " + curDateTime + " parsed successfully");
            log.Info(strMessage);
            return true;
        }
    }
}