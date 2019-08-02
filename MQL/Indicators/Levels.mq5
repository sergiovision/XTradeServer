#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

//---- Indicator version number
#property version   "1.00"
//---- drawing the indicator in the main window
#property indicator_chart_window 
//---- no buffers used for the calculation and drawing of the indicator
#property indicator_buffers 0
//---- 0 graphical plots are used
#property indicator_plots   0

#define LEVEL_LINE_STYLE STYLE_DASH
#define LEVEL_LINE_WIDTH 1
#define LEVEL_LINE_COLOR clrViolet

#include <XTrade\IUtils.mqh>

#ifdef __MQL5__
#property indicator_applied_price PRICE_CLOSE
#endif

input string levels_string = "";
string levels_result[];

void OnInit()
{
   string name = StringFormat("Levels (%s) - %s", Symbol(), levels_string);
   Utils = CreateUtils((short)2010, name); 
   if (Utils == NULL)
      Print("Failed create Utils!!!");
   Utils.SetIndiName(name);
   
   string sep = ",";              // A separator as a character 
   ushort u_sep;                  // The code of the separator characte
   u_sep = StringGetCharacter(sep, 0);
   StringSplit(levels_string, u_sep, levels_result);
   
   IndicatorSetInteger(INDICATOR_DIGITS, _Digits);
   CreateLevels();
   Utils.Info(StringFormat("Inited %s", name));
}

//+------------------------------------------------------------------+
//| Create Levels                                                    |
//+------------------------------------------------------------------+    
void CreateLevels()
{
   for (int i = 0; i < ArraySize(levels_result); i++)
   {
      double level_price = StringToDouble(levels_result[i]);
      string level_name  = "Level_" + levels_result[i];
      string level_tooplip = level_name;
      if (!Utils.ObjExist(level_name))
         Utils.HLineCreate(level_name,0,level_price,LEVEL_LINE_COLOR,LEVEL_LINE_STYLE,LEVEL_LINE_WIDTH,false, true,false,0,level_tooplip);
      else 
         Utils.HLineMove(level_name, level_price, level_tooplip);
   }
   ChartRedraw(0);  

}
//+------------------------------------------------------------------+
//| Custom indicator deinitialization function                       |
//+------------------------------------------------------------------+    
void OnDeinit(const int reason)
{
   for (int i = 0; i < ArraySize(levels_result); i++)
   {
      double level_price = StringToDouble(levels_result[i]);
      string level_name  = "Level_" + levels_result[i];
      if (Utils.ObjExist(level_name))
         Utils.ObjDelete(level_name);
   }
   DELETE_PTR(Utils)
}
//+------------------------------------------------------------------+
//| Custom indicator iteration function                              |
//+------------------------------------------------------------------+
int OnCalculate(
    const int rates_total,    // amount of history in bars at the current tick
    const int prev_calculated,// amount of history in bars at the previous tick
    const datetime &time[],
    const double &open[],
    const double& high[],     // price array of maximums of price for the calculation of indicator
    const double& low[],      // price array of price lows for the indicator calculation
    const double &close[],
    const long &tick_volume[],
    const long &volume[],
    const int &spread[]
                )
{
   if (prev_calculated != rates_total) {
         CreateLevels();
   }
//----
   return(rates_total);
}
//+------------------------------------------------------------------+
