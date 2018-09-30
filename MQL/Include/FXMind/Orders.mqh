//+------------------------------------------------------------------+
//|                                                       Orders.mqh |
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <Arrays/List.mqh>
#include <FXMind/GenericTypes.mqh>
#include <FXMind/IUtils.mqh>

#define FOREACH_LIST(list) for(CObject* node = (list).GetFirstNode(); node != NULL; node = (list).GetNextNode())
#define FOREACH_ORDER(list) for(Order* order = (list).GetFirstNode(); order != NULL; order = (list).GetNextNode())

#define ASCENDING             -1
#define DESCENDING            1
#define NEWEST                DESCENDING
#define OLDEST                ASCENDING

//INPUT_VARIABLE(SLTPDivergence, double, 0.2)

#define SLTPDivergence    0.3

#define SL_LINE_STYLE STYLE_SOLID
#define TP_LINE_STYLE STYLE_SOLID

//+------------------------------------------------------------------+
//| Order class                                                      |
//+------------------------------------------------------------------+

class Order : public CObject
{
   protected:
      ENUM_ORDERROLE role;
      double   stopLoss;
      double   takeProfit;
      double   realStopLoss;
      double   realTakeProfit;
      long     ticket;
      
      color  slColor;
      color  tpColor;
      color  opColor;
      short SL_LINE_WIDTH; 
      short TP_LINE_WIDTH; 

   public:   

   void Order(long Ticket) 
   {    
      ticket = Ticket;
      TrailingType = TrailingDefault;
      role = RegularTrail;
      bDirty = true; // By default Order unsync with broker and dirty.
      signalName = "";
      //Print(StringFormat("C-tor Order(%d) ", ticket)); 
      
      slColor = clrRed;
      tpColor = clrOrange;
      opColor = clrGreen;
      SL_LINE_WIDTH = 2; 
      TP_LINE_WIDTH = 2; 
   }
   
   void ~Order()
   {
      // Print(StringFormat("D-tor Order(%d) ", ticket)); 
      Destroy();
   }
   
   void Destroy() 
   {
      if (Utils.Trade().AllowVStops())
      {
         string name = StringFormat("SLLINE_%s_%s:%d", TypeToString(), symbol, ticket);
         Utils.ObjDelete(name);
         name = StringFormat("TPLINE_%s_%s:%d", TypeToString(), symbol, ticket);
         Utils.ObjDelete(name);
      }
   }
   
   long Id() { return ticket; }
   
   void SetId(long t)
   {
      if (Utils.Trade().AllowVStops() && !isPending())
      {
         string name = StringFormat("SLLINE_%s_%s:%d", TypeToString(), symbol, ticket);
         Utils.ObjDelete(name);
         name = StringFormat("TPLINE_%s_%s:%d", TypeToString(), symbol, ticket);
         Utils.ObjDelete(name);
      }
      ticket = t;
      setStopLoss(stopLoss);
      setTakeProfit(takeProfit);
   }
   
   virtual void doSelect(bool v) 
   {
        
   }

   double StopLoss(bool real) 
   {
      if (real)
         return realStopLoss;
      return stopLoss;
   }   

   double TakeProfit(bool real) 
   {
      if (real)
         return realTakeProfit;
      return takeProfit;
   }
   
   void setStopLoss(double sl) 
   {
      stopLoss = sl;
      realStopLoss = sl;
      if (sl == 0) 
        return;      
      double slPoints = Utils.Trade().DefaultStopLoss() * Point() * SLTPDivergence;
      if (Utils.Trade().AllowRStops())
      {   
         if ( type == OP_BUY )
            realStopLoss = stopLoss - slPoints;
         if ( type == OP_SELL )
            realStopLoss = stopLoss + slPoints;
      } else {
         realStopLoss = 0;
      }          
      if (Utils.Trade().AllowVStops() && (ticket != -1) && !isPending())
      {
         string name = StringFormat("SLLINE_%s_%s:%d", TypeToString(), symbol, ticket);
         if (!Utils.ObjExist(name))
            HLineCreate(ChartID(),name,0,stopLoss,slColor,SL_LINE_STYLE,SL_LINE_WIDTH,false, true,false,0,name);
         else 
            HLineMove(ChartID(), name, stopLoss);
      }
   }
   
