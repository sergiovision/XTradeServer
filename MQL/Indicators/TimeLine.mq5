//+------------------------------------------------------------------+
//|                                                     TimeLine.mq5 |
//|                                        Copyright 2018, AZ-iNVEST |
//|                                          http://www.az-invest.eu |
//+------------------------------------------------------------------+
#property copyright "Copyright 2018, AZ-iNVEST"
#property link      "http://www.az-invest.eu"
#property version   "1.01"
#property indicator_separate_window
//#property indicator_chart_window
#property indicator_plots 0

#define GET 
#include <XTrade/IUtils.mqh>

#define PREFIX_SEED "TimeLine"
#define START_COLOR clrDarkGreen

static long __chartId  = ChartID();
static int  __subWinId = ChartWindowFind();


input int                  Magic = FAKE_MAGICNUMBER;
input int                  ThriftPort = 2010;
input int                  SubWindow = 1;
input color                InfoTextColor = clrBlack;    // Font color
input int                  InpFontSize  = 9;                // Font size
input int                  InpSpacing = 8;                  // Date/Time spacing
// input int                  Height = 28;                  // Date/Time spacing

int prevDay = 0;
string strPREFIX = PREFIX_SEED;


#include <XTrade/MedianRenko/MedianRenkoIndicator.mqh>
MedianRenkoIndicator customChartIndicator;

//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int OnInit()
{
   string name = PREFIX_SEED;
   strPREFIX = name + IntegerToString(Magic);
   Utils = CreateUtils((short)ThriftPort, name);

   //--- indicator buffers mapping
   IndicatorSetString(INDICATOR_SHORTNAME,"\n");
   IndicatorSetDouble(INDICATOR_MINIMUM,0); 
   IndicatorSetDouble(INDICATOR_MAXIMUM,9);  
   //IndicatorSetInteger(INDICATOR_HEIGHT, Height);  
   IndicatorSetInteger(INDICATOR_DIGITS,0);
   //---

   customChartIndicator.SetGetTimeFlag();   

   return(INIT_SUCCEEDED);
}
  
void OnDeinit(const int r)
{
   //Utils.ObjDeleteAll(strPREFIX);
      
   DELETE_PTR(Utils);   

   ObjectsDeleteAll(__chartId,PREFIX_SEED);
}
//+------------------------------------------------------------------+
//| Custom indicator iteration function                              |
//+------------------------------------------------------------------+
int OnCalculate(const int rates_total,
                const int prev_calculated,
                const datetime &time[],
                const double &open[],
                const double &high[],
                const double &low[],
                const double &close[],
                const long &tick_volume[],
                const long &volume[],
                const int &spread[])
{
   if(!customChartIndicator.OnCalculate(rates_total,prev_calculated,time))
      return(0);
      
   int start = customChartIndicator.GetPrevCalculated() - 1;
//--- correct position
   if(start<0) 
      start=0;

   if((start == 0) || customChartIndicator.IsNewBar)
   {
      ObjectsDeleteAll(__chartId,PREFIX_SEED);

      DrawTimeLine(0,rates_total,time);            
   }

   //--- return value of prev_calculated for next call
   return(rates_total);
}
//+------------------------------------------------------------------+

void DrawTimeLine(const int nPosition, const int nRatesCount, const datetime &canvasTime[])
{
   datetime curBarTime = 0;   
   bool     _start = false;
   int      c = 0;
   
   if (ArraySize(customChartIndicator.Time) < nRatesCount)
       return;
   for(int i=nPosition;i<nRatesCount;i++)
   {
      curBarTime = (datetime)customChartIndicator.Time[i];
      if(curBarTime == 0)
         continue;
      else
         _start = true;

      MqlDateTime dt;
      TimeToStruct(curBarTime, dt);
      DrawVLine(canvasTime[i], dt);
      
      if(c%InpSpacing == 0)
         DrawDateTimeMarker(i,curBarTime,canvasTime[i]);
      
      if(_start)
         c++;
   }
   
   ChartRedraw();
}

