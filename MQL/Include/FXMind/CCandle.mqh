#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\IndiBase.mqh>
#include <FXMind\TradeSignals.mqh>

class CCandle : public IndiBase
{
protected:
   string prevTime;
public:
   CCandle(TradeSignals* s);
   ~CCandle(void);
   virtual bool Init(ENUM_TIMEFRAMES timeframe);
   virtual bool Process(Signal& signal);
   virtual void Trail(Order &order, int indent) {}
   virtual void Delete();
   virtual double    GetData(const int buffer_num,const int index) const;
   bool              Initialize();
   virtual int       Type(void) const { return(IND_CUSTOM); }
   void CleanGlobalVars();
};
//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CCandle::CCandle(TradeSignals* s) 
   :IndiBase(s)  
{
   signals = s;
   m_name = "CandlePatterns";
}

void CCandle::CleanGlobalVars()
{
    GlobalVariablesDeleteAll("CandleSignal");
    if (signals.thrift != NULL)
    {
       string strMagic = IntegerToString(signals.thrift.MagicNumber());
       signals.thrift.InitNewsVariables(strMagic);
    }
}   

bool CCandle::Init(ENUM_TIMEFRAMES timeframe)
{
   if (Initialized())
      return true;
   CleanGlobalVars();
   SetSymbolPeriod(signals.Symbol,timeframe);   
   MqlParam params[];   
   ArrayResize(params,3);
   params[0].type = TYPE_STRING;
   params[0].string_value = m_name;
   params[1].type = TYPE_INT;
   params[1].integer_value = NumBarsToAnalyze;
   params[2].type = TYPE_INT;
   params[2].integer_value = signals.thrift.MagicNumber();   
   m_bInited = Create(signals.Symbol, (ENUM_TIMEFRAMES)m_period, IND_CUSTOM, 3, params);
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

void CCandle::Delete()
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
CCandle::~CCandle(void)
{
   Delete();
}

//+------------------------------------------------------------------+
//| Initialize indicator with the special parameters                 |
//+------------------------------------------------------------------+
bool CCandle::Initialize()
{
#ifdef  __MQL5__
   if(CreateBuffers(m_symbol,m_period,5))
   {
      ((CIndicatorBuffer*)At(4)).Name("CANDLE");
      return(true);
   }
   //--- error
   return(false);
#else 
   return(true);
#endif   
}


double CCandle::GetData(const int buffer_num,const int index) const
{   
#ifdef __MQL4__
   double val = iCustom(NULL,m_period, m_name, buffer_num, index);
   Utils.Info(StringFormat("CandlePatterns BufIndex=%d, index=%d, val=%g", buffer_num, index, val));
   return val;
#else    
   double Buff[2];
   CopyBuffer(m_handle, buffer_num, index, 1, Buff); 
   return Buff[0];
#endif   
}

bool CCandle::Process(Signal& signal)
{
    /*
    double result[];
    double resultTime[];
    int array_size = 4;
    //ArrayResize(result, array_size);
    //ArrayResize(resultTime, array_size);
    ArraySetAsSeries(result,false);
    ArraySetAsSeries(resultTime,false);
    int i = 0; 
    CopyBuffer(IndiHandle, 0, 0, array_size, result);
    CopyBuffer(IndiHandle, 1, 0, array_size, resultTime);
    datetime signalTime = (datetime)resultTime[i];
    double res = result[0];
    */
#ifdef __MQL4__         
      double val = iCustom(Symbol, timeframe, "CandlePatterns", NumBarsToAnalyze, (int)thrift.MagicNumber(), 0, 0); 
#endif                

      string strMagic = IntegerToString(signals.thrift.MagicNumber());
      datetime time = (datetime) GlobalVariableGet("CandleSignalTime" + strMagic);
      string SignalTime = TimeToString(time);
      if (StringCompare(prevTime, SignalTime) == 0)
          return false; // Signal handled;
      datetime currentTime = Utils.CurrentTimeOnTF();
      //string CurrentTime = TimeToString(currentTime);
      int PeriodsPassed = (int)(currentTime-time)/PeriodSeconds();
      if ( PeriodsPassed > 1)
         return false; // Signal too old;
      
      signal.Init(signal.UseAsFilter);
      signal.type = SignalQuiet;

      double res = GlobalVariableGet("CandleSignal" + strMagic);
      
      //if (MathAbs(res) < 2)
      //   return false;
                  
      for (int i = 0; i < GlobalVariablesTotal() ; i++)
      {
         string name = GlobalVariableName(i);
         if (StringFind(name, "CandleSignalName" + strMagic) >= 0) 
         {
            int startPos = StringFind(name, "|");
            if (startPos > 0)
            {
               string strName = StringSubstr(name, startPos + 1);
               signal.SetName(strName);
            }   
            break;
         }
      }
      
       if (StringCompare(SignalTime, prevTime) != 0)
          prevTime = SignalTime;
       signal.Handled = false;
       //if (signal.type == SignalQuiet)
       signal.type = SignalCANDLE;
       
       //if (MathAbs(res) == 1.5)             
         signal.Value = (int)res;
       //else signal.Value = 0;
       signals.StatusString += StringFormat("/ Candle %s", EnumToString(signal.type)); 
      
        //    return false;
       //for (int i = 0; i <1000; i++)
       //{
          //if (result[i] != 0)
          //{
            //Print(StringFormat("Bufffer %d = %g", i, result[i]));                  
          //}
       //}
    return true;
}
