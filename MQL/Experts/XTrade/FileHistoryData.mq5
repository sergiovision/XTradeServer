//+------------------------------------------------------------------+
//|                                              FileHistoryData.mq5 |
//|                                 Copyright 2018, Sergei Zhuravlev |
//|                                          https://www.sergego.com |
//+------------------------------------------------------------------+
#property copyright "Copyright 2018, Sergei Zhuravlev"
#property link      "https://www.sergego.com"
#property version   "1.00"
//+------------------------------------------------------------------+
//| Include                                                          |
//+------------------------------------------------------------------+
#include <Files\FileTxt.mqh>
//+------------------------------------------------------------------+
//| Inputs                                                           |
//+------------------------------------------------------------------+
string symbolName   = "EURUSD";
input int year = 2019;
input int month = 1;
input int day = 1;
input bool IsTicksOrRates = false;
input bool IsVolumeTickOrReal = false;
input ENUM_TIMEFRAMES timeframe = PERIOD_M1;
CFileTxt file;
int handle = 0;
MqlTick tick_array[];
MqlRates rates_array[];
int ticks_count = 10000000;
//+------------------------------------------------------------------+
//| Global expert object                                             |
//+------------------------------------------------------------------+

int GetTicks(string fileName, datetime startday)
{
         MqlTick prev;       // To receive last tick data 
         SymbolInfoTick(symbolName, prev); 
         int result = CopyTicks(symbolName, tick_array, COPY_TICKS_ALL, startday*1000, ticks_count);
         if (result > 0)
         {
            Print(StringFormat("Symbol %s Found %d ticks", fileName, result));

            FileWrite(handle,       "time,bid,ask,volume");
            //file.WriteString("time,bid,ask,last,volume,flags,volume_real\n");
            int j = 0;
            for (int i = 0; i < ArraySize(tick_array); i++) 
            {
               if ((prev.ask == tick_array[i].ask) && (prev.bid == tick_array[i].bid))
                  continue;
               prev = tick_array[i];
               double volume = IsVolumeTickOrReal?(double)tick_array[i].volume:(double)tick_array[i].volume;
               string str = StringFormat("%s,%g,%g,%d", 
                  TimeToString(tick_array[i].time), // Time of the last prices update 
                  tick_array[i].bid, // Current Bid price 
                  tick_array[i].ask, // Current Ask price 
                  //tick_array[i].last,// Price of the last deal (Last) 
                  volume // Volume for the current Last price 
                   ); 
                  //file.WriteString(str);
               FileWrite(handle, str);
               j++;
            }
            Print(StringFormat("File %s written successfully with %d rows", fileName, j));
            FileClose(handle);
         }
     return INIT_SUCCEEDED;
}

int GetRates(string fileName, datetime startday)
{
         ArraySetAsSeries(rates_array, false); 
         datetime endday = TimeCurrent();
         int result = CopyRates(symbolName, timeframe, startday, endday, rates_array);
         if (result > 0)
         {
            Print(StringFormat("Symbol %s Found %d rates", fileName, result));

            FileWrite(handle, "time,close,open,high,low,volume");
            //file.WriteString("time,bid,ask,last,volume,flags,volume_real\n");
            int j = 0;
            for (int i = 0; i < ArraySize(rates_array); i++) 
            {
               double volume = IsVolumeTickOrReal?(double)rates_array[i].tick_volume:(double)rates_array[i].real_volume;

               string str = StringFormat("%s,%g,%g,%g,%g,%g", 
                  TimeToString(rates_array[i].time), // Time of the last prices update 
                  rates_array[i].close, 
                  rates_array[i].open, 
                  rates_array[i].high, 
                  rates_array[i].low, 
                  volume
                  );
               
               //file.WriteString(str);
               FileWrite(handle, str);
               j++;
            }
            Print(StringFormat("File %s written successfully with %d rows", fileName, j));
            FileClose(handle);
         }
     return INIT_SUCCEEDED;
}

//+------------------------------------------------------------------+
//| Initialization function of the expert                            |
//+------------------------------------------------------------------+
int OnInit()
{
   symbolName = Symbol();
   ResetLastError(); 
   string fileName = symbolName + ".csv";// TerminalInfoString(TERMINAL_DATA_PATH) + "\\MQL5\\Files\\" + symbolName + ".csv";
   handle = FileOpen(fileName, FILE_READ|FILE_WRITE|FILE_TXT | FILE_SHARE_READ | FILE_ANSI);      
   int     attempts=0; 
   if (handle != INVALID_HANDLE)
   {      
      while(attempts<3) 
      { 
         datetime current_time=TimeCurrent();                          
         MqlDateTime from; 
         datetime fromdate = D'2017.01.01';
         TimeToStruct(fromdate, from);
         from.year = year;
         from.mon = month;
         from.day = day;
         datetime startday=StructToTime(from);

         if (IsTicksOrRates)
         {
            if (GetTicks(fileName, startday) == INIT_SUCCEEDED)
               return INIT_SUCCEEDED;
         } else 
         {
            if (GetRates(fileName, startday) == INIT_SUCCEEDED)
               return INIT_SUCCEEDED;
         }
         //--- Counting attempts 
         attempts++; 
         //--- A one-second pause to wait for the end of synchronization of the tick database 
         Sleep(1000); 
      }

   }
//--- Initializing expert
   return(INIT_SUCCEEDED);
}
//+------------------------------------------------------------------+
//| Deinitialization function of the expert                          |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
{
  FileClose(handle);
   //file.Close();
}
//+------------------------------------------------------------------+
//| "Tick" event handler function                                    |
//+------------------------------------------------------------------+
void OnTick()
{
}
//+------------------------------------------------------------------+
//| "Trade" event handler function                                   |
//+------------------------------------------------------------------+
void OnTrade()
{
}
