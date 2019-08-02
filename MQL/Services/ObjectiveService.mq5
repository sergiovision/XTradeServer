//+------------------------------------------------------------------+
//|                                             ObjectiveService.mq5 |
//|                                 Copyright 2018, Sergei Zhuravlev |
//|                                          https://www.sergego.com |
//+------------------------------------------------------------------+
#property service
#property copyright "Copyright 2018, Sergei Zhuravlev"
#property link      "https://www.sergego.com"
#property version   "1.00"

#define THRIFT 1

#define TEMPLATE_NAME   "ObjectiveService"


#include <XTrade\TradeExpert.mqh>

TradeExpert* expert = NULL;

//+------------------------------------------------------------------+ 
//| Script program start function                                    | 
//+------------------------------------------------------------------+ 
void OnStart()
{      
   string comment = "ObjectiveService";

   if ((bool)MQLInfoInteger(MQL_TESTER))
      comment = "ObjectiveService Debug";
      
   Utils = CreateUtils((short)2010, comment);

   if ( CheckPointer(Utils) == POINTER_INVALID )
   {
      Print("FAILED TO CREATE IUtils!!! Exit ObjectiveService!");
      return;
   }
   
   expert = new TradeExpert();
   expert.StartAsService();
   
   expert.DeInit(0);
   DELETE_PTR(expert);
}
//+------------------------------------------------------------------+
