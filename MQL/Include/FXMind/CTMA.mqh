#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\InputTypes.mqh>
#include <FXMind\IndiBase.mqh>
#include <FXMind\Orders.mqh>
class TradeSignals;
#include <FXMind\TradeSignals.mqh>

class CTMA : public IndiBase
{
protected:
   //int    tma_period;
   int    atr_multiplier;
   //int    atr_period;
   //double TrendThreshold;
   //bool   PercentileATR;
   //double PercentileRank;   
public:
   TYPE_TREND Trend;
   CTMA(TradeSignals* s);
   ~CTMA();
   virtual bool Init(ENUM_TIMEFRAMES timeframe);
   virtual bool Process(Signal& signal);
   virtual void Trail(Order &order, int indent);
   virtual void Delete();
   virtual double GetData(const int buffer_num,const int index) const;
   void RetrieveTrend();

   
   virtual int       Type(void) const { return(IND_OSMA); }
};

//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CTMA::CTMA(TradeSignals* s) 
   :IndiBase(s)  
{
   m_name = "TMA";  
   Trend = LATERAL;  
}

bool CTMA::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;      
   m_period = timeframe;
   if (timeframe <= PERIOD_M15)
   {
      atr_multiplier = 2;
   }
   else 
      atr_multiplier = 3;
   int TMAPeriod = 56;
   int TMAATRPeriod = 100;
   double TMATrend = 0.45;
   
   SetSymbolPeriod(signals.Symbol, m_period);
   MqlParam params[];   
   ArrayResize(params,9);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = m_period;
   params[2].type = TYPE_INT;
   params[2].integer_value = TMAPeriod;
   params[3].type = TYPE_INT;
   params[3].integer_value = atr_multiplier;
   params[4].type = TYPE_INT;
   params[4].integer_value = TMAATRPeriod;
   params[5].type = TYPE_DOUBLE;
   params[5].double_value = TMATrend;
   params[6].type = TYPE_INT;
   params[6].integer_value = (m_period == PERIOD_H4)?false:true;
   params[7].type = TYPE_INT;
   params[7].integer_value = true;
   params[8].type = TYPE_INT;
   params[8].integer_value = 10; // Percentile Rank
   
   m_bInited = Create(signals.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, 9, params);
   if (m_bInited)
   {
#ifdef __MQL5__    
      FullRelease(!Utils.IsTesting());
      AddToChart(signals.chartID, signals.subWindow);
#else 
      Utils.AddToChart(0, m_name, signals.chartID, signals.subWindow);
#endif
      return true;

   }
   Utils.Info("Indicator TMA - failed to load!!!!!!!!!!!!!");
   return m_bInited;
}

void CTMA::Delete()
{
#ifdef __MQL5__
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(signals.chartID, signals.subWindow);
    }
#endif  
}

//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CTMA::~CTMA(void)
{
   Delete();
}

double CTMA::GetData(const int buffer_num,const int index) const
{   
#ifdef __MQL4__   
   double val = iCustom(NULL
      ,m_period
      ,m_name
      ,m_period
      ,tma_period
      ,atr_multiplier
      ,atr_period
      ,TrendThreshold
      ,PercentileATR
      ,PercentileRank
      ,buffer_num,index);
   //Utils.Info(StringFormat("OsMA BufIndex=%d, index=%d, val=%g", buffer_num, index, val));
   return val;
#else   
   double Buff[2];
   CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   return Buff[0];
#endif    
}

void CTMA::RetrieveTrend()
{
   double bull = GetData(3, 0);
   double bear = GetData(4, 0);
   //double neutral = GetData(5, 0);
   Trend = LATERAL;
   if (bull != EMPTY_VALUE)
     Trend = UPPER;
   if (bear != EMPTY_VALUE)
     Trend = DOWN;
     
   //if (signals.trendTMA == GetPointer(this))
   //{
   //    signals.Trend = Trend;
   //}
}

