#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\IUtils.mqh>
#include <Trade\Trade.mqh>
#include <FXMind\Orders.mqh>

#ifdef __MQL5__

class MT5Utils: public IUtils
{
protected:
   string Symbol;
public:
   MT5Utils()
   {   
       Symbol = Symbol();
       m_symbol.Name(Symbol);                  // sets symbol name
       m_symbol.Refresh();
   }
   
   bool IsMQL5()
   {
      return true;
   }
   
   double Ask()
   {
      return SymbolInfoDouble(Symbol(),SYMBOL_ASK);

   }
   
   double Bid()
   {
      return SymbolInfoDouble(Symbol(),SYMBOL_BID);
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
      if(!m_symbol.RefreshRates())
         return(false);
      //--- protection against the return value of "zero"
      if(m_symbol.Ask()==0 || m_symbol.Bid()==0)
         return(false);
      //---
      return(true);
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
   
   
   CTrade m_trade;
   CSymbolInfo    m_symbol;    

   
   bool  _OrderClose(int ticket,double lots, double price, int slippage, color arrow_color)
   {
      /*if(!PositionSelectByTicket(ticket))
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
   
   bool _OrderClosePartially(int ticket, double lots, double price, int slippage)
   {
      return m_trade.PositionClosePartial(ticket, lots, slippage);
   }
   
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
          order.takeProfit = NormalizeDouble(order.takeProfit, digits); 
          order.stopLoss = NormalizeDouble(order.stopLoss, digits);
          order.openPrice = NormalizeDouble(order.openPrice, digits);
          m_trade.SetExpertMagicNumber(order.magic);
          m_trade.SetDeviationInPoints(slipPage);
          if (order.type == OP_BUY) 
          {
             Color = Blue;
             order.openPrice = Utils.Ask();
             double check_volume=m_trade.CheckVolume(order.symbol,order.lots,order.openPrice,ORDER_TYPE_BUY);
             if(!m_trade.Buy(order.lots,order.symbol, order.openPrice,order.stopLoss,order.takeProfit,order.comment))
             {  
                Print("ERROR BUY -> false. Result Retcode: ",m_trade.ResultRetcode(),
                        ", description of result: ",m_trade.ResultRetcodeDescription(),
                        ", ticket of deal: ",m_trade.ResultDeal());
                delete &order;
                return false;
             }
             order.ticket = (int)m_trade.ResultOrder();
             m_trade.PrintResult();
          }
          if (order.type == OP_SELL) 
          {
             Color = Red;
             order.openPrice = Utils.Bid();
             double check_volume=m_trade.CheckVolume(order.symbol,order.lots,order.openPrice,ORDER_TYPE_SELL);
             if(!m_trade.Sell(order.lots,order.symbol,order.openPrice,order.stopLoss,order.takeProfit,order.comment))
             {   
                Print("ERROR SELL -> false. Result Retcode: ",m_trade.ResultRetcode(),
                  ", description of Retcode: ",m_trade.ResultRetcodeDescription(),
                  ", ticket of order: ",m_trade.ResultOrder());
                delete &order;
                return false;
             }
             order.ticket = (int)m_trade.ResultOrder();
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

   /*int  _OrderSend(string   symbol, int cmd, double volume, double price, int slippage, double stoploss, double takeprofit, string comm=NULL, int magic=0,datetime expiration=0, color arrow_color=clrNONE)
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

   bool _OrderModify(int  ticket, double price, double stoploss, double takeprofit, datetime expiration, color arrow_color)
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
      return OrderSend(m_request,m_result);*/
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