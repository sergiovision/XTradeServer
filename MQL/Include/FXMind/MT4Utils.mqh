#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\IUtils.mqh>
#include <FXMind\Orders.mqh>

#ifdef __MQL4__

class MT4Utils: public IUtils
{
   protected:
   string Symbol;
public:
   MT4Utils()
   {   
       Symbol = ::Symbol();
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
   
   bool SelectOrder(int ticket)
   {
      return ::OrderSelect(ticket, SELECT_BY_TICKET);
   }
   
   long GetAccountNumer()
   {
      return ::AccountNumber();
   }
   
   bool SelectOrderByPos(int Position)
   {
      return ::OrderSelect(Position, SELECT_BY_POS);
   }
   
   int OrderTicket()
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
       return (int)SymbolInfoInteger(Symbol(), SYMBOL_SPREAD);
   }

   int StopLevel()
   {
      return (int)SymbolInfoInteger(Symbol(), SYMBOL_TRADE_STOPS_LEVEL);
   }
   
   int OrdersTotal()
   {
       return ::OrdersTotal();
   }
   
   double StopLevelPoints()
   {
      return StopLevel()*Point();
   }
      
   bool  _OrderClose(int ticket,double lots, double price, int slippage, color arrow_color)
   {
      return ::OrderClose(ticket, lots, price, slippage, arrow_color);
   }
   
   bool  _OrderClosePartially(int ticket, double lots, double price, int slippage)
   {
      return ::OrderClose(ticket, lots, price, slippage);
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
          ticket = ::OrderSend(order.symbol, order.type, order.lots, order.openPrice, slipPage, order.stopLoss, order.takeProfit, order.comment, order.magic, order.expiration, Color);
          
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
   
   bool  _OrderModify(int  ticket, double price, double stoploss, double takeprofit, datetime expiration, color arrow_color)
   {
      return ::OrderModify(ticket, price, stoploss, takeprofit, expiration, arrow_color);
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

};

#endif
