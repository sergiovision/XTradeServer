using BusinessObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUIK
{
    public class QUIKExpert : Adviser, IExpert
    {
        public string Comment;
        public string PortfolioName;
        public short Volume;

        public QUIKExpert(Adviser adv)
        {
            Id = adv.Id;
            Name = adv.Name;
            Running = adv.Running;
            CloseReason = adv.CloseReason;
            TerminalId = adv.TerminalId;
            CodeBase = adv.CodeBase;
            Broker = adv.Broker;
            FullPath = adv.FullPath;
            AccountNumber = adv.AccountNumber;
            LastUpdate = adv.LastUpdate;
            Disabled = adv.Disabled;
            Symbol = adv.Symbol;
            SymbolId = adv.SymbolId;
            MetaSymbol = adv.MetaSymbol;
            Timeframe = adv.Timeframe;
            State = adv.State;

            Volume = 1;
            PortfolioName = "SPBFUTK3JVN";
            Comment = "QUIK";

            //ExpertsRepository.toDTO(dbAdviser, ref refThis);
            Dictionary<string, string> parameters =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(State);

            if (parameters != null)
            {
                if (parameters.ContainsKey("Volume")) Volume = short.Parse(parameters["Volume"]);

                if (parameters.ContainsKey("PortfolioName")) PortfolioName = parameters["PortfolioName"];

                if (parameters.ContainsKey("Comment")) Comment = parameters["Comment"];
            }
        }

        public string AccountName()
        {
            return PortfolioName;
        }

        public long Magic()
        {
            return Id;
        }

        string IExpert.Comment()
        {
            return Comment;
        }

        string IExpert.Symbol()
        {
            return Symbol;
        }

        double IExpert.Volume()
        {
            return Volume;
        }

        public string Serialize()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["Volume"] = Volume.ToString();
            parameters["Comment"] = Comment;
            parameters["PortfolioName"] = PortfolioName;
            return JsonConvert.SerializeObject(parameters);
        }
    }
}