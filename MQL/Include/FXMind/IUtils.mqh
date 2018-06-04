//+------------------------------------------------------------------+
//|                                                       IUtils.mqh |
//|                        Copyright 2018, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

interface IUtils
{
   datetime CurrentTimeOnTF();
   bool SelectOrder(int ticket);
   long GetAccountNumer();
};

#define DELETE_PTR(pointer)  if (pointer != NULL) { delete pointer; pointer = NULL; }


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


