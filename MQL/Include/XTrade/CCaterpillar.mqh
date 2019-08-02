//+------------------------------------------------------------------+
//|                                                     CSSAMode.mqh |
//|                               Copyright 2016, Roman Korotchenko  |
//|                            https://login.mql5.com/ru/users/Lizar |
//|                                             Revision 01 Jun 2016 |
//+------------------------------------------------------------------+
#property copyright   "Copyright 2016, Roman Korotchenko"
#property link        "https://login.mql5.com/ru/users/Solitonic"
#property version     "1.00"

#include <Math\Alglib\linalg.mqh>
//#include <SSA\FileDataPrinter.mqh>
//+-------------------------------------------------------------------+
//| Class CCaterpillar. Singular Specral Analysis. Mode Decompositinon|
//| Appointment: класс предназначен для разложения временного ряда    |
//|    по эмпирическим собственным функциям (с.ф.)с целью выделения   |
//|    "сезонных" (разномасштабных по времени) составляющих.          |
//|    Выполняет декомпозицию/реконструкцию вкладов от разночастотных |
//|    аддитивных компонент и удаление "шума" в составе данных.       |
//|    Заданное количество с.ф. выделяет подпространство сигнала и    |
//|    фильтрует "шум". Фрагмент и блок (лаг)обработки ряда определяют|
//|    гладкость восстанавливаемого сигнала.                          |
//|                                                                   |
//| Link: http://www.mql5.com/ru/code/???                             |
//|                                                                   |
//| Remark: может использоваться в качестве замены скользящей средней.|  
//|  Важное отличие от MA:  не запаздывает, а согласован по времени   |
//+-------------------------------------------------------------------+
//| Acknowledgment: особая благодарность Сергею Бочканову за          |
//| предоставленную замечательную библиотеку численных                |
//| методов ALGLIB.                                                   |
//| https://www.mql5.com/ru/code/1146                                 |
//+-------------------------------------------------------------------+
#define SERIA_DFLT_LENGTH 32768    // Резервируемый по-умолчанию объем выделенной памяти;

class CCaterpillar
  {
private:
   double MASHEPSILON;
   double Shift0;
   int    SignalEigNoiseLimit;
   double EigNoiseValue;
   

   int EIF_NUM_MAX;  // предельное число функций для тестирования максимального "разумного" количества в разложении (EstimateEigenMaxLimit)
   double            m_series[];       // фрагмент данных   
   int               m_serlen;         // длина фрагмента
   int               m_Lag;            // длина блока 
   
   CMatrixDouble     mTraj;  // траекторная матрица
   CMatrixDouble     mU,mV;  // левые и правые собственные вектора
   double            LV[];
   int mTrajRow, mTrajCol;
   int mEigVectorCount;  // выделено векторов при расчете
   int mEigValueCount;   // ограниченное число исходя из уровня шума 
   int mEigValueCtrl;    // разрешим использовать число по условию выбора фиксированного набора или заданного уровня шума
   
   CMatrixDouble     mS;             // Матрица для группировки в выбранном поддиапазоне собственных значений (восстановленнная траекторная матрица)
	double            vR[], EigenValues[]; // Вектор для выделения сигнала (тренда) при антидиагональном усредении
  //svd params (ALGLIB)
	int uneeded ;
	int vtneeded;
   int addmem;
	//----
   
   int  TrajectoryMatrixFill(double &ser[], const int serlen, const int lag);
   int  SVD(double weightEigenValuesInPercent);  // Сингулярное разложение траекторной матрицы. Возвращает число с.ф. удовл. условию ограничения   
   
   void Grouping(int eigN0, int eigN1); // Группировка в поддиапазоне собственных векторов - реконструкция траекторной матрицы
   void Grouping(int eigNum) { Grouping(0, eigNum-1); }
   int  SkewDiagAvr();
   int  RestoreTrend();
   
   
   double datCoeff, firstValue, lastValue, AmplValue;
   double m_convData[];
   
   
   void  ForwardDataConversion (double &seria[],int serLength, double &convData[], int convMode);  
   void  BackwardDataConversion(double &data[], int datLength, double &result[],  int convMode);
   
   
 //  CFileDataPrinter DataPrinter;
   
   
public:

   int    convMode;
   int    ModeNUM1, ModeNUM2; // Границы набора мод
   double Trend[];
   
                     CCaterpillar();
                     
                    ~CCaterpillar();
                    
   void  TrendSize(int segment, int lag);    
   void  SetNoiseRegime(int flag, double PercentValue);             
   int   DoAnalyse(double &segmData[], int segmlen, int lag,   int eigMin, int eigMax);
   
   
  };
