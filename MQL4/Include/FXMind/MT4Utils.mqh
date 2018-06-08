#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\IUtils.mqh>

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

   int  _OrderSend(string   symbol, int cmd, double volume, double price, int slippage, double stoploss, double takeprofit, string comm=NULL, int magic=0,datetime expiration=0, color arrow_color=clrNONE)
   {
      return ::OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit, comm, magic, expiration, arrow_color);
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
