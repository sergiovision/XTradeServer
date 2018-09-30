#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\IUtils.mqh>
#include <FXMind\Orders.mqh>
#ifdef THRIFT
#include <FXMind\FXMindClient.mqh>
#endif



#ifdef __MQL4__

class MQL4Utils: public IUtils
{
protected:
   IFXMindService* service;
public:
   MQL4Utils(short Port, string EA)
   {   
       Symbol = ::Symbol();
       Period = (ENUM_TIMEFRAMES)Period();
       
#ifdef THRIFT   
       service = new FXMindClient(Port, EA);
#else 
       service = new IFXMindService(Port, EA);
#endif       
   }
   
   virtual ~MQL4Utils()
   {
       DELETE_PTR(service);
   }   
   
   IFXMindService* Service()
   {
      return service;  
   }   
   
   bool IsMQL5()
   {
      return false;
   }
   
   double Ask()
   {
      return Ask;

   }
   
   double Bid()
   {
      return Bid;
   }


   datetime CurrentTimeOnTF()
   {
       return Time[0];
   }
   
   bool SelectOrder(long ticket)
   {
      return ::OrderSelect((int)ticket, SELECT_BY_TICKET);
   }
   
   long GetAccountNumer()
   {
      return ::AccountNumber();
   }
   
   bool SelectOrderByPos(int Position)
   {
      return ::OrderSelect(Position, SELECT_BY_POS);
   }
   
   long OrderTicket()
   {
      return ::OrderTicket();
   }
   
   int TimeMinute(datetime date)
   {
      return ::TimeMinute(date);
   }
   
   double OrderSwap()
   {
      return ::OrderSwap();
   }
   string OrderSymbol()
   {
      return ::OrderSymbol();
   }
   string OrderComment()
   {
      return ::OrderComment();
   }
   double OrderProfit()
   {
      return ::OrderProfit();
   }
   double OrderCommission()
   {
      return ::OrderCommission();
   }
   int OrderType()
   {
      return ::OrderType();
   }
   
   int OrderMagicNumber()
   {
      return ::OrderMagicNumber();
   }
   
   double OrderLots()
   {
      return ::OrderLots();   
   }
   
   double OrderOpenPrice()
   {
      return ::OrderOpenPrice();   
   }
   
   double OrderStopLoss()
   {
      return ::OrderStopLoss();   
   }
   
   double OrderTakeProfit()
   {
      return ::OrderTakeProfit();   
   }
         
   datetime OrderOpenTime()
   {
      return ::OrderOpenTime();
   }
   
   datetime OrderExpiration()
   {
      return ::OrderExpiration();
   }
   
   bool IsTesting()
   {
      return ::IsTesting();
   }
   
   bool IsVisualMode()
   {
      return ::IsVisualMode();
   }

   bool RefreshRates()
   {
       return ::RefreshRates();
   }
   
   double AccountBalance()
   {
      return ::AccountBalance();
   }
   
   int Spread()
   {
       return (int)SymbolInfoInteger(Symbol, SYMBOL_SPREAD);
   }

   int StopLevel()
   {
      return (int)SymbolInfoInteger(Symbol, SYMBOL_TRADE_STOPS_LEVEL);
   }
   
   int OrdersTotal()
   {
       return ::OrdersTotal();
   }
   
   double StopLevelPoints()
   {
      return StopLevel()*Point();
   }
      
   bool  OrderClose(long ticket,double lots, double price, int slippage, color arrow_color)
   {
      return ::OrderClose((int)ticket, lots, price, slippage, arrow_color);
   }
   
   bool  OrderClosePartially(long ticket, double lots, double price, int slippage)
   {
      return ::OrderClose((int)ticket, lots, price, slippage);
   }

   /*int  _OrderSend(string   symbol, int cmd, double volume, double price, int slippage, double stoploss, double takeprofit, string comm=NULL, int magic=0,datetime expiration=0, color arrow_color=clrNONE)
   {
      return ::OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit, comm, magic, expiration, arrow_color);
   }
   */
   