//------------------------------------------------------------------

CCaterpillar::CCaterpillar()
  {
   datCoeff  = 1;
   convMode  = 1; // сдвиг начала ряда к нулю
      
   MASHEPSILON = 1e-15; // условный "ноль" для отсечки расчетных погрешностей
   
   SignalEigNoiseLimit = 0;   // флаг для "отсечки" шумовых колебаний в составе временного ряда числом мод (0) или процентами (1)
   EigNoiseValue       = 2;   // Уровень шума 2% по-умолчанию
   
   EIF_NUM_MAX = 400;          // максимально допустимое число мод (обычно хватает 5-8 шт)
      
   uneeded  = 1;
	vtneeded = 1;
   addmem   = 2;
   
   m_serlen = 1024;
   ArrayResize(m_series  ,m_serlen, SERIA_DFLT_LENGTH);
   ArrayResize(m_convData,m_serlen, SERIA_DFLT_LENGTH);
   ArrayResize(vR, m_serlen, SERIA_DFLT_LENGTH);
   ArrayResize(Trend, m_serlen, SERIA_DFLT_LENGTH);
   
   ModeNUM1 = 1;
   ModeNUM2 = 10;
   mTrajRow = 0; 
	mTrajCol = 0;
  }
//------------------------------------------------------------------

void CCaterpillar::TrendSize(int segment, int lag)
{
   m_serlen = segment;
   m_Lag = lag;
   
   ArrayResize(m_series  ,m_serlen, SERIA_DFLT_LENGTH);
   ArrayResize(m_convData,m_serlen, SERIA_DFLT_LENGTH);
   ArrayResize(vR,        m_serlen, SERIA_DFLT_LENGTH);
   ArrayResize(Trend,     m_serlen, SERIA_DFLT_LENGTH);
}

CCaterpillar::~CCaterpillar()
  {
    ArrayFree(m_series);
    ArrayFree(m_convData);
    ArrayFree(vR);    
    ArrayFree(Trend);    
  }
//------------------------------------------------------------------+

int  CCaterpillar::TrajectoryMatrixFill(double &ser[], const int serlen, const int lag)
{// данные уже трансформированы. 1-й элемент = 0.
	m_Lag = lag;	
	if (ArraySize(ser)<2 || serlen<2) return 0;
	
	int N = serlen, L = lag, K = N - L + 1;

	 if( mTrajRow < L ||  mTrajCol < K) mTraj.Resize(L, K);
	 mTrajRow = L; 
	 mTrajCol = K;

	for (int j = 0; j < L; j++) // Цикл по строкам
	{
		for (int k = 0; k < K; k++) // по столбцам
		{		
			mTraj[j].Set(k, ser[k + j]);
		}
	}

	//			CFileDataPrinter::writeMatrix_("Data//TRJ.csv", mTraj);

	return K;  // Число столбцов
}
//---------------------------------------------------------------------------------------

