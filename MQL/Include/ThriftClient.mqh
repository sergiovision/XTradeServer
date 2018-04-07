//+------------------------------------------------------------------+
//|                                                 ThriftClient.mqh |
//|                                                 Sergei Zhuravlev |
//|                        https://www.facebook.com/sergei.zhuravlev |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "https://www.facebook.com/sergei.zhuravlev"
#property strict

struct THRIFT_CLIENT
{
   ushort port;
   int Magic;
   int accountNumber;
   uchar ip0;
   uchar ip1;
   uchar ip2;
   uchar ip3;
};

//#import "fxmindmql.dll"
#import "ThriftMQL.dll"
long ProcessStringData(string& inoutdata, string parameters, THRIFT_CLIENT &tc);
long ProcessDoubleData(double &arr[], int arr_size, string parameters, string indata, THRIFT_CLIENT &tc);
long IsServerActive(THRIFT_CLIENT &tc);
void PostStatusMessage(THRIFT_CLIENT &tc, string message);
void CloseClient(THRIFT_CLIENT &tc);
#import

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class ThriftClient
{
protected:
   THRIFT_CLIENT client;
   bool   IsActive;
   ushort sep;   
public:
   ThriftClient(int accountNumber, ushort port, int magic)
   {
      client.Magic = magic;
      client.port = port;
      client.accountNumber = accountNumber;
      //client.ip0 = 192;
      //client.ip1 = 168;
      //client.ip2 = 10;
      //client.ip3 = 2;      
      
      client.ip0 = 127;
      client.ip1 = 0;
      client.ip2 = 0;
      client.ip3 = 1;
            
      sep = StringGetCharacter("|", 0);
      IsActive = false;
      CheckActive();
   }
   
   virtual bool Init()
   {
      storeEventTime = TimeCurrent();
      prevSenttime = storeEventTime;
      storeEventMessage = "no event";
      return IsActive;
   }

   bool CheckActive() {   
      IsActive = IsServerActive(client) > 0;
      return IsActive;
   }
   
   bool isActive() {   
      return IsActive;
   }
   
   string storeEventstr;
   datetime storeEventTime;
   datetime storeRaiseTime;
   string storeEventMessage;
   int storeRes;
   int GetNextNewsEvent(string symbol, ushort Importance, string& eventMessage, datetime& raiseDate)
   {
      //datetime curtime = iTime( NULL, PERIOD_M15, 0 );
      datetime curtime = Time[0];
      raiseDate = storeRaiseTime;
      eventMessage = storeEventMessage;
      if (storeEventTime != curtime)
         storeEventTime = curtime;
      else
         return 0;
   	string instr = "func=NextNewsEvent|symbol=" + symbol + "|importance=" + IntegerToString(Importance) + "|time=" + TimeToString(curtime);
   	if ( StringCompare(storeEventstr, instr) == 0)
         return 0;
   	storeRes = 0;
   	storeEventstr = instr;
   	string rawMessage;
   	StringInit(rawMessage, 256, 0);
   	long retval = ProcessStringData(rawMessage, instr, client);
   	if ( retval > 0 )
   	{
   	   //Print("rawMessage = " + rawMessage +" , retval = " + IntegerToString(retval));   	
         //if (StringLen(rawMessage) > 0) {
            //Print(rawMessage);
            string result[];
            int count = StringSplit(rawMessage, sep, result);
            if (count >= 4) {
               raiseDate = StringToTime(result[2]);
               eventMessage = result[0] + ":" + result[1] + ":" + result[2] + ":" + result[3];
               if (StringCompare(eventMessage, storeEventMessage) != 0) 
               {
                  storeRaiseTime = raiseDate;
                  storeEventMessage = eventMessage;
                  storeRes = StrToInteger(result[1]);
               }
            }
   	   //} else 
         //   res = 0;
   	}
   	return storeRes;
   }
   
   string oldSentstr;
   datetime prevSenttime;
   int GetCurrentSentiments(string symbol, double& longVal, double& shortVal) 
   {
      datetime curtime = Time[0];
      if (prevSenttime != curtime)
         prevSenttime = curtime;
      else
         return 0;
   	string instr = "func=CurrentSentiments|symbol=" + symbol + "|time=" + TimeToString(curtime);
   	if ( StringCompare(oldSentstr, instr) == 0)
   	   return 0;
   	oldSentstr = instr;
   	double resDouble[2];
   	//ArrayResize(resDouble, 2);
   	ArrayFill(resDouble, 0, 2, 0);
   	string rawMessage = "0|0";
   	long retval = ProcessDoubleData(resDouble, 2, instr, rawMessage, client);
   	int res = 0;   	
   	if ( retval == 0 )
   	{
         longVal = resDouble[0];
         shortVal = resDouble[1];
         res = 1;
      } else 
            res = 0;
      return res;
   }
   
   long GetSentimentsArray(string symbol, int offset, int limit, int site, const datetime& times[], double &arr[])
   {
   	string parameters = "func=SentimentsArray|symbol=" + symbol + "|size=" + IntegerToString(limit)
   	   + "|site=" + IntegerToString(site);
   	string timeArray;
   	for (int i = 0; i < limit; i++)
   	{
  	      timeArray += TimeToString(times[i]);
  	      if (i < (limit-1) )
  	         timeArray += "|";
     	}
     	double retarr[];
     	ArrayResize(retarr, limit);
      long retval = ProcessDoubleData(retarr, limit, parameters, timeArray, client);
      ArrayCopy(arr, retarr, offset, 0, limit);
      return retval;
   }

   long GetCurrencyStrengthArray(string currency, int offset, int limit, int timeframe, const datetime& times[], double &arr[])
   {
   	string parameters = "func=CurrencyStrengthArray|currency=" + currency + "|timeframe=" + IntegerToString(timeframe);
   	string timeArray;
   	for (int i = 0; i < limit; i++)
   	{
  	      timeArray += TimeToString(times[i]);
  	      if (i < (limit-1) )
  	         timeArray += "|";
     	}
     	double retarr[];
     	ArrayResize(retarr, limit);
      long retval = ProcessDoubleData(retarr, limit, parameters, timeArray, client);
      ArrayCopy(arr, retarr, offset, 0, limit);
      return retval;
   }
   
   void PostMessage(string message) 
   {
      PostStatusMessage(client, message);
   }

   virtual uint DeInit()
   {
      CloseClient(client);
      Print("ThriftClient closed");
      return 0;
   }

   virtual ~ThriftClient()
   {
      DeInit();
   }
};
