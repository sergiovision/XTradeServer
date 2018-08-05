using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class Account
    {
        public int ID { get; set; }
        public long AccountNumber { get; set; }

        public string Broker { get; set; }

        public string FullPath { get; set; }
        public string CodeBase { get; set; }
        public bool Disabled { get; set; }
    }
}