//+------------------------------------------------------------------+
//|                                                  TrendOnIndi.mq4 |
//|                                 Copyright 2013, Sergei Zhuravlev |
//|                        https://www.facebook.com/sergei.zhuravlev |
//+------------------------------------------------------------------+
#property copyright "Copyright 2013, Sergei Zhuravlev"
#property link      "https://www.facebook.com/sergei.zhuravlev"
#property strict
#include <stdlib.mqh>
#include <ThriftClient.mqh>

extern double Lots = 0.1;
extern int   Magic = 123456;
extern ushort ThriftPORT = 2010;
extern string comment = "GridTrendOnIndi";
extern bool  AllowBUY = true;
extern bool  AllowSELL = true;
extern int   TakeProfitLevel = 10;
extern int   RetryOnErrorNumber = 5;
extern int   MaxOpenedTrades = 10;
extern int   Slippage = 10;
// Grid data
//--------------------------------------------------------------------
extern int  GridStep = 65;
extern double GridMultiplier = 2.0;
extern int  GridProfit = 25;
extern bool TrailGridHead = false;
extern bool MartinLotsCalc = false;
// Stop Trailing data
//--------------------------------------------------------------------
extern int  TrailingStop     = 0;   // trail by fractals or candles
extern bool TrailByPSAR = true;
extern int  TrailingTimeFrame = 0;
//--------------------------------------------------------------------
// Indicators 
extern bool  EnableEMAWMA = false;
extern bool  EnableBillWilliams = false;
extern bool  EnableBBands = false;
extern bool  EnableNewsSignal = false;
extern bool  EnableSentimentsLotSize = false;
extern int   IndicatorTimeFrame = PERIOD_H1;
//--------------------------------------------------------------------
// News Params
extern int RaiseSignalBeforeEventMinutes = 30;
extern int NewsPeriodMinutes = 200;
extern ushort MinImportance = 1;
extern bool RevertNewsTrend = true;

//--------------------------------------------------------------------
// private properties
int   NumBarsFractals = 100;
//extern int   ExpiredOrderDays = 12;

