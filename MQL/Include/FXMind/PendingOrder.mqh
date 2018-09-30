//+------------------------------------------------------------------+
//|                                                 ThriftClient.mqh |
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict


#define ADD_MINUTES 68
#define PENDING_BUY_TICKET  -2
#define PENDING_SELL_TICKET -3

#include <FXMind\GenericTypes.mqh>
#include <FXMind\Orders.mqh>
#include <FXMind\PendingOrders.mqh>

#include <ChartObjects\ChartObjectsShapes.mqh>
#include <ChartObjects\ChartObjectsBmpControls.mqh>

class PendingOrders;

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class PendingOrder : public Order
{
protected:
   double defaultPriceShift;
   CChartObjectRectangle rectReward;
   CChartObjectRectangle rectRisk;
   CChartObjectBitmap OP_arrow;
   PendingOrders* parent;
   datetime shiftTime;
public:
   static string idRewardRectBUY;
   static string idRiskRectBUY;
   static string idRewardRectSELL;
   static string idRiskRectSELL;
   static string idPendingSELL;
   static string idPendingBUY;
   PendingOrder(int type, double lots, PendingOrders* parent, long t);
   void Drag( const long &lparam,const double &dparam, const string &sparam);
   void ShiftUp();
   void ShiftDown();
   virtual void MarkToClose();
   virtual void SetOPLine();
   
   void rectUpdate(datetime startTime);
   bool isSelected() { return OP_arrow.Selected(); }
   virtual void doSelect(bool v) 
   { 
      setStopLoss(stopLoss);
      setTakeProfit(takeProfit);
      OP_arrow.Selected(v);
   }
   void InitLoaded();

   ~PendingOrder();
};

string PendingOrder::idPendingSELL = "OrderPending_SELL";
string PendingOrder::idPendingBUY = "OrderPending_BUY";   

string PendingOrder::idRewardRectBUY = "rewardRect_BUY";
string PendingOrder::idRewardRectSELL = "rewardRect_SELL";

