using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class PositionInfo
    {
        public string AccountName { get; set; }
        public long Account { get; set; }
        public int Type { get; set; }
        public long Magic { get; set; }
        public long Ticket { get; set; }        
        public double Lots { get; set; }
        public string Symbol { get; set; }
        public string MetaSymbol { get; set; }
        public decimal ProfitStopsPercent { get; set; }    
        public decimal ProfitBricks { get; set; }
        public decimal Profit { get; set; }

        public void Update()
        {
            decimal change = GenerateChange();
            decimal newProfit = change;
            Profit = newProfit;
        }

        static Random random = new Random();

        decimal GenerateChange()
        {
            return (decimal)random.Next(-200, 200) / 10000;
        }
    }

}
