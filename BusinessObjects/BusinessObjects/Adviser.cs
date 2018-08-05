using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class Adviser
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Broker { get; set; }
        public string FullPath { get; set; }
        public string CodeBase { get; set; }
        public int TERMINAL_ID { get; set; }
        public int SYMBOL_ID { get; set; }
        public string Symbol { get; set; }
        public string Timeframe { get; set; }
        public bool Disabled { get; set; }
        public bool Running { get; set; }
        //public string STATE { get; set; }
        public DateTime LastUpdate { get; set; }
        public int CloseReason { get; set; }

    }
}