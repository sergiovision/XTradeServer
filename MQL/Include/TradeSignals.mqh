//+------------------------------------------------------------------+
//|                                                 TradeSignals.mqh |
//|                        Copyright 2018, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <ThriftClient.mqh>

enum ENUM_INDICATORS  
{
    EMAWMAIndicator,
    BillWilliamsIndicator,
    ZigZagIndicator,
    BandsIndicator
};

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class TradeSignals
{
protected:
   ThriftClient* thrift;
   int prevIndiSignal;
   ushort MinImportance;
   int currentImportance;
   int NewsPeriodMinutes;
   int RaiseSignalBeforeEventMinutes;
   datetime timeNewsPeriodStarted;
   bool RevertNewsTrend;
   ENUM_TIMEFRAMES IndicatorTimeFrame;

public:
   bool EventRaiseSoon;
   bool InNewsPeriod;

      TradeSignals(ThriftClient* th, ushort minimp, int newsperiod, int raisebefore, bool revertnews, int timeframe) {
         thrift = th;
         prevIndiSignal = 0;
         MinImportance = minimp;
         currentImportance = minimp;
         EventRaiseSoon = false;
         InNewsPeriod = false;
         NewsPeriodMinutes = newsperiod;
         timeNewsPeriodStarted = TimeCurrent();
         RaiseSignalBeforeEventMinutes = raisebefore;
         RevertNewsTrend = revertnews;
         IndicatorTimeFrame = (ENUM_TIMEFRAMES) timeframe;
      }
      
      //~TradeSignals();
     
      //--------------------------------------------------------------------
      int GetNewsSignal(int indiSignal, string& NewsString, string& NewsStatString, string& TrendString)
      {   
         if (indiSignal != 0)
            prevIndiSignal = indiSignal;
         string message = "no event";
         datetime raiseDateTime;
         string raisedStr = "Upcoming in ";
         int signal = thrift.GetNextNewsEvent(Symbol(), MinImportance, message, raiseDateTime);
         if (signal > 0) {
            EventRaiseSoon = true;
            currentImportance = signal;
         }
         int minsRemained = MathRound((double)(raiseDateTime - TimeCurrent())/60);
                  
         if (InNewsPeriod) {
            int minsNewsPeriod = (TimeCurrent() - timeNewsPeriodStarted)/60;
            if (minsNewsPeriod >=NewsPeriodMinutes)
               InNewsPeriod = false;
         }
                  
         //if (!IsTesting() || IsVisualMode())
         //{
         string trendString = "NEUTRAL";
         if (prevIndiSignal < 0)
            trendString = "SELL";
         else if (prevIndiSignal > 0)
                 trendString = "BUY";
         TrendString = StringFormat("%s On %s", trendString, EnumToString(IndicatorTimeFrame));
         
         if (minsRemained < 0)
            raisedStr = " Passed " + IntegerToString(-1*minsRemained) + " min ago: ";
         else
            raisedStr += IntegerToString(minsRemained) + " min: ";
           
         NewsStatString = StringFormat("News Period(%s)", (string)InNewsPeriod);
         NewsString = raisedStr + message;
         //} 
         
         if (EventRaiseSoon && (minsRemained >= 0) && (minsRemained <= RaiseSignalBeforeEventMinutes))
         {
            //CreateTextLabel(message, currentImportance, raiseDateTime);
            EventRaiseSoon = false;
            int coef = 1;
            if (RevertNewsTrend)
               coef = -1;
            InNewsPeriod = true;
            timeNewsPeriodStarted = TimeCurrent();
            return currentImportance * prevIndiSignal * coef;
         }
         return 0;
      }
      
      //+------------------------------------------------------------------+
      int GetBWTrend()
      {
         int signal = 0;
         double isBuy = iCustom(NULL,IndicatorTimeFrame,"BillWilliams_ATZ",   0,0);
         if (isBuy!=0)
            return ++signal;
         double isSell = iCustom(NULL,IndicatorTimeFrame,"BillWilliams_ATZ",   1,0);
         if (isSell!=0)
            return --signal;
         return (signal);   
      }
      //+------------------------------------------------------------------+
      int GetZigZagTrend()
      {
         int n, i;
         double zag = 0, zig = 0;
         i = 0;
         while(n < 2)
         {
            if(zig>0)
             zag=zig;
            zig = iCustom(NULL, IndicatorTimeFrame, "ZigZag", 0, i);
            if(zig>0) n+=1;
            i++;
         }
         if (zag>zig)
           return -1;

         if(zig>zag)
           return 1;
         return 0;   
      }
      //+------------------------------------------------------------------+

      //+------------------------------------------------------------------+
      int GetEMAWMATrend()
      {
         int     signal = 0;
         int     period_EMA           = 28;
         int     period_WMA           = 8;
         int     period_RSI           = 14;
                     
         double EMA0 = iMA(NULL,IndicatorTimeFrame,period_EMA,0,MODE_EMA, PRICE_OPEN,0);
         double WMA0 = iMA(NULL,IndicatorTimeFrame,period_WMA,0,MODE_LWMA,PRICE_OPEN,0);
         double EMA1 = iMA(NULL,IndicatorTimeFrame,period_EMA,0,MODE_EMA, PRICE_OPEN,1);
         double WMA1 = iMA(NULL,IndicatorTimeFrame,period_WMA,0,MODE_LWMA,PRICE_OPEN,1);
         double RSI  = iRSI(NULL,IndicatorTimeFrame,period_RSI,PRICE_OPEN,0);
         //double MFI  = iMFI(NULL,PERIOD_H1,period_RSI,0);
         
         if (EMA0 < WMA0 && EMA1 > WMA1 && RSI >= 50)
            return ++signal;
            
         if (EMA0 > WMA0 && EMA1 < WMA1 && RSI <= 50)
            return --signal;
       
         return (signal);   
      }
      
      //+------------------------------------------------------------------+
      int GetBandsTrend()
      {
         int signal = 0;
      
         double isBuy = iBands(NULL, IndicatorTimeFrame, 20, 2, 0, PRICE_LOW, MODE_LOWER, 0); 
         if (isBuy > Ask)
            return ++signal;
            
         double isSell = iBands(NULL, IndicatorTimeFrame, 20, 2, 0, PRICE_HIGH, MODE_UPPER, 0); 
         if (isSell < Bid)
            return --signal;
         return (signal);   
      }
      
      int TrendIndicator(ENUM_INDICATORS indi) 
      {
          switch(indi)
          {
             case EMAWMAIndicator:
                return GetEMAWMATrend();
             case BandsIndicator:
                return GetBandsTrend();
             case BillWilliamsIndicator:
                return GetBWTrend();
             case ZigZagIndicator:
                return GetZigZagTrend();
             default:
                return 0;
          }
         return 0;
      }

};


