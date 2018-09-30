#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\SettingsFile.mqh>
#include <FXMind\IUtils.mqh>
#include <FXMind\Orders.mqh>
//#include <Strings\String.mqh>

#ifdef THRIFT
#include <FXMind\FXMindClient.mqh>
#endif

#ifdef __MQL5__
#include <Trade\Trade.mqh>

IUtils* CreateUtils(short Port, string EA) export 
{
   return new MQL5Utils(Port, EA);  
}


class MQL5Utils: public IUtils
{
protected:
   IFXMindService* service;
   CTrade m_trade;
   //CSymbolInfo    m_symbol;
public:
   MQL5Utils(short Port, string EA)
   {   
       Symbol = Symbol();
       Period = (ENUM_TIMEFRAMES)Period();
#ifdef THRIFT   
       service = new FXMindClient(Port, EA);
#else 
       service = new IFXMindService(Port, EA);
#endif       
       //m_symbol.Name(Symbol);                  // sets symbol name
       //m_symbol.Refresh();
       
       m_trade.LogLevel(LOG_LEVEL_NO);
   }

   virtual ~MQL5Utils()
   {
       DELETE_PTR(service);
   }   
   
   IFXMindService* Service()
   {
      return service;  
   }   
   
   bool IsMQL5()
   {
      return true;
   }
   
   double Ask()
   {
      return SymbolInfoDouble(Symbol, SYMBOL_ASK);
   }
   
   double Bid()
   {
      return SymbolInfoDouble(Symbol, SYMBOL_BID);
   }

   datetime CurrentTimeOnTF()
   {
      datetime Time[];
      int count = 1;   // number of elements to copy
      ArraySetAsSeries(Time, true);
      CopyTime(Symbol, Period,0,count,Time);
      return Time[0];
   }
   
   bool SelectOrder(long ticket)
   {
      return PositionSelectByTicket(ticket);
   }
   
   bool SelectOrderByPos(int index)
   {
      ENUM_ACCOUNT_MARGIN_MODE margin_mode=(ENUM_ACCOUNT_MARGIN_MODE)AccountInfoInteger(ACCOUNT_MARGIN_MODE);
      //---
      if(margin_mode==ACCOUNT_MARGIN_MODE_RETAIL_HEDGING)
        {
         ulong ticket=PositionGetTicket(index);
         if(ticket==0)
            return(false);
        }
      else
        {
         string name=PositionGetSymbol(index);
         if(name=="")
            return(false);
        }
      return true;     
   }
   
   long OrderTicket()
   {
      return (int)PositionGetInteger(POSITION_IDENTIFIER);
   }
   
   int TimeMinute(datetime date)
   {
      MqlDateTime tm;
      TimeToStruct(date,tm);
      return(tm.min);
   }

   long GetAccountNumer()
   {
      return AccountInfoInteger(ACCOUNT_LOGIN);
   }
   double OrderSwap()
   {
      return PositionGetDouble(POSITION_SWAP);
   }
   string OrderSymbol()
   {
      string sym = PositionGetString(POSITION_SYMBOL);
      return sym;
   }
   string OrderComment()
   {
      return PositionGetString(POSITION_COMMENT);
   }
   double OrderProfit()
   {
      return PositionGetDouble(POSITION_PROFIT);
   }
   double OrderCommission()
   {
      return PositionGetDouble(POSITION_COMMISSION);
   }
   
   int OrderType()
   {
      return (int)PositionGetInteger(POSITION_TYPE);
   }
   
   int OrderMagicNumber()
   {
      int magic = (int)PositionGetInteger(POSITION_MAGIC);
      return magic;
   }
   
   double OrderLots()
   {
      return PositionGetDouble(POSITION_VOLUME);   
   }
   
   double OrderOpenPrice()
   {
      return PositionGetDouble(POSITION_PRICE_OPEN);   
   }
   
   double OrderStopLoss()
   {
      return PositionGetDouble(POSITION_SL);   
   }
   
   double OrderTakeProfit()
   {
      return PositionGetDouble(POSITION_TP);   
   }
         
   datetime OrderOpenTime()
   {
      return((datetime)PositionGetInteger(POSITION_TIME));
   }
   
