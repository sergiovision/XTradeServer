#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\InputTypes.mqh>
#include <XTrade\IndiBase.mqh>
#include <XTrade\Signals.mqh>
//#include <XTrade\TradeSignals.mqh>
#include <XTrade\GenericTypes.mqh>

class CNewsIndicator : public IndiBase
{
//protected:
public:
   CNewsIndicator();
   ~CNewsIndicator();
   bool Init(ENUM_TIMEFRAMES timeframe);
   void Process();
   void Delete();
   virtual int       Type(void) const { return(0); }
#ifdef __MQL4__      
   double            GetData(const int buffer_num,const int index) const;
#endif   
   bool OpenNewsSTOPOrders(Signal& signal);
};
//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CNewsIndicator::CNewsIndicator() 
{
   m_bInited = false;
   m_name = "NewsIndicator";   
}

bool CNewsIndicator::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;
   SetSymbolPeriod(Utils.Symbol, timeframe);
   //signals.NewsMinsRemained = INT_MIN;
   
   MqlParam params[6];
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = Utils.Service().MagicNumber();
   params[2].type = TYPE_INT;
   params[2].integer_value = ThriftPORT;
   params[3].type = TYPE_INT;
   params[3].integer_value = GET(MinImportance);
   params[4].type = TYPE_INT;
   params[4].integer_value = GET(PanelSize);
   params[5].type = TYPE_INT;
   params[5].integer_value = Utils.Service().MagicNumber();
   
   m_bInited = Create(Utils.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, 6, params);
   if (m_bInited)
   {
      AddToChart(Utils.Trade().ChartId(), Utils.Trade().IndiSubWindow());
      FullRelease(!Utils.IsTesting());
   }
   return m_bInited;
}

void CNewsIndicator::Delete()
{
  DeleteFromChart(Utils.Trade().ChartId(), Utils.Trade().IndiSubWindow());
}

//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CNewsIndicator::~CNewsIndicator(void)
{
   Delete();
}

//--------------------------------------------------------------------
void CNewsIndicator::Process()
{   
   datetime curtime = TimeCurrent();
   
   //SignalNews* lastNews = signals.thrift.GetLastNewsEvent();
   //if ( lastNews == NULL)
   //   return;

   //signals.NewsMinsRemained = INT_MIN;
   //if (lastNews != NULL)
   //{
   //    signals.NewsMinsRemained = (int)MathRound((lastNews.RaiseTime() - curtime)/60);
   //}
   //Utils.Info(StringFormat("In %d mins News Alert %s", signals.NewsMinsRemained, lastNews.GetName()));
   
      
   //if (signals.InNewsPeriod)
   //{
   //   int minsNewsPeriod = (int)MathRound((curtime - signals.timeNewsPeriodStarted)/60);
   //   if (minsNewsPeriod >= GET(NewsPeriodMinutes))
   //      signals.InNewsPeriod = false;
   //}
                    
   /*
   string eventString = signal.ToString();
   if (signals.NewsMinsRemained < 0)
      eventString = StringFormat("InNews=%s %s Passed %d min ago", (string)signals.InNewsPeriod, eventString, -1*signals.NewsMinsRemained);
   else
      eventString = StringFormat("InNews=%s %s Upcoming in %d min", (string)signals.InNewsPeriod, eventString, signals.NewsMinsRemained);
   signals.StatusString = eventString;
   */
   
   //if (lastNews.OnAlert() && (signals.NewsMinsRemained >= 0) && (signals.NewsMinsRemained <= GET(RaiseSignalBeforeEventMinutes))) 
   {
      /*signals.InNewsPeriod = true;
      signals.timeNewsPeriodStarted = curtime;
      signal = lastNews;
      signal.type = SignalNEWS;
      signal.Value = 0; //newsignal.Importance + 1;
      signal.Handled = false;
      signal.SetRaiseTime(curtime);
      signal.SetName(lastNews.ToString());
      Utils.Debug(StringFormat("In %d mins News Alert %s", signals.NewsMinsRemained, signal.ToString()));*/
   }
}

bool CNewsIndicator::OpenNewsSTOPOrders(Signal& signal)
{
   /*double ask = Utils.Ask();
   double bid = Utils.Bid();
   string newsResistance = "NewsResistanceLine";
   string newsSupport = "NewsSupportLine";
   if ((signals.InNewsPeriod) && (signals.NewsMinsRemained > 0))
   {
      double atr = Utils.PercentileATR(signals.Symbol, signals.methods.Period, SL_PERCENTILE, (int)GET(NumBarsToAnalyze), 0);      

      double RP = ask + atr;
      double SP = bid - atr;
      if ( ObjectFind(signals.chartID, newsResistance) == -1 )
      {
         if(!ObjectCreate(signals.chartID,newsResistance,OBJ_HLINE,signals.subWindow,0,RP))
            Utils.Info(StringFormat("Error creating Horizontal Resistance Line: %d!",GetLastError()));
         ObjectSetInteger(signals.chartID, newsResistance, OBJPROP_PRICE_SCALE, true);
         ObjectSetDouble(signals.chartID, newsResistance, OBJPROP_PRICE, RP);
         ObjectSetInteger(signals.chartID,newsResistance,OBJPROP_COLOR,clrRed);
      }
      

      if ( ObjectFind(signals.chartID, newsSupport) == -1 )
      {
         if(!ObjectCreate(signals.chartID,newsSupport,OBJ_HLINE,signals.subWindow,0,SP))
            Utils.Info(StringFormat("Error creating Horizontal Support Line: %d!",GetLastError()));
         ObjectSetInteger(signals.chartID, newsSupport, OBJPROP_PRICE_SCALE, true);
         ObjectSetDouble(signals.chartID, newsSupport, OBJPROP_PRICE, SP);
         ObjectSetInteger(signals.chartID,newsSupport,OBJPROP_COLOR,clrRoyalBlue);
      }
   }
   if (signals.InNewsPeriod ) //&& (signals.NewsMinsRemained <= 0)
   {
      double atr = Utils.PercentileATR(signals.Symbol, signals.methods.Period, SL_PERCENTILE, (int)GET(NumBarsToAnalyze), 0);      
      double getSP = ObjectGetDouble(signals.chartID, newsSupport, OBJPROP_PRICE);
      double distance = (getSP - bid); 
      if ((getSP > 0) && (bid < getSP) && (distance < atr) && (distance >=0))
      {
         RaiseMarketSignal(1, StringFormat("TREND(%s) %s News DOWN=%g", EnumToString(signals.Trend),EnumToString(signal.type), getSP));
         return true;
      }
      double getRP = ObjectGetDouble(signals.chartID, newsResistance, OBJPROP_PRICE);
      distance = (ask - getRP); 
      if ((getRP > 0) && (ask > getRP) &&(distance < atr) && (distance >=0))
      {
         RaiseMarketSignal(1, StringFormat("TREND(%s) %s News UP=%g", EnumToString(signals.Trend), EnumToString(signal.type), getRP));
         return true;
      }
   }
   if (!signals.InNewsPeriod)
   {
      if ( ObjectFind(signals.chartID, newsSupport) != -1 )
         ObjectDelete(signals.chartID, newsSupport);
   
      if ( ObjectFind(signals.chartID, newsResistance) != -1 )
         ObjectDelete(signals.chartID, newsResistance);
   }
   */
   return false;
}
