#property copyright "Extreme TMA System"
#property link      "http://www.forexfactory.com/showthread.php?t=343533m"

#property indicator_chart_window
#property indicator_buffers    6
#property indicator_plots      6 

#property indicator_color1     White
#property indicator_type1      DRAW_LINE
#property indicator_style1     STYLE_SOLID
#property indicator_label1     "Middle"

#property indicator_color2     Red
#property indicator_type2      DRAW_LINE
#property indicator_style2     STYLE_DOT
#property indicator_label2     "Upper"


#property indicator_color3     Aqua
#property indicator_type3      DRAW_LINE
#property indicator_style3     STYLE_DOT
#property indicator_label3     "Lower"

#property indicator_color4     Lime 
#property indicator_type4      DRAW_LINE
#property indicator_style4     STYLE_SOLID
#property indicator_label4     "Bull"


#property indicator_color5     clrTomato
#property indicator_type5      DRAW_LINE
#property indicator_style5     STYLE_SOLID
#property indicator_label5     "Bear"


#property indicator_color6     SlateGray
#property indicator_type6      DRAW_LINE
#property indicator_style6     STYLE_SOLID
#property indicator_label6     "Neutral"

#property  indicator_width1 1
#property  indicator_width2 3
#property  indicator_width3 3
#property  indicator_width4 4
#property  indicator_width5 4
#property  indicator_width6 4

#ifdef __MQL5__
#property indicator_applied_price PRICE_CLOSE

#endif

#include <FXMind\IUtils.mqh>

input ENUM_TIMEFRAMES TimeFrame  = PERIOD_CURRENT;
input int    TMAPeriod       = 56;
input double ATRMultiplier   = 3.0;
input int    ATRPeriod       = 100;
input double TrendThreshold  = 0.45;
input bool   ShowChannels    = true;
input bool   PercentileATR     = true;
input int    Percentile  = 10;
bool         ShowCenterLine = true;
int ThriftPORT = Constants::FXMindMQL_PORT;


double tma[];
double upperBand[];
double lowerBand[];
double bull[];
double bear[];
double neutral[];
 
bool setAsSeries = true;
ENUM_TIMEFRAMES actTimeFrame = PERIOD_CURRENT;
ENUM_APPLIED_PRICE    Price   = PRICE_CLOSE;
double BarsPerTma = 1;
int VisibleBars = 0;

int ATRHandle = INVALID_HANDLE;

int OnInit()
{
   actTimeFrame = TimeFrame;
   if (TimeFrame == 0)
      actTimeFrame = (ENUM_TIMEFRAMES)Period();
      
   string name = StringFormat("%s TMA bands (%d)", EnumToString(actTimeFrame),TMAPeriod);
   Utils = CreateUtils((short)ThriftPORT, name); 
   if (Utils == NULL)
      Print("Failed create Utils!!!");
      
   Utils.SetIndiName(name);

#ifdef __MQL4__                                
   IndicatorBuffers(6); 
#endif   
   BarsPerTma = Utils.BarsPerPeriod(actTimeFrame);
   
   int HalfLength = 0;//(int) double(TMAPeriod * BarsPerTma);
   IndicatorSetInteger(INDICATOR_DIGITS, _Digits);
      
   Utils.AddBuffer(0, tma,  true, indicator_label1, HalfLength);
   Utils.AddBuffer(1, upperBand, true, indicator_label2, HalfLength);
   Utils.AddBuffer(2, lowerBand, true, indicator_label3, HalfLength);
   Utils.AddBuffer(3, bull, true, indicator_label4, HalfLength);
   Utils.AddBuffer(4, bear, true, indicator_label5, HalfLength);
   Utils.AddBuffer(5, neutral, true, indicator_label6, HalfLength);

   if (Percentile > 100 || Percentile < 0)
   {
      Utils.Info("Percentile should be between [0,100]");
   }
#ifdef  __MQL5__   
   ATRHandle = iCustom(Symbol(), actTimeFrame, "NATR", ATRPeriod, Percentile, PercentileATR);
   if (ATRHandle == INVALID_HANDLE)
      Utils.Info("Failed Init NATR!!!");
#endif    
   Utils.Info(StringFormat("Init %s TF=%s", name, EnumToString(actTimeFrame)));
   return( INIT_SUCCEEDED);
}

