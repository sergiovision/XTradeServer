//+------------------------------------------------------------------+
//| BillWilliams_ATZ.mq4 |
//| Copyright © Pointzero-indicator.com
//+------------------------------------------------------------------+
#property copyright "Copyright © Pointzero-indicator.com"
#property link      "http://www.pointzero-indicator.com"
#property indicator_chart_window
#property indicator_buffers 2
#property indicator_color1 Blue
#property indicator_color2 Red
#define OP_NOTHING 6

#define MACD_a         5
#define MACD_b         34
#define MACD_c         5

//-------------------------------
// Input parameters: none
//-------------------------------

//-------------------------------
// Buffers
//-------------------------------
double ExtMapBuffer1[];
double ExtMapBuffer2[];

//-------------------------------
// Internal variables
//-------------------------------
int    nShift;   

//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int init()
{
   // Buffers and style
    SetIndexStyle(0, DRAW_ARROW, 0, 1);
    SetIndexArrow(0, 233);
    SetIndexBuffer(0, ExtMapBuffer1);
    SetIndexStyle(1, DRAW_ARROW, 0, 1);
    SetIndexArrow(1, 234);
    SetIndexBuffer(1, ExtMapBuffer2);

    // Data window
    IndicatorShortName("Bill Williams ATZ");
    SetIndexLabel(0, "Buy arrow");
    SetIndexLabel(1, "Sell arrow"); 
    
    Comment("Copyright © http://www.pointzero-indicator.com");
    
    // Chart offset calculation
    switch(Period())
    {
        case     1: nShift = 1;   break;    
        case     5: nShift = 3;   break; 
        case    15: nShift = 5;   break; 
        case    30: nShift = 10;  break; 
        case    60: nShift = 15;  break; 
        case   240: nShift = 20;  break; 
        case  1440: nShift = 80;  break; 
        case 10080: nShift = 100; break; 
        case 43200: nShift = 200; break;               
    }

    return(0);
}
//+------------------------------------------------------------------+
//| Custor indicator deinitialization function                       |
//+------------------------------------------------------------------+
int deinit()
  {
//----
    return(0);
  }
//+------------------------------------------------------------------+
//| Custom indicator iteration function                              |
//+------------------------------------------------------------------+
int start()
  {
    int limit;
    int counted_bars = IndicatorCounted();

    // check for possible errors
    if(counted_bars < 0) 
        return(-1);

   //last counted bar will be recounted
    if(counted_bars > 0) 
        counted_bars--;
    limit = Bars - counted_bars;

    // Check the signal foreach bar
    for(int i = 0; i < limit; i++)
    {   
        // Indicate the trend
        int ma_trend = OP_NOTHING;
        int a_trend = OP_NOTHING;
        
        // Bill williams zonetrade indicators to point detect early strenght
        int tz = tradezone(i);
        int tz1 = tradezone(i+1);
        
        // Open and close of the current candle to filter tz signals
        double CLOSE = iClose(Symbol(),0, i);
        double OPEN = iOpen(Symbol(),0, i);
        double HIGH = iHigh(Symbol(),0, i);
        double LOW = iLow(Symbol(),0, i);
        
        // Macd present and past
        double MACD_main         = iMACD(NULL,0, MACD_a, MACD_b, MACD_c, PRICE_CLOSE, MODE_MAIN, i);
        double MACD_signal       = iMACD(NULL,0, MACD_a, MACD_b, MACD_c, PRICE_CLOSE, MODE_SIGNAL, i);
        double MACD_main_last    = iMACD(NULL,0, MACD_a, MACD_b, MACD_c, PRICE_CLOSE, MODE_MAIN, i+1);
        double MACD_signal_last  = iMACD(NULL,0, MACD_a, MACD_b, MACD_c, PRICE_CLOSE, MODE_SIGNAL, i+1);
        
        // Alligator
        double a_jaw = iAlligator(Symbol(), 0, 13, 8, 8, 5, 5, 3, MODE_SMMA, PRICE_MEDIAN, MODE_GATORJAW, i);
        double a_teeth = iAlligator(Symbol(), 0, 13, 8, 8, 5, 5, 3, MODE_SMMA, PRICE_MEDIAN, MODE_GATORTEETH, i);
        double a_lips = iAlligator(Symbol(), 0, 13, 8, 8, 5, 5, 3, MODE_SMMA, PRICE_MEDIAN, MODE_GATORLIPS, i);
        
        // Calculate alligator trend
        if(a_lips > a_teeth && a_teeth > a_jaw)
            a_trend = OP_BUY;
        else if(a_lips < a_teeth && a_teeth < a_jaw)
            a_trend = OP_SELL;
            
        // Evaluate if going long or short is dangerous now
        bool long_dangerous = false;
        bool short_dangerous = false;
        if(CLOSE > a_lips && MACD_main < MACD_signal) long_dangerous = true;
        if(CLOSE < a_lips && MACD_main > MACD_signal) short_dangerous = true;
         
        // Long signal 
        if((tz == OP_BUY && tz1 != OP_BUY && a_trend == OP_BUY && CLOSE > OPEN && long_dangerous == false))
        {
            // Display only if signal is not repated
             ExtMapBuffer1[i] = Low[i] - nShift*Point;
             
             // Throw Message
             //Print("[BILL WILLIAMS ATZ] Buy stop at "+ HIGH);
        }
        
        // Short signal
        if(tz == OP_SELL && tz1 != OP_SELL && a_trend == OP_SELL && CLOSE < OPEN && short_dangerous == false)
        {
            // Display only if signal is not repeated
            ExtMapBuffer2[i] = High[i] + nShift*Point;
            
            // Throw Message
            //Print("[BILL WILLIAMS ATZ] Sell stop at "+ LOW);
        }
    }
    return(0);
}

/**
* Returns bill williams trade zone for the candle received has parameter.
* @param    int   shift
* @return   int
*/

int tradezone(int shift = 1)
{
   // AC and AO for current and last candle
   double AC = iAC(Symbol(), 0, shift);
   double AC_last = iAC(Symbol(), 0, shift+1);
   double AO = iAO(Symbol(), 0, shift);
   double AO_last = iAO(Symbol(), 0, shift+1);
   
   // Returns action for this candle
   if(AO < AO_last && AC < AC_last) return(OP_SELL);
   if(AO > AO_last && AC > AC_last) return(OP_BUY);
   return(OP_NOTHING);
}

//+------------------------------------------------------------------+

