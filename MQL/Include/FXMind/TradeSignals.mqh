//+------------------------------------------------------------------+
//|                                                 TradeSignals.mqh |
//|                        Copyright 2018, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\InputTypes.mqh>
#include <FXMind\IFXMindService.mqh>
#include <FXMind\TradeMethods.mqh>
#include <FXMind\PanelBase.mqh>
#include <FXMind\Signals.mqh>

#include <Indicators\Trend.mqh>
//#include <Indicators\Oscilators.mqh>
//#include <Indicators\Volumes.mqh>

class CIchimoku;
class CNewsIndicator;
class COsMA;
class CHeikenAshi;
class CTMA;
class CNATR;

#include <FXMind\CIchimoku.mqh>
#include <FXMind\CNewsIndicator.mqh>
#include <FXMind\COsMA.mqh>
//#include <FXMind\CHeikenAshi.mqh>
//#include <FXMind\CTMA.mqh>
#include <FXMind\CNATR.mqh>
#include <FXMind\CBBands.mqh>
#include <FXMind\CCandle.mqh>

class TradeMethods;

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class TradeSignals 
{
public:
   IFXMindService* thrift;
   string Symbol;
   long        chartID;
   int         subWindow;
   int         indiSubWindow; 
   TradeMethods* methods;
   Signal LastSignal;
   Signal PrevSignal;
   Signal FilterSignal;
   bool InNewsPeriod;
   int NewsMinsRemained;
   TYPE_TREND Trend;
   //COsMA*  OsMA;
   //CiMFI    MFI;
   CNATR*  ATRD1;
   //CNATR  ATRCurrent;
   CBBands* Bands;
   //CIchimoku Ichimoku;
   CNewsIndicator* News;
   //CHeikenAshi    HA;
   //CTMA           TMAM15;
   //CTMA*          trendTMA;
   //CTMA           TMAH4;
   IndiBase*   FI;  
   IndiBase*   SI;
   //CFractalZigZag FRZ;
   double trailDelta;
   double deltaCross;
   string StatusString;
   datetime timeNewsPeriodStarted;
   
   TradeSignals(TradeMethods* me, PanelBase* p)
     //:Ichimoku(GetPointer(this))
     //News(GetPointer(this))
     //,OsMA(GetPointer(this))
     //,HA(GetPointer(this))
     //,TMAM15(GetPointer(this))
     //,ATRCurrent(GetPointer(this))
     //,TMAH4(GetPointer(this))
     //,FRZ(GetPointer(this))
   { 
      methods = me;
      Trend = LATERAL;
      thrift = Utils.Service();
      //currentImportance = MinImportance;
      InNewsPeriod = false;
      timeNewsPeriodStarted = TimeCurrent();
      LastSignal.Handled = true; // first signal is handled!
      Symbol = Symbol();
      chartID = (p==NULL)?ChartID():p.chartID;
      subWindow = (p==NULL)?0:p.subWin;
      indiSubWindow = subWindow + 1;//(int)ChartGetInteger(chartID, CHART_WINDOWS_TOTAL);
      //CleanGlobalVars(); 

      InitATR();
      // HA.Init(FilterTimeFrame);

      //if (Period() != SignalTimeFrame)
      //   Utils.Info(StringFormat("Chart Period should be equal to Signal TimeFrame!!! ChartTF=%s SignalTF=%s", EnumToString(Period()), EnumToString(SignalTimeFrame)));
            
      if (EnableNews)
      {
         GlobalVariablesDeleteAll("NewsSignal");
         string strMagic = IntegerToString(thrift.MagicNumber());
         thrift.InitNewsVariables(strMagic);
         News = new CNewsIndicator(GetPointer(this));
         News.Init(methods.Period);
      }
      
      //if (TPByPercentile)
      //   InitZigZag();
      InitBands(methods.Period);

      if (FilterIndicator == SignalIndicator)
      { 
         FI = InitIndicator(FilterIndicator, methods.Period);
         SI = FI;
      }
      else 
      {
         FI = InitIndicator(FilterIndicator, methods.Period);
         SI = InitIndicator(SignalIndicator, methods.Period);
      }  
   }
   
      IndiBase* InitIndicator(ENUM_INDICATORS indi, ENUM_TIMEFRAMES tf)
      {
         IndiBase* ind = NULL;
         switch(indi)
         {
            //case BWZoneIndicator:
            //   return InitBWZone(indi, tf);
            case IshimokuIndicator:
               //InitMFI(0, FilterTimeFrame);
               //InitBands(methods.Period);
               ind = new CIchimoku(GetPointer(this));
               ind.Init(tf);
               return ind;
            case CandleIndicator:
               ind = new CCandle(GetPointer(this));
               ind.Init(tf);
               return ind;
            //case FractalZigZagIndicator:
               //return InitFractalZigZag(tf);
            case OsMAIndicator:
               //InitMFI(0, FilterTimeFrame);
               InitBands(methods.Period);
               ind = new COsMA(GetPointer(this));
               //Ichimoku.Init(methods.Period);
               ind.Init(tf);
               return ind;
            /*case TMAIndicator:
               TMAM15.Init(methods.Period);
               if (TMAH4.Init(PERIOD_H4))
               {
                  trendTMA = &TMAH4;
                  return true;
               }
               return false; */
               //return TMAH1.Init(PERIOD_H1);
            case NoIndicator:
               return NULL;            
            default:
              Utils.Info(StringFormat("Indicator %s not implemented!!!", EnumToString(indi)));
         }
         return NULL;
      }
      
      bool InitBands(ENUM_TIMEFRAMES tf)
      {
         Bands = new CBBands(GetPointer(this));         
         return Bands.Init(tf);
      }      
      
      /*bool InitBWZone(ENUM_INDICATORS indi, ENUM_TIMEFRAMES timeframe)
      {
         if (indi != BWZoneIndicator)
            return false;
         BWZoneHandle = Utils.iCustomHandle(timeframe, "BWZoneTrade", 0, 0, 0);
         if (BWZoneHandle != INVALID_HANDLE)
         {
              Utils.AddToChart(BWZoneHandle, "BWZoneTrade", chartID, subWindow);
              return true;
         }
         Utils.Info("Indicator BWZone - failed to load!!!!!!!!!!!!!");
         return false;
      }*/
      
/*      bool InitCandle(ENUM_INDICATORS indi, ENUM_TIMEFRAMES timeframe)
      {
         CandleHandle = Utils.iCustomHandle(timeframe, "CandlePatterns", NumBarsToAnalyze, (int)thrift.MagicNumber(), -1);
         if (CandleHandle != INVALID_HANDLE)
         {
            Utils.AddToChart(CandleHandle, "CandlePatterns", chartID, subWindow);
            return true;
         }
         Utils.Info("Indicator CandlePatterns - failed to load!!!!!!!!!!!!!");
         return false;
      }*/
      
      /*bool InitFractalZigZag(ENUM_TIMEFRAMES timeframe)
      {
         bool res = FRZ.Init(methods.Period);
         if (res)
         {
            Utils.AddToChart(FRZ.Handle(), FRZ.Name(), chartID, subWindow);
            return true;
         }
         Utils.Info("Indicator FractalZigZag - failed to load!!!!!!!!!!!!!");
         return false;
      } */
      
      bool InitATR()
      {
/*         bool res = ATRCurrent.Init(methods.Period);
         if (res)
         {
#ifdef __MQL5__         
             ATRCurrent.FullRelease(!Utils.IsTesting());
#endif             
         } */
         ATRD1 = new CNATR(GetPointer(this));
         bool res = ATRD1.Init(PERIOD_D1);
         if (res)
         {
#ifdef __MQL5__         
             ATRD1.FullRelease(!Utils.IsTesting());
#endif             
         }
         return res;
      }
      
/*      bool InitRSI(ENUM_INDICATORS indi, ENUM_TIMEFRAMES timeframe)
      {
         bool res = RSI.Create(Symbol, timeframe, NumBarsToAnalyze, PRICE_CLOSE);
         if (res)
         {
#ifdef __MQL5__         
         
             RSI.FullRelease(!Utils.IsTesting());
#endif
         }
         return res;
      }
      */
      
      /*
      bool InitMFI(ENUM_INDICATORS indi, ENUM_TIMEFRAMES timeframe)
      {
#ifdef __MQL5__               
         ENUM_APPLIED_VOLUME volume = VOLUME_TICK;
         if(SymbolInfoInteger(Symbol(),SYMBOL_TRADE_CALC_MODE)!=(int)SYMBOL_CALC_MODE_FOREX)
           volume = VOLUME_REAL;

         bool res = MFI.Create((string)Symbol, (ENUM_TIMEFRAMES) timeframe, (int)NumBarsToAnalyze, volume);
         if (res)
         {
             MFI.AddToChart(chartID, indiSubWindow);

             MFI.FullRelease(!Utils.IsTesting());
         }
#else 
         bool res = MFI.Create((string)Symbol, (ENUM_TIMEFRAMES) timeframe, (int)NumBarsToAnalyze);         
         Utils.AddToChart(0, "MFI", chartID, subWindow);
#endif
         return res;
      }*/
                  
      void RefreshIndicators()
      {      
/*#ifdef __MQL5__      
          if (RSI.Handle() != INVALID_HANDLE)
#endif          
             RSI.Refresh();
             */
//#ifdef __MQL5__      
//          if (MFI.Handle() != INVALID_HANDLE)
//#endif          
//             MFI.Refresh();
/*
#ifdef __MQL5__      
          if (ATRCurrent.Handle() != INVALID_HANDLE)
#endif          
            ATRCurrent.Refresh();

#ifdef __MQL5__      
          if (ATRD1.Handle() != INVALID_HANDLE)
#endif          
            ATRD1.Refresh();
             
#ifdef __MQL5__      
          if (Bands.Handle() != INVALID_HANDLE)
#endif          
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
            
      void SignalHandled()
      {
         LastSignal.Handled = true;
         PrevSignal = LastSignal;
         LastSignal.Init(false);
      }
      
      bool IsSignalChanged() const
      {
         return !(LastSignal == PrevSignal); 
      }
      
      ~TradeSignals()
      {
#ifdef __MQL5__             
          if (!Utils.IsTesting())
          {
             //if (ATR.Handle() != INVALID_HANDLE)
             //{
             //    ATR.DeleteFromChart(chartID, indiSubWindow);
             //}

             //if (MFI.Handle() != INVALID_HANDLE)
             //{
             //    MFI.DeleteFromChart(chartID, indiSubWindow);
             //}

             if (Bands != NULL)            
             if (Bands.Initialized())
             {
                Bands.Delete();
                Bands.DeleteFromChart(chartID, subWindow);
             }
                          
             if (News != NULL)            
             if (News.Initialized())
             {
                 News.DeleteFromChart(chartID, indiSubWindow);
             }

             if (FI != NULL)
             if (FI.Initialized())
             {
                 FI.DeleteFromChart(chartID, subWindow);
             }
             
             if (SI != NULL)
             if (SI.Initialized())
             {
                 SI.DeleteFromChart(chartID, subWindow);
             }
          }
#endif     
          DELETE_PTR(FI);
          DELETE_PTR(SI);
          DELETE_PTR(ATRD1);
          DELETE_PTR(News);
          DELETE_PTR(Bands);
      }
      
      string GetLastSignalName(string comment)
      {
         if (StringLen(comment)>0)
            return comment;
         if (FilterIndicator == CandleIndicator)
            return FilterSignal.GetName();
         if (StringLen(LastSignal.GetName()) == 0)
            return "Manual";  
         return LastSignal.GetName();
      }
           
      string GetStatusString()
      {
          return StatusString;
      }
      
      TYPE_TREND GetTrend() const
      {
         return Trend;
      }
            
      //+------------------------------------------------------------------+
      /*int ProcessZigZag(Signal& signal)
      {
         int n = 0, i;
         double zag = 0, zig = 0;
         i = 0;
         while(n < 2)
         {
            if(zig>0)
             zag=zig;
            zig = Utils.iCustom(methods.Period, "ZigZag", 0, i);
            if(zig>0) n+=1;
            i++;
         }
         if (zag>zig)
         {
           signal.Value--;
           return true;
         }

         if(zig>zag)
         {
           signal.Value++;
           return true;
         }
         return false;   
      }

      //+------------------------------------------------------------------+
      int ProcessEMAWMA(ENUM_TIMEFRAMES timeframe, Signal &signal)
      {
         int     period_EMA           = 28;
         int     period_WMA           = 8;
         int     period_RSI           = 14;
         double EMA0 = Utils.iMA(timeframe,period_EMA,0,MODE_EMA, PRICE_OPEN,0);
         double WMA0 = Utils.iMA(timeframe,period_WMA,0,MODE_LWMA,PRICE_OPEN,0);
         double EMA1 = Utils.iMA(timeframe,period_EMA,0,MODE_EMA, PRICE_OPEN,1);
         double WMA1 = Utils.iMA(timeframe,period_WMA,0,MODE_LWMA,PRICE_OPEN,1);
         //double rsi  = RSI.GetData(0, 0);//iRSI(timeframe,period_RSI,PRICE_OPEN,0);
         //double mfi  = MFI.GetData(0, 0);//(NULL,PERIOD_H1,period_RSI,0);
         if ((EMA0 < WMA0) && (EMA1 > WMA1))// && mfi > 50)
         {
            signal.Value = 1;
            return true;
         }
         if ((EMA0 > WMA0) && (EMA1 < WMA1))// && (mfi < 50))
         {
            signal.Value = -1;
            return true;
         }
         return false;   
      }
      
      //+------------------------------------------------------------------+
      int ProcessBands(ENUM_TIMEFRAMES timeframe, Signal& signal)
      {
         double isBuy = Utils.iBands(timeframe, 20, 2, 0, PRICE_LOW, MODE_LOWER, 0); 
         if (isBuy > Utils.tick.ask)
         {
            signal.Value = 1;
            signal.type = SignalBUY;
            return true;
         }
            
         double isSell = Utils.iBands(timeframe, 20, 2, 0, PRICE_HIGH, MODE_UPPER, 0); 
         if (isSell < Utils.tick.bid)
         {
            signal.Value = -1;
            signal.type = SignalSELL;
            return true;
         }
         return false;   
      }
      
      bool ProcessBWZone(ENUM_TIMEFRAMES timeframe, Signal& signal)
      {
          if (BWZoneHandle == NULL)
          {
             return false;
          }
          signal.Init(signal.UseAsFilter);
          signal.type = SignalQuiet;

          double result[2];
          if (Utils.GetIndicatorData(BWZoneHandle, 4, 0, 2, result) > 0)
          {
             if ((result[0] == 0.0) && (result[1] == 0.0))
             {
                signal.Handled = false;
                signal.type = SignalBUY;
                signal.Value = 1;
                return true;
             }
             if ((result[0] == 0.0)  && (result[1] == 2.0))
             {
                signal.Handled = false;
                signal.type = SignalBUY;
                signal.Value = 1;
                return true;
             }
             if ((result[0] == 1.0) && (result[1] == 1.0))
             {
                signal.Handled = false;
                signal.type = SignalSELL;
                signal.Value = -1;
                return true;
             }
             if ((result[0] == 1.0) && (result[1] == 2.0))
             {
                signal.Handled = false;
                signal.type = SignalSELL;
                signal.Value = -1;
                return true;
             }
          }
          return false;
      }
      */
                        
      bool ProcessFilter()
      {
         if (FilterIndicator == NoIndicator)
            return true;
         if (FI == NULL)
             return true;

         FilterSignal.UseAsFilter = true;
         bool result = FI.Process(FilterSignal);
         //bool result = ProcessIndicator(FilterIndicator, methods.Period, FilterSignal);
         return result;
      }
      
      bool ProcessSignal()
      {
          LastSignal.UseAsFilter = false;
          if (SI == NULL)
             return false;
          bool result = SI.Process(LastSignal);  //ProcessIndicator(SignalIndicator, methods.Period, LastSignal);
          if ((!LastSignal.Handled) && result && (LastSignal.type != SignalQuiet))
          { 
              if (!IsSignalChanged())
              {
                 LastSignal.Init(false);
                 LastSignal.Handled = true;
                 return false;
              }
          }
         return result;
      }
      
      /*int InitZigZag()
      {
#ifdef __MQL5__      
         ZigZagHandle = ::iCustom(Symbol, methods.Period, "Examples\\ZigZag", 12, 5, 3, PRICE_CLOSE);
         if (ZigZagHandle == INVALID_HANDLE)
            return -1;
#endif            
         return ZigZagHandle;
      }
      
      bool ZigZagPercentile(double& TPpercBUY, double& TPpercSELL)
      {
         if ((ZigZagHandle < 0) || (!TPByPercentile))
            return false;
#ifdef __MQL5__            
         double ArrayImpulseBUY[];
         ArrayResize(ArrayImpulseBUY, NumBarsToAnalyze);
         double ArrayImpulseSELL[];
         ArrayResize(ArrayImpulseSELL, NumBarsToAnalyze);
         ArrayInitialize(ArrayImpulseBUY, 0);
         ArrayInitialize(ArrayImpulseSELL, 0);
         
         int n = 0, k = 0, i;
         double zag = 0;
         double zig = 0;
         i = 0;
         
         double result[];
         //ArraySetAsSeries(result,true); 
         ArrayResize(result, 1000);
         ArrayInitialize(result, 0);
         int count = CopyBuffer(ZigZagHandle, 0, 0, 1000, result);
         if ( count < 0)
            return false;
         while(((n < NumBarsToAnalyze) || (k < NumBarsToAnalyze)) && (i < count)) 
         {
            if (zig > 0)
            {
               if (zag > 0)
               {
                  if (( zag > zig) && (n < NumBarsToAnalyze))  
                    ArrayImpulseBUY[n++] = (zag - zig)/methods.Point;
                  if ((zig > zag) && (k < NumBarsToAnalyze))
                    ArrayImpulseSELL[k++] = (zig - zag)/methods.Point;
               }
               zag = zig;               
            }
            zig = result[i];
            i++;
         }
         
         ArraySort(ArrayImpulseBUY);
         ArraySort(ArrayImpulseSELL);
         
         double rank = TP_PERCENTILE;
         //if (Trend == LATERAL)
         //   rank = TP_PERCENTILEMIN;
         
         TPpercBUY = Utils.ArrayPercentile(ArrayImpulseBUY, rank);
         TPpercSELL = Utils.ArrayPercentile(ArrayImpulseSELL, rank);
         //Print(StringFormat("Perc TP BUY=%g TP SELL=%g", TPpercBUY, TPpercSELL));
#endif         
         return true;   
      }*/
};


