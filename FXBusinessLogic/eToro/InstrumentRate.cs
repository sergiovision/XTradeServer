namespace FXBusinessLogic.eToro
{
    public class InstrumentRate : IJSONObject
    {
        public decimal? SentimentPercent { get; set; } // :59.00,
        public string SentimentType { get; set; } //:"Buying",

        public Rates Rates { get; set; }

        //":{"Ask":91.96,"Bid":91.89,"Precision":2,"InstrumentID":14,"PeriodChangePrecent":0.4042832167832167832167832200,"PeriodChangeValue":0.37},
        public int InstrumentID { get; set; } //":14,
        public decimal? PeriodChangePrecent { get; set; } //:0.4042832167832167832167832200,
        public decimal? PeriodChangeValue { get; set; } //":0.37
    }
}