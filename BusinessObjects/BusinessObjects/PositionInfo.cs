using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class PositionInfo
    {
        private static readonly Random random = new Random();
        public string AccountName { get; set; }
        public long Account { get; set; }
        public int Type { get; set; }
        public long Magic { get; set; }
        public long Ticket { get; set; }
        public double Lots { get; set; }
        public string Symbol { get; set; }
        public string MetaSymbol { get; set; }
        public double ProfitStopsPercent { get; set; }
        public double ProfitBricks { get; set; }
        public double Profit { get; set; }
        public string Role { get; set; }

        public void Update()
        {
            double change = GenerateChange();
            double newProfit = change;
            Profit = newProfit;
        }

        private double GenerateChange()
        {
            return (double) random.Next(-200, 200) / 10000;
        }
    }
}