int grid_count = 0;
int grid_optype = -1;
int head_grid_ticket = -1;
datetime TIME;
string globalComment = "";
ThriftClient* thrift = NULL;
int currentImportance = MinImportance;
int prevIndiSignal = 0;
bool EventRaiseSoon = false;
bool InNewsPeriod = false;
datetime timeNewsPeriodStarted;
string labelEventString;
void CreateTextLabel(string msg, int Importance, datetime raisetime) 
{
   if (StringCompare(labelEventString, msg) ==0)
         return;
   labelEventString = msg;
   Print( " Upcoming: " + msg );
   if (!IsTesting() || IsVisualMode()) {
      string name = "newsevent" + MathRand();
      ObjectCreate(name,OBJ_TEXT,0,raisetime,High[0]);
      ObjectSetString(0, name,OBJPROP_TEXT,msg);
      ObjectSet(name,OBJPROP_ANGLE,90);
      color clr = clrNONE;
      switch(Importance) {
          case -1:
          case 1:
          clr = clrOrange;
          break;
          case -2:
          case 2:
          clr = clrRed;
          break;
          default:
             clr = clrGray;
          break;
      }
      ObjectSet(name,OBJPROP_COLOR,clr);
   }
}
//--------------------------------------------------------------------
int GetNewsSignal(int indiSignal)
{   
   if (!EnableNewsSignal)
      return indiSignal;
   if (indiSignal != 0)
      prevIndiSignal = indiSignal;
   string message = "no event";
   datetime raiseDateTime;
   string raisedStr = " Upcoming in ";
   int signal = thrift.GetNextNewsEvent(Symbol(), MinImportance, message, raiseDateTime);
   if (signal > 0) {
      EventRaiseSoon = true;
      currentImportance = signal;
   }
   int minsRemained = (raiseDateTime - TimeCurrent())/60;
   if (InNewsPeriod) {
      int minsNewsPeriod = (TimeCurrent() - timeNewsPeriodStarted)/60;
      if (minsNewsPeriod >=NewsPeriodMinutes)
         InNewsPeriod = false;
   }
   if (!IsTesting() || IsVisualMode())
   {
      //bool isActive = thrift.isActive();
      //globalComment += "News Signal Active: " + isActive + "\n";
      globalComment += "In News period: " + InNewsPeriod + "\n";
      string trendString = "Netral";
      if (prevIndiSignal < 0)
         trendString = "Sell";
      else if (prevIndiSignal > 0)
              trendString = "Buy";
      if (minsRemained < 0)
         raisedStr = " Passed " + IntegerToString(-1*minsRemained) + " min ago: ";
      else
         raisedStr += IntegerToString(minsRemained) + " min: ";
        
      globalComment += "Trend: " + trendString + prevIndiSignal + raisedStr + message + "\n";
   }
   if (EventRaiseSoon && (minsRemained >= 0) && (minsRemained <= RaiseSignalBeforeEventMinutes))
   {
      CreateTextLabel(message, currentImportance, raiseDateTime);
      //EventRaiseSoon = false;
      int coef = 1;
      if (RevertNewsTrend)
         coef = -1;
      InNewsPeriod = true;
      timeNewsPeriodStarted = TimeCurrent();
      return currentImportance * prevIndiSignal * coef;
   }
   return 0;
}
//--------------------------------------------------------------------
double longPos = -1;
double shortPos = -1;
void ShowGlobalSentiments()  {
   if (!IsTesting() || IsVisualMode())
   {
      if (thrift != NULL) 
      {
         string symbolName = Symbol();
         double longVal = -1;
         double shortVal = -1;
         if (thrift.GetCurrentSentiments(symbolName, longVal, shortVal) != 0)
         {
            longPos = NormalizeDouble(longVal, 2);
            shortPos = NormalizeDouble(shortVal, 2);
         }   
         globalComment += "Sentiments " + symbolName + ": Buying("  + DoubleToString(longPos, 2) + ") Selling (" + DoubleToString(shortPos, 2) + ")\n";
      }
   }
}
//+------------------------------------------------------------------+
int GetBWSignal()
{
   int signal = 0;
   double isBuy = iCustom(NULL,IndicatorTimeFrame,"BillWilliams_ATZ",   0,0);
   if (isBuy!=0)
      return ++signal;
   double isSell = iCustom(NULL,IndicatorTimeFrame,"BillWilliams_ATZ",   1,0);
   if (isSell!=0)
      return --signal;
   return (signal);   
}

//+------------------------------------------------------------------+
int GetEMAWMASignal()
{
   int     signal = 0;
   int     period_EMA           = 28;
   int     period_WMA           = 8;
   int     period_RSI           = 14;
               
   double EMA0 = iMA(NULL,IndicatorTimeFrame,period_EMA,0,MODE_EMA, PRICE_OPEN,0);
   double WMA0 = iMA(NULL,IndicatorTimeFrame,period_WMA,0,MODE_LWMA,PRICE_OPEN,0);
   double EMA1 = iMA(NULL,IndicatorTimeFrame,period_EMA,0,MODE_EMA, PRICE_OPEN,1);
   double WMA1 = iMA(NULL,IndicatorTimeFrame,period_WMA,0,MODE_LWMA,PRICE_OPEN,1);
   double RSI  = iRSI(NULL,IndicatorTimeFrame,period_RSI,PRICE_OPEN,0);
   //double MFI  = iMFI(NULL,PERIOD_H1,period_RSI,0);
   
   if (EMA0 < WMA0 && EMA1 > WMA1 && RSI >= 50)
      return ++signal;
      
   if (EMA0 > WMA0 && EMA1 < WMA1 && RSI <= 50)
      return --signal;
 
   return (signal);   
}

