//+------------------------------------------------------------------+
//|                                                 TradeIndicators.mqh |
//|                        Copyright 2018, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict


#include <XTrade\InputTypes.mqh>
#include <XTrade\ITradeService.mqh>
#include <XTrade\TradeMethods.mqh>
#include <XTrade\PanelBase.mqh>
#include <XTrade\Signals.mqh>
#include <Indicators\Trend.mqh>
#include <XTrade\CMedianRenko.mqh>

class CIchimoku;
class CNewsIndicator;
class COsMA;
class CHeikenAshi;
class CNATR;
class CMedianRenko;
class CIchimokuRenko;
class CTimeLine;
class SSAFastTrend;
class CLevels;

#include <XTrade\CIchimoku.mqh>
#include <XTrade\CNewsIndicator.mqh>
#include <XTrade\COsMA.mqh>
#include <XTrade\CNATR.mqh>
#include <XTrade\CBBands.mqh>
#include <XTrade\CCandle.mqh>
#include <XTrade\CIchimokuRenko.mqh>
#include <XTrade\CTimeLine.mqh>
#include <XTrade\CMACD.mqh>
#include <XTrade\SSAFastTrend.mqh>
#include <XTrade\CLevels.mqh>

class TradeMethods;

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class TradeIndicators 
{
public:
   ITradeService* thrift;
   string Symbol;
   TradeMethods* methods;
   bool InNewsPeriod;
   int NewsMinsRemained;
   TYPE_TREND Trend;
   CNATR*  ATRD1;
   CBBands* Bands;
   CNewsIndicator* News;
   CMedianRenko*  medianRenko;
   CTimeLine*     timeLine;
   CMACD*         macd;
   CLevels*       levels;
   SSAFastTrend* ssaTrend;
   IndiBase*   FI;
   IndiBase*   SI;
   double deltaCross;
   string StatusString;
   datetime timeNewsPeriodStarted;
   bool AllowAutoTrading;

   TradeIndicators(TradeMethods* me, PanelBase* p)
   { 
      AllowAutoTrading = false;
      ssaTrend = NULL;
      medianRenko = NULL;
      timeLine = NULL;
      macd = NULL;
      levels = NULL;
      methods = me;
      Trend = LATERAL;
      thrift = Utils.Service();
      // currentImportance = MinImportance;
      InNewsPeriod = false;
      timeNewsPeriodStarted = TimeCurrent();
      //LastSignal.Handled = true; // first signal is handled!
      Symbol = Symbol();
      
      if (GET(EnableRenko))
      {
         InitMedianRenko(methods.Period);
         if (!Utils.IsTesting())
            InitTimeline(methods.Period);
      } else {
         InitSSATrend(methods.Period);
      }

      InitATR();
      
      InitLevels(methods.Period);
      
      if (GET(EnableNews))
      {
         GlobalVariablesDeleteAll("NewsSignal");
         string strMagic = IntegerToString(thrift.MagicNumber());
         thrift.InitNewsVariables(strMagic);
         News = new CNewsIndicator();
         News.Init(methods.Period);
      }
      
      //InitBands(methods.Period);
      
      if (GET(FilterIndicator) == GET(SignalIndicator))
      {
         FI = InitIndicator((ENUM_INDICATORS)GET(FilterIndicator), methods.Period);
         SI = FI;
      }
      else 
      {
         FI = InitIndicator((ENUM_INDICATORS)GET(FilterIndicator), methods.Period);
         SI = InitIndicator((ENUM_INDICATORS)GET(SignalIndicator), methods.Period);
      }  
   }
   
   IndiBase* InitIndicator(ENUM_INDICATORS indi, ENUM_TIMEFRAMES tf)
   {
      IndiBase* ind = NULL;
      switch(indi)
      {
         case IshimokuIndicator:
            ind = new CIchimoku();
            ind.Init(tf);
            return ind;
         case IchimokuRenkoIndicator:
         {  
            CIchimokuRenko* iind = new CIchimokuRenko();
            iind.Init(tf);
            iind.SetMedianRenko(medianRenko);
            ind = iind;
            return ind;
         }
         case CandleIndicator:
            ind = new CCandle();
            ind.Init(tf);
            return ind;
         case OsMAIndicator:
            //InitMFI(0, FilterTimeFrame);
            //InitBands(methods.Period);
            ind = new COsMA();
            ind.Init(tf);
            return ind;
         case DefaultIndicator:
         case NoIndicator:
            return NULL;            
         default:
           Utils.Info(StringFormat("Indicator %s not implemented!!!", EnumToString(indi)));
      }
      return NULL;
   }
   
   bool InitMedianRenko(ENUM_TIMEFRAMES tf)
   {
      if (medianRenko != NULL)
         if (medianRenko.Initialized())
             return true;
      medianRenko = new CMedianRenko();
      return medianRenko.Init(tf);
   }
   
   bool InitSSATrend(ENUM_TIMEFRAMES tf)
   {
      if (ssaTrend != NULL)
         if (ssaTrend.Initialized())
             return true;
      ssaTrend = new SSAFastTrend();
      return ssaTrend.Init(tf);
   }
      
      
   bool InitTimeline(ENUM_TIMEFRAMES tf)
   {
      if ((timeLine != NULL) || (macd != NULL))
         return false;
      timeLine = new CTimeLine();
      //macd = new CMACD();
      bool res = timeLine.Init(tf);
      //macd.Init(tf);
      return res;

   }

   bool InitBands(ENUM_TIMEFRAMES tf)
   {
      Bands = new CBBands();         
      return Bands.Init(tf);
   }      
            
   bool InitATR()
   {
      ATRD1 = new CNATR();
      bool res = ATRD1.Init(PERIOD_D1);
      if (res)
      {
          ATRD1.FullRelease(!Utils.IsTesting());
      }
      return res;
   }
   
   bool InitLevels(ENUM_TIMEFRAMES tf)
   {
      levels = new CLevels();
      bool res = levels.Init(tf);
      if (res)
      {
         levels.FullRelease(!Utils.IsTesting());
      }
      return res;
   }
   
      void RefreshIndicators()
      {    
         if (!AllowAutoTrading)
             return;
          if (GET(EnableRenko)) {
             if (medianRenko != NULL)
               medianRenko.Process();  
          } else {
             if (ssaTrend != NULL)
               ssaTrend.Process();  
          }
/*
          if (ATRCurrent.Handle() != INVALID_HANDLE)
            ATRCurrent.Refresh();

          if (ATRD1.Handle() != INVALID_HANDLE)
            ATRD1.Refresh();
             
          if (Bands.Handle() != INVALID_HANDLE)
              Bands.Refresh();
*/
          //if (OsMA.Initialized())
          //   OsMA.Refresh();
             
          //if (Ichimoku.Initialized())
          //   Ichimoku.Refresh();
      }
      
      double GetATR(int shift)
      {
         return ATRD1.GetData(0, shift);
      }
            
      ~TradeIndicators()
      {
          if (!Utils.IsTesting())
          {
             if (GET(EnableRenko)) 
             {
                if (macd != NULL)            
                {
                   if (macd.Initialized())
                      macd.Delete();
                }

                if (timeLine != NULL)            
                {
                   if (timeLine.Initialized())
                      timeLine.Delete();
                }
             } else {
                  if (ssaTrend != NULL) {
                      if (ssaTrend.Initialized())
                         ssaTrend.Delete();
                  }
             }

             if (Bands != NULL)            
             if (Bands.Initialized())
             {
                Bands.Delete();
                Bands.DeleteFromChart(methods.ChartId(), methods.SubWindow());
             }
             
             if (levels.Initialized())
             {
                levels.DeleteFromChart(methods.ChartId(), methods.SubWindow());
             }

             if (News != NULL)            
             if (News.Initialized())
             {
                 News.DeleteFromChart(methods.ChartId(), methods.IndiSubWindow());
             }

             if (FI != NULL)
             if (FI.Initialized())
             {
                 FI.DeleteFromChart(methods.ChartId(), methods.SubWindow());
             }
             
             if (SI != NULL)
             if (SI.Initialized())
             {
                 SI.DeleteFromChart(methods.ChartId(), methods.SubWindow());
             }


          }
          DELETE_PTR(FI);
          DELETE_PTR(SI);
          DELETE_PTR(levels);
          DELETE_PTR(ATRD1);
          DELETE_PTR(News);
          DELETE_PTR(Bands);
          if (GET(EnableRenko))
          {
            DELETE_PTR(macd);
            DELETE_PTR(timeLine);
            if (medianRenko != NULL) 
            {  
               medianRenko.Delete();
               DELETE_PTR(medianRenko);
            }
          } else 
          {
            DELETE_PTR(ssaTrend);
          }
      }
      
      string GetStatusString()
      {
          return StatusString;
      }
      
      TYPE_TREND GetTrend() const
      {
         return Trend;
      }
                                   
      void ProcessFilter()
      {
         if (GET(FilterIndicator) == NoIndicator)
            return;
          if (!AllowAutoTrading)
              return;
         if (FI == NULL)
             return;
         FI.Process();
      }
      
      void ProcessSignal()
      {
          if (!AllowAutoTrading)
              return;
          if (SI == NULL)
             return;
          SI.Process();  
      }
};


