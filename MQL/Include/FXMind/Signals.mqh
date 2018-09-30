//+------------------------------------------------------------------+
//|                                                 TradeSignals.mqh |
//|                        Copyright 2018, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict


enum SignalType
{
   SignalQuiet,
   SignalBUY, 
   SignalSELL, 
   SignalNEWS,
   SignalCANDLE,
   SignalCLOSEBUYPOS, 
   SignalCLOSESELLPOS,   
   SignalCLOSEALL, 
};

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+

class Signal
{
protected:
    string Name;
    datetime RaiseDateTime; // date returned in MTFormat
public:
    bool Handled;
    int Value;
    SignalType type;
    bool UseAsFilter;

    Signal()
    {
       Init();
    }
    
    void SetName(string name)
    {
       Name = name;
    }
    
    void SetRaiseTime(datetime time)
    {
       RaiseDateTime = time;
    }
    
    string GetName() const { return Name;}

    datetime RaiseTime() const { return RaiseDateTime; }
    
    virtual void Init(bool isFilter = true)
    {
        RaiseDateTime = 0;
        Handled = false;
        Value = 0;
        UseAsFilter = isFilter;
        type = SignalQuiet;    
    }

    virtual bool OnAlert()
    {
       return (!UseAsFilter) && (!Handled) && (type != SignalQuiet);
    }
    
    virtual void operator=(const Signal &n) 
    {
        RaiseDateTime = n.RaiseDateTime;
        Handled = n.Handled;
        Value = n.Value;
        UseAsFilter = n.UseAsFilter;
        type = n.type;
   }
      
   virtual bool operator==(const Signal &n) const
   {
     if (Value != n.Value)
        return false;
     if (type != n.type)
        return false;
     if (UseAsFilter != n.UseAsFilter)
        return false;
     return true;
   }
   
   virtual string DateToString()
   {
      MqlDateTime mqlDate;
      TimeToStruct(RaiseDateTime, mqlDate);
      return StringFormat("%02d/%02d %02d:%02d", mqlDate.mon, mqlDate.day, mqlDate.hour, mqlDate.min);
   }
   
   virtual string ToString()
   {
      return Name;
   }

};

class  SignalNews : public Signal
{
public:
   string Currency;
   int    Importance;
   string Forecast;
  
   SignalNews()
   {
      Init();
   }   
  
   virtual void Init(bool isFilter = false)
   {
      Signal::Init(isFilter);
      UseAsFilter = false;
      
      Name = "No news";
      Currency = "";
      Importance =0;
      type = SignalNEWS;
   }

   virtual void operator=(const SignalNews &n) {
      type = n.type;
      Value = n.Value;
      UseAsFilter = n.UseAsFilter; 
      Currency = n.Currency;
      Importance = n.Importance;
      RaiseDateTime = n.RaiseDateTime;
      Name = n.Name;
   }
      
  virtual bool operator==(const SignalNews &n)
  {
     if (StringCompare(Name, n.Name)!=0)
        return false;
     if (RaiseDateTime != n.RaiseDateTime)
        return false;
     if (Importance != n.Importance)
        return false;
     if (StringCompare(Currency, n.Currency) !=0)
        return false;
     return true;
  }
  
   virtual bool operator!=(const SignalNews &n)
   {
     if (StringCompare(Name, n.Name)!=0)
        return true;
     if (RaiseDateTime != n.RaiseDateTime)
        return true;
     if (Importance != n.Importance)
        return true;
     if (StringCompare(Currency, n.Currency) !=0)
        return true;
     return false;
   }
  
  
  string ToString()
  {
     if (StringCompare("No news", Name)==0)
        return "";
     return Currency + " "+ IntegerToString(Importance) + " " + DateToString() + " " + Name;
  }
};

