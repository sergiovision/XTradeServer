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
using FXBusinessLogic.BusinessObjects;
using System.Text;
using System.Xml.Linq;
using FXBusinessLogic;

namespace com.fxmind.manager.jobs
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class ForexFactoryNewsJob : GenericJob
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(ExnessNewsJob));
        public static string DATEFORMAT = "MM-dd-yyyy";
        public static string TIMEFORMAT = "h:mm tt";
        public static string SHORTDATETIMEFORMAT = "yyyy-M-d";
        protected string mCheckQuery;

        protected DateTime parseDateTime;

        public static String URL = "http://www.forexfactory.com/ffcal_week_this.xml";
        public int eventsAdded = 0;

        public ForexFactoryNewsJob()
                : base(log)
        {
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

//            mCheckQuery = "SELECT NE.ID, NE.HappenTime, NE.Name, NE.ParseTime, NE.Raised, NE.Importance, NE.CurrencyId FROM NewsEvent NE " +
//                                     "WHERE (NE.CurrencyId=?) AND YEAR(NE.HappenTime)=? AND MONTH(NE.HappenTime) = ? AND DAY(NE.HappenTime)=? AND HOUR(NE.HappenTime)=? " +
//                                     " AND MINUTE(NE.HappenTime)=? AND (NE.Importance = ?)";
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

                TimeZoneInfo tz = MainService.thisGlobal.GetBrokerTimeZone();

                parseDateTime = DateTime.UtcNow;
                
                DateTime curDate = TimeZoneInfo.ConvertTimeFromUtc(parseDateTime, tz);
                DateTime nowDate = curDate;

                //var browser = new ScrapingBrowser();
                //WebPage homePage = browser.NavigateToPage(new Uri(URL));
                XElement homePage = XElement.Load(URL);

                Session session = FXConnectionHelper.GetNewSession();

                DateTime midnight = new DateTime(curDate.Year, curDate.Month, curDate.Day);
                ParseOnePage(homePage, session, midnight);

                session.Disconnect();
                session.Dispose();

                //SetMessage("Succeeded");
            }
            catch (Exception ex)
            {
                SetMessage("ERROR: " + ex);
            }

            Exit(context);
            await Task.CompletedTask;
        }

        private bool ParseOnePage(XElement document, Session session, DateTime curDateTime)
        {
            eventsAdded = 0;
            DBNewsEvent eventRow = null;
            var nodes = document.Elements("event");
            foreach (var el in nodes) {
                try {
                    eventRow = new DBNewsEvent(session);
                    eventRow.IndicatorValue = 0.0;
                    eventRow.ParseTime = parseDateTime;

                    var eTitle = el.Element("title");
                    String eventName = eTitle.Value;
                    eventRow.Name = eventName;

                    var eCountry = el.Element("country");
                    string Curr = eCountry.Value;
                    if (!String.IsNullOrEmpty(Curr)) {
                        if (Curr.Equals("ALL"))
                            eventRow.CurrencyId = FXMindHelpers.getCurrencyID(session, "USD");
                        else
                            eventRow.CurrencyId = FXMindHelpers.getCurrencyID(session, Curr);
                    }

                    var eDate = el.Element("date");
                    String strDate = eDate.Value;

                    DateTime curDate = curDateTime;
                    if (DateTime.TryParseExact(strDate, DATEFORMAT,
                        CultureInfo.InvariantCulture.DateTimeFormat,
                        DateTimeStyles.None, out curDate))
                    {

                    }

                    var eTime = el.Element("time");
                    String Time = eTime.Value;
                    int i = Time.IndexOf('a');
                    if (i <= 0)
                        i = Time.IndexOf('p');
                    Time = new StringBuilder(Time).Insert(i, " ").ToString();
                    Time = Time.ToUpper();

                    DateTime currentTime = curDateTime;
                    if (DateTime.TryParseExact(Time, TIMEFORMAT,
                                CultureInfo.InvariantCulture.DateTimeFormat,
                                DateTimeStyles.None, out currentTime))
                    {

                    }

                    curDateTime = new DateTime(curDate.Year, curDate.Month, curDate.Day,
                            currentTime.Hour, currentTime.Minute, 0);

                    eventRow.HappenTime = curDateTime; // Usually in UTC on this website.

                    var eImpact = el.Element("impact");
                    String Impact = eImpact.Value;
                    eventRow.Importance = (byte)0;
                    switch (Impact) {
                        case "Medium":
                            eventRow.Importance = (byte)1;
                            break;
                        case "High":
                            eventRow.Importance = (byte)2;
                            break;
                        case "Low":
                        default:
                            eventRow.Importance = (byte)0;
                            break;
                    }

                    var eForecast = el.Element("forecast");
                    String Forecast = eForecast.Value;
                    eventRow.ForecastVal = Forecast;

                    var ePrevious = el.Element("previous");
                    String Previous = ePrevious.Value;
                    eventRow.PreviousVal = Previous;

                    // save event
                    string resultQuery = string.Format(mCheckQuery, eventRow.CurrencyId.Name, curDateTime.Year,
                        curDateTime.Month, curDateTime.Day, curDateTime.Hour, curDateTime.Minute, eventRow.Importance);

                    SelectedData data = session.ExecuteQuery(resultQuery);
                    int count = data.ResultSet[0].Rows.Count();
                    if (count > 0)
                        continue;

                    if (parseDateTime >= eventRow.HappenTime)
                        eventRow.Raised = true;
                    else
                        eventRow.Raised = false;

                    eventsAdded++;

                    session.Save(eventRow);

                }
                catch (Exception e) {
                    log.Info("Error parsing news event: " + el.Value + "Error: " + e.ToString());
                    continue;
                }
            }
            SetMessage("ForexFactoryEvents for End Date: " + curDateTime + " done. Added " + eventsAdded + " events");
            return true;
        }
    }

}