   datetime OrderExpiration()
   {
      return((datetime)OrderGetInteger(ORDER_TIME_EXPIRATION));
   }
   
   bool IsTesting()
   {
      return (bool)MQLInfoInteger(MQL_TESTER);
   }
   
   bool IsVisualMode()
   {
      return (bool)MQLInfoInteger(MQL_VISUAL_MODE);
   }

   bool RefreshRates()
   {   
      //--- refresh rates
      //if(!m_symbol.RefreshRates())
      //   return(false);
      //--- protection against the return value of "zero"
      //if(m_symbol.Ask()==0 || m_symbol.Bid()==0)
      //   return(false);
      //---
      return SymbolInfoTick(Symbol, tick);
   }
   
   double AccountBalance()
   {
      return(AccountInfoDouble(ACCOUNT_BALANCE));
   }
   
   int Spread()
   {
       return (int)SymbolInfoInteger(Symbol(), SYMBOL_SPREAD);
   }

   int StopLevel()
   {
      int stop = (int)SymbolInfoInteger(Symbol(), SYMBOL_TRADE_STOPS_LEVEL);
      return (int)stop;
   }
   
   int OrdersTotal()
   {
       return PositionsTotal();
   }
   
   double StopLevelPoints()
   {
      return StopLevel()*Point();
   }
   
   bool  OrderClose(long ticket,double lots, double price, int slippage, color arrow_color)
   {
      /*
      if(!PositionSelectByTicket(ticket))
         return(false);
      ZeroMemory(m_request);
      int cmd = this.OrderType();
      if (cmd == OP_BUY)
         m_request.type     = ORDER_TYPE_SELL;
      else if (cmd == OP_SELL)
         m_request.type     = ORDER_TYPE_BUY;

      m_request.price = price;
      m_request.action   =TRADE_ACTION_DEAL;
      m_request.symbol   =OrderSymbol();
      if (lots == 0.0)
      {
         m_request.volume   =PositionGetDouble(POSITION_VOLUME);
      } else 
         m_request.volume   = lots;
      

      m_request.magic    =this.OrderMagicNumber();
      m_request.deviation=slippage;
      m_request.position = ticket;
      return(OrderSend(m_request,m_result));*/
      if(!PositionSelectByTicket(ticket))
         return(false);
      m_trade.SetExpertMagicNumber(OrderMagicNumber());
      return m_trade.PositionClose(ticket, slippage);
   }
   
   bool OrderClosePartially(long ticket, double lots, double price, int slippage)
   {
      return m_trade.PositionClosePartial(ticket, lots, slippage);
   }
   
   /*double Normalize (double val, int digits)
   {
       CString numberString;
       numberString.Assign(StringFormat("%." + IntegerToString(digits) + "f", val));
       numberString.TrimRight(".0");
       //double valR = (int)double(MathRound(val)* MathPow(10, digits));
       //double result = val0 - valR;
       //result = result / MathPow(10, digits);
       //result = MathRound(val) + result;
       double result = StringToDouble(numberString.Str());
       return result;
   }*/
   