//+------------------------------------------------------------------+
int GetBandsSignal()
{
   int signal = 0;

   double isBuy = iBands(NULL, IndicatorTimeFrame, 20, 2, 0, PRICE_LOW, MODE_LOWER, 0); 
   if (isBuy > Ask)
      return ++signal;
      
   double isSell = iBands(NULL, IndicatorTimeFrame, 20, 2, 0, PRICE_HIGH, MODE_UPPER, 0); 
   if (isSell < Bid)
      return --signal;
   return (signal);   
}

//+------------------------------------------------------------------+
int GetSignalOperationType()
{
   int signal = 0;
   if (EnableBBands)
      signal += GetBandsSignal();
   if (EnableBillWilliams)
     signal += GetBWSignal();
   if (EnableEMAWMA)
      signal += GetEMAWMASignal();

   if (EnableNewsSignal)
   {
      signal = GetNewsSignal(signal);
   }
   
   if (signal > 0)
      return (OP_BUY);
   if (signal < 0)
      return (OP_SELL);
   return (-1);
}

//+------------------------------------------------------------------+
//| expert start function                                            |
//+------------------------------------------------------------------+
void OnTick()
{  	
   globalComment = "";
   AddSpreadComment();
   if (ProcessOrders())
   {
      DoTrailing();
   }
   ShowComment();
}

//+------------------------------------------------------------------+
void AddSpreadComment()
{
   if (!IsTesting() || IsVisualMode()) {
      double spread = MarketInfo(Symbol(), MODE_SPREAD);
      if (Digits == 5 || Digits == 3)
      {
         spread = spread / 10;
      }
      globalComment = globalComment + "Current spread: " + spread + "\n";
   }
}
//+------------------------------------------------------------------+
void ShowComment() {
   if (!IsTesting() || IsVisualMode()) {
      ShowGlobalSentiments();
      Comment(globalComment);
   }
}
//+------------------------------------------------------------------+
//| expert initialization function                                   |
//+------------------------------------------------------------------+
int OnInit()
{

   thrift = new ThriftClient(AccountNumber(), ThriftPORT, Magic);
   
   string initMessage = "OnInit GridTrendOnIndi Magic: " + IntegerToString(Magic) + " Digits: " + Digits;
   Print(initMessage);
   thrift.PostMessage(initMessage);
  	  	
  	//double longVal = 0;
  	//double shortVal = 0;
   //thrift.GetCurrentSentiments(Symbol(), longVal, shortVal);
   //Print(Symbol() + " Long: " + longVal + ", ShortVal: " + shortVal);

   if ( Digits == 3 || Digits == 5 )
   {
      Slippage *= 10;
      TakeProfitLevel *= 10;
      GridStep *= 10;
   }
   
   grid_count = 0;
   head_grid_ticket = -1;
   globalComment = "";
   
   EventRaiseSoon = false;
   InNewsPeriod = false;
   timeNewsPeriodStarted = TimeCurrent();
   return (INIT_SUCCEEDED);
}
//+------------------------------------------------------------------+
//| expert deinitialization function                                 |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
{
   //TrailingFinalize();
   if (thrift != NULL)
      delete thrift;
}

