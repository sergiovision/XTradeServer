//+------------------------------------------------------------------+
//|                                                 ThriftClient.mqh |
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <SettingsFile.mqh>

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

class Constants {
public:

  double GAP_VALUE;
  string MTDATETIMEFORMAT;
  string MYSQLDATETIMEFORMAT;
  string SOLRDATETIMEFORMAT;
  int SENTIMENTS_FETCH_PERIOD;
  short FXMindMQL_PORT;
  short AppService_PORT;
  string JOBGROUP_TECHDETAIL;
  string JOBGROUP_OPENPOSRATIO;
  string JOBGROUP_EXECRULES;
  string JOBGROUP_NEWS;
  string JOBGROUP_THRIFT;
  string CRON_MANUAL;
  string SETTINGS_PROPERTY_BROKERSERVERTIMEZONE;
  string SETTINGS_PROPERTY_PARSEHISTORY;
  string SETTINGS_PROPERTY_STARTHISTORYDATE;
  string SETTINGS_PROPERTY_USERTIMEZONE;
  string SETTINGS_PROPERTY_NETSERVERPORT;
  string SETTINGS_PROPERTY_ENDHISTORYDATE;
  string SETTINGS_PROPERTY_THRIFTPORT;
  string SETTINGS_PROPERTY_INSTALLDIR;
  string SETTINGS_PROPERTY_RUNTERMINALUSER;
  string PARAMS_SEPARATOR;
  string LIST_SEPARATOR;
  string GLOBAL_SECTION_NAME;

  
  Constants() 
  {
  GAP_VALUE = -125;

  MTDATETIMEFORMAT = "yyyy.MM.dd HH:mm";

  MYSQLDATETIMEFORMAT = "yyyy-MM-dd HH:mm:ss";

  SOLRDATETIMEFORMAT = "yyyy-MM-dd'T'HH:mm:ss'Z'";

  SENTIMENTS_FETCH_PERIOD = 100;

  FXMindMQL_PORT = 2010;

  AppService_PORT = 2012;

  JOBGROUP_TECHDETAIL = "Technical Details";

  JOBGROUP_OPENPOSRATIO = "Positions Ratio";

  JOBGROUP_EXECRULES = "Run Rules";

  JOBGROUP_NEWS = "News";

  JOBGROUP_THRIFT = "ThriftServer";

  CRON_MANUAL = "0 0 0 1 1 ? 2100";

  SETTINGS_PROPERTY_BROKERSERVERTIMEZONE = "BrokerServerTimeZone";

  SETTINGS_PROPERTY_PARSEHISTORY = "NewsEvent.ParseHistory";

  SETTINGS_PROPERTY_STARTHISTORYDATE = "NewsEvent.StartHistoryDate";

  SETTINGS_PROPERTY_USERTIMEZONE = "UserTimeZone";

  SETTINGS_PROPERTY_NETSERVERPORT = "FXMind.NETServerPort";

  SETTINGS_PROPERTY_ENDHISTORYDATE = "NewsEvent.EndHistoryDate";

  SETTINGS_PROPERTY_THRIFTPORT = "FXMind.ThriftPort";

  SETTINGS_PROPERTY_INSTALLDIR = "FXMind.InstallDir";

  SETTINGS_PROPERTY_RUNTERMINALUSER = "FXMind.TerminalUser";
  
  PARAMS_SEPARATOR = "|";

  LIST_SEPARATOR = "~";
  
  GLOBAL_SECTION_NAME = "Global";


}

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
  
  string DateToString()
  {
      return StringFormat("%02d/%02d %02d:%02d", TimeMonth(RaiseDateTime),TimeDay(RaiseDateTime), TimeHour(RaiseDateTime), TimeMinute(RaiseDateTime));
  }
  
  string ToString()
  {
     if (StringCompare("No news", Name)==0)
        return "";
     return Currency + " "+ IntegerToString(Importance) + " " + DateToString() + " " + Name;
  }
};

#define MAX_NEWS_PER_DAY  5

#import "ThriftMQL.dll"
long ProcessStringData(string& inoutdata, string parameters, THRIFT_CLIENT &tc);
long ProcessDoubleData(double &arr[], int arr_size, string parameters, string indata, THRIFT_CLIENT &tc);
long IsServerActive(THRIFT_CLIENT &tc);
void PostStatusMessage(THRIFT_CLIENT &tc, string message);
void GetGlobalProperty(string& RetValue, string PropName, THRIFT_CLIENT &tc); // returns length of the result value. -1 - on error
long InitExpert(string ChartTimeFrame, string Symbol, string comment, THRIFT_CLIENT &tc); // Returns Magic Number, 0 or error
void SaveExpert(string ActiveOrdersList, THRIFT_CLIENT &tc);
void DeInitExpert(int Reason, THRIFT_CLIENT &tc); // DeInit for Expert Advisers only
void CloseClient(THRIFT_CLIENT &tc); // Free memory
#import

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class FXMindClient
{
protected:
   THRIFT_CLIENT client;
   bool   IsActive;
public:
   Constants constant;
   ushort sep;   
   ushort sepList; 
   int MagicNumber;
   string fileName;
   SettingsFile* set;

   FXMindClient(short Port, string EAName)
   {
      sep = StringGetCharacter(constant.PARAMS_SEPARATOR, 0);
      sepList = StringGetCharacter(constant.LIST_SEPARATOR, 0);
      client.ip0 = 127;
      client.ip1 = 0;
      client.ip2 = 0;
      client.ip3 = 1;
      client.port = Port;
      client.accountNumber = AccountNumber();
      string periodStr = EnumToString((ENUM_TIMEFRAMES)Period());
      string sym = Symbol();
      long Magic = InitExpert(periodStr, sym, EAName, client);
      if (Magic <= 0)
         Print(StringFormat("InitExpert(%d, %s, %s) FAILED!!!", client.accountNumber, periodStr, sym));
      client.Magic = (int)Magic;
      MagicNumber = (int)Magic;
      
      fileName = StringFormat("%d_%s_%s_%d.set", client.accountNumber, sym, periodStr, MagicNumber);
      
      IsActive = false;
      CheckActive();
   }
   
   virtual bool Init() // Should be called after MagicNumber obtained
   {
      set = new SettingsFile(constant.GLOBAL_SECTION_NAME, fileName);

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
         eventInfo = storeEvent;
         return true;
      }
      string instr = StringFormat("func=NextNewsEvent|symbol=%s|importance=%d|time=%s", symbol, Importance, TimeToString(curtime));
   	if ( StringCompare(storeParamstrEvent, instr) == 0)
      { 
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
   
   
   void SaveAllSettings(string ActiveOrdersList)
   {
      
      SaveExpert(ActiveOrdersList, client);
   }
   

   virtual uint DeInit(int Reason)
   {
      DeInitExpert(Reason, client); // DeInit for Expert Advisers only
      CloseClient(client);
      Print("Connection with FXMind service closed.");
      return 0;
   }

   virtual ~FXMindClient()
   {
      if (set != NULL)
      {
         delete set;
         set = NULL;
      }

   }
};