   bool OpenOrder(Order& order, int slipPage) 
   {
       //int err = ::GetLastError();
       //err = 0;
       int Color = 0;
       //bool exit_loop = false;
       //int cnt = 0;
       //int ticket = 0;
       //while (!exit_loop)
       //{
       
          int digits = Digits();
          order.lots = NormalizeDouble(order.lots, 2);                 
          order.setTakeProfit(NormalizeDouble(order.TakeProfit(false), digits)); 
          order.setStopLoss(NormalizeDouble(order.StopLoss(false), digits)); 
          //order.openPrice = Normalize(order.openPrice, digits);
          m_trade.SetExpertMagicNumber(order.magic);
          m_trade.SetDeviationInPoints(slipPage);
          if (order.type == OP_BUY) 
          {
             Color = Blue;
             order.openPrice = Utils.Ask();
             double check_volume=m_trade.CheckVolume(order.symbol,order.lots,order.openPrice,ORDER_TYPE_BUY);
             order.CheckSL();
             order.CheckTP();
             if(!m_trade.Buy(order.lots,order.symbol, order.openPrice,order.StopLoss(true),order.TakeProfit(true),order.comment))
             {  
                Utils.Info(StringFormat("ERROR BUY -> false. Result Retcode: %d, description of result:%d , ticket of deal: %d",
                        m_trade.ResultRetcode(),m_trade.ResultRetcodeDescription(), m_trade.ResultDeal()));
                delete &order;
                return false;
             }
             order.SetId((int)m_trade.ResultOrder());
             m_trade.PrintResult();
          }
          if (order.type == OP_SELL) 
          {
             Color = Red;
             order.openPrice = Utils.Bid();
             double check_volume=m_trade.CheckVolume(order.symbol,order.lots,order.openPrice,ORDER_TYPE_SELL);
             order.CheckSL();
             order.CheckTP();
             if(!m_trade.Sell(order.lots,order.symbol,order.openPrice,order.StopLoss(true),order.TakeProfit(true),order.comment))
             {   
                Utils.Info(StringFormat("ERROR SELL -> false. Result Retcode: %d , description of Retcode: %d , ticket of order: %d", 
                  m_trade.ResultRetcode(),m_trade.ResultRetcodeDescription(), m_trade.ResultOrder()));
                delete &order;
                return false;
             }
             order.SetId((int)m_trade.ResultOrder());
             //m_trade.PrintResult();
          }

          //order.PrintIfWrong("OpenOrder");
          //ticket = Utils._OrderSend(Symbol(), order.type, order.lots, order.openPrice, Slippage, order.stopLoss, order.takeProfit, order.comment, order.magic, order.expiration, Color);
          //LogInfo(order.ToString());
/*          err = ::GetLastError();
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
             if (Utils.SelectOrder(ticket))
             {
                order.ticket = ticket;
                globalOrders.Fill(order);
                globalOrders.Add(&order);
                if (!Utils.IsTesting())
                  SaveOrders(thrift.set);
                return &order;
             }
          }
          delete &order;
          Print(StringFormat("Still error after %d retries!!! %s", RetryOnErrorNumber , TimeToString(TimeCurrent(),TIME_MINUTES)));
          return NULL;
       }*/
       return true;
   }

   /*
   int  _OrderSend(string   symbol, int cmd, double volume, double price, int slippage, double stoploss, double takeprofit, string comm=NULL, int magic=0,datetime expiration=0, color arrow_color=clrNONE)
   {
      MqlTradeRequest  m_request;
      MqlTradeResult   m_result;   

      ZeroMemory(m_request);
      m_request.action   = TRADE_ACTION_DEAL;
      m_request.symbol   = symbol;
      m_request.magic    = magic; 
      m_request.volume   = volume;
      if (cmd == OP_BUY)
         m_request.type     = ORDER_TYPE_BUY;
      else if (cmd == OP_SELL)
         m_request.type     = ORDER_TYPE_SELL;
   
      m_request.price    = price;
      m_request.sl       = stoploss;
      m_request.tp       = takeprofit;
      m_request.deviation= slippage;
      m_request.comment=comm;
      m_request.expiration = expiration;
      m_trade.SetExpertMagicNumber(magic);

      //MqlTradeCheckResult checkResult; 
      //ZeroMemory(checkResult);
      
      //if (!m_trade.OrderCheck(m_request, checkResult))
      //   return -1;
         
      if (m_trade.OrderSend(m_request, m_result))
      {
          m_result
          return (int)m_result.order;
      }
      return -1;
   }
   */