int OpenOrder(int op_type, double LotSize) 
{
   if (op_type < 0 || op_type > 2)
      return -1;
   if (CountTrades() >= MaxOpenedTrades)
      return -1;
   if ((AllowBUY == false)&& op_type==OP_BUY)
      return -1; 
   if ((AllowSELL == false)&& op_type==OP_SELL)
      return -1; 
    int err = ::GetLastError();
    err = 0;
    double openPrice = 0;
    int Color = 0;
    if (op_type == OP_BUY) 
    {
       Color = Blue;
       openPrice = Ask;
    }
    if (op_type == OP_SELL) 
    {
       Color = Red;
       openPrice = Bid;
    }
    LotSize = NormalizeDouble(LotSize, 2);
    bool exit_loop = false;
    int cnt = 0;
    int ticket = 0;
    while (!exit_loop) 
    {
       ticket = OrderSend(Symbol(), op_type, LotSize, openPrice, Slippage, 0, 0, comment, Magic, 0, Color);
       err = ::GetLastError();
       switch (err)
       {
          case ERR_NO_ERROR:
             exit_loop = true;
          break;
          case ERR_SERVER_BUSY:
          case ERR_BROKER_BUSY:
          case ERR_TRADE_CONTEXT_BUSY:
             cnt++;
          break;
          case ERR_INVALID_PRICE:
          case ERR_PRICE_CHANGED:
          case ERR_OFF_QUOTES:
          case ERR_REQUOTE:
             RefreshRates();
             continue;
          break;
          default:
             exit_loop = true;
       }   
       if (cnt > RetryOnErrorNumber )
          exit_loop = true;
          
       if ( !exit_loop )
       {
          if (!IsTesting())
             Sleep(1000);
          RefreshRates();
       }
       else 
       {
          if (err != ERR_NO_ERROR) 
          {
             globalComment += "^^^^Error Opening Order ticket:  " + ticket + ", error#: " + err + "\n";
          }
       }
       if (err == ERR_NO_ERROR) 
       {
          if (OrderSelect( ticket, SELECT_BY_TICKET, MODE_TRADES))
             return (ticket);
       }
       string err_str = "Still error after " + RetryOnErrorNumber + " retries!!! " + TimeToStr(TimeCurrent(),TIME_MINUTES);
       globalComment += err_str;
       Print(err_str);
       return -1;
    }
    return ticket;
}
//+------------------------------------------------------------------+
bool ChangeOrder(int ticket, double price, double stoploss, double takeprofit, datetime expiration, color arrow_color)
{
   if (OrderSelect(ticket, SELECT_BY_TICKET))
   {
      price = NormalizeDouble(price, Digits);
      stoploss = NormalizeDouble(stoploss, Digits);
      takeprofit = NormalizeDouble(takeprofit, Digits);
      CheckValidStop(Symbol(), price, stoploss);
      int err = ::GetLastError();
      err = 0;
      bool result = false;
      bool exit_loop = false;
      int cnt = 0;
      while (!exit_loop)
      {
         result = OrderModify(ticket, price, stoploss, takeprofit, expiration, arrow_color);
         if (result == true)
            return true;
         err = ::GetLastError();
         switch (err)
         {
            case ERR_NO_ERROR:
               exit_loop = true;
            break;
            case ERR_SERVER_BUSY:
            case ERR_BROKER_BUSY:
            case ERR_TRADE_CONTEXT_BUSY:
               cnt++;
            break;
            case ERR_INVALID_PRICE:
            case ERR_PRICE_CHANGED:
            case ERR_OFF_QUOTES:
            case ERR_REQUOTE:
               RefreshRates();
               continue;
            break;
            default:
             exit_loop = true;
         }
         if (cnt > RetryOnErrorNumber )
            exit_loop = true;
         
         if ( !exit_loop )
         {
            if (!IsTesting())
               Sleep(1000);
            RefreshRates();
         }
         else 
         {
            if (err != ERR_NO_ERROR) 
            {
               Print("^^^^^^^^^^Error OrderModify ticket: " + ticket + ", error#: " + err);
            }
         }
      }
      return result;
   }
   else 
   {
      Print("Unable to select Order ticket: " + ticket);
      return false;
   }
}
//+------------------------------------------------------------------+
void CheckValidStop(string symbol, double price, double& sl) 
{
   if (sl == 0)
      return;
   double servers_min_stop = MarketInfo(symbol, MODE_STOPLEVEL) * MarketInfo(symbol, MODE_POINT);
   if (MathAbs(price - sl) <= servers_min_stop)
   {
       if ( price > sl)
          sl = price - servers_min_stop;
       else 
          sl = sl + servers_min_stop;
   }   
   sl = NormalizeDouble(sl, (int)MarketInfo(symbol, MODE_DIGITS));
}
//+------------------------------------------------------------------+
//+------------------------------------------------------------------+
bool CloseOrder(int ticket, double lots, double price, int slippage, color arrow_color)
{
   if (OrderSelect(ticket, SELECT_BY_TICKET))
   {
      price = NormalizeDouble(price, Digits);
      int err = ::GetLastError();
      err = 0;
      bool result = false;
      bool exit_loop = false;
      int cnt = 0;
      while (!exit_loop)
      {
         lots = NormalizeDouble(lots, 2);
         price = NormalizeDouble(price, Digits);
         result = OrderClose(ticket, lots, price, slippage, arrow_color);
         if (result == true) 
         {
            if (!IsTesting())
            {
               Sleep(100);
            }
            return true;        
         } 
         err = ::GetLastError();
         switch (err)
         {
            case ERR_NO_ERROR:
               exit_loop = true;
            break;
            case ERR_SERVER_BUSY:
            case ERR_BROKER_BUSY:
            case ERR_TRADE_CONTEXT_BUSY:
               cnt++;
            break;
            case ERR_INVALID_PRICE:
            case ERR_PRICE_CHANGED:
            case ERR_OFF_QUOTES:
            case ERR_REQUOTE:
               RefreshRates();
               continue;
            break;
            default:
             exit_loop = true;
         }
         if (cnt > RetryOnErrorNumber )
            exit_loop = true;
         
         if ( !exit_loop )
         {
            if (!IsTesting())
               Sleep(1000);
            RefreshRates();
         }
         else 
         {
            if (err != ERR_NO_ERROR) 
            {
               Print("^^^^^^^^^^Error Close Order ticket: " + ticket + ", error#: " + err);
            }
         }
      }
      return result;
   }
   else 
   {
      Print("Unable to Close Order by ticket: " + ticket);
      return false;
   }
}
//+------------------------------------------------------------------+