   bool OpenOrder(Order& order, int slipPage) 
   {
       int err = ::GetLastError();
       err = 0;
       int Color = 0;
       bool exit_loop = false;
       int cnt = 0;
       int ticket = 0;
       while (!exit_loop)
       {
          if (order.type == OP_BUY) 
          {
             Color = Blue;
             order.openPrice = Utils.Ask();
          }
          if (order.type == OP_SELL) 
          {
             Color = Red;
             order.openPrice = Utils.Bid();
          }
          order.lots = NormalizeDouble(order.lots, 2);       
          order.takeProfit = NormalizeDouble(order.takeProfit, Digits()); 
          order.stopLoss = NormalizeDouble(order.stopLoss, Digits());
          order.openPrice = NormalizeDouble(order.openPrice, Digits());
          //order.PrintIfWrong("OpenOrder");
          ticket = ::OrderSend(order.symbol, order.type, order.lots, order.openPrice, slipPage, order.stopLoss, order.takeProfit, order.comment, (int)order.magic, order.expiration, Color);
          
          //Utils._OrderSend(Symbol(), order.type, order.lots, order.openPrice, Slippage, order.stopLoss, order.takeProfit, order.comment, order.magic, order.expiration, Color);
          //LogInfo(order.ToString());
          err = ::GetLastError();
          switch (err)
          {
             case ERR_NO_ERROR:
                exit_loop = true;
             break;
             case ERR_SERVER_BUSY:
             case ERR_BROKER_BUSY:
             case ERR_TRADE_CONTEXT_BUSY:
             case ERR_TRADE_SEND_FAILED:
                cnt++;
             break;
             case ERR_INVALID_FUNCTION_PARAMVALUE:
               Print (StringFormat("!!!!!Invalid parameters: %s", order.ToString()));
               exit_loop = true;
             break;
             case ERR_INVALID_PRICE:
             case ERR_PRICE_CHANGED:
             case ERR_OFF_QUOTES:
             case ERR_REQUOTE:
                Utils.RefreshRates();
                Sleep(SLEEP_DELAY_MSEC);
                continue;
             break;
             default:
                exit_loop = true;
          }   
          if (cnt > RetryOnErrorNumber )
             exit_loop = true;
             
          if ( !exit_loop )
          {
             if (!Utils.IsTesting())
               Sleep(SLEEP_DELAY_MSEC);
             Utils.RefreshRates();
          }
          if ((err == ERR_NO_ERROR) && (ticket != -1)  ) 
          {
             order.ticket = ticket;
             return true;
          }
          delete &order;
          Print(StringFormat("Still error after %d retries!!! %s", RetryOnErrorNumber , TimeToString(TimeCurrent(),TIME_MINUTES)));
          return false;
       }
       delete &order;
       return false;
   }
   
   bool  OrderModify(long  ticket, double price, double stoploss, double takeprofit, datetime expiration, color arrow_color)
   {
      return ::OrderModify((int)ticket, price, stoploss, takeprofit, expiration, arrow_color);
   }
   
////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////
   double iATR(ENUM_TIMEFRAMES timeframe, int period, int shift)
   {
       return ::iATR(Symbol, timeframe, period, shift);
   }
   
   double iMA(ENUM_TIMEFRAMES timeframe, int ma_period, int ma_shift, ENUM_MA_METHOD ma_method, ENUM_APPLIED_PRICE applied_price, int shift)
   {
       return ::iMA(Symbol, timeframe, ma_period, ma_shift, ma_method, applied_price, shift);
   }

   double iRSI(ENUM_TIMEFRAMES period, int ma_period, ENUM_APPLIED_PRICE  applied_price, int shift)
   {
       return ::iRSI(Symbol, period, ma_period, applied_price, shift);
   }

