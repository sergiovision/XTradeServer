using System;
using System.Collections.Generic;
using System.Globalization;
using BusinessObjects;
using FXBusinessLogic.BusinessObjects;
using log4net;

namespace FXBusinessLogic.ThriftServer
{
    internal class FXMindMQLHandler : FXMindMQL.Iface
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FXMindMQLHandler));

        private readonly MainService fxmind;

        public FXMindMQLHandler()
        {
            fxmind = MainService.thisGlobal; //.Resolve<IMainService>();
        }

        public List<string> ProcessStringData(Dictionary<string, string> paramsList, List<string> inputData)
        {
            if (fxmind.IsDebug())
                log.Info("server(" + GetHashCode() + ") ProcessStringData: " + inputData.Count);
            var list = new List<string>();
            if (!paramsList.ContainsKey("func"))
            {
                log.Error("server(" + GetHashCode() + ") ProcessStringData: Params error");
                return list;
            }

            string func = paramsList["func"];
            try
            {
                switch (func)
                {
                    case "NextNewsEvent":
                    {
                        DateTime date;
                        DateTime.TryParseExact(paramsList["time"], fxmindConstants.MTDATETIMEFORMAT,
                            CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out date);

                        string symbolStr = paramsList["symbol"];
                        //int BeforeMin = Int32.Parse(paramsList["before"]);
                        byte minImportance = byte.Parse(paramsList["importance"]);
                        NewsEventInfo info = null;
                        if (fxmind.GetNextNewsEvent(date, symbolStr, minImportance, ref info))
                        {
                            //log.Info( info.RaiseDateTime.ToString(MainService.MTDATETIMEFORMAT) + " Got news: (" + info.Name + ") Importance:  " + info.Importance.ToString());
                            list.Add(info.Currency);
                            list.Add(info.Importance.ToString());
                            list.Add(info.RaiseDateTime.ToString(fxmindConstants.MTDATETIMEFORMAT));
                            list.Add(info.Name);
                        }
                    }
                        break;
                    case "Somefunc":
                    {
                    }
                        break;
                }
            }
            catch (Exception e)
            {
                log.Error("ProcessStringData Error:" + e);
            }

            return list;
        }

        public List<double> ProcessDoubleData(Dictionary<string, string> paramsList, List<string> inputData)
        {
            if (fxmind.IsDebug())
                log.Info("server(" + GetHashCode() + ") ProcessDoubleData: " + inputData.Count);
            var list = new List<double>();
            if (!paramsList.ContainsKey("func"))
            {
                log.Error("server(" + GetHashCode() + ") ProcessDoubleData: Params error");
                return list;
            }

            try
            {
                string func = paramsList["func"];
                switch (func)
                {
                    case "CurrentSentiments":
                    {
                        string symbolStr = paramsList["symbol"];
                        DateTime date;
                        DateTime.TryParseExact(paramsList["time"], fxmindConstants.MTDATETIMEFORMAT,
                            CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out date);
                        double longVal = 0;
                        double shortVal = 0;
                        fxmind.GetAverageLastGlobalSentiments(date, symbolStr, out longVal, out shortVal);
                        list.Add(longVal);
                        list.Add(shortVal);
                    }
                        break;
                    case "SentimentsArray":
                    {
                        string symbolStr = paramsList["symbol"];
                        int siteId = int.Parse(paramsList["site"]);
                        list = fxmind.iGlobalSentimentsArray(symbolStr, inputData, siteId);
                    }
                        break;
                    case "CurrencyStrengthArray":
                    {
                        string currencyStr = paramsList["currency"];
                        int timeframe = int.Parse(paramsList["timeframe"]);
                        list = fxmind.iCurrencyStrengthAll(currencyStr, inputData, timeframe);
                    }
                        break;
                }
            }
            catch (Exception e)
            {
                log.Error("ProcessDoubleData Error:" + e);
            }

            return list;
        }

        public long IsServerActive(Dictionary<string, string> paramsList)
        {
            if (fxmind.IsDebug())
                log.Info("server(" + GetHashCode() + ") IsServerActive");
            return 1;
        }

        public void PostStatusMessage(Dictionary<string, string> paramsList)
        {
            //if (fxmind.IsDebug())
            log.Info("server(" + GetHashCode() + ") PostStatusMessage ("
                     + paramsList["account"] + ", " + paramsList["magic"] + "): " + paramsList["message"]);
        }
    }
}