   void setTakeProfit(double tp)
   {
      takeProfit = tp;
      realTakeProfit = tp;
      if (tp == 0)
         return;
      double tpPoints = Utils.Trade().DefaultTakeProfit() * Point() * SLTPDivergence;
      if (Utils.Trade().AllowRStops())
      {   
         if ( type == OP_BUY )
            realTakeProfit = takeProfit + tpPoints;
         if ( type == OP_SELL )
            realTakeProfit = takeProfit - tpPoints;
      } else {
         realTakeProfit = 0;
      }
            
      if (Utils.Trade().AllowVStops() && (ticket != -1) && !isPending())
      {
         string name = StringFormat("TPLINE_%s_%s:%d", TypeToString(), symbol, ticket);
         if (!Utils.ObjExist(name))
            HLineCreate(ChartID(),name,0,takeProfit,tpColor,TP_LINE_STYLE,TP_LINE_WIDTH,false,true,false,0,name);
         else 
            HLineMove(ChartID(), name, takeProfit);
      }
   }
     
   int type;
   long magic;
   double   lots;
   double   openPrice;
   double   closePrice;
   datetime openTime;
   //datetime closeTime;
   double   profit;
   double   swap;
   double   commission;
   datetime expiration;
   string comment;
   string symbol;
   string signalName;
   
   // specific properties
   ENUM_TRAILING  TrailingType;
   bool bDirty;
   
   void SetRole(ENUM_ORDERROLE newrole)
   {
      if (role != ShouldBeClosed)
         role = newrole;
   }
   
   ENUM_ORDERROLE Role()
   {
      return role;
   }
   
   virtual void MarkToClose()
   {
      role = ShouldBeClosed;
   }
   
   double RealProfit() 
   {
      return Utils.OrderSwap() + Utils.OrderCommission() + Utils.OrderProfit();
   }
   
   double Profit() 
   {
      return this.commission + swap + profit;
   }
   
   
   double PriceDistanceInPoint()
   {
      double CheckPrice = 0;
      if (type == OP_BUY) 
      {
         CheckPrice = Utils.Ask();
         return  (openPrice - CheckPrice)/Point();
      }
      else 
      {
         CheckPrice = Utils.Bid();
         return (CheckPrice - openPrice)/Point();
      }
      return 0;
   }
         
   bool CheckSL()
   {
      if (stopLoss == 0)
        return true;
      double pd = 0;
      if (type == OP_BUY)
      {
          pd = openPrice - stopLoss;
      }
      if (type == OP_SELL)
      {
          pd = stopLoss - openPrice;
      }
      double slp = Utils.StopLevelPoints();
      if (slp > pd)
      {
         if (pd < 0)
            Utils.Info("Wrong STOP LOSS higher/lower than open price!");
         else 
            Utils.Info("Wrong STOP LOSS less than minimal stop level!");
         return false;  
      }
      
      return true;
   }
   bool CheckTP() 
   {
      if (takeProfit == 0)
         return 0;
      double pd = 0;
      if (type == OP_BUY)
      {
          pd = takeProfit - openPrice;
      }
      if (type == OP_SELL)
      {
          pd = openPrice - takeProfit;
      }
      double slp = Utils.StopLevelPoints();
      if (slp > pd)
      {
         if (pd < 0)
            Utils.Info("Wrong TAKE PROFIT higher/lower than open price!");
         else 
            Utils.Info("Wrong TAKE PROFIT less than minimal stop level!");
         return false;  
      }
      
      return true;
   }
   
   bool isGridOrder()
   {
       return (role == GridHead) || (role == GridTail);
   }

   bool isPending()
   {
       return (role == PendingLimit) || (role == PendingStop);
   }
   
   virtual int Compare(const CObject *node, const int mode=0) const 
   {
       if (node == NULL)
       {
          Print(StringFormat("Null value passed to Order comparison %d", ticket));
          return -1;
       }
       //int _direction = ASCENDING;
       //Order* order = (Order*)node;
       const Order* order = dynamic_cast<const Order*>(node);
       if (order == NULL)
       {
          Print(StringFormat("Unable to Cast Order pointer for comparison %d", ticket));
          return -1;
       }
       long result = (ticket - order.ticket);
       return (int)result;
   }
   
