//+------------------------------------------------------------------+
//|                                                 ThriftClient.mqh |
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\GenericTypes.mqh>
#include <FXMind\IsSession.mqh>

INPUT_VARIABLE(LotsBUY, double, 0.02)
INPUT_VARIABLE(LotsSELL, double, 0.02)
INPUT_VARIABLE(LotsMIN, double, 0.01)
INPUT_VARIABLE(PanelSize, ENUM_TRADE_PANEL_SIZE, PanelNormal)
INPUT_VARIABLE(RefreshTimeFrame, ENUM_TIMEFRAMES, PERIOD_M1)
INPUT_VARIABLE(AllowBUY, bool, true)
INPUT_VARIABLE(AllowSELL, bool, true)
INPUT_VARIABLE(BUYBegin, datetime, 0) 
INPUT_VARIABLE(BUYEnd, datetime, SECONDS_DAY-1) 
INPUT_VARIABLE(SELLBegin, datetime, 0) 
INPUT_VARIABLE(SELLEnd, datetime, SECONDS_DAY-1) 

//INPUT_VARIABLE(AllowStopLossByDefault, bool, true)
INPUT_VARIABLE(AllowVirtualStops, bool, true)
INPUT_VARIABLE(AllowRealStops, bool, true)
INPUT_VARIABLE(CoeffSL, double, 1.5)
INPUT_VARIABLE(CoeffTP, double, 1.5)
INPUT_VARIABLE(PendingOrderStep, int,  4)
INPUT_VARIABLE(MoreTriesOpenOrder, bool, false)

//--------------------------------------------------------------------
// Indicators 
INPUT_VARIABLE(FilterIndicator, ENUM_INDICATORS, IshimokuIndicator)
INPUT_VARIABLE(SignalIndicator, ENUM_INDICATORS, NoIndicator)
INPUT_VARIABLE(WeightCalculation, ENUM_SIGNALWEIGHTCALC, WeightBySignal)
INPUT_VARIABLE(NumBarsToAnalyze, int, 20)
INPUT_VARIABLE(NumBarsFlatPeriod, int, ISHIMOKU_PLAIN_NOTRADE)
INPUT_VARIABLE(IshimokuPeriod1, int, 2)
INPUT_VARIABLE(IshimokuPeriod2, int, 24)
INPUT_VARIABLE(IshimokuPeriod3, int, 120)

INPUT_VARIABLE(BandsPeriod, int, 20)
INPUT_VARIABLE(BandsDeviation, double, 1.618)
// Trailing data
//--------------------------------------------------------------------
INPUT_VARIABLE(TrailingType, ENUM_TRAILING, TrailingManual)
INPUT_VARIABLE(TrailingIndent, int, 12)
INPUT_VARIABLE(TrailInLoss, bool, false) // If true - stoploss should be defined!!!
//--------------------------------------------------------------------
// Grid data
//--------------------------------------------------------------------
//INPUT_VARIABLE(MartingaleFlat, bool, false)
bool MartingaleFlat =  false;
INPUT_VARIABLE(AllowGRIDBUY, bool,  false)
bool actualAllowGRIDBUY = false;
INPUT_VARIABLE(AllowGRIDSELL, bool, false)
bool actualAllowGRIDSELL = false;
INPUT_VARIABLE(GridMultiplier, double, 2.0)
//INPUT_VARIABLE(GridProfit, int, 10)
int GridProfit = 10;
//INPUT_VARIABLE(MaxGridOrders, int, 3)
int MaxGridOrders = 3;

// News Params
INPUT_VARIABLE(EnableNews, bool, false)
INPUT_VARIABLE(RaiseSignalBeforeEventMinutes, int, 65)
INPUT_VARIABLE(NewsPeriodMinutes, int, 200)
INPUT_VARIABLE(MinImportance, int, 1)
//--------------------------------------------------------------------
//INPUT_VARIABLE(ThriftPORT, int, 2010)
int ThriftPORT = Constants::FXMindMQL_PORT;
//INPUT_VARIABLE(MaxOpenedTrades, int, 4)
int MaxOpenedTrades = 4;
INPUT_VARIABLE(Slippage, int,  10)
int   actualSlippage = Slippage;
