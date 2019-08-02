//+------------------------------------------------------------------+
//|                                                   SingularMA.mq5 |
//|                               Copyright 2016, Roman Korotchenko  |
//|                         https://login.mql5.comru/users/Solitonic |
//|                                             Revision 26 Jun 2016 |
//+------------------------------------------------------------------+


#property copyright   "Copyright 2016, Roman Korotchenko"
#property link        "https://login.mql5.com/ru/users/Solitonic"

#property version   "1.00"
#property indicator_chart_window //---- отрисовка индикатора в основном окне
#property indicator_buffers 1    //---- для расчёта и отрисовки индикатора 
#property indicator_plots   1    //---- использовано всего одно графическое построение
//--- plot Trend
#property indicator_label1  "Trend SSA"
#property indicator_type1   DRAW_LINE
#property indicator_color1  clrBlue
#property indicator_style1  STYLE_SOLID
#property indicator_width1  3  //---- толщина линии индикатора равна 

#include <XTrade\CCaterpillar.mqh>

//--- input parameters
input ENUM_TIMEFRAMES period= PERIOD_CURRENT;               // Calculation period to build the chart
input ENUM_TIMEFRAMES period_to_redraw=PERIOD_M3;           // Refresh period chart



input int      SegmentLength=120;    // Фрагмент истории
input int      SegmentLag=50;        // Окно (в пределах от 1/4 до 1/2 длины сегмента)


input int      EigMax=10;            // Число мод (размерность подпространства сигнала. "Вне" - ошибки)

input double   EigNoiseLevel=2.0;    // Допускаемый процент вклада шума в суммарную "энергию колебаний" ряда
input int      EigNoiseFlag =0;      // Метод ограничения числа мод
                                     // 0 - указана размерность пространства сигнала [EigMin,EigMax]. EigNoiseLevel игнорируется.
                                     // 1 - подбирается исходя из заданной ошибки EigNoiseLevel для отдельной функции  в составе сигнала.
                                     // 2 - подбирается исходя из заданной ошибки EigNoiseLevel для набора функций  в составе сигнала.
                                     


//--- indicator buffers
double         TrendBuffer[];
double         ResultBuffer[];
//-- класс расчета SSA - тренда
CCaterpillar   Caterpillar;

//--- вспомогательные переменные
int      EigMin=1;
double   wrkData[];
int      OldSegmentLength;
int      OldSegmentLag;
//
datetime start_data;           // Start time to build the chart
datetime stop_data;      // Текущее время

//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int OnInit()
  {
//--- indicator buffers mapping
   SetIndexBuffer(0,TrendBuffer,INDICATOR_DATA); 
   //ArraySetAsSeries(TrendBuffer,true);
   
   //--- добавляем буфер для копирования данных о ценах, дла расчета
   // SetIndexBuffer(1,ResultBuffer,INDICATOR_CALCULATIONS);
  
  
  OldSegmentLength = 0;
  OldSegmentLag    = 0;
  
  ArrayResize(wrkData  ,SegmentLength, SERIA_DFLT_LENGTH);
  ArrayResize(ResultBuffer,SegmentLength, SERIA_DFLT_LENGTH);
   
//--- графика    
   string shortname;
   StringConcatenate(shortname, "SSA(", SegmentLength, ",", SegmentLag, ")", "C.Ф. 1-", EigMax);
   //--- создание метки для отображения в DataWindow
   PlotIndexSetString(0, PLOT_LABEL, shortname);   
   
   
   return(INIT_SUCCEEDED);
  }
  
