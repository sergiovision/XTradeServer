//+------------------------------------------------------------------+
//|                                                 ThriftClient.mqh |
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\GenericTypes.mqh>
#include <FXMind\Orders.mqh>
#include <FXMind\ITrade.mqh>
#include <FXMind\InputTypes.mqh>
#include <FXMind\PendingOrder.mqh>

#include <ChartObjects\ChartObjectsShapes.mqh>
#include <ChartObjects\ChartObjectsBmpControls.mqh>


class PendingOrder;
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class PendingOrders
{
protected:
   // RIDs
   string switchOrderTypeButtonID;   
   //////////////////////////////////////////////////
   // PendingOrders
   datetime           currentTime;
   int                pending_optype;      
   
public:
   PendingOrder*      pendingBUY;
   PendingOrder*      pendingSELL;
   long chartID;
   int SubWindow;
   //int X, Y;
   PendingOrders(long chart, int subWin);
   ~PendingOrders();
   void DeleteBUY();
   void DeleteSELL();
   void   CreatePendingOrderBUY(double lots);
   void   CreatePendingOrderSELL(double lots);
   void   rectUpdate(Order* order, datetime startTime, double price);
   void   LoadOrders(SettingsFile* set);

   void   Init();
   void   OnEvent(const int id,
                           const long &lparam,
                           const double &dparam,
                           const string &sparam);
};

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
PendingOrders::PendingOrders(long chart, int subWin)
{    
   chartID = chart;     
   SubWindow = subWin; 
     
   pendingBUY = NULL;
   pendingSELL = NULL;
   
   switchOrderTypeButtonID = "switchOrderTypeButton";
}

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
PendingOrders::~PendingOrders()
{
   //DeleteBUY();
   //DeleteSELL();
}

void PendingOrders::DeleteBUY() 
{
   if (pendingBUY != NULL)
   {
      pendingBUY.SetRole(ShouldBeClosed);
      SettingsFile* set = Utils.Service().Settings();
      if (set != NULL)
      {
         int role = (int)History;
         set.SetParam(pendingBUY.OrderSection(), "role", role);
      }
      // DELETE_PTR(pendingBUY);
      pendingBUY = NULL;
   }
}

void PendingOrders::DeleteSELL()
{
   if (pendingSELL != NULL)
   {
      pendingSELL.SetRole(ShouldBeClosed);
      SettingsFile* set = Utils.Service().Settings();
      if (set != NULL)
      {
         int role = (int)History;
         set.SetParam(pendingSELL.OrderSection(), "role", role);
      }
      //DELETE_PTR(pendingSELL);
      pendingSELL = NULL;
   }
}

void PendingOrders::LoadOrders(SettingsFile* set)
{
   if (set != NULL)
   {
      if (set.OrderSectionExist(PENDING_BUY_TICKET, Order::OrderSection(PENDING_BUY_TICKET)))
      {
         PendingOrder* order = new PendingOrder(OP_BUY, 0.01, GetPointer(this), PENDING_BUY_TICKET);
         string orderSection = order.OrderSection();
         set.GetIniKey(orderSection, "lots", order.lots);
         long t = order.Id();
         set.GetIniKey(orderSection, "ticket", t);
         order.SetId(t);
         set.GetIniKey(orderSection, "openPrice", order.openPrice);
         int role = 0;
         set.GetIniKey(orderSection, "role", role);
         order.SetRole((ENUM_ORDERROLE)role);
         int tt = 0;
         set.GetIniKey(orderSection, "TrailingType", tt);
         order.TrailingType = (ENUM_TRAILING)tt;
         double sl = 0;
         set.GetIniKey(orderSection, "stopLoss", sl);
         order.setStopLoss(sl);
         double tp = 0;         
         set.GetIniKey(orderSection, "takeProfit", tp);
         order.setTakeProfit(tp);
         pendingBUY = order;
         Utils.Trade().Orders().Add(pendingBUY);
         pendingBUY.InitLoaded();
         pendingBUY.doSelect(false);
         if (!Utils.IsTesting())
            Print(StringFormat("Order %s %d restored successfully ", EnumToString(order.Role()), order.Id()));
      }
      if (set.OrderSectionExist(PENDING_SELL_TICKET, Order::OrderSection(PENDING_SELL_TICKET)))
      {
         PendingOrder* order = new PendingOrder(OP_SELL, 0.01, GetPointer(this), PENDING_SELL_TICKET);
         string orderSection = order.OrderSection();
         set.GetIniKey(orderSection, "lots", order.lots);
         long t = order.Id();
         set.GetIniKey(orderSection, "ticket", t);
         order.SetId(t);
         set.GetIniKey(orderSection, "openPrice", order.openPrice);
         int role = 0;
         set.GetIniKey(orderSection, "role", role);
         order.SetRole((ENUM_ORDERROLE)role);
         int tt = 0;
         set.GetIniKey(orderSection, "TrailingType", tt);
         order.TrailingType = (ENUM_TRAILING)tt;
         double sl = 0;
         set.GetIniKey(orderSection, "stopLoss", sl);
         order.setStopLoss(sl);
         double tp = 0;
         set.GetIniKey(orderSection, "takeProfit", tp);
         order.setTakeProfit(tp);
         pendingSELL = order;
         Utils.Trade().Orders().Add(pendingSELL);
         pendingSELL.InitLoaded();
         pendingSELL.doSelect(false);
         if (!Utils.IsTesting())
            Print(StringFormat("Order %s %d restored successfully ", EnumToString(order.Role()), order.Id()));
      }
   }
}