int  CCaterpillar::SVD(double weightEigenValuesInPercent)  // Сингулярное разложение траекторной матрицы. Возвращает число с.ф. удовл. условию ограничения
{
   if( mU.Size() < mTrajRow  ||  mU[0].Size() < mTrajCol ) mU.Resize(mTrajRow, mTrajCol);
   if( mV.Size() < mTrajCol  ||  mU[0].Size() < mTrajRow ) mV.Resize(mTrajCol, mTrajRow);
   	 
	CSingValueDecompose::RMatrixSVD(mTraj, mTrajRow, mTrajCol, uneeded, vtneeded, addmem, LV, mU, mV);
	
 // Посчитаем число с.зн., отличных от "нуля". Вклад от др. С.Ф. дает ошибку
	
	int NMax = MathMin(mTrajRow,mTrajCol); // Возможное число векторов
   double sumVal = 0;
 
   mEigVectorCount = 0;  
	for(int i=0; i<NMax; i++)
	{
	  if( fabs(LV[i]) > MASHEPSILON) 
	  { 
	    mEigVectorCount++;
	    sumVal += LV[i];
	  }
	}
	
 //Максимально возможное (правильно вычисленные, с ненулевыми с.зн.) число векторов
 if(weightEigenValuesInPercent < MASHEPSILON )  return mEigVectorCount;

  // Анализируем с.зн. и заданное ограничение
  double minLevel  = weightEigenValuesInPercent*0.01, curLevel; // Из процентов к долям
  double cum;
  
 
	mEigValueCount    = mEigVectorCount;
	cum = 0;
	for (int i = 0; i < mEigVectorCount; i++)
	{
	  if(SignalEigNoiseLimit==1 && LV[i]/ sumVal < minLevel )  // "энергия" отдельного вектора "не значима"
	  { 
		  mEigValueCount = i;   // кол-во векторов с  вкладом превышающим шум
		  break;		            // завершим этот цикл
	  }
	  
	  if(SignalEigNoiseLimit==2) // оценка "кумулятивной энергии"
	   {
	   cum = cum+LV[i];         
	   curLevel = cum / sumVal; // вклад в "общее дело"
		if ((1-curLevel) < minLevel) { // завершим этот цикл
		  mEigValueCount = i;          // кол-во векторов с нужным суммарный вкладом   
		  break;
		}}
	   	   
	  
	}
   return mEigValueCount;  // ограниченное по уровню шума число с.ф.
}
//---------------------------------------------------------------------------------------


void CCaterpillar::Grouping(int eigN0, int eigN1) // Группировка в поддиапазоне собственных векторов - реконструкция траекторной матрицы
{
	int eigMax = mEigVectorCount;
	int Unrow = mTrajRow;
	int Vnrow = mTrajCol;      
	
	if (eigN1 > eigMax-1) eigN1 = eigMax-1;        // ограничение

	if( mS.Size() < mTrajRow  ||  mS[0].Size() < mTrajCol )	mS.Resize(Unrow, Vnrow);

	double sum;
	for (int m = 0; m < Unrow; m++)
	{
		for (int n = 0; n < Vnrow; n++){
			sum = 0;
			for (int k = eigN0; k <= eigN1; k++)
			{				
				sum = sum + mU[m][k]* mV[k][n] * LV[k];    
			}
			mS[m].Set(n,sum);
		}
	}

	 //							writeMatrix_("C:/Temp/@/_ALGLIBgrouping.X", mS);

}
//-----------------------------------------------------------------------------------------

int  CCaterpillar::SkewDiagAvr()
{// антидиагональное усреднение
	int nrow = mTrajRow,
		 ncol = mTrajCol;
		 
	int L = nrow; 
	int K = ncol; 

	int N = nrow + ncol-1; // длина V
	int k, j, flagTransp = 0;

	double s;


	if (nrow > ncol){ // транспонируем матрицу
		flagTransp = 1;
		L = ncol;
		K = nrow;
	}

	//string fn = "C:/TEMP/@/skew.s"; 	FILE* outfp;// = fopen(fn.c_str(), "wt"); 	int err = fopen_s(&outfp, fn.c_str(), "wt");
   if( ArraySize(vR)<N ) 	ArrayResize(vR, 2*N, SERIA_DFLT_LENGTH);
	
	for ( k = 1; k <= L; k++){
		s = 0;
		for (j = 1; j <= k; j++){
			if (!flagTransp)  s += mS[j-1][k-j];
			else              s += mS[k - j][j - 1];
		}
		s = s / k;
		vR[k - 1] =  s;
	}
	//---------------------------------------------------------------------
	for (k = L+1; k < K; k++)
	{
		s = 0;
		for (j = 1; j <= L; j++){
			if (!flagTransp)  s += mS[j - 1][k - j];
			else              s += mS[k - j][j - 1];
		}
		s = s / L;
		vR[k - 1] =  s;
		// fprintf(outfp, "%d %lf\n", k - 1, s);
	}
	//---------------------------------------------------------------------
	for (k = K; k <= N; k++)	{
		s = 0;
		for (j = k-K+1; j <= L; j++){
			if (!flagTransp) s += mS[j - 1][k - j];
			else             s += mS[k - j][j - 1];
		}
		s = s / (N - k + 1);
		vR[k-1] =  s ;
		// fprintf(outfp, "%d %lf\n", k - 1, s);
	}
	return N;
}
//-----------------------------------------------------------------------------------------