bool DrawDateTimeMarker(const int ix, const datetime timeStamp, const datetime canvasTime)
{
   if(timeStamp == 0)
      return false;
      
   int colorText = InfoTextColor;
   MqlDateTime dt;
   TimeToStruct(timeStamp, dt);
   bool showDate = false;
   if (prevDay != dt.day)
   {
      prevDay = dt.day;
      colorText = START_COLOR;
      showDate = true;
   }
      
      
   TextCreate(__chartId,strPREFIX+(string)timeStamp,__subWinId,canvasTime,9,NormalizeTime(dt, timeStamp, showDate),"Calibri",InpFontSize,colorText);
   return true;
}  

void DrawVLine(datetime drawTime, MqlDateTime& dt)
{
   string name = StringFormat("%s_VerticalDayStart_%d_%d_%d", strPREFIX, dt.year, dt.mon, dt.day);
   if (Utils.ObjExist(name))
      return;
   if (ObjectCreate(__chartId, name,OBJ_VLINE,0, drawTime, 0))
   {
       ObjectSetInteger(__chartId,name,OBJPROP_COLOR,START_COLOR);
   }
}


string NormalizeTime(MqlDateTime&    dt, datetime _dt, bool isShowDate)
{
   string minute = (dt.min<10)  ? ("0"+(string)dt.min)  : (string)dt.min;
   string hour   = (dt.hour<10) ? ("0"+(string)dt.hour) : (string)dt.hour;
   
   string month = (dt.mon<10) ? ("0"+(string)dt.mon) : (string)dt.mon;
      
   string result = StringFormat("%s:%s", hour, minute);
   if (isShowDate)
      result = StringFormat("%d.%s ", dt.day, month) + result;
   result = "'" + result;   

   return result;
}

//
// GUI wrapper function 
// https://www.mql5.com/en/docs/constants/objectconstants/enum_object/obj_text
//

bool TextCreate(const long              chart_ID=0,               // chart's ID 
                const string            name="Text",              // object name 
                const int               sub_window=0,             // subwindow index 
                datetime                time=0,                   // anchor point time 
                double                  price=0,                  // anchor point price 
                const string            text="Text",              // the text itself 
                const string            font="Calibri",           // font 
                const int               font_size=9,              // font size 
                const color             clr=clrWhiteSmoke,        // color 
                const double            angle=0.0,                // text slope 
                const ENUM_ANCHOR_POINT anchor=ANCHOR_LEFT_UPPER, // anchor type 
                const bool              back=false,               // in the background 
                const bool              selection=false,          // highlight to move 
                const bool              hidden=true,              // hidden in the object list 
                const long              z_order=0)                // priority for mouse click 
{ 
//--- reset the error value 
   ResetLastError(); 
//--- create Text object 
   if(!ObjectCreate(chart_ID,name,OBJ_TEXT,sub_window,time,price)) 
   { 
      Print(__FUNCTION__,": failed to create \"Text\" object! Error code = ",GetLastError()); 
      return(false); 
   } 
//--- set the text 
   ObjectSetString(chart_ID,name,OBJPROP_TEXT,text); 
//--- set text font 
   ObjectSetString(chart_ID,name,OBJPROP_FONT,font); 
//--- set font size 
   ObjectSetInteger(chart_ID,name,OBJPROP_FONTSIZE,font_size); 
//--- set the slope angle of the text 
   ObjectSetDouble(chart_ID,name,OBJPROP_ANGLE,angle); 
//--- set anchor type 
   ObjectSetInteger(chart_ID,name,OBJPROP_ANCHOR,anchor); 
//--- set color 
   ObjectSetInteger(chart_ID,name,OBJPROP_COLOR,clr); 
//--- display in the foreground (false) or background (true) 
   ObjectSetInteger(chart_ID,name,OBJPROP_BACK,back); 
//--- enable (true) or disable (false) the mode of moving the object by mouse 
   ObjectSetInteger(chart_ID,name,OBJPROP_SELECTABLE,selection); 
   ObjectSetInteger(chart_ID,name,OBJPROP_SELECTED,selection); 
//--- hide (true) or display (false) graphical object name in the object list 
   ObjectSetInteger(chart_ID,name,OBJPROP_HIDDEN,hidden); 
//--- set the priority for receiving the event of a mouse click in the chart 
   ObjectSetInteger(chart_ID,name,OBJPROP_ZORDER,z_order); 
//--- switch off tooltips   
   ObjectSetString(chart_ID,name,OBJPROP_TOOLTIP,"\n");
//--- successful execution 
   return(true); 
} 
  
  