#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\IUtils.mqh>

class MT4Utils: public IUtils
{
   datetime CurrentTimeOnTF()
   {
       return Time[0];
   }
   bool SelectOrder(int ticket)
   {
      return OrderSelect(ticket, SELECT_BY_TICKET);
   }
   long GetAccountNumer()
   {
      return AccountNumber();
   }
};

