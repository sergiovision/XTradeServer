//+------------------------------------------------------------------+
//|                                                 ThriftClient.mqh |
//|                                                 Sergei Zhuravlev |
//|                                   http://github.com/sergiovision |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <FXMind\GenericTypes.mqh>
#include <FXMind\Signals.mqh>

#define MAX_NEWS_PER_DAY  5
class SettingsFile;

class IFXMindService
{

protected:
   bool   isActive;
   long magic;
   SettingsFile* set;
public:
   string fileName;
   string IniFilePath;
   string EAName;
   string sym;
   Constants constant;
   ushort sep;   
   ushort sepList; 

   IFXMindService(short Port, string EA)
   {
      sep = StringGetCharacter(constant.PARAMS_SEPARATOR, 0);
      sepList = StringGetCharacter(constant.LIST_SEPARATOR, 0);
      magic = DEFAULT_MAGIC_NUMBER;
      EAName = EA;
      sym = "";
      isActive = false;
      set = NULL;
      
   }

   virtual bool Init(bool isEA)
   {
       return true;
   }
   
   virtual long MagicNumber() {
      return magic;
   }
   
   virtual string Name() { return EAName; }
   virtual bool CheckActive() { return isActive;}
   virtual bool IsActive() { return isActive; }
   virtual int  GetTodayNews(ushort Importance, SignalNews &arr[], datetime curtime)
   {
      return -1;
   }
   
   virtual SignalNews* GetLastNewsEvent() {return NULL; }
   
   virtual bool GetNextNewsEvent(ushort Importance, SignalNews& eventInfo) 
   {
      return false;
   }
   virtual int  GetCurrentSentiments(double& longVal, double& shortVal)
   {
      return 0;
   }
   virtual long GetSentimentsArray(int offset, int limit, int site, const datetime& times[], double &arr[])
   {
       return 0;
   }
   virtual long GetCurrencyStrengthArray(string currency, int offset, int limit, int timeframe, const datetime& times[], double &arr[])
   {
      return 0;
   }
   virtual void PostMessage(string message)
   {
      //Print(message);
   }     
   virtual void SaveAllSettings(string ActiveOrdersList)
   {
       
   }
   
   virtual uint DeInit(int Reason)
   {
       return INIT_SUCCEEDED;
   }
   
   virtual SettingsFile* Settings()
   {
       return set;
   }
   
   virtual string GetProfileString(string lpSection, string lpKey)
   {
      return "";
   }
   
   virtual long WriteProfileString(string lpSection, string lpKey,string lpValue)
   {
      return 0;
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
   


};

