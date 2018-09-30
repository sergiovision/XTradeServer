//+------------------------------------------------------------------+
//|                                                       IUtils.mqh |
//|                        Copyright 2018, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+

#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\InitMQL.mqh>

class OrderSelection;
class Order;

class ITrade 
{
public:

   virtual double CalculateLotSize(int op_type, bool GridLot = false) = 0;
   virtual int ATROnIndicator(double rank) = 0;
   virtual int DefaultStopLoss() = 0;
   virtual int DefaultTakeProfit() = 0;
   virtual double StopLoss(double price, int op_type) = 0;
   virtual double TakeProfit(double price, int op_type) = 0;
   virtual OrderSelection* Orders() = 0; 
   virtual void SaveOrders() = 0;
   virtual int GetGridStepValue() = 0;
   virtual long CloseOrderPartially(Order& order, double newLotSize) = 0;
   virtual int GetMartinMultiplier() = 0;
   virtual bool AllowVStops() = 0;
   virtual bool AllowRStops() = 0;
};
