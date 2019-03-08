using BusinessObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Autofac;
using System.Threading.Tasks;

namespace BusinessLogic.BusinessObjects
{
    public class ServerSignalsHandler : ISignalHandler
    {
        private readonly MainService xtrade;
        private readonly IWebLog log;
        private readonly ITerminalEvents terminals;

        public ServerSignalsHandler()
        {
            xtrade = MainService.thisGlobal; //.Resolve<IMainService>();
            terminals = MainService.thisGlobal.Container.Resolve<ITerminalEvents>();
            log = MainService.thisGlobal.Container.Resolve<IWebLog>();
        }

        public SignalInfo ListenSignal(long flags, long objectId)
        {
            return xtrade.ListenSignal(flags, objectId);
        }

        public void PostSignal(SignalInfo signal)
        {
            if ((SignalFlags) signal.Flags == SignalFlags.Cluster)
            {
                xtrade.PostSignalTo(signal);
                return;
            }

            switch ((EnumSignals) signal.Id)
            {
                case EnumSignals.SIGNAL_CHECK_HEALTH:
                    if (xtrade.IsDebug())
                        log.Info("CheckHealth: " + signal.Flags);
                    break;
                case EnumSignals.SIGNAL_DEALS_HISTORY:
                {
                    List<DealInfo> deals = null;
                    if (signal.Data != null)
                        deals = JsonConvert.DeserializeObject<List<DealInfo>>(signal.Data.ToString());
                    else
                        deals = new List<DealInfo>();
                    xtrade.SaveDeals(deals);
                }
                    break;
                case EnumSignals.SIGNAL_CHECK_BALANCE:
                {
                    if (signal.Data == null)
                        break;
                    JArray jarray = (JArray) signal.Data;
                    if (jarray == null || jarray.Count == 0)
                        break;
                    decimal balance = jarray.First.Value<decimal?>("Balance") ?? 0;
                    decimal equity = jarray.First.Value<decimal?>("Equity") ?? 0;
                    int Account = jarray.First.Value<int?>("Account") ?? 0;
                    xtrade.UpdateBalance(Account, balance, equity);
                }
                    break;
                case EnumSignals.SIGNAL_UPDATE_RATES:
                    break;
                case EnumSignals.SIGNAL_ACTIVE_ORDERS:
                {
                    // Dictionary<string, string> signal = JsonConvert.DeserializeObject<Dictionary<string, string>>(parameters);
                    // var jObject = JObject.Parse(parameters);
                    // var jTokenData = jObject.GetValue("Data");
                    // var Value = jObject.GetValue("Value");
                    // if (jTokenData != null)
                    List<PositionInfo> positions = null;
                    if (signal.Data != null)
                        positions = JsonConvert.DeserializeObject<List<PositionInfo>>(signal.Data.ToString());
                    else
                        positions = new List<PositionInfo>();
                    terminals.UpdatePositions(signal.ObjectId, signal.Value, positions);
                }
                    break;
                case EnumSignals.SIGNAL_WARN_NEWS:
                    break;
                    case EnumSignals.SIGNAL_DEINIT_EXPERT:
                    {
                        ExpertInfo expert = JsonConvert.DeserializeObject<ExpertInfo>(signal.Data.ToString());
                        xtrade.DeInitExpert(expert);
                    }
                    break;
                case EnumSignals.SIGNAL_DEINIT_TERMINAL:
                    {
                        ExpertInfo expert = JsonConvert.DeserializeObject<ExpertInfo>(signal.Data.ToString());
                        xtrade.DeInitTerminal(expert);
                    }
                    break;
                case EnumSignals.SIGNAL_SAVE_EXPERT:
                {
                    ExpertInfo expert = JsonConvert.DeserializeObject<ExpertInfo>(signal.Data.ToString());
                    if (expert != null)
                        xtrade.SaveExpert(expert);
                }
                    break;
                case EnumSignals.SIGNAL_POST_LOG:
                {
                    if (signal.Data == null)
                        break;
                    Dictionary<string, string> paramsList = JsonConvert.DeserializeObject<Dictionary<string, string>>(signal.Data.ToString());
                    StringBuilder message = new StringBuilder();
                    if (paramsList.ContainsKey("Account"))
                        message.Append("<" + paramsList["Account"] + ">:");
                    if (paramsList.ContainsKey("Magic"))
                        message.Append("_" + paramsList["Magic"] + "_:");
                    if (paramsList.ContainsKey("order"))
                        message.Append("**" + paramsList["order"] + "**");
                    if (paramsList.ContainsKey("message"))
                        message.Append(paramsList["message"]);
                    log.Log(message.ToString());
                    // log.Info(message);
                }
                break;
            }
        }

        public List<string> ProcessStringData(Dictionary<string, string> paramsList, List<string> inputData)
        {
            var list = new List<string>();
            if (!paramsList.ContainsKey("func"))
            {
                log.Error("ProcessStringData: Params error");
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
                        DateTime.TryParseExact(paramsList["time"], xtradeConstants.MTDATETIMEFORMAT,
                            CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out date);

                        string symbolStr = paramsList["symbol"];
                        // int BeforeMin = Int32.Parse(paramsList["before"]);
                        byte minImportance = byte.Parse(paramsList["importance"]);
                        NewsEventInfo info = null;
                        if (xtrade.GetNextNewsEvent(date, symbolStr, minImportance, ref info))
                        {
                            //log.Info( info.RaiseDateTime.ToString(MainService.MTDATETIMEFORMAT) + " Got news: (" + info.Name + ") Importance:  " + info.Importance.ToString());
                            list.Add(info.Currency);
                            list.Add(info.Importance.ToString());
                            list.Add(info.RaiseDateTime);
                            list.Add(info.Name);
                        }
                    }
                        break;
                    case "GetTodayNews":
                    {
                        DateTime date;
                        DateTime.TryParseExact(paramsList["time"], xtradeConstants.MTDATETIMEFORMAT,
                            CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out date);
                        string symbolStr = paramsList["symbol"];
                        byte minImportance = byte.Parse(paramsList["importance"]);
                        int i = 0;
                        var news = xtrade.GetTodayNews(date, symbolStr, minImportance);
                        foreach (var info in news)
                        {
                            list.Add(info.Currency);
                            list.Add(info.Importance.ToString());
                            list.Add(info.RaiseDateTime);

                            if (i == news.Count - 1)
                                list.Add(info.Name);
                            else
                                list.Add(info.Name + "~"); // Delimiter 
                            i++;
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
    }
}