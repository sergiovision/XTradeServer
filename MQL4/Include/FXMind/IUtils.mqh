//+------------------------------------------------------------------+
//|                                                       IUtils.mqh |
//|                        Copyright 2018, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\InitMQL.mqh>

interface IUtils
{
   bool IsMQL5();
   datetime CurrentTimeOnTF();
   bool SelectOrder(int ticket);
   bool SelectOrderByPos(int Positiong);
   
   int OrdersTotal();
   
   long GetAccountNumer();
   double OrderSwap();
   string OrderSymbol();
   string OrderComment();
   double OrderProfit();
   double OrderCommission();
   int    OrderType();
   int    OrderMagicNumber();   
   double OrderLots();
   double OrderOpenPrice();
   datetime OrderOpenTime();
   //datetime OrderCloseTime();
   double OrderStopLoss();
   double OrderTakeProfit();
   datetime OrderExpiration();
   int OrderTicket();
   bool IsTesting();
   bool IsVisualMode();
   bool RefreshRates();
   double AccountBalance();
   
   //////////////////////////
   int Spread();
   int StopLevel();
   double StopLevelPoints();   
   
   int TimeMinute(datetime date);
   bool  _OrderClose(int ticket,double lots, double price, int slippage, color arrow_color);
   int  _OrderSend(string   symbol, int cmd, double volume, double price, int slippage, double stoploss, double takeprofit, string comment=NULL, int magic=0,datetime expiration=0, color arrow_color=clrNONE);
   bool  _OrderModify(int  ticket, double price, double stoploss, double takeprofit, datetime expiration, color arrow_color);
     
   /// Indicators  
   double iATR(ENUM_TIMEFRAMES timeframe, int period, int shift);
   double iMA(ENUM_TIMEFRAMES timeframe, int ma_period, int ma_shift, ENUM_MA_METHOD ma_method, ENUM_APPLIED_PRICE applied_price, int shift);
   double iRSI(ENUM_TIMEFRAMES period, int ma_period, ENUM_APPLIED_PRICE  applied_price, int shift);
   double iBands(ENUM_TIMEFRAMES period, int  bands_period, int  bands_shift, double  deviation, ENUM_APPLIED_PRICE  applied_price, int bufIndex, int shift);
   double iCustom(ENUM_TIMEFRAMES period, string name, int bufIndex, int shift);
};


static IUtils* Utils = NULL;
IUtils* GetUtils()
{
    return Utils;
}