void OnDeinit(const int reason) 
{
#ifdef __MQL5__
  if (ATRHandle != INVALID_HANDLE)
  {
     IndicatorRelease(ATRHandle);
  }
#endif 
  DELETE_PTR(Utils)
}

int OnCalculate(const int rates_total,
                const int prev_calculated,
                const datetime& time[],
                const double& open[],
                const double& high[],
                const double& low[],
                const double& close[],
                const long& tick_volume[],
                const long& volume[],
                const int& spread[])
{
#ifdef __MQL5__     
   ArraySetAsSeries(time, setAsSeries);
   ArraySetAsSeries(open, setAsSeries);
   ArraySetAsSeries(close, setAsSeries);
   ArraySetAsSeries(high, setAsSeries);
   ArraySetAsSeries(low, setAsSeries);
#endif   
   int i, limit = 0;
   int bars = rates_total;

   if( prev_calculated >= rates_total - 1) 
   {
      limit = 0;
   }
   //int(Utils.Bars() + TMAPeriod * BarsPerTma); 
   int calcBars =  (int)(MathMax( Utils.ChartFirstVisibleBar(), ChartGetInteger(0, CHART_VISIBLE_BARS) ));
   if (VisibleBars != calcBars)
   {
      VisibleBars = calcBars;      
      limit = VisibleBars;  
   }
   
   int arraySize = ArraySize(tma);
   if ( limit >= (arraySize - 1) )
     limit = arraySize - 1;
      
   //limit = (int)MathMin(1,Bars-counted_bars+ TMAPeriod * barsPerTma );
   int mtfShift = 0;
   int lastMtfShift = 999;
   double tmaVal = tma[limit];
   double range = 0;
   
   double slope = 0;
   double prevTma = tma[limit+1];
   double prevSlope = 0;
   
   for (i=limit; i>=0; i--)
   {
      if (actTimeFrame == Period())
      {
         mtfShift = i;
      }
      else
      {         
         mtfShift = iBarShift(Symbol(), actTimeFrame,time[i]);
      } 
      
      if(mtfShift == lastMtfShift)
      {       
         tma[i] =tma[i+1] + ((tmaVal - prevTma) * (1/BarsPerTma));         
         if (ShowChannels)
         {
            upperBand[i] =  tma[i] + range;
            lowerBand[i] = tma[i] - range;
         }
         DrawCenterLine(i, slope);   
         continue;
      }
      
      lastMtfShift = mtfShift;
      prevTma = tmaVal;
      tmaVal = Utils.CalculateTMA(actTimeFrame, TMAPeriod, mtfShift, time, close);
      
      double atrValue = 0;
#ifdef __MQL5__      
      double ATRBuf[1];
      CopyBuffer(ATRHandle, 0, mtfShift, 1, ATRBuf); 
      atrValue = ATRBuf[0];
#else
      atrValue = iCustom(Symbol(), actTimeFrame, "NATR", ATRPeriod, Percentile, PercentileATR);      
#endif       
      //if (PercentileATR)
         //atrValue = Utils.PercentileATR(Symbol(), actTimeFrame, PercentileRank, ATRPeriod, mtfShift+10);
      //   atrValue = Utils.PercentileATRIndi(actTimeFrame, low, high, PercentileRank, ATRPeriod, mtfShift);
      //else 
      //   atrValue = Utils.iATR(actTimeFrame, ATRPeriod, mtfShift);
      
      range = atrValue*ATRMultiplier;
      if(range == 0) 
      { 
         atrValue = 0.001;   
         range = atrValue*ATRMultiplier;
      }
      
      if (BarsPerTma > 1)
      {
         tma[i] = prevTma + ((tmaVal - prevTma)/BarsPerTma);
      }
      else
      {
         tma[i] = tmaVal;
      }
      if (ShowChannels)
      {
         upperBand[i] = tma[i]+range;
         lowerBand[i] = tma[i]-range;
      }
      
      if (BarsPerTma == 1)
         slope = (tma[i] - tma[i+1])/(atrValue  * 0.1);            
      else 
         slope = (tmaVal - prevTma)/(atrValue  * 0.1);    
                 
      DrawCenterLine(i, slope);          
   }
   return(rates_total);
}