bool CTMA::Process(Signal& signal)
{
  
   double tmaValue = GetData(0, 0);          
   //double upperBand = GetData(1, 0);
   //double lowerBand = GetData(2, 0);   
   
   RetrieveTrend();
   if (m_period == PERIOD_H4)
      return false;
   
   //signals.trendTMA.RetrieveTrend();
  
   MqlRates rates[];
   ArrayResize(rates, 3);
   //ArraySetAsSeries(rates, false);
   CopyRates(signals.Symbol, (ENUM_TIMEFRAMES)m_period, 0, 3, rates);
   
   //double priceHigh = MathMax(rates[0].high, rates[1].high);//
   //double priceLow = MathMin(rates[0].low, rates[1].low);   
   
   double UpperBuffer[3];
   Utils.GetIndicatorData(this, 1, 0, 3, UpperBuffer); 
   double LowerBuffer[3];
   Utils.GetIndicatorData(this, 2, 0, 3, LowerBuffer);


  // bool AfterNews = true;
  // if (EnableNews)
   //{
  //     AfterNews = (!signals.InNewsPeriod) || (signals.NewsMinsRemained < 0);
  // }
      
   /*if (!signal.UseAsFilter)
   {    
      if ((signals.trendTMA.Trend == LATERAL) && AfterNews)
      {
         if (LowerBuffer[0] > Utils.tick.bid ) 
         {
            signal.Init(false);   
            signal.Value = 1;
            signal.type = SignalBUY;         
            return true;
         }
         if (UpperBuffer[0] < Utils.tick.ask ) 
         {
            signal.Init(false);   
            signal.Value = -1;
            signal.type = SignalSELL;         
            return true;
         }      
      }

      if ( (signals.trendTMA.Trend == UPPER) || (signals.trendTMA.Trend == LATERAL))
      {
         CROSS_TYPE isCrossed = Utils.CandleBodyCross(LowerBuffer, rates);
         if (AfterNews && (isCrossed >0))
         {
            signal.Init(false);   
            signal.Value = 1;
            signal.type = SignalBUY;
            if (signals.thrift.GetLastNewsEvent() != NULL)
            signal.SetName(signals.thrift.GetLastNewsEvent().GetName());
            signals.FilterStatusString = StringFormat("LOCALTREND(%s) %s On %s ", EnumToString(Trend), EnumToString(signal.type), EnumToString(TMAIndicator));
            return true;
         }         
      }
   
      if ( (signals.trendTMA.Trend == DOWN) || (signals.trendTMA.Trend == LATERAL))
      {
         CROSS_TYPE isCrossed = Utils.CandleBodyCross(UpperBuffer, rates);
         if (AfterNews && (isCrossed >0))
         {
            signal.Init(false);   
            signal.Value = -1;
            signal.type = SignalSELL;
            if (signals.thrift.GetLastNewsEvent() != NULL)
               signal.SetName(signals.thrift.GetLastNewsEvent().GetName());
            signals.FilterStatusString = StringFormat("LOCALTREND(%s) %s On %s ", EnumToString(Trend), EnumToString(signal.type), EnumToString(TMAIndicator));
            return true;
         }         
      }
     
   } else 
   {
   */
      // Filter mode
      /*if (AfterNews) //(signals.trendTMA.Trend == LATERAL) && 
      {
         CROSS_TYPE crossUp = Utils.CandleBodyCross(LowerBuffer, rates); //Utils.CandleCross(LowerBuffer[0], rates);
         if ((crossUp > CROSS_UP ))// && ((signals.Trend == LATERAL) || (signals.Trend == UPPER))  ) 
         {
            signal.Init(false);   
            signal.Value = 1;
            signal.type = SignalBUY;         
            return true;
         }
         CROSS_TYPE crossDown = Utils.CandleBodyCross(UpperBuffer, rates); //Utils.CandleCross(UpperBuffer[0], rates);
         if ((crossDown > CROSS_DOWN ))// && ((signals.Trend == LATERAL) || (signals.Trend == DOWN))) 
         {
            signal.Init(false);   
            signal.Value = -1;
            signal.type = SignalSELL;         
            return true;
         }      
      }
      */
      //if ((signals.trendTMA.Trend == DOWN))
      {
         //CROSS_TYPE crossUp = Utils.CandleCross(tmaValue, rates);
         if (( tmaValue < rates[0].low ) && ( UpperBuffer[0] < rates[0].open )) 
         {
            signal.Init(false);   
            signal.Value = -1;
            signal.type = SignalSELL;         
            return true;
         }
      }
      //if ((signals.trendTMA.Trend == UPPER))
      {
         //CROSS_TYPE crossDown = Utils.CandleCross(tmaValue, rates);
         if ( (tmaValue > rates[0].high) && ( LowerBuffer[0] > rates[0].open ) ) 
         {
            signal.Init(false);   
            signal.Value = 1;
            signal.type = SignalBUY;         
            return true;
         }      
      }
   return false;
}