   double iBands(ENUM_TIMEFRAMES period, int  bands_period, int  bands_shift, double  deviation, ENUM_APPLIED_PRICE  applied_price, int bufIndex, int shift)
   {
       return ::iBands(Symbol, period, bands_period, deviation, bands_shift, applied_price, bufIndex, shift);
   }

   double iCustom(ENUM_TIMEFRAMES period, string name, int bufIndex, int shift)
   {
       return ::iCustom(Symbol, period, name, bufIndex, shift);
   }
   
   double iCustom(ENUM_TIMEFRAMES period, string name, int param1, int param2, int param3, int bufIndex, int shift)
   {
       return ::iCustom(Symbol, period, name, param1, param2,  param3, bufIndex, shift);
   }
      
   double iClose(ENUM_TIMEFRAMES tf, int shift)
   {
       MqlRates rates[];
       ArraySetAsSeries(rates, true);
       int bars_count = Bars(Symbol, tf);
       int copied = CopyRates(Symbol, tf, shift, bars_count, rates); // Copied all datas
       return rates[0].close;
   }
   
   virtual void AddToChart(int Handle, string IndicatorName, long chartID, int subWin)
   { 
      //if (IsVisualMode())
      //    return;
      bool AutomaticallyAcceptDefaults = true;
       int hWnd = WindowHandle(Symbol(), 0);
      Sleep(100);
      uchar name2[];
      ArrayResize(name2, StringLen(IndicatorName) + 1);
      StringToCharArray(IndicatorName, name2, 0, StringLen(IndicatorName));
      int MessageNumber = RegisterWindowMessageW("MetaTrader4_Internal_Message");
      int r = PostMessageW(hWnd, MessageNumber, 15, name2); 
      Sleep(500);
      if(AutomaticallyAcceptDefaults) {
         int ind_settings = FindWindowW(NULL, "Custom Indicator - " + IndicatorName);
         if (ind_settings != 0)
            PostMessageW(ind_settings,0x100,VK_RETURN,name2);
      }
   }
   
   virtual int iCustomHandle(ENUM_TIMEFRAMES period, string name, int param1, int param2, int param3) 
   {
      double value = ::iCustom(Symbol, period, name, 0, 0); 
      return 0;
   }
   
   virtual int GetIndicatorData(CIndicator& indi, int BuffIndex, int startPos, int Count, double &Buffer[])
   {
     int j = 0;
     int i = Count - 1;
     double value = 0;
     for (; i >= 0; i--)
     {
         value = indi.GetData(BuffIndex, i);
         Buffer[i] = value;
         j++;
     }
     return j;
   }
   
    virtual bool GetIndicatorMinMax( CIndicator& indi, double& Min, double& Max, TYPE_TREND& trend, int BuffIndex, int numBars)
    {
        int i = numBars - 1;
        double value = 0;
        Min = DBL_MAX;
        Max = DBL_MIN;
        double aver = 0;
        int count = numBars;
        double Buff[];
        ArrayResize(Buff, numBars);
        ArraySetAsSeries(Buff, true);
        for (; i >= 0; i--)
        {
          value = indi.GetData(BuffIndex, i);
          Buff[i] = value;
          aver += value;
        }
        i = ArrayMinimum(Buff);
        if (i >= 0)
           Min = Buff[i];
        i = ArrayMaximum(Buff);
        if (i >= 0)
           Max = Buff[i];
        aver = aver / count;
        if(aver < value)
           trend=UPPER;
        if(aver > value) 
           trend=DOWN;
        if(aver == value) 
           trend=LATERAL;
        if ((Min == DBL_MAX) || (Max == DBL_MIN))
          return false;
        return true;
    }
    
    virtual int Bars() { return Bars;}

};


IUtils* CreateUtils(short Port, string EA) export 
{ 
   return new MQL4Utils(Port, EA);  
}

#endif