void DrawCenterLine(int shift, double slope)
{

   bull[shift] = EMPTY_VALUE;
   bear[shift] = EMPTY_VALUE;          
   neutral[shift] = EMPTY_VALUE; 
   if (ShowCenterLine)
   {
      if(slope > TrendThreshold)
      {
         bull[shift] = tma[shift];
      }
      else if(slope < -1 * TrendThreshold)
      {
         bear[shift] = tma[shift];
      }
      else
      {
         neutral[shift] = tma[shift];
      }
   }
}

double CalcTma(int inx, const datetime& time[], const double& close[])
{ 
   double dblSum  = (TMAPeriod + 1) * iClose(Symbol(),actTimeFrame,inx);
   double dblSumw = (TMAPeriod + 1);
   int jnx, knx;         
   for ( jnx = 1, knx = TMAPeriod; jnx <= TMAPeriod; jnx++, knx-- )
   {
      double closeValue = 0;
      if (BarsPerTma == 1)
         closeValue = close[inx+jnx];
      else
         closeValue = iClose(Symbol(),actTimeFrame,inx+jnx);
      dblSum  += ( knx * closeValue );
      dblSumw += knx;      
      
      if ( jnx <= inx )
      {         
         datetime timeValue = 0;
         if (BarsPerTma == 1)
            timeValue = time[inx-jnx];
         else
            timeValue = iTime(Symbol(),actTimeFrame,inx-jnx);

         if (timeValue > time[0])
         {
            //Print (" TimeFrameValue ", TimeFrameValue , " inx ", inx," jnx ", jnx, " iTime(Symbol(),TimeFrameValue,inx-jnx) ", TimeToStr(iTime(Symbol(),TimeFrameValue,inx-jnx)), " Time[0] ", TimeToStr(Time[0])); 
            continue;
         }
         double closeValue2 = 0;
         if (BarsPerTma == 1)
            closeValue2 = close[inx-jnx];
         else
            closeValue2 = iClose(Symbol(),actTimeFrame,inx-jnx);

         dblSum  += ( knx * closeValue2 );
         dblSumw += knx;
      }
   }   
   return( dblSum / dblSumw );
}
 
/*
void manageAlerts()
{
   if (alertsOn)
   { 
      int trend;        
      if (Close[0] > upperBand[0]) trend =  1;
      else if (Close[0] < lowerBand[0]) trend = -1;
      else {AlertHappened = false;}
            
      if (!AlertHappened && AlertTime != Time[0])
      {       
         if (trend == 1) doAlert("up");
         if (trend ==-1) doAlert("down");
      }         
   }
}

void doAlert(string doWhat)
{ 
   if (AlertHappened) return;
   AlertHappened = true;
   AlertTime = Time[0];
   string message;
     
   message =  StringConcatenate(Symbol()," at ",TimeToStr(TimeLocal(),TIME_SECONDS)," "+TimeFrameValueToString(TimeFrameValue)+" TMA bands price penetrated ",doWhat," band");
   if (alertsMessage) Alert(message);
   if (alertsEmail)   SendMail(StringConcatenate(Symbol(),"TMA bands "),message);
   if (alertsSound)   PlaySound("alert2.wav");

}

*/

