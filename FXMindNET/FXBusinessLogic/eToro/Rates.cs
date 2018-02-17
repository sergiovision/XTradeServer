namespace FXBusinessLogic.eToro
{
    public class Rates : IJSONObject
    {
        public decimal? Ask { get; set; } //:91.96
        public decimal? Bid { get; set; } //:91.89
        public int Precision { get; set; } //:2
        public int InstrumentID { get; set; } //:14
        public decimal? PeriodChangePrecent { get; set; } //:0.4042832167832167832167832200
        public decimal? PeriodChangeValue { get; set; } //:0.37
    }
}