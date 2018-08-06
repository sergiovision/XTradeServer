using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class WalletBalance 
    {
        public int WALLET_ID
        {
            get;
            set;
        }
        public string NAME
        {
            get;
            set;
        }
        public decimal BALANCE
        {
            get;
            set;
        }

        public string formula
        {
            get;
            set;
        }

        public DateTime DATE
        {
            get;
            set;
        }
    }
}