//+------------------------------------------------------------------+
void PendingOrders::Init()
{
}

void PendingOrders::CreatePendingOrderBUY(double lots)
{
   if (pendingBUY != NULL) 
   {
       Utils.Info("Pending BUY already exists. Skipping.");
       return;
   }
      
   pendingBUY = new PendingOrder(OP_BUY, lots, GetPointer(this), PENDING_BUY_TICKET);
   Utils.Trade().Orders().Add(pendingBUY);
   ChartRedraw(chartID); 
}

void PendingOrders::CreatePendingOrderSELL(double lots)
{
   if (pendingSELL != NULL)
   {
      Utils.Info("Pending SELL already exists. Skipping.");
      return;
   }
   pendingSELL = new PendingOrder(OP_SELL, lots, GetPointer(this), PENDING_SELL_TICKET);
   Utils.Trade().Orders().Add(pendingSELL);
   ChartRedraw(chartID); 
}

void PendingOrders::OnEvent(const int id,
                           const long &lparam,
                           const double &dparam,
                           const string &sparam)
{

/*
  if ( id == CHARTEVENT_CLICK  )
  {
     X = (int)lparam;
     Y = (int)dparam;
  }
  if ( id == CHARTEVENT_MOUSE_MOVE  )
  {
     X = (int)lparam;
     Y = (int)dparam;
  }
*/

  if ( id == CHARTEVENT_OBJECT_DRAG )
  {
      if ( (StringCompare(sparam,PendingOrder::idPendingSELL,0)==0)
             && (pendingSELL != NULL))
      {
         pendingSELL.Drag(lparam, dparam, sparam);
      }
            
      if ( (StringCompare(sparam,PendingOrder::idPendingBUY,0)==0) 
             && (pendingBUY != NULL))
      {
         pendingBUY.Drag(lparam, dparam, sparam);
      }
  }
  
  if ( id == CHARTEVENT_KEYDOWN  )
  {
      switch(int(lparam))
      {
         
         case KEY_NUMLOCK_UP:    
         case KEY_UP:  
            if (pendingSELL != NULL)
            {
               if (pendingSELL.isSelected())
               {
                 pendingSELL.ShiftUp();
               }
            }
            if (pendingBUY != NULL)
            {
               if (pendingBUY.isSelected())
               {
                 pendingBUY.ShiftUp();
               }
            }
          
         
         break;
         
         case KEY_NUMLOCK_DOWN:  
         case KEY_DOWN:     
            if (pendingSELL != NULL)
            {
               if (pendingSELL.isSelected())
               {
                 pendingSELL.ShiftDown();
               }
            }
            if (pendingBUY != NULL)
            {
               if (pendingBUY.isSelected())
               {
                 pendingBUY.ShiftDown();
               }
            }
         break;
         case KEY_NUMLOCK_RIGHT: 
         case KEY_RIGHT:         
         break;
         case KEY_NUMLOCK_LEFT:  
         case KEY_LEFT:          
         break;
         
         default:     
            ;
      }       
      if ((lparam == 'u') || (lparam == 'U'))
      {
         if (pendingSELL != NULL)
         {
            pendingSELL.doSelect(false);
            if (!Utils.IsTesting())
               Utils.Trade().SaveOrders();

            ChartRedraw(chartID);
         }
         if (pendingBUY != NULL)
         {
            pendingBUY.doSelect(false);
            if (!Utils.IsTesting())
               Utils.Trade().SaveOrders();
            ChartRedraw(chartID);
         }
      }

      if ((lparam == 'd') || (lparam == 'D'))
      {
         if (pendingSELL != NULL)
         {
            if (pendingSELL.isSelected())
            {
               DeleteSELL();
               ChartRedraw(chartID);
            }
         }
         if (pendingBUY != NULL)
         {
            if (pendingBUY.isSelected())
            {
               DeleteBUY();
               ChartRedraw(chartID);
            }
         }
      }
  }
}