void CTMA::Trail(Order &order, int indent)
{      
   if (!m_bInited)
     return;
   double mediaBand = GetData(0, 0);          
   double upperBand = GetData(1, 0);
   double lowerBand = GetData(2, 0);
   MqlRates rates[];
   ArrayResize(rates, 3);
   ArraySetAsSeries(rates, true);
   CopyRates(signals.Symbol, (ENUM_TIMEFRAMES)m_period, 0, 3, rates);
   
   double Pt = signals.methods.Point;
   signals.trailDelta = (Utils.Spread() + indent + signals.methods.StopLevelPoints)*Pt;
   double mediaPrice = (Utils.tick.ask + Utils.tick.bid)/2.0;

   order.stopLoss = Utils.OrderStopLoss();
   order.takeProfit = Utils.OrderTakeProfit();    
   order.openPrice = Utils.OrderOpenPrice();
   order.profit = order.RealProfit();
          
   double SL = order.stopLoss;
   double TP = order.takeProfit;
   double OP = order.openPrice;
   double Profit = order.profit;             
   if (MathAbs(OP - mediaPrice) <= (signals.trailDelta*2))
    return; // Skip trailing
    
   order.Select();
   CROSS_TYPE upperCross = Utils.CandleCross(upperBand, rates);
   if ((upperCross == CROSS_DOWN) && (order.type == OP_BUY) ) //((Trend == LATERAL) || (Trend == DOWN)))
   {
      //Utils.Info(StringFormat("Order(%d) hit Upper band set new SL", order.ticket));
      if (TrailLevel(order, Utils.tick.ask, Utils.tick.bid, SL, TP, upperBand))
         return;
      //order.SetRole(ShouldBeClosed);
      return;
   }

   CROSS_TYPE lowerCross = Utils.CandleCross(lowerBand, rates);
   if ((lowerCross == CROSS_UP) && (order.type == OP_SELL) )//((Trend == LATERAL) || (Trend == UPPER)) )
   {
      //Utils.Info(StringFormat("Order(%d) hit Lower band set new SL", order.ticket));
      if (TrailLevel(order, Utils.tick.ask, Utils.tick.bid, SL, TP, lowerBand))
         return;
      //order.SetRole(ShouldBeClosed);
      return;
   }
   /*
   if ( (!AllowStopLossByDefault) && (order.RealProfit() < 4))
   {
         if ((order.type == OP_BUY)) 
         {
            signals.LastSignal.Init(false);
            signals.LastSignal.Value = -1;
            signals.LastSignal.type = SignalSELL;
            return;
         }
         if ((order.type == OP_SELL)) 
         {
            signals.LastSignal.Init(false);   
            signals.LastSignal.Value = 1;
            signals.LastSignal.type = SignalBUY;         
            return;
         }      
   }
   */

   if (EnableNews)
   {
     if (!signals.InNewsPeriod)
       return;
   }

   if (order.RealProfit() > 0)
   {
      if (TrailLevel(order, Utils.tick.ask, Utils.tick.bid, SL, TP, lowerBand))
         return;
      //if (TrailLevel(order, Utils.tick.ask, Utils.tick.bid, SL, TP, mediaBand))
      //   return;       
      if (TrailLevel(order, Utils.tick.ask, Utils.tick.bid, SL, TP, upperBand))
         return;
   }

}