void  CCaterpillar::SetNoiseRegime(int flag, double PercentValue)
{
 //ограничение числа с.ф. в модели
 SignalEigNoiseLimit = flag;
		EigNoiseValue  = PercentValue;   // В ПРОЦЕНТАХ!
}
//-----------------------------------------------------------------------------------------

int CCaterpillar::DoAnalyse(double &segmData[], int segmlen, int lag, int eigMin, int eigMax)  
{	
	double evLimPercent;
	
		ModeNUM1 = eigMin;
		ModeNUM2 = eigMax;
		
		ForwardDataConversion(segmData,   segmlen, m_convData,  convMode); // Переходим к конвертированным данным
		TrajectoryMatrixFill (m_convData, segmlen, lag);  // ТРАЕКТОРНАЯ МАТРИЦА

	 //ограничение числа с.ф. в модели
		evLimPercent  = (SignalEigNoiseLimit)? EigNoiseValue : 0.0;   // В ПРОЦЕНТАХ!
		mEigValueCtrl = SVD(evLimPercent); // будет разрешено использовать количество векторов в разложении
		
		if(mEigValueCtrl >= EIF_NUM_MAX)
		{
		  mEigValueCtrl = EIF_NUM_MAX; // ОБЩЕЕ ОГРАНИЧЕНИЕ 
		}

        
		if( ModeNUM2 >= mEigValueCtrl) // ПОЛЬЗОВАТЕЛЬСКИЙ ПАРАМЕТР ЧИСЛА С.Ф. ЗАДАН НЕВЕРНО!
		{ // Установим предельное значение согласно используемому ограничению
			   ModeNUM2 = mEigValueCtrl; // ВВОДИМ ОГРАНИЧЕНИЕ!			   
		}

		if( ArraySize(EigenValues)<mEigValueCtrl ) ArrayResize(EigenValues, 2*mEigValueCtrl, EIF_NUM_MAX);		
		for (int k = 0; k < mEigValueCtrl; k++) EigenValues[k] = LV[k];
			
		RestoreTrend();
		
			
		BackwardDataConversion(vR, segmlen, Trend, convMode);
		
	 //  CFileDataPrinter::writeVector_("Data//TREND.csv",Trend) ;
	   
	return 1;
}
//-----------------------------------------------------------------------------------------

int  CCaterpillar:: RestoreTrend()
{ // ИСПОЛЬЗУЕТСЯ ПОСЛЕДНИЙ НАБОР {U,L,V}
   int efMin, efMax, N;

	efMin = ModeNUM1-1;
	efMax = ModeNUM2-1;
  // Группируем выборочный набор
	Grouping(efMin, efMax);
	N = SkewDiagAvr(); // расчитываем тренд/сигнал vR[]
	return N;
}
//-----------------------------------------------------------------------------------------

void  CCaterpillar::ForwardDataConversion(double &seria[], int serLength, double &convData[], int Mode)
{
	int i;
	
	if( ArraySize(convData)<serLength ) ArrayResize(convData,2*serLength, SERIA_DFLT_LENGTH);
	
   lastValue = seria[serLength - 1];

	switch (Mode)
	{
		case 0:
			for (i = 0; i < serLength; i++)  convData[i] = datCoeff*seria[i];
		  break;

		case 1:  	// нормализация и сдвиг на величину первого элемента ряда
			
			AmplValue  = seria[0]; 
			for (i = 1; i < serLength; i++) {
			   AmplValue  = MathMax(AmplValue,seria[i]);
			}
			
			firstValue = seria[0]/AmplValue;
			for (i = 0; i < serLength; i++) {
				convData[i] = (seria[i]/AmplValue - firstValue);								
			}
			break;
    }
    
}
//-----------------------------------------------------------------------------------------

void  CCaterpillar::BackwardDataConversion(double &data[], int datlen, double &result[], int Mode)
{
 	int i;
   if( ArraySize(result)<datlen ) ArrayResize(result,2*datlen,SERIA_DFLT_LENGTH);
   
	switch (Mode)
	{
	case 0:
		for (i = 0; i < datlen; i++)  result[i] = data[i]/datCoeff;
		break;

	case 1:	// ренормализация 
		for (i = 0; i < datlen; i++) {
			result[i]  =  (firstValue + data[i]) * AmplValue;
		}
		break;
   }
}
//-----------------------------------------------------------------------------------------