double CalculateLotSize(int op_type, double prevLot)
{
    double sentimentsMultiplier = 1;
    if (EnableSentimentsLotSize) 
    {
       string symbolName = Symbol();
       double longVal = -1;
       double shortVal = -1;
       thrift.GetCurrentSentiments(symbolName, longVal, shortVal);
       if ((longVal > 0) && (op_type == OP_BUY))
       {
//          if (longVal > 75)
//            sentimentsMultiplier = 2;
          if (longVal > 83)
            sentimentsMultiplier = GridMultiplier;
       }
       if ((shortVal > 0) && (op_type == OP_SELL))
       {
//        if (shortVal > 75)
//            sentimentsMultiplier = 2;
          if (shortVal > 83)
            sentimentsMultiplier = GridMultiplier;
       }
    }
    double lotSize = Lots * sentimentsMultiplier;
    if ( op_type == grid_optype )
    {
       if (MartinLotsCalc)
       {
          lotSize = prevLot * GridMultiplier;
       }
       else 
         lotSize = Lots * GridMultiplier * sentimentsMultiplier; 
    } 
    if ( EnableSentimentsLotSize && (sentimentsMultiplier > 1) )
       Print ( ">>>>>>>>>>>>>>> Increase multiplier: " + lotSize);
    return lotSize;     
}

