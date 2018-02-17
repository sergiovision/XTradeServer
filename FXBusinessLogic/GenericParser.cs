using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DevExpress.Xpo;
using FXBusinessLogic.fx_mind;

namespace FXBusinessLogic
{
    public abstract class GenericParser
    {
        public const string UserAgent =
            "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; Zune 4.0; InfoPath.3; MS-RTC LM 8; .NET4.0C; .NET4.0E)";

        protected bool? bUseInterval;
        protected bool doHistoryParsing;

        protected DateTime IntervalEndDate;
        protected DateTime IntervalStartDate;
        protected Session session;

        public GenericParser(Session ses, bool parseHistory)
        {
            session = ses;
            doHistoryParsing = parseHistory;
        }

        public void setDatesInterval()
        {
            if (IsUseDateInterval())
            {
                string strFrom = FXMindHelpers.GetGlobalVar(session, "StartDateInterval");
                string strTo = FXMindHelpers.GetGlobalVar(session, "EndDateInterval");
                DateTime outdt = DateTime.MinValue;
                DateTime.TryParseExact(strFrom, "MM/dd/yyyy", CultureInfo.InvariantCulture.DateTimeFormat,
                    DateTimeStyles.AssumeUniversal, out IntervalStartDate);
                DateTime.TryParseExact(strTo, "MM/dd/yyyy", CultureInfo.InvariantCulture.DateTimeFormat,
                    DateTimeStyles.AssumeUniversal, out IntervalEndDate);
            }
        }

        public bool IsDateInInterval(DateTime date)
        {
            if (doHistoryParsing == false)
                return true;
            if (IsUseDateInterval() == false)
                return true;
            if (date >= IntervalStartDate && date <= IntervalEndDate)
                return true;
            return false;
        }

        public bool IsUseDateInterval()
        {
            if (!bUseInterval.HasValue)
            {
                string val = FXMindHelpers.GetGlobalVar(session, "UseDateInterval");
                if (val.Equals("true"))
                    bUseInterval = true;
                else
                    bUseInterval = false;
            }

            return bUseInterval.Value;
        }

        public static async Task<string> GetDataRequest(string url, bool useAgent)
        {
            var httpClient = new HttpClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            if (useAgent) httpRequestMessage.Headers.Add("User-Agent", UserAgent);
            HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        public DBSymbol getSymbolID(string SymbolStr)
        {
            var symbolsQuery = new XPQuery<DBSymbol>(session);
            IQueryable<DBSymbol> symbols = from c in symbolsQuery
                where c.Name == SymbolStr
                select c;
            if (symbols.Any()) return symbols.First();
            return null;
        }
    }
}