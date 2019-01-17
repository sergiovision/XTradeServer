#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\IndiBase.mqh>
#include <XTrade\TradeSignals.mqh>

class CHeikenAshi : public IndiBase
{
protected:
public:
   CHeikenAshi(TradeSignals* s);
   ~CHeikenAshi(void);
   virtual bool Init(ENUM_TIMEFRAMES timeframe);
   virtual bool Process(Signal& signal);
   virtual void Trail(Order &order, int indent) {}
   virtual void Delete();
   virtual double    GetData(const int buffer_num,const int index) const;
   bool              Initialize();
   virtual int       Type(void) const { return(IND_CUSTOM); }
};
//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CHeikenAshi::CHeikenAshi(TradeSignals* s) 
   :IndiBase(s)  
{
   signals = s;
   m_name = "Heiken_Ashi";
}

bool CHeikenAshi::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;
   SetSymbolPeriod(signals.Symbol,timeframe);   
   MqlParam params[];   
   ArrayResize(params,1);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   
   m_bInited = Create(signals.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, 1, params);
   if (m_bInited)
     m_bInited = Initialize();
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
   Utils.Info(StringFormat("Indicator %s - failed to load!!!!!!!!!!!!!", m_name));
   return false;
}

void CHeikenAshi::Delete()
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
CHeikenAshi::~CHeikenAshi(void)
{
   Delete();
}


//+------------------------------------------------------------------+
//| Initialize indicator with the special parameters                 |
//+------------------------------------------------------------------+
bool CHeikenAshi::Initialize()
{
#ifdef  __MQL5__
   if(CreateBuffers(m_symbol,m_period,5))
   {
      ((CIndicatorBuffer*)At(4)).Name("COLOR");
      //((CIndicatorBuffer*)At(1)).Name("KIJUNSEN_LINE");
      return(true);
   }
   //--- error
   return(false);
#else 
   return(true);
#endif   
}


double CHeikenAshi::GetData(const int buffer_num,const int index) const
{   
#ifdef __MQL4__
   double val = iCustom(NULL,m_period, m_name, buffer_num, index);
   Utils.Info(StringFormat("Heiken Ashi BufIndex=%d, index=%d, val=%g", buffer_num, index, val));
   return val;
#else    
   double Buff[2];
   CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   return Buff[0];
#endif   
}

bool CHeikenAshi::Process(Signal& signal)
{
    double cl = GetData(4, 0);
    
    signal.Init(signal.UseAsFilter);
    signal.type = SignalQuiet;

    //if (!signal.UseAsFilter)
    //{ 
       // First Sell Signal             
    if ( cl == 1.0 )
    {
        signal.Handled = false;
        signal.Value = -1; 
        signal.type = SignalSELL;
        signals.StatusString = StringFormat("%s On in Heiken Ashi", EnumToString(signal.type));
        return true;
    }
    
    // First Buy Signal
    if ( cl == 0.0 )
    {
        signal.Handled = false;
        signal.Value = 1; 
        signal.type = SignalBUY;
        signals.StatusString = StringFormat("%s On in Heiken Ashi", EnumToString(signal.type));
        return true;
    }
    //}
      

   return false;
}