   bool Valid()
   {
      return (ticket != 0) && (ticket != -1);
   }
   
   bool Select()
   {
      bool Sel = Utils.SelectOrder(ticket);
      return Sel;
   }
   
   bool NeedChanges(double sl, double tp, datetime expe, int trailingIndent)
   {      
      double Pt = trailingIndent * Point();
      if (MathAbs(sl-stopLoss)>Pt) 
         return true;
      if (MathAbs(tp-takeProfit)>Pt) 
         return true;
      if (expiration != expe)
         return true;
      return false;
   }
   
   string TypeToString()
   {
       switch(type)
       {
          case OP_BUY:
             return "BUY";
          break;
          case OP_BUYSTOP:
             return "BUYSTOP";
          break;
          case OP_BUYLIMIT:
             return "BUYLIMIT";
          break;
          case OP_SELL:
             return "SELL";
          break;
          case OP_SELLLIMIT:
             return "SELLLIMIT";
          break;
          case OP_SELLSTOP:
             return "SELLSTOP";
          break;           
       }
       Utils.Info("Error order type not set");
       return "NO_TYPE";
   }
   
   string ToString()
   {
       double currentPrice = 0;
       string orderTypeString = TypeToString();
       int sl_points = 0;
       int tp_points = 0;
       double pt = Point();
       switch(type)
       {
          case OP_BUY:
             currentPrice = Utils.Ask();
          break;
          case OP_BUYSTOP:
             currentPrice = Utils.Ask();
          break;
          case OP_BUYLIMIT:
             currentPrice = Utils.Ask();
          break;
          case OP_SELL:
             currentPrice = Utils.Bid();
          break;
          case OP_SELLLIMIT:
             currentPrice = Utils.Bid();
          break;
          case OP_SELLSTOP:
             currentPrice = Utils.Bid();
          break;            
       }
       if (stopLoss != 0.0)
         sl_points = (int)MathRound(MathAbs(openPrice-stopLoss)/pt);
       
       if (takeProfit != 0.0)
         tp_points = (int)MathRound(MathAbs(openPrice-takeProfit)/pt);
         
       string signalStr = StringSubstr(signalName, 0, 15);

       string result = StringFormat("%s %s(%d) %g OP=%g SL=%d TP=%d %s", orderTypeString, EnumToString(role), ticket, lots, openPrice, sl_points, tp_points, signalStr);
       return result;
   }
   
   string OrderSection()
   {
      return StringFormat("ORDER_%d", ticket);
   }
   
   static string OrderSection(long Ticket)
   {
      return StringFormat("ORDER_%d", Ticket);
   }

   
   bool IsWrong()
   {
      if ((stopLoss == 0.0) && (takeProfit == 0.0))
      {
          return false;
      }
      if ((type == OP_BUY) || (type == OP_BUYLIMIT) || (type == OP_BUYSTOP))
      {
          if ((openPrice >= takeProfit) || (openPrice <= stopLoss))
             return true;
      }    
      if ((type == OP_SELL) || (type == OP_SELLLIMIT) || (type == OP_SELLSTOP))
      {
          if ((openPrice <= takeProfit) || (openPrice >= stopLoss))             
            return true;
      }    
      return false;
   }
   
   void PrintIfWrong(string scope)
   {
      if (IsWrong())
         Print(scope + " Wrong " + ToString());
   }
   
   void Print()
   {
       Print(ToString());
   }
   
   static void OrderMessage(Order* order, string message)
   {
      if (order != NULL)
         Utils.Info(StringFormat("Order (%s) %d Created SUCCESSFULLY", message, order.ticket));
      else    
         Utils.Info(StringFormat("Order (%s) Creation FAILED", message) );
   }
   
