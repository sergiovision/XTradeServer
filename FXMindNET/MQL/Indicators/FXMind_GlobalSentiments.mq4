#property copyright "Copyright FXMind 2013"
#property link      "http://"

#include <ThriftClient.mqh>

//property indicator_chart_window
#property indicator_separate_window
#property indicator_minimum 0
#property indicator_maximum 100
#property indicator_buffers 8
#property indicator_color1 Green
#property indicator_color2 DarkBlue
#property indicator_color3 Red
#property indicator_color4 DarkOrange  
#property indicator_color5 Silver 
#property indicator_color6 Purple
#property indicator_color7 Maroon
#property indicator_color8 Teal

//#property show_inputs
#property strict

extern bool AverageOnly = true;
extern ushort ThriftPORT = 2010;

color Color_EToro = Green;
color Color_MyFXBook = DarkBlue;
color Color_OANDA = Silver;
color Color_Average = Red;
int   MagicNumber = 21212121;
int   Line_Thickness = 2;
int   GAP_VALUE = -125;

//--- buffers
double ExtMapBuffer1[];
double ExtMapBuffer2[];
double ExtMapBuffer3[];
double ExtMapBuffer4[];
//double ExtMapBuffer5[];
//double ExtMapBuffer6[];
//double ExtMapBuffer7[];
//double ExtMapBuffer8[];

bool Initialized = false;

//+------------------------------------------------------------------+   
//+------------------------------------------------------------------+   
ThriftClient* thrift = NULL;

int OnInit()
{
  	thrift = new ThriftClient(AccountNumber(), ThriftPORT, MagicNumber);
  	thrift.CheckActive();
  	thrift.PostMessage("OnInit FXMind.GlobalSentiments Indicator. AverageOnly = " + AverageOnly);
  	
	IndicatorShortName("FXMind.GlobalSentiments");

   int width = Line_Thickness;
   
   if (AverageOnly == false) { 
      SetIndexStyle(0, DRAW_LINE, DRAW_LINE, width, Color_EToro);
      SetIndexBuffer(0, ExtMapBuffer1);
      SetIndexEmptyValue(0, GAP_VALUE);
      SetIndexLabel(0, "EToro"); 
   
      SetIndexStyle(1, DRAW_LINE, DRAW_LINE, width, Color_MyFXBook);
      SetIndexBuffer(1, ExtMapBuffer2);
      SetIndexEmptyValue(1, GAP_VALUE);
      SetIndexLabel(1, "MyFXBook"); 
   
      SetIndexStyle(2, DRAW_LINE, DRAW_LINE, width, Color_OANDA);
      SetIndexBuffer(2, ExtMapBuffer3);
      SetIndexEmptyValue(2, GAP_VALUE);
      SetIndexLabel(2, "OANDA"); 
   } else { 
      SetIndexStyle(3, DRAW_LINE, DRAW_LINE, width, Color_Average);
      SetIndexBuffer(3, ExtMapBuffer4);
      SetIndexEmptyValue(3, GAP_VALUE);
      SetIndexLabel(3, "Average");   
   } 
   
   ChartRedraw();
   return (0);
}

int OnCalculate(const int rates_total,
                const int prev_calculated,
                const datetime& time[],
                const double& open[],
                const double& high[],
                const double& low[],
                const double& close[],
                const long& tick_volume[],
                const long& volume[],
                const int& spread[])
{

   int first;
   int nVizCount = (int)ChartGetInteger(0, CHART_VISIBLE_BARS);
   int limit = nVizCount;
   if (prev_calculated == 0) // проверка на первый старт расчёта индикатора
   {
      first = rates_total - limit - 1; // стартовый номер для расчёта всех баров
   }
   else
   {
      first = prev_calculated - 1; // стартовый номер для расчёта новых баров
      limit = rates_total - first;
   }
   first = 0;  
   int timeframe = Period();
   string symbolName = Symbol();
   int size = ArraySize(ExtMapBuffer1);

   if ( AverageOnly == false ) { 
      //thrift.PostMessage("FXMind.GlobalSentiments calculate EToro Values.");
      //ArrayFill(ExtMapBuffer1, first, rates_total, 20); 
      thrift.GetSentimentsArray(symbolName, first, limit, 2, time, ExtMapBuffer1); 
      //thrift.PostMessage("FXMind.GlobalSentiments calculate MyFXBook Values.");
      //ArrayFill(ExtMapBuffer2, first, rates_total, 40); 
      thrift.GetSentimentsArray(symbolName, first, limit, 4, time, ExtMapBuffer2); 
      //thrift.PostMessage("FXMind.GlobalSentiments calculate OANDA Values.");
      //ArrayFill(ExtMapBuffer3, first, limit, 50); 
      thrift.GetSentimentsArray(symbolName, first, limit, 5, time, ExtMapBuffer3); 
   } else {
      //thrift.PostMessage("FXMind.GlobalSentiments calculate Average Values.");
      // ArrayFill(ExtMapBuffer4, first, limit, 50); 
      thrift.GetSentimentsArray(symbolName, first, limit, 0, time, ExtMapBuffer4); 
   }
   return(rates_total);
}

void OnDeinit(const int reason)
{
   if (thrift != NULL)
      delete thrift;
	//return (0);
}

