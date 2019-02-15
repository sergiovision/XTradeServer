using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using System.Net.Http;
using System.Net;
using System.Globalization;

public class NewsCalendarEvent
{
    public string text { get; set; }
    public DateTime startDate { get; set; }
    public DateTime endDate { get; set; }
    public string currency { get; set; }

    public int importance { get; set; }
    //public string forecastVal { get; set; }
    //public string previousVal { get; set; }
}


namespace XTrade.MainServer
{
    [RoutePrefix("api")]
    [Authorize]
    public class NewsController : BaseController
    {
        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<NewsCalendarEvent> Get([FromUri] string datetime, [FromUri] string symbol,
            [FromUri] int importance, [FromUri] int timezoneoffset)
        {
            try
            {
                DateTime today = DateTime.Now;
                DateTime brokerTimeToday = today;
                if (string.IsNullOrEmpty(datetime))
                {
                    today = DateTime.MaxValue;
                    brokerTimeToday = today;
                }
                else
                {
                    today = DateTime.Parse(datetime);
                    TimeZoneInfo BrokerTimeZoneInfo = MainService.GetBrokerTimeZone();
                    brokerTimeToday = TimeZoneInfo.ConvertTimeFromUtc(today, BrokerTimeZoneInfo);
                }

                IEnumerable<NewsEventInfo> events =
                    MainService.GetTodayNews(brokerTimeToday, symbol, (byte) importance, timezoneoffset);
                if (events != null && events.Count() > 0)
                {
                    List<NewsCalendarEvent> news = new List<NewsCalendarEvent>();
                    foreach (var ev in events)
                    {
                        NewsCalendarEvent nce = new NewsCalendarEvent();
                        nce.currency = ev.Currency;
                        DateTime date;
                        DateTime.TryParseExact(ev.RaiseDateTime, xtradeConstants.MTDATETIMEFORMAT,
                            CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out date);

                        nce.startDate = date;
                        nce.endDate = date.AddHours(1);
                        nce.importance = ev.Importance;
                        //nce.forecastVal = ev.ForecastVal;
                        //nce.previousVal = ev.PreviousVal;
                        string vals = ". f=" + ev.ForecastVal;
                        if (!string.IsNullOrEmpty(ev.PreviousVal))
                            vals += " p=" + ev.PreviousVal;
                        nce.text = ev.Currency + ": " + ev.Name + vals;
                        news.Add(nce);
                    }

                    return news;
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }

            return null;
        }

        /*
        [AcceptVerbs("PUT")]
        [HttpPut]
        public HttpResponseMessage Put(Terminal terminal)
        {
            try
            {
                if (terminal == null)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Empty Terminal passed to Put method!");
                }
                if (MainService.UpdateTerminals(terminal))
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Failed to update");
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        */
    }
}