   bool OrderModify(long  ticket, double price, double stoploss, double takeprofit, datetime expiration, color arrow_color)
   {
      if(!PositionSelectByTicket(ticket))
         return(false);
      m_trade.SetExpertMagicNumber(OrderMagicNumber());
      return m_trade.PositionModify(ticket, stoploss, takeprofit);
      //return m_trade.OrderModify(ticket, price, stoploss, takeprofit, ORDER_TIME_GTC, expiration);
      /*
      ZeroMemory(m_request);
      int cmd = this.OrderType();
      if (cmd == OP_BUY)
         m_request.type     = ORDER_TYPE_BUY;
      else if (cmd == OP_SELL)
         m_request.type     = ORDER_TYPE_SELL;
      m_request.action  = TRADE_ACTION_SLTP;
      m_request.symbol  = this.OrderSymbol();
      m_request.magic   = this.OrderMagicNumber();
      m_request.sl      = stoploss;
      m_request.tp      = takeprofit;
      m_request.position= ticket;
      m_request.expiration = expiration;
      return OrderSend(m_request,m_result);
      */
   }
   
////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////
double iATR(ENUM_TIMEFRAMES timeframe, int period, int shift)
{
    int handle = iATR(Symbol, timeframe, period);
    if (handle != INVALID_HANDLE)
    {
        double result[1];
        //ArrayResize(result, 1);
        //ArraySetAsSeries(result, true);
        if (CopyBuffer(handle, 0, shift, 1, result) > 0)
           return result[0];
    }
    return 0;
}

double iMA(ENUM_TIMEFRAMES timeframe, int ma_period, int ma_shift, ENUM_MA_METHOD ma_method, ENUM_APPLIED_PRICE applied_price, int shift)
{
    int handle = iMA(Symbol, timeframe, ma_period, ma_shift, ma_method, applied_price);
    if (handle != INVALID_HANDLE)
    {
        double result[1];
        if (CopyBuffer(handle, 0, shift, 1, result) > 0)
           return result[0];
    }
    return 0;
}

double iRSI(ENUM_TIMEFRAMES period, int ma_period, ENUM_APPLIED_PRICE  applied_price, int shift)
{
    int handle = iRSI(Symbol, period, ma_period, applied_price);
    if (handle != INVALID_HANDLE)
    {
        double result[1];
        if (CopyBuffer(handle, 0, shift, 1, result) > 0)
           return result[0];
    }
    return 0;
}

double iBands(ENUM_TIMEFRAMES period, int  bands_period, int  bands_shift, double  deviation, ENUM_APPLIED_PRICE  applied_price, int bufIndex, int shift)
{
    int handle = iBands(Symbol, period, bands_period, bands_shift, deviation, applied_price);
    if (handle != INVALID_HANDLE)
    {
        double result[1];
        if (CopyBuffer(handle, bufIndex, shift, 1, result) > 0)
           return result[0];
    }
    return 0;
}

   double iCustom(ENUM_TIMEFRAMES period, string name, int bufIndex, int shift)
   {
       int handle = iCustom(Symbol, period, name, bufIndex, shift);
       if (handle != INVALID_HANDLE)
       {
           double result[1];
           CopyBuffer(handle, bufIndex, shift, 1, result);
           IndicatorRelease(handle);    
           return result[0];
       }
       return 0;
   }
   
   virtual int GetIndicatorData(CIndicator& indi, int BuffIndex, int startPos, int Count, double &Buffer[])
   { 
       //ArraySetAsSeries(Buffer, true);
       return CopyBuffer(indi.Handle(), BuffIndex, startPos, Count, Buffer);
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
        if (CopyBuffer(indi.Handle(), BuffIndex, 0, numBars, Buff) > 0)
        {
           i = ArrayMinimum(Buff);
           if (i >= 0)
              Min = Buff[i];
           i = ArrayMaximum(Buff);
           if (i >= 0)
              Max = Buff[i];
           i = numBars - 1;   
           for (; i >= 0; i--)
           {
             value = Buff[i];
             aver += value;
           }
        } else {
            Utils.Info(StringFormat("CopyBuffer failed for Indicator: %s", indi.Name()));
            return false;
        }    
        //if (count == 0)
        //   return false;
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

   virtual int iCustomHandle(ENUM_TIMEFRAMES period, string name, int param1, int param2, int param3) 
   {
       if (param1 == -1)
         return iCustom(Symbol, period, name);       
       if (param2 == -1)
         return iCustom(Symbol, period, name, param1);       
       
       if (param3 == -1)
         return iCustom(Symbol, period, name, param1, param2);     
           
       return iCustom(Symbol, period, name, param1, param2, param3);       
   }
   

   void AddToChart(int Handle, string IndicatorName, long chartID, int subWin)
   {
       bool result = true;
       result = ChartIndicatorAdd(chartID,subWin,Handle);
   }
   
   int Bars() 
   {
     return (int)Utils.ChartFirstVisibleBar() +(int) ChartGetInteger(0, CHART_VISIBLE_BARS) ;
   }

};


#endif

