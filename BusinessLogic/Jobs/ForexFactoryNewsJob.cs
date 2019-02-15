using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Scheduler;
using Quartz;
using BusinessLogic.BusinessObjects;
using System.Text;
using System.Xml.Linq;
using BusinessLogic.Repo;
using Autofac;
using NHibernate;

namespace BusinessLogic.Jobs
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class ForexFactoryNewsJob : GenericJob
    {
        public static string DATEFORMAT = "MM-dd-yyyy";
        public static string TIMEFORMAT = "h:mm tt";
        public static string SHORTDATETIMEFORMAT = "yyyy-M-d";
        public static string URL = "http://www.forexfactory.com/ffcal_week_this.xml";
        protected DataService dataService;
        public int eventsAdded;
        protected string mCheckQuery;
        protected DateTime parseDateTime;

        public ForexFactoryNewsJob()
        {
            mCheckQuery = @"SELECT NE.*
                      FROM newsevent NE
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

                TimeZoneInfo tz = MainService.thisGlobal.GetBrokerTimeZone();

                parseDateTime = DateTime.UtcNow;

                DateTime curDate = TimeZoneInfo.ConvertTimeFromUtc(parseDateTime, tz);
                DateTime nowDate = curDate;

                //var browser = new ScrapingBrowser();
                //WebPage homePage = browser.NavigateToPage(new Uri(URL));
                XElement homePage = XElement.Load(URL);

                dataService = MainService.thisGlobal.Container.Resolve<DataService>();

                DateTime midnight = new DateTime(curDate.Year, curDate.Month, curDate.Day);
                ParseOnePage(homePage, midnight);
            }
            catch (Exception ex)
            {
                SetMessage("ERROR: " + ex);
            }

            Exit(context);
            await Task.CompletedTask;
        }

        private bool ParseOnePage(XElement document, DateTime curDateTime)
        {
            eventsAdded = 0;
            DBNewsevent eventRow = null;
            var nodes = document.Elements("event");

            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                foreach (var el in nodes)
                    try
                    {
                        eventRow = new DBNewsevent();
                        eventRow.Parsetime = parseDateTime;

                        var eTitle = el.Element("title");
                        string eventName = eTitle.Value;
                        eventRow.Name = eventName;

                        var eCountry = el.Element("country");
                        string Curr = eCountry.Value;
                        if (!string.IsNullOrEmpty(Curr))
                        {
                            if (Curr.Equals("ALL"))
                                eventRow.Currency = dataService.getCurrencyID("USD");
                            else
                                eventRow.Currency = dataService.getCurrencyID(Curr);
                        }

                        var eDate = el.Element("date");
                        string strDate = eDate.Value;

                        DateTime curDate = curDateTime;
                        if (DateTime.TryParseExact(strDate, DATEFORMAT,
                            CultureInfo.InvariantCulture.DateTimeFormat,
                            DateTimeStyles.None, out curDate))
                        {
                        }

                        var eTime = el.Element("time");
                        string Time = eTime.Value;
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

                        eventRow.Happentime = curDateTime; // Usually in UTC on this website.

                        var eImpact = el.Element("impact");
                        string Impact = eImpact.Value;
                        eventRow.Importance = 0;
                        switch (Impact)
                        {
                            case "Medium":
                                eventRow.Importance = 1;
                                break;
                            case "High":
                                eventRow.Importance = 2;
                                break;
                            case "Low":
                            default:
                                eventRow.Importance = 0;
                                break;
                        }

                        var eForecast = el.Element("forecast");
                        string Forecast = eForecast.Value;
                        eventRow.Forecastval = Forecast;

                        var ePrevious = el.Element("previous");
                        string Previous = ePrevious.Value;
                        eventRow.Previousval = Previous;

                        // save event
                        string resultQuery = string.Format(mCheckQuery, eventRow.Currency.Name, curDateTime.Year,
                            curDateTime.Month, curDateTime.Day, curDateTime.Hour, curDateTime.Minute,
                            eventRow.Importance);

                        IList<DBNewsevent> result =
                            dataService.ExecuteNativeQuery<DBNewsevent>(Session, resultQuery, "NE");
                        int count = result.Count;
                        if (count > 0)
                            continue;

                        if (parseDateTime >= eventRow.Happentime)
                            eventRow.Raised = 1;
                        else
                            eventRow.Raised = 0;

                        eventsAdded++;

                        dataService.SaveInsertNewsEvent(eventRow);
                    }
                    catch (Exception e)
                    {
                        log.Info("Error parsing news event: " + el.Value + "Error: " + e);
                    }
            }

            SetMessage("ForexFactoryEvents for End Date: " + curDateTime + " done. Added " + eventsAdded + " events");
            return true;
        }
    }
}