string PendingOrder::idRiskRectBUY = "riskRect_BUY";
string PendingOrder::idRiskRectSELL = "riskRect_SELL";

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
PendingOrder::PendingOrder(int typ, double lot, PendingOrders* p, long t)
   :Order(-1)
{
   slColor = clrBlue;
   tpColor = clrLightBlue;
   opColor = clrGray;
   SL_LINE_WIDTH = 1; 
   TP_LINE_WIDTH = 1; 

   symbol = Utils.Symbol;
   ticket = t;
   bDirty = false;
   parent = p;
   type = typ;
   SetRole(PendingLimit);
   lots = lot;
   string id = idPendingBUY;
   double ask = Utils.Ask();
   double bid = Utils.Bid();
   shiftTime = 5 * ADD_MINUTES * PeriodSeconds() / 60;
   if (type == OP_BUY)
   {
      ticket = PENDING_BUY_TICKET;
      defaultPriceShift = Utils.Trade().DefaultStopLoss()*Point();
      openPrice = ask - defaultPriceShift;
      stopLoss = Utils.Trade().StopLoss(openPrice, type);
      takeProfit = Utils.Trade().TakeProfit(openPrice, type);
      id = idPendingBUY;
   }
   else if (type == OP_SELL)
   {
      ticket = PENDING_SELL_TICKET;
      defaultPriceShift = Utils.Trade().DefaultStopLoss()*Point();
      openPrice = bid + defaultPriceShift;
      stopLoss = Utils.Trade().StopLoss(openPrice, type);
      takeProfit = Utils.Trade().TakeProfit(openPrice, type);
      id = idPendingSELL;
   }   
   datetime currentTime = TimeCurrent();
   datetime AddTime = currentTime + shiftTime;
   
   OP_arrow.Create(parent.chartID, id, parent.SubWindow, AddTime, openPrice);   
   
   if (type == OP_BUY)
   {
      OP_arrow.BmpFile("/Images/buy.bmp");
   }
   
   if (type == OP_SELL)
   {
      OP_arrow.BmpFile("/Images/sell.bmp");
   }
   
   rectUpdate(AddTime);
   
   OP_arrow.Selectable(true);
   OP_arrow.Selected(true);
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void PendingOrder::InitLoaded(void)
{
   datetime startTime = 0;
   OP_arrow.GetInteger(OBJPROP_TIME,0,startTime);

   OP_arrow.SetDouble(OBJPROP_PRICE, openPrice);
   rectReward.Delete();
   rectRisk.Delete();
   rectUpdate(startTime);
}

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
PendingOrder::~PendingOrder()
{
   OP_arrow.Delete();
   rectReward.Delete();
   rectRisk.Delete();   
   if (Utils.Trade().AllowVStops())
   {
      string name = StringFormat("OPLINE_%s_%s:%d", TypeToString(), symbol, ticket);
      Utils.ObjDelete(name);
   }
}
//+------------------------------------------------------------------+
void PendingOrder::Drag( const long &lparam,const double &dparam, const string &sparam)
{
/*
   int       x      = parent.X;
   int       y      = parent.Y;
   datetime  dt     = 0;
   double    price  = 0;
   datetime  startTime;
   
   //--- Convert the X and Y coordinates in terms of date/time
   if (!ChartXYToTimePrice(parent.chartID, x, y, parent.SubWindow, startTime, price))
   {
      return;
   }
   
   openPrice = price;
   OP_arrow.SetDouble(OBJPROP_PRICE, price);
   stopLoss = parent.methods.StopLoss(price, type);
   takeProfit = parent.methods.TakeProfit(price, type);
   rectReward.Delete();
   rectRisk.Delete();
   rectUpdate(startTime, price);
   ChartRedraw(parent.chartID);   
*/
}

void PendingOrder::SetOPLine(void)
{
   if (Utils.Trade().AllowVStops() && (ticket != -1))
   {
      string name = StringFormat("OPLINE_%s_%s:%d", TypeToString(), symbol, ticket);
      if (!Utils.ObjExist(name))
         HLineCreate(ChartID(),name,0,openPrice,opColor,SL_LINE_STYLE,TP_LINE_WIDTH,false,false,false,0,name);
      else
         HLineMove(ChartID(), name, openPrice);
   }

}

void PendingOrder::ShiftUp()
{
   datetime startTime = 0;
   OP_arrow.GetInteger(OBJPROP_TIME,0,startTime);

   double shift = MathMax( PendingOrderStep, Utils.Spread())*Point();
   openPrice += shift;
   OP_arrow.SetDouble(OBJPROP_PRICE, openPrice);
   stopLoss = Utils.Trade().StopLoss(openPrice, type);
   takeProfit = Utils.Trade().TakeProfit(openPrice, type);
   rectReward.Delete();
   rectRisk.Delete();
   rectUpdate(startTime);
   ChartRedraw(parent.chartID);   
}

void PendingOrder::ShiftDown()
{
   datetime startTime = 0;
   OP_arrow.GetInteger(OBJPROP_TIME, 0, startTime);

   double shift = MathMax(PendingOrderStep, Utils.Spread())*Point();
   openPrice -=  shift;
   OP_arrow.SetDouble(OBJPROP_PRICE, openPrice);
   stopLoss = Utils.Trade().StopLoss(openPrice, type);
   takeProfit = Utils.Trade().TakeProfit(openPrice, type);
   rectReward.Delete();
   rectRisk.Delete();
   rectUpdate(startTime);
   ChartRedraw(parent.chartID);
}

void PendingOrder::MarkToClose()
{
   role = ShouldBeClosed;
   if (type == OP_BUY)
   {
      parent.pendingBUY = NULL;
   }
   if (type == OP_SELL)
   {
      parent.pendingSELL = NULL;
   }
}

void PendingOrder::rectUpdate(datetime AddTime)
{
   double ask = Utils.Ask();
   double bid = Utils.Bid();
   if (type == OP_BUY)
   {
      if (openPrice < bid)
         role = PendingLimit;
      if (openPrice > ask)
         role = PendingStop;          
      
      datetime  endTime = AddTime + shiftTime; 

      rectReward.Create(parent.chartID, idRewardRectBUY,parent.SubWindow,AddTime,openPrice,endTime,takeProfit);
      rectReward.Color(LightGreen);
      rectReward.Background(true);
   
      rectRisk.Create(parent.chartID,idRiskRectBUY,parent.SubWindow,AddTime,stopLoss,endTime,openPrice);
      rectRisk.Color(LightPink);
      rectRisk.Background(true);
      ObjectSetInteger(parent.chartID,idPendingBUY, OBJPROP_BACK, 1);

   }
   
   if (type == OP_SELL)
   {      
      if (openPrice > ask)
         role = PendingLimit;
      if (openPrice < bid)
         role = PendingStop;

      datetime  endTime = AddTime + shiftTime;

      rectReward.Create(parent.chartID,idRewardRectSELL,parent.SubWindow,AddTime,openPrice,endTime,takeProfit);
      rectReward.Color(LightGreen);
      rectReward.Background(true);

      rectRisk.Create(parent.chartID,idRiskRectSELL,parent.SubWindow,AddTime,stopLoss,endTime,openPrice);
      rectRisk.Color(LightPink);
      rectRisk.Background(true);
      ObjectSetInteger(parent.chartID,idPendingSELL, OBJPROP_BACK, 1);
   }
   SetOPLine();

}