   // Graphical objects
   //+------------------------------------------------------------------+
//| Create the horizontal line                                       |
//+------------------------------------------------------------------+
bool HLineCreate(const long            chart_ID=0,// chart's ID
                 const string          name="HLine_max",// line name
                 const int             sub_window=0,// subwindow index
                 double                hprice=0,// line price
                 const color           clr=clrRed,        // line color
                 const ENUM_LINE_STYLE style=STYLE_SOLID, // line style
                 const int             width=1,           // line width
                 const bool            back=false,        // in the background
                 const bool            selection=true,    // highlight to move
                 const bool            hidden=true,       // hidden in the object list
                 const long            z_order=0,         // priority for mouse click
                 const string          tooltip="")
  {
   ObjectDelete(chart_ID,name);
//--- reset the error value
   ResetLastError();
//--- create a horizontal line
   if(!ObjectCreate(chart_ID,name,OBJ_HLINE,sub_window,0,hprice))
     {
      Print(__FUNCTION__,
            ": failed to create a horizontal line! Error code = ",GetLastError());
      return(false);
     }
//--- set line color
   ObjectSetInteger(chart_ID,name,OBJPROP_COLOR,clr);
//--- set line display style
   ObjectSetInteger(chart_ID,name,OBJPROP_STYLE,style);
//--- set line width
   ObjectSetInteger(chart_ID,name,OBJPROP_WIDTH,width);
//--- display in the foreground (false) or background (true)
   ObjectSetInteger(chart_ID,name,OBJPROP_BACK,back);
//--- enable (true) or disable (false) the mode of moving the line by mouse
//--- when creating a graphical object using ObjectCreate function, the object cannot be
//--- highlighted and moved by default. Inside this method, selection parameter
//--- is true by default making it possible to highlight and move the object
   ObjectSetInteger(chart_ID,name,OBJPROP_SELECTABLE,selection);
   ObjectSetInteger(chart_ID,name,OBJPROP_SELECTED,selection);
//--- hide (true) or display (false) graphical object name in the object list
   ObjectSetInteger(chart_ID,name,OBJPROP_HIDDEN,hidden);
//--- set the priority for receiving the event of a mouse click in the chart
   ObjectSetInteger(chart_ID,name,OBJPROP_ZORDER,z_order);

   ObjectSetString(chart_ID,name,OBJPROP_TOOLTIP,tooltip);
//--- successful execution
   return(true);
  }
//+------------------------------------------------------------------+
//| Move horizontal line                                             |
//+------------------------------------------------------------------+
bool HLineMove(const long   chart_ID=0,   // chart's ID
               const string name="HLine", // line name
               double       pricel=0) // line price
  {
//--- if the line price is not set, move it to the current Bid price level
   if(!pricel)
      pricel=SymbolInfoDouble(Symbol(),SYMBOL_BID);
//--- reset the error value
   ResetLastError();
//--- move a horizontal line
   if(!ObjectMove(chart_ID,name,0,0,pricel))
     {
      Print(__FUNCTION__,
            ": failed to move the horizontal line! Error code = ",GetLastError());
      return(false);
     }
//--- successful execution
   return(true);
  }

};
  
//+------------------------------------------------------------------+
//| OrderSelection class                                             |
//+------------------------------------------------------------------+
/**
* Container for a selection of orders
*/

class OrderSelection //: public CList
{
   protected:
      Order            *m_array[];
      int              iterator;
      int              m_capacity;
      int              m_size;


   public:
   void OrderSelection(int capacity) // constructor for container
   {
      ArrayResize(m_array, capacity);
      m_capacity = ArraySize(m_array);
      m_size = 0;
      //Print("C-tor OrderSelection"); 
   }

   void DeleteCurrent()
   {
       Order* order = m_array[iterator];
       if (order != NULL)
       {
          delete order;
          m_array[iterator] = NULL;
          //m_size--;
          //iterator++;
          //if (iterator >= m_capacity)
          //  iterator = 0;
       }
   }
   
   Order *GetFirstNode()
   {
      iterator = 0;
      if (m_size==0)
         return NULL;
      return m_array[iterator];
   }

   Order *GetNextNode()
   {
      if (m_size==0)
         return NULL;
      iterator++;
      if (iterator < m_size)
         return m_array[iterator];
      return NULL; // reach the end of array
   }
   
