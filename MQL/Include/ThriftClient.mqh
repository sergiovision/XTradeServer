//+------------------------------------------------------------------+
//|                                                 ThriftClient.mqh |
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
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

class  NewsEventInfo
{
 public:
  string Currency;
  int     Importance;
  datetime RaiseDateTime; // date returned in MTFormat
  string Name;
  
  NewsEventInfo()
  {
      Name = "No news";
      Currency = "UnDefined";
      Importance = 0;
  }
    
  void Clear()
  {
     Name = "No news";
     Currency = "";
     Importance =0;
     RaiseDateTime = 0;
  }
  
   void operator=(const NewsEventInfo &n) {
      Currency = n.Currency;
      Importance = n.Importance;
      RaiseDateTime = n.RaiseDateTime;
      Name = n.Name;
   }
      
  bool operator==(const NewsEventInfo &n)
  {
     if (StringCompare(Name, n.Name)!=0)
        return false;
     if (RaiseDateTime != n.RaiseDateTime)
        return false;
     if (Importance != n.Importance)
        return false;
     if (StringCompare(Currency, n.Currency) !=0)
        return false;
     return true;
  }
  
   bool operator!=(const NewsEventInfo &n)
   {
     if (StringCompare(Name, n.Name)!=0)
        return true;
     if (RaiseDateTime != n.RaiseDateTime)
        return true;
     if (Importance != n.Importance)
        return true;
     if (StringCompare(Currency, n.Currency) !=0)
        return true;
     return false;
   }

  
  string ToString()
  {
     if (StringCompare("No news", Name)==0)
        return "";
     return Currency + " "+ IntegerToString(Importance) + " " + TimeToString(RaiseDateTime) + " " + Name;
  }
};

#define MAX_NEWS_PER_DAY  5


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
public:
   ushort sep;   
   ushort sepList;   
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
      sepList = StringGetCharacter("~", 0);
      IsActive = false;
      CheckActive();
   }
   
   virtual bool Init()
   {
      storeEventTime = TimeCurrent();
      prevSenttime = storeEventTime;
      return IsActive;
   }

   bool CheckActive() {   
      IsActive = IsServerActive(client) > 0;
      return IsActive;
   }
   
   bool isActive() {   
      return IsActive;
   }
   
   bool NewsFromString(string newsstring, NewsEventInfo& news)
   {
      //Print(newsstring);
      string result[];
      if (StringGetCharacter(newsstring, 0) == sep)
         newsstring = StringSubstr(newsstring, 1);
      int count = StringSplit(newsstring, sep, result);
      if (count >= 4) 
      {
         news.Currency = result[0];
         news.Importance = StrToInteger(result[1]);
         news.RaiseDateTime = StringToTime(result[2]);
         news.Name = result[3];
         //Print(news.ToString());
         return true;
      }
      return false;
   }
   
   datetime storeEventTime;
   NewsEventInfo storeEvent;
   string storeParamstrEvent;
   bool GetNextNewsEvent(string symbol, ushort Importance, NewsEventInfo& eventInfo)
   {
      //datetime curtime = iTime( NULL, PERIOD_M15, 0 );
      datetime curtime = Time[0];
      if (storeEventTime != curtime)
         storeEventTime = curtime;
      else
      { 
         //storeEvent.AssignCopy(eventInfo);
         eventInfo = storeEvent;
         return true;
      }
      string instr = StringFormat("func=NextNewsEvent|symbol=%s|importance=%d|time=%s", symbol, Importance, TimeToString(curtime));
   	if ( StringCompare(storeParamstrEvent, instr) == 0)
      { 
         //storeEvent.AssignCopy(eventInfo);
         eventInfo = storeEvent;
         return true;
      }
   	storeParamstrEvent = instr;
   	string rawMessage;
   	StringInit(rawMessage, 512, 0);
   	long retval = ProcessStringData(rawMessage, instr, client);
   	if ( retval > 0 )
   	{
   	   if (NewsFromString(rawMessage, eventInfo))
   	   {
   	      storeEvent = eventInfo;
   	      //Print(eventInfo.ToString());
   	      return true;
   	   }
   	}
   	return false;
   }
   
   string storeParamstr;
   int GetTodayNews(string symbol, ushort Importance, NewsEventInfo &arr[])
   {
      datetime curtime = Time[0];
   	string instr = "func=GetTodayNews|symbol=" + symbol + "|importance=" + IntegerToString(Importance) + "|time=" + TimeToString(curtime);
   	if ( StringCompare(storeParamstr, instr) == 0)
         return 0;
   	storeParamstr = instr;
   	int storeRes = 0;
   	string rawMessage;
   	StringInit(rawMessage, MAX_NEWS_PER_DAY*512, 0);
   	long retval = ProcessStringData(rawMessage, instr, client);
   	if ( retval > 0 )
   	{
   	   //Print(rawMessage);
         string result[];
         int count = StringSplit(rawMessage, sepList, result);
         count = (int)MathMin(count, MAX_NEWS_PER_DAY);
         if (count >= 1) {
            for (int i=0; i<MAX_NEWS_PER_DAY;i++)
            { 
               arr[i].Clear();
            }
            storeRes = count;
            for (int i=0; i<count;i++)
            {                
               NewsFromString(result[i], arr[i]);
            }
         }
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
      Print("Connection with Thrift service closed.");
      return 0;
   }

   virtual ~ThriftClient()
   {
      DeInit();
   }
};
