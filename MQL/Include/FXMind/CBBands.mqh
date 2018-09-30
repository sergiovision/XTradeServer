#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\InputTypes.mqh>
#include <FXMind\IndiBase.mqh>
#include <FXMind\Orders.mqh>
#include <FXMind\TradeSignals.mqh>
#include <FXMind/SmoothAlgorithms.mqh>

class CBBands : public IndiBase
{
protected:
   ENUM_MA_METHOD MA_Method;
   Applied_price_ IPC;
   
public:
   CBBands(TradeSignals* s);
   ~CBBands();
   virtual bool Init(ENUM_TIMEFRAMES timeframe);
   virtual bool Process(Signal& signal);
   virtual void Trail(Order &order, int indent);
   virtual void Delete();
   virtual double GetData(const int buffer_num,const int index) const;
   virtual int  Type(void) const { return(IND_CUSTOM); }
   virtual bool      Initialize(const string symbol,const ENUM_TIMEFRAMES period,
                                const int num_params,const MqlParam &params[]);
};

//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CBBands::CBBands(TradeSignals* s) 
   :IndiBase(s)  
{
   m_name = "BBands";  
}

bool CBBands::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;      
   m_period = timeframe;

   IPC = PRICE_CLOSE_;
   MA_Method = MODE_SMA;
  
   SetSymbolPeriod(signals.Symbol, m_period);
   MqlParam params[];   
   ArrayResize(params,5);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = BandsPeriod;
   params[2].type = TYPE_DOUBLE;
   params[2].double_value = BandsDeviation;
   params[3].type = TYPE_INT;
   params[3].integer_value = MA_Method;
   params[4].type = TYPE_INT;
   params[4].integer_value = IPC;
      
   m_bInited = Create(signals.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, 5, params);
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
   return m_bInited;
}

void CBBands::Delete()
{
#ifdef __MQL5__
    if (Handle() != INVALID_HANDLE)
    {
        DeleteFromChart(signals.chartID, signals.subWindow);
    }
#endif  
}

bool CBBands::Initialize(const string symbol,const ENUM_TIMEFRAMES period, const int num_params,const MqlParam &params[]) 
{
#ifdef  __MQL5__
   if(CreateBuffers(symbol,period,3))
   {
      ((CIndicatorBuffer*)At(0)).Name("Upper");
      ((CIndicatorBuffer*)At(1)).Name("Middle");
      ((CIndicatorBuffer*)At(2)).Name("Lower");
      return(true);
   }
   //--- error
   return(false);
#else 
   return(true);
#endif   
}

//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CBBands::~CBBands(void)
{
   Delete();
}

double CBBands::GetData(const int buffer_num,const int index) const
{   
#ifdef __MQL4__   
   double val = iCustom(NULL
      ,m_period
      ,m_name
      ,BandsPeriod
      ,BandsDeviation
      ,MA_Method
      ,IPC
      ,buffer_num,index);
   //Utils.Info(StringFormat("OsMA BufIndex=%d, index=%d, val=%g", buffer_num, index, val));
   return val;
#else   
   //return CIndicator::GetData(buffer_num, index);   
   double Buff[1];
   //ArrayResize(Buff, 1);
   //ArraySetAsSeries(Buff, true);
   int res = CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   if (res > 0)
      return Buff[0];
   else 
      return 0;
#endif    
}

bool CBBands::Process(Signal& signal)
{
   double tmaValue = GetData(0, 0);          
   return false;
}

void CBBands::Trail(Order &order, int indent)
{      
   if (!m_bInited)
     return;   
}


