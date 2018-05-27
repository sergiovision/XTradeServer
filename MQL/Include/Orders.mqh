//+------------------------------------------------------------------+
//|                                                 Orders.mqh |
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <Arrays/List.mqh>

#define FOREACH_LIST(list) for(CObject* node = (list).GetFirstNode(); node != NULL; node = (list).GetNextNode())
#define FOREACH_ORDER(list) for(Order* order = (list).GetFirstNode(); order != NULL; order = (list).GetNextNode())

enum ENUM_ORDERROLE  
{
    RegularTrail, 
    GridHead, 
    GridTail,
    ShouldBeClosed
};

enum ENUM_TRAILING  
{
    TrailingDefault,
    TrailingByFractals,
    TrailingByShadows,
    TrailingRatchetB,
    TrailingStairs,
    TrailingByATR,
    TrailingByMA,
    TrailingUdavka,
    TrailingByTime,
    TrailingByPriceChannel,
    TrailingFiftyFifty,
    TrailingKillLoss
};

enum ENUM_MARKETSTATE  
{
    FlatTrend, 
    UpTrend, 
    DownTrend
};

//+------------------------------------------------------------------+
//| defines                                                          |
//+------------------------------------------------------------------+
/**
* filter defines
*/
#define FILTER_MARKET         1
#define FILTER_PENDING        2
#define FILTER_LONG           3
#define FILTER_SHORT          4
#define MYFILTER              5

/**
* sorting defines
*/
#define ORDER_TICKET          0
#define ORDER_TYPE            1
#define ORDER_MAGIC_NUMBER    2
#define ORDER_LOTS            3
#define ORDER_OPEN_PRICE      4
#define ORDER_CLOSE_PRICE     5
#define ORDER_OPEN_TIME       6
#define ORDER_CLOSE_TIME      7
#define ORDER_NET_PROFIT      8  // Profit + Swap + Commission
#define ORDER_STOP_LOSS       9
#define ORDER_TAKE_PROFIT     10

#define ASCENDING             -1
#define DESCENDING            1
#define NEWEST                DESCENDING
#define OLDEST                ASCENDING

//+------------------------------------------------------------------+
//| Order class                                                      |
//+------------------------------------------------------------------+

class Order : public CObject
{
   private:
      ENUM_ORDERROLE role;

   public:
   void Order(int Ticket) 
   {    
      ticket = Ticket;
      //if (Ticket == -1)
      //Print(StringFormat("Create order with ticket: %d",ticket));
      TrailingType = TrailingDefault;
      role = RegularTrail;
      bDirty = true; // By default Order unsync with broker and dirty.
      //Print(StringFormat("C-tor Order(%d) ", ticket)); 
   }
   
   //void ~Order()
   //{
      //Print(StringFormat("D-tor Order(%d) ", ticket)); 
   //}
   
   int ticket;
   int type;
   int magic;
   double   lots;
   double   openPrice;
   double   closePrice;
   datetime openTime;
   datetime closeTime;
   double   profit;
   double   swap;
   double   commission;
   double   stopLoss;
   double   takeProfit;
   datetime expiration;
   string comment;
   string symbol;
   
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
   
   void MarkToClose()
   {
      role = ShouldBeClosed;
   }
   
   double RealProfit() 
   {
      return OrderCommission() + OrderSwap() + OrderProfit();
   }
   
   bool isGridOrder()
   {
       return (role == GridHead) || (role == GridTail);
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
       int result = (ticket - order.ticket);
       return result;
   }
   
   bool Valid()
   {
      return (ticket != 0) && (ticket != -1);
   }
   
   bool Select()
   {
      return OrderSelect(ticket,SELECT_BY_TICKET);
   }
   
   bool NeedChanges(double sl, double tp, datetime expe)
   {
      if (sl != stopLoss)
         return true;
      if (tp != takeProfit)
         return true;
      if (expiration != expe)
         return true;
      return false;
   }
   
   string ToString()
   {
       //double currentPrice = 0;
       string orderTypeString = "";
       switch(type)
       {
          case OP_BUY:
             orderTypeString = "BUY";
             //currentPrice = Ask;
          break;
          case OP_BUYSTOP:
             orderTypeString = "BUYSTOP";
             //currentPrice = Ask;
          break;
          case OP_BUYLIMIT:
             orderTypeString = "BUYLIMIT";
             //currentPrice = Ask;
          break;
          case OP_SELL:
             orderTypeString = "SELL";
             //currentPrice = Bid;
          break;
          case OP_SELLLIMIT:
             orderTypeString = "SELLLIMIT";
             //currentPrice = Bid;
          break;
          case OP_SELLSTOP:
             orderTypeString = "SELLSTOP";
             //currentPrice = Bid;
          break;            
       }
       string result = StringFormat("Order%s(%d) %s OP:%f SL:%f TP:%f", EnumToString(role), ticket, orderTypeString, openPrice, stopLoss, takeProfit);
       return result;
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
      order.type = OrderType();
      order.magic = OrderMagicNumber();
      order.lots = OrderLots();
      order.openPrice = OrderOpenPrice();
      order.closePrice = OrderClosePrice();
      order.openTime = OrderOpenTime();
      order.closeTime = OrderCloseTime();
      order.profit = OrderProfit();
      order.swap = OrderSwap();
      order.commission = OrderCommission();
      order.stopLoss = OrderStopLoss();
      order.takeProfit = OrderTakeProfit();
      order.expiration = OrderExpiration();
      order.comment = OrderComment();
      order.symbol = OrderSymbol();
      order.bDirty = false;
   }
    
   void AddUpdateByTicket(int Ticket)
   {
      Order* oldOrder = SearchOrder(Ticket);
      if (oldOrder != NULL)
      {
         Fill(oldOrder);
      } else 
      {
         Order* order = new Order(Ticket);
         Fill(order);
         Add(order);
      }
   }
   
   void DeleteByTicket(int Ticket)
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
       for(i = 0; i < m_capacity;i++)
          m_array[i] = newarray[i];
       m_size = j;
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
         if (order.bDirty) {
            DeleteCurrent();
         }
      }    
      Sort();  
   }
   
   void MarkOrdersAsDirty()
   {
      FOREACH_ORDER(this)
      {
         order.bDirty = true;
      }
   }
   
  Order* SearchOrder(int ticket)
  {
       for (iterator=0;iterator<m_size;iterator++)
       {
          Order* order = m_array[iterator];
          if (order != NULL)
          { 
            if (order.ticket == ticket)
            {
                return order;
            }
          }
       }
       return NULL;
  }

   
   
};
   