//--------------------------------------------------------------------
double CalcOrderRealProfit() 
{
   return OrderCommission()+OrderSwap()+OrderProfit();
}
//+------------------------------------------------------------------+
int CountTradesByType(int op_type) 
{
    int count = 0;
    for (int i = OrdersTotal()-1; i>=0;i--)
    {
       if (OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
       {
           if (OrderSymbol() == Symbol() && OrderMagicNumber() == Magic )
           {
               if (OrderType() == op_type)
                  count++;
           } 
       }
    }
    return(count);
}
//+------------------------------------------------------------------+
int CountTrades() 
{
    int count = 0;
    for (int i = OrdersTotal()-1; i>=0;i--)
    {
       if (OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
       {
           if (OrderSymbol() == Symbol() && OrderMagicNumber() == Magic )
           {
               if (OrderType() == OP_BUY || OrderType() == OP_SELL)
                  count++;
           } 
       }
    }
    return(count);
}

int searchNewTicket(int oldTicket)
{
   for(int i=OrdersTotal()-1; i>=0; i--)
      if(OrderSelect(i,SELECT_BY_POS) && 
         StrToInteger(StringSubstr(OrderComment(),StringFind(OrderComment(),"#")+1)) == oldTicket )
         return (OrderTicket());
   return (-1);
}

int CloseMyOrder(int Ticket)
{
   if (grid_optype == OrderType())
      if (head_grid_ticket != Ticket)
         return -1;
      else 
         if (TrailGridHead == false)
              return -1;
         
   double lots = OrderLots();
//   if (lots < Lots && grid_count > 1)
//      return -1;   
   double partLot = NormalizeDouble(Lots/2.0, 2);
   if (lots > partLot)
      partLot = NormalizeDouble(lots/2.0,2);
   double closePrice = 0;
   if (OP_BUY == OrderType())
   {
      closePrice = Bid;
   }
   else 
   {
      closePrice = Ask;
   }
   if (CloseOrder(Ticket, partLot, closePrice, Slippage, Yellow)) 
   {
      int newticket = searchNewTicket(Ticket);
      return newticket;
   } else 
     return -1;
}

bool JustCloseOrder(int Ticket) 
{
   double lots = OrderLots();
   Print("!!!Close expired order: " + Ticket);
   double closePrice = 0;
   if (OP_BUY == OrderType())
      closePrice = Bid;
   else 
      closePrice = Ask;
   return CloseOrder(Ticket, lots, closePrice, Slippage, Yellow); 
}

bool ProcessOrders()
{
   int countBUY = CountTradesByType(OP_BUY);
   int countSELL = CountTradesByType(OP_SELL);
   grid_count = MathMax(countBUY, countSELL);
   if (grid_count <= 1) 
   {
      grid_optype = -1;
      head_grid_ticket = -1;
   }
   double Profit = 0;
   double CheckPrice = 0;
   double LossLevel = 0;
   double orderProfit = 0;
   int isbuy = 0;
   int tip = 0;
   int i = 0;
   int grid_ticket = -1;
   double lotSizeOfHead = 0;
   datetime HeadOpenTime;
   for (i=0; i<OrdersTotal(); i++)
   {
      if (OrderSelect(i, SELECT_BY_POS))
      {  
         tip = OrderType();
         //int secDuration = TimeCurrent() - OrderOpenTime();
         //if ( (secDuration/86400) > ExpiredOrderDays) {
         //    JustCloseOrder(OrderTicket());
         //    return false; 
         //}
         if ((OrderSymbol()==Symbol()) && (OrderMagicNumber()==Magic))
         {
            orderProfit = CalcOrderRealProfit();
            if ((head_grid_ticket == -1) && (orderProfit < 0) && (grid_count == 1))
            {
               // this is a first order to start build grid
               if (tip == OP_BUY) 
               {
                  CheckPrice = Ask;
               } 
               else 
               {
                  CheckPrice = Bid;
               }
               LossLevel = MathAbs( CheckPrice - OrderOpenPrice() )/Point;
               if ( LossLevel >= GridStep ) { 
                  grid_optype = tip;
                  double lotsize = CalculateLotSize(grid_optype, OrderLots());
                  grid_ticket = OpenOrder(tip, lotsize);
                  if (grid_ticket != -1)
                  {
                     head_grid_ticket = grid_ticket;
                     Print("!!!Grid Started Ticket: " + grid_ticket);
                     return true;
                  }
               }
            }
            if ((head_grid_ticket == -1) && (grid_count >1)) { // refind grid head if lost
               double currentLots = OrderLots();
               datetime currentOpenTime = OrderOpenTime();
               if ( (currentLots > lotSizeOfHead) || ((currentLots == lotSizeOfHead) && (HeadOpenTime < currentOpenTime))) {
                  lotSizeOfHead = currentLots;
                  HeadOpenTime = currentOpenTime;
                  head_grid_ticket = OrderTicket();
                  grid_optype = tip;
               }
            }
            if ( (head_grid_ticket != -1 ) && (tip == grid_optype) )
            {
               Profit += orderProfit;
            }
         } 
      }
   }
   // CLOSING GRID
   if ((Profit >= GridProfit) && (grid_optype != -1 ) && (head_grid_ticket != -1)) {
      double closePrice = 0;
      Print("*******Close Grid!!! count: " + grid_count + " *********");
      i = 0;
      while (i < OrdersTotal())
      {
         if (OrderSelect(i, SELECT_BY_POS))
         {  
            tip = OrderType();
            if ( (tip == grid_optype) && (OrderSymbol()==Symbol()) && (OrderMagicNumber()==Magic) )
            {
               if (OP_BUY == tip) {
                  closePrice = Bid;
               }
               else {
                  closePrice = Ask;
               }
               if (TrailGridHead && (head_grid_ticket==OrderTicket()) )
                  continue;
               if (CloseOrder(OrderTicket(), OrderLots(), closePrice, Slippage, clrMediumSpringGreen))
               {
                  i = 0;
                  continue;
               }
            }
            i++;
         }
      }
      grid_optype = -1;
      head_grid_ticket = -1;
      return false;
   }

   // STARTING POINT FOR OPENING ORDERS
   int op_type = GetSignalOperationType(); 
   double TP = 0;
   int ticket = -1;
   if ((op_type == OP_BUY) && (countBUY == 0) && (grid_optype == -1) ) 
   {
      ticket = OpenOrder(op_type, Lots);
      if (ticket != -1) {
         TP = NormalizeDouble(Bid + TakeProfitLevel* Point, Digits);
         OrderModify(ticket, OrderOpenPrice(), 0, TP, 0);        
         EventRaiseSoon = false;
         return true;
      }
   }
   if ((op_type == OP_SELL) && (countSELL == 0) && (grid_optype == -1)) 
   {
      ticket = OpenOrder(op_type, Lots);
      if (ticket != -1) {
         TP = NormalizeDouble(Bid - TakeProfitLevel* Point, Digits);
         OrderModify(ticket, OrderOpenPrice(), 0, TP, 0);        
         EventRaiseSoon = false;
         return true;
      }
   }
   if ((head_grid_ticket != -1) && (grid_optype >= 0))
   {
      if (OrderSelect(head_grid_ticket, SELECT_BY_TICKET))
      {
         double lotsize = OrderLots();
         tip = OrderType();
         if (tip == grid_optype)
         {
            if (tip == OP_BUY) 
            {
               CheckPrice = Bid;
            } 
            else 
            {
               CheckPrice = Ask;
            }
            LossLevel = MathAbs(OrderOpenPrice() - CheckPrice)/Point;
            if ( (LossLevel > GridStep) && (InNewsPeriod == false)) 
            {
               isbuy = GetBWSignal();
               if (((isbuy > 0) && (grid_optype == OP_SELL)) || ((isbuy < 0) && (grid_optype == OP_BUY)))         
               {
                  grid_optype = tip;
                  lotsize = CalculateLotSize(grid_optype, lotsize);
                  grid_ticket = OpenOrder(tip, lotsize);
                  if (grid_ticket != -1) {
                     EventRaiseSoon = false;
                     head_grid_ticket = grid_ticket;
                     return true;
                  }
               }
            }
         }
      }
   }
   return true;
}

//+------------------------------------------------------------------+
void DoTrailing()
{
   int tip,Ticket;
   double StLo,OSL,OOP;
   int newTicket = -1;
   TIME = iTime(Symbol(),0,0);
   double fr;
   double prevfr;
   // datetime currentTime = TimeCurrent();
   for (int i=0; i<OrdersTotal(); i++)
   {
      if (OrderSelect(i, SELECT_BY_POS)==true)
      {  
         tip = OrderType();

         if (tip == grid_optype) {
            if ( TrailGridHead && (head_grid_ticket == OrderTicket()) )
               ;
            else 
               continue;
         }
         if ( (tip < 2) && (OrderSymbol()==Symbol()) && (OrderMagicNumber()==Magic)) // && CalcOrderRealProfit() >= 0)
         {
            OSL   = OrderStopLoss();
            OOP   = OrderOpenPrice();
            Ticket = OrderTicket();
            double TPPoints = 0;
            if (tip == OP_BUY && AllowBUY)             
            {  
               if (TrailByPSAR) 
               {
                  fr = iSAR(Symbol(),TrailingTimeFrame,0.01,0.1,0);
                  prevfr = iSAR(Symbol(),TrailingTimeFrame,0.01,0.1,1);
                  if ( Bid > fr && Bid > prevfr )
                     continue; 
               }               
               StLo = SlLastBar(1,Bid,TrailingStop);
               if (StLo==0) 
                  continue;        
               if (StLo <= OOP )
                  continue;
               if (StLo > OSL || OSL==0)
               {
                  newTicket = CloseMyOrder(Ticket);
                  if ((newTicket != -1) && (InNewsPeriod == false)) 
                  {
                     Ticket = newTicket;
                     if (TakeProfitLevel > 0)
                     {
                        TPPoints = OOP + TakeProfitLevel * Point;
                     } else 
                        TPPoints = OrderTakeProfit();
                     ChangeOrder(Ticket,OOP,StLo, TPPoints,0, Yellow);
                  }  
                  globalComment += "TrailingStop " + Ticket + " " + TimeToStr(TimeCurrent(),TIME_MINUTES) + "\n";
                  break;
               } 
            }                                         
            if (tip==OP_SELL && AllowSELL)        
            {  
               if (TrailByPSAR) 
               {
                  fr = iSAR(Symbol(),TrailingTimeFrame,0.01,0.1,0);
                  prevfr = iSAR(Symbol(),TrailingTimeFrame,0.01,0.1,1);
                  if ( Ask > fr && Ask > prevfr )
                     continue; 
               }
               StLo = SlLastBar(-1,Ask,TrailingStop);  
               if (StLo==0) 
                   continue;        
               if (StLo >= OOP)
                   continue;
               if (StLo < OSL || OSL==0)
               {
                  newTicket = CloseMyOrder(Ticket);
                  if ((newTicket != -1) && (InNewsPeriod == false)) 
                  {
                     Ticket = newTicket;
                     if (TakeProfitLevel > 0)
                        TPPoints = OOP - TakeProfitLevel * Point;
                     else 
                        TPPoints = OrderTakeProfit();
                     ChangeOrder(Ticket,OOP, StLo, TPPoints,0,Yellow);
                  }  
                  globalComment += "TrailingStop " + Ticket + " " + TimeToStr(TimeCurrent(),TIME_MINUTES) + "\n";
                  break;
               }
            } 
         }
      }
   }
}
//--------------------------------------------------------------------
double SlLastBar(int tip,double price, int tral)
{
   double fr = 0;
   double frprev = 0;
   int jj = 0;
   int ii = 0;
   if (tral==0)
   {
         if (tip== 1)
         for (ii=1; ii<NumBarsFractals; ii++) 
         {
            fr = iFractals(NULL,TrailingTimeFrame,MODE_LOWER,ii);
            if (fr!=0) 
              if (price > fr)
                 break;
            else
              fr=0;
         }
         if (tip==-1)
         for (jj=1; jj<NumBarsFractals; jj++) 
         {
            fr = iFractals(NULL,TrailingTimeFrame,MODE_UPPER,jj);
            if (fr!=0) 
               if (price < fr) 
                  break;
            else fr=0;
         }
   } else if (tral > 0)  
         {
            if (tip==1) 
              fr = Bid - tral*Point;  
            else 
              fr = Ask + tral*Point;  
         } 
         else 
         {
            if (tip== 1)
            {
               fr = NormalizeDouble(Bid - iATR(Symbol(),TrailingTimeFrame,14,0), Digits);
            }
            if (tip==-1)
            {
               fr = NormalizeDouble(Ask + iATR(Symbol(),TrailingTimeFrame,14,0), Digits);
            }
         }
   return fr;
}