//+------------------------------------------------------------------+
//| Custom indicator iteration function                              |
//+------------------------------------------------------------------+
static int reCalcOn = 0, curCalcFinish = 1;
static int ReCalcLim = 7;
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
  int idx, nshift, ntime;
  
  int fEigMin = 1;      // Для тренда должна быть = 1
  int fEigMax = EigMax; // ограничение векторов. Гладкость
  
  //--- получим количество доступных баров для текущих символа и периода на графике  
 // int nbars=Bars(Symbol(),0); 
   
  if( rates_total < SegmentLength ) {   
      PrintFormat("Число данных меньше длины индикатора. Без расчета."); 
      return(0); 
  }

   
      reCalcOn++; 
   //if(reCalcOn != 1 )  curCalcFinish = 0; else  curCalcFinish = 1;
   curCalcFinish = (reCalcOn != 1 )? 0:1; // Будем считать тренд не каждый приход данных, а с периодом 
       reCalcOn  = MathMod(reCalcOn, ReCalcLim);  // каждые 7 новых отсчетов
    
         
   if (!curCalcFinish) // предыдущий расчет не закончен
   {
     if(prev_calculated != 0) { 
         ArrayCopy(TrendBuffer,ResultBuffer,rates_total-SegmentLength,0,SegmentLength);
     }
     else { // исторические данные были изменены
         ArrayFill (TrendBuffer,0, rates_total, EMPTY_VALUE); // Зачистка
     }     
    return(rates_total);  // Новые данные поступают быстрее расчета   
   }
   //---------------------------------------------------------------------------        
         
   //---- Изменилась длина тренда
   if(ArraySize(wrkData)< SegmentLength) 
   {   
     ArrayResize(wrkData  ,SegmentLength, SERIA_DFLT_LENGTH);            
     reCalcOn = 1;   
   }
   
   if(OldSegmentLength != SegmentLength ||   OldSegmentLag != SegmentLag)
   {
      Caterpillar.TrendSize(SegmentLength, SegmentLag);
      OldSegmentLength = SegmentLength;
      OldSegmentLag    = SegmentLag;                         
      reCalcOn = 1;
      
     if(ArraySize(ResultBuffer)< SegmentLength) {
       ArrayResize(ResultBuffer,2*SegmentLength, SERIA_DFLT_LENGTH);  
     }
     
     ArrayFill(ResultBuffer,0, SegmentLength, EMPTY_VALUE); // Зачистка
     ArrayFill(TrendBuffer, 0, rates_total, EMPTY_VALUE);   // Зачистка
   }             
   
 
   
   curCalcFinish = 0; // блокируем запрос нового рачета, пока не выполнен текущий
   
   ntime  = ArraySize(time);       // эквивалентно rates_total
   nshift = ntime - SegmentLength; // начальный индекс для конечного отрезка данных 
 
   for( int i=0; i<SegmentLength; i++) 
   {
     idx = i+nshift;
     wrkData[i] = (high[idx] + low[idx] + close[idx])/3;     
   }

 // EigNoiseFlag: 0 (указана размерность пространства сигнала) или 1,2 (подбирается исходя из допускаемого шума EigNoiseLevel)
 // если EigNoiseFlag = 1,2 EigNoiseLevel должна быть в процентах! Иначе этим параметром пренебрегается.
   Caterpillar.SetNoiseRegime(EigNoiseFlag, EigNoiseLevel);  
      
 // Разложение и восстановление в ограниченом подпространстве   
   Caterpillar.DoAnalyse(wrkData, SegmentLength, SegmentLag, fEigMin, fEigMax);     
          
       start_data  = time[nshift];   
       stop_data   = time[nshift+SegmentLength-1];
       
   ArrayFill(ResultBuffer,0, SegmentLength, EMPTY_VALUE);            
   
   for( int i= 0; i<SegmentLength; i++) 
   {      
      ResultBuffer[i] =  Caterpillar.Trend[i];//(high[i] + close[i])/2;//
   }                
   
   ArrayCopy(TrendBuffer,ResultBuffer,rates_total-SegmentLength,0,SegmentLength);
   
   ChartRedraw(0);    //--- периресовка графика
   curCalcFinish = 1; // возможен новый расчет
   return(rates_total);
 }
//+------------------------------------------------------------------+


