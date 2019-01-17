//+------------------------------------------------------------------+
//|                                                 NewsIndicator.mqh|
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict


#include <XTrade\IUtils.mqh>
#include <XTrade\TradeConnector.mqh>
#include <XTrade\NewsPanel.mqh>

#ifdef __MQL5__
#property indicator_applied_price PRICE_CLOSE
#endif

//#property indicator_separate_window
#property indicator_chart_window

#property indicator_width1    1
#property indicator_buffers   1
#property indicator_plots     1

//--- input parameters
input int      Magic = FAKE_MAGICNUMBER;
input int      ThriftPort = 2010;
input int      NewsImportance = 1;
input ENUM_TRADE_PANEL_SIZE   PanelSize = PanelNormal;
input int      SubWindow = 0;

ENUM_TIMEFRAMES PeriodTF = PERIOD_CURRENT;

double ExtSentBuffer[];

ITradeService* thrift = NULL;
NewsPanel* newsPanel = NULL;

//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int OnInit()
{
   string name = "NewsIndicator";
   Utils = CreateUtils((short)ThriftPort, name);  

   //--- indicator buffers mapping
   PeriodTF = (ENUM_TIMEFRAMES)Period();
   
   SetIndexBuffer(0, ExtSentBuffer, INDICATOR_DATA);      
   ArrayInitialize(ExtSentBuffer, 1);
   //string IndicatorName = "NewsIndicator";
   
   thrift = Utils.Service();
   if (thrift == NULL)
      return INIT_FAILED;
   
   if (!thrift.Init(false))
      return INIT_FAILED;

   string strMagic = IntegerToString(Magic);
   thrift.InitNewsVariables(strMagic);
      
#ifdef __MQL5__   
   IndicatorSetString(INDICATOR_SHORTNAME, name);
#else 
   IndicatorShortName(name); 
#endif       
   IndicatorSetInteger(INDICATOR_DIGITS, Digits());

   int realWindow = SubWindow;
   if (SubWindow == -1)
     realWindow = ChartWindowFind(ChartID(), name);
   newsPanel = new NewsPanel(PanelSize, NewsImportance, realWindow, PeriodTF);
   newsPanel.NewsStatString = StringFormat("%s(%d)", thrift.Name(), Magic);   
   newsPanel.Init(Magic, ThriftPort, NewsImportance);
   Utils.Info(StringFormat("%s Init: Magic=%d, Port=%d Imp=%d Panel=%s Chart=%d SubWin=%d",
    name, Magic, ThriftPort, NewsImportance, EnumToString(PanelSize), newsPanel.chartID, newsPanel.subWin));

   return(INIT_SUCCEEDED);
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
//---
   if (thrift == NULL)
      return rates_total;
     
   ArraySetAsSeries(time, true);   
   ArraySetAsSeries(ExtSentBuffer, true);
//--- preliminary calculations
   /*int i,limit;
   if(prev_calculated==0)
     {
      ExtSentBuffer[0]=1;
      //--- filling out the array of True Range values for each period
      //for(i=0; i<1; i++)
      //   ExtSentBuffer[i]=1;
      limit=1;
     }
   else
      limit=prev_calculated-1;
//--- the main loop of calculations
   */
   //for(i=limit; i<rates_total; i++)
   //{
   //   ExtSentBuffer[i]=0;
   //}
   //Utils.Info(StringFormat("News Calculating for time %s", TimeToString(time[0])));
   if (newsPanel != NULL)
   {
     newsPanel.ObtainNews(time[0]);
     newsPanel.Draw();
   }
   
//--- return value of prev_calculated for next call
   return(rates_total);
}
//+------------------------------------------------------------------+
//| ChartEvent function                                              |
//+------------------------------------------------------------------+
void OnChartEvent(const int id,
                  const long &lparam,
                  const double &dparam,
                  const string &sparam)
{
//---
   if (newsPanel != NULL)
   {
      newsPanel.OnEvent(id, lparam, dparam, sparam);
   } else 
   {
/*      if(id==CHARTEVENT_CHART_CHANGE)
      {
      }*/

   }

}
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
{
   DELETE_PTR(newsPanel)

   if (thrift != NULL)
      thrift.DeInit(reason);
   
   DELETE_PTR(Utils);   
}