   Order* GetNodeAtIndex(int index)
   {
       if ((m_size==0) || (index >= m_size))
          return NULL;
       iterator = index;
       return m_array[iterator];
   }
   
   void Clear()
   {
       for (int i=0;i<m_capacity;i++){
          Order* order = m_array[i];
          if (order != NULL)
          {
            delete order;
            m_array[i] = NULL;
          }
       }
       m_size = 0;
       iterator = 0;
   }
   
   
   void Add(Order* order)
   {
       while(m_array[iterator]!=NULL)
       {
           iterator++;
       }
       if (iterator >= m_capacity)
          Print(StringFormat("Can't add order to collection. Limit %d reached!", m_size));
       m_array[iterator] = order;
       m_size++;
   }

   void ~OrderSelection()  // destructor frees the memory
   {
      Clear();
      //Print("D-tor ~OrderSelection");
   }
   
   void Fill(Order &order)
   {
      //order.ticket = OrderTicket();
      order.type = Utils.OrderType();
      order.magic = Utils.OrderMagicNumber();
      order.lots = Utils.OrderLots();
      order.openPrice = Utils.OrderOpenPrice();
      //order.closePrice = Utils.OrderClosePrice();
      order.openTime = Utils.OrderOpenTime();
      //order.closeTime = Utils.OrderCloseTime();
      order.profit = Utils.OrderProfit();
      order.swap = Utils.OrderSwap();
      order.commission = Utils.OrderCommission();
      //order.stopLoss = Utils.OrderStopLoss();
      //order.takeProfit = Utils.OrderTakeProfit();
      order.expiration = Utils.OrderExpiration();
      order.comment = Utils.OrderComment();
      order.symbol = Utils.OrderSymbol();
      order.bDirty = false;
   }
    
   void AddUpdateByTicket(long Ticket)
   {
      Order* oldOrder = SearchOrder(Ticket);
      if (oldOrder != NULL)
      {
         Fill(oldOrder);
         //oldOrder.setStopLoss(oldOrder.StopLoss(false));
         //oldOrder.setTakeProfit(oldOrder.TakeProfit(false));
      } else 
      {
         Order* order = new Order(Ticket);
         Fill(order);
         Add(order);
         //order.setStopLoss(order.StopLoss(false));
         //order.setTakeProfit(order.TakeProfit(false));
      }
   }
   
   void DeleteByTicket(long Ticket)
   {
      Order* foundOrder = SearchOrder(Ticket);
      if (foundOrder != NULL)
      {
         DeleteCurrent();
         Sort();
      } else 
          Print(StringFormat("Order with this ticket %d doesn't exist", Ticket));
   }
   
   
   void Sort()
   {
       int i = 0;
       Order* newarray[];
       ArrayResize(newarray, m_capacity);
       Order* order = NULL;
       int j = 0;
       for(i = 0; i < m_capacity; i++)
       {
           order = m_array[i];
           if (order != NULL)
           {
               newarray[j] = order;
               j++;
           }
       }
       // reinit all m_array with NULL;
       for(i = 0; i < m_capacity; i++)
       {
          m_array[i] = NULL;
       }
       m_size = j;
       for(i = 0; i < m_size;i++)
          m_array[i] = newarray[i];
       iterator = 0;
   }
   
   int Total() 
   {
       return m_size;
   }
   
   int Capacity() 
   {
       return m_capacity;
   }
   
   void RemoveDirtyObsoleteOrders()
   {
      FOREACH_ORDER(this)
      {
         if (order.bDirty && (!order.isPending())) {            
            DeleteCurrent();
         }
      }    
      Sort();  
   }
   
   void MarkOrdersAsDirty()
   {
      FOREACH_ORDER(this)
      {
         if (!order.isPending())
            order.bDirty = true;
      }
   }
   
  Order* SearchOrder(long ticket)
  {
       for (iterator=0;iterator<m_size;iterator++)
       {
          Order* order = m_array[iterator];
          if (order != NULL)
          { 
            if (order.Id() == ticket)
            {
                return order;
            }
          }
       }
       return NULL;
  }

   
};
   
