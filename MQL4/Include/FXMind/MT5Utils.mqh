#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\IUtils.mqh>

#ifdef __MQL5__

class MT5Utils: public IUtils
{
protected:
   string Symbol;
public:
   MT5Utils()
   {   
       Symbol = Symbol();
   }
   
   bool IsMQL5()
   {
      return true;
   }

   datetime CurrentTimeOnTF()
   {
      datetime Time[];
      int count = 2;   // number of elements to copy
      ArraySetAsSeries(Time,true);
      CopyTime(_Symbol,_Period,0,count,Time);
      return Time[0];
   }
   
   bool SelectOrder(int ticket)
   {
      return PositionSelectByTicket(ticket);
   }
   
   bool SelectOrderByPos(int Position)
   {
      return StringCompare("", PositionGetSymbol(Position)) != NULL;
   }
   
   int OrderTicket()
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
      return PositionGetString(POSITION_SYMBOL);
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
      return (int)PositionGetInteger(POSITION_MAGIC);
   }
   
   double OrderLots()
   {
      return (int)PositionGetDouble(POSITION_VOLUME);   
   }
   
   double OrderOpenPrice()
   {
      return (int)PositionGetDouble(POSITION_PRICE_OPEN);   
   }
   
   double OrderStopLoss()
   {
      return (int)PositionGetDouble(POSITION_SL);   
   }
   
   double OrderTakeProfit()
   {
      return (int)PositionGetDouble(POSITION_TP);   
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
      MqlTick tick;
      return SymbolInfoTick(Symbol(), tick);
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
      return (int)SymbolInfoInteger(Symbol(), SYMBOL_TRADE_STOPS_LEVEL);
   }
   
   int OrdersTotal()
   {
       return PositionsTotal();
   }
   
   double StopLevelPoints()
   {
      return StopLevel()*Point();
   }
   
   
   MqlTradeRequest  m_request;
   MqlTradeResult   m_result;   
   bool  _OrderClose(int ticket,double lots, double price, int slippage, color arrow_color)
   {
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
      return(OrderSend(m_request,m_result));
   }

   int  _OrderSend(string   symbol, int cmd, double volume, double price, int slippage, double stoploss, double takeprofit, string comm=NULL, int magic=0,datetime expiration=0, color arrow_color=clrNONE)
   {
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
      if (OrderSend(m_request,m_result))
      {
          return (int)m_result.order;
      }
      return -1;
   }

   bool  _OrderModify(int  ticket, double price, double stoploss, double takeprofit, datetime expiration, color arrow_color)
   {
      if(!PositionSelectByTicket(ticket))
         return(false);
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
   }
   
////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////
double iATR(ENUM_TIMEFRAMES timeframe, int period, int shift)
{
    int handle = iATR(Symbol, timeframe, period);
    if (handle != INVALID_HANDLE)
    {
        double result[1];
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
        if (CopyBuffer(handle, bufIndex, shift, 1, result) > 0)
           return result[0];
    }
    return 0;
}

};


#endif