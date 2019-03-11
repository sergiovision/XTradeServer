//+------------------------------------------------------------------+
//|                                                 ITradeService.mqh|
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\GenericTypes.mqh>
#include <XTrade\Signals.mqh>

#define MAX_NEWS_PER_DAY  5

class SettingsFile;

class ITradeService
{

protected:
   bool   isActive;
   long   magic;
   bool   isMaster;
   //SettingsFile* set;
public:
   string fileName;
   string IniFilePath;
   string EAName;
   string sym;
   Constants constant;
   ushort sep;   
   ushort sepList; 
   bool IsEA;

   ITradeService(short Port, string EA)
   {
      sep = StringGetCharacter(constant.PARAMS_SEPARATOR, 0);
      sepList = StringGetCharacter(constant.LIST_SEPARATOR, 0);
      magic = DEFAULT_MAGIC_NUMBER;
      EAName = EA;
      sym = "";
      isActive = false;
      //set = NULL;
      isMaster = false;
      //IsEA = true;
   }
   
   virtual bool Init(bool isEA)
   {
       IsEA = isEA;
       return true;
   }
   
   virtual long MagicNumber() {
      return magic;
   }
   
   virtual bool IsMaster() const {
      return isMaster;
   }
   
   virtual void SaveAllSettings(string strExpertData, string strOrdersData) 
   {
   }

   
   virtual void CallLoadParams(CJAVal* pars) {
   }
   
   virtual string CallStoreParamsFunc() {
      return "";
   }
   
   virtual string Name() { return EAName; }
   virtual bool CheckActive() { return isActive;}
   virtual bool IsActive() { return isActive; }
   virtual int  GetTodayNews(ushort Importance, SignalNews &arr[], datetime curtime)
   {
      return -1;
   }
   
   virtual Signal* ListenSignal(long flags, long ObjectId) { return NULL; };
   
   virtual SignalNews* GetLastNewsEvent() {return NULL; }
   
   virtual bool GetNextNewsEvent(ushort Importance, SignalNews& eventInfo) 
   {
      return false;
   }
   
   virtual void Log(string message)
   {
      //Print(message);
   }
   
   virtual void ProcessSignals() {   
      // TODO: Implement local signals QUEUE   
   }
   
   virtual uint DeInit(int Reason)
   {
       return INIT_SUCCEEDED;
   }
   
   //virtual SettingsFile* Settings()
   //{
   //    return NULL;
   //}
   
   virtual string GetProfileString(string lpSection, string lpKey)
   {
      return "";
   }
   
   virtual bool WriteProfileString(string lpSection, string lpKey,string lpValue)
   {
      return false;
   }
   
   virtual string FileName()
   {
      return fileName;
   }

   virtual string FilePath()
   {
      return IniFilePath;
   }

   virtual void InitNewsVariables(string strMagic) 
   {
      
   }
   
   virtual void SetGlobalNewsSignal()
   {
       
   }
   
   virtual void PostSignal(Signal* s) {
      PostSignalLocally(s);
   }
   
   virtual void PostSignalLocally(Signal* signal)
   {
      if (IsEA) 
      {
         ushort event_id = (ushort)signal.type;
         EventChartCustom(Utils.Trade().ChartId(), event_id, signal.ObjectId, signal.Value, signal.Serialize());
         DELETE_PTR(signal);
      } 
   }
   
   //void NotifyUpdatePositions()
   //{ 
   //   if (Utils.IsTesting())
   //       return;
   //   Signal* signal = new Signal(SignalToServer, SIGNAL_ACTIVE_ORDERS, this.MagicNumber());
   //   PostSignalLocally(signal);
   //}
};

