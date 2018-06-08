//+------------------------------------------------------------------+
//|                                                 ThriftClient.mqh |
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

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

enum ENUM_ORDERROLE  
{
    RegularTrail, 
    GridHead, 
    GridTail,
    ShouldBeClosed,
    History
};


enum ENUM_MARKETSTATE  
{
    FlatTrend, 
    UpTrend, 
    DownTrend
};

enum ENUM_INDICATORS  
{
    EMAWMAIndicator,
    BillWilliamsIndicator,
    ZigZagIndicator,
    BandsIndicator
};

enum ENUM_TRADE_PANEL_SIZE  
{
    PanelNormal,
    PanelSmall
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




#define INPUT_VARIABLE(var_name, var_type, def_value) input var_type var_name = def_value;

sinput ENUM_MARKETSTATE MarketState = FlatTrend; // Market state on current chart
INPUT_VARIABLE(LotsBUY, double, 0.02)
INPUT_VARIABLE(LotsSELL, double, 0.02)
INPUT_VARIABLE(TakeProfitLevel, int, 10)
int   actualTakeProfitLevel = TakeProfitLevel;
INPUT_VARIABLE(StopLossLevel, int, 85)
int   actualStopLossLevel = StopLossLevel;
INPUT_VARIABLE(AllowStopLossByDefault, bool, false)
INPUT_VARIABLE(ThriftPORT, int, 2010)
INPUT_VARIABLE(AllowBUY, bool, true)
INPUT_VARIABLE(AllowSELL, bool, true)
INPUT_VARIABLE(MaxOpenedTrades, int, 10)
INPUT_VARIABLE(Slippage, int,  10)
int   actualSlippage = Slippage;
// Grid data
//--------------------------------------------------------------------
INPUT_VARIABLE(AllowGRIDBUY, bool,  false)
INPUT_VARIABLE(AllowGRIDSELL, bool, false)
INPUT_VARIABLE(GridStep, int, 90)
int   actualGridStep = GridStep;
INPUT_VARIABLE(GridMultiplier, double, 2.0)
INPUT_VARIABLE(GridProfit, int, 25)
INPUT_VARIABLE(MaxGridOrders, int, 3)
// Stop Trailing data
//--------------------------------------------------------------------
INPUT_VARIABLE(TrailingIndent, int, 3)
INPUT_VARIABLE(TrailingTimeFrame,ENUM_TIMEFRAMES, PERIOD_M30)
INPUT_VARIABLE(TrailingType, ENUM_TRAILING, TrailingByFractals)
INPUT_VARIABLE(TrailInLoss, bool, true) // If true - stoploss should be defined!!!
INPUT_VARIABLE(NumBarsFractals, int, 5)
//--------------------------------------------------------------------
// Indicators 
INPUT_VARIABLE(TrendIndicator, ENUM_INDICATORS, EMAWMAIndicator)
INPUT_VARIABLE(IndicatorTimeFrame, ENUM_TIMEFRAMES, PERIOD_H1)
//--------------------------------------------------------------------
// News Params
INPUT_VARIABLE(RaiseSignalBeforeEventMinutes, int, 40)
INPUT_VARIABLE(NewsPeriodMinutes, int, 200)
INPUT_VARIABLE(MinImportance, int, 1)
INPUT_VARIABLE(RevertNewsTrend, bool, true)
//--------------------------------------------------------------------
INPUT_VARIABLE(PanelSize, ENUM_TRADE_PANEL_SIZE, PanelSmall)
