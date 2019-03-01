//+------------------------------------------------------------------+
//|                                                 TradeSignals.mqh |
//|                        Copyright 2018, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <XTrade\GenericTypes.mqh>


//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+

class Signal : public SerializableEntity
{
protected:
    string Name;
    datetime RaiseDateTime; // date returned in MTFormat
public:
    //bool Handled;
    int Value;
    long ObjectId;
    SignalType type;
    bool UseAsFilter;
    long flags;
    //CJAVal data;

    virtual CJAVal* Persistent() {
        obj["RaiseDateTime"] = TimeToString(RaiseDateTime);
        //obj["Handled"] = Handled;
        obj["Value"] = Value;
        obj["Id"] = (int)type;
        obj["Flags"] = (int)flags;
        //obj["Data"].AddBase(data);
        obj["Name"] = Name;
        obj["ObjectId"] = ObjectId;
        return &obj;
    }
    
    Signal(string fromJson)
    {
        obj.Deserialize(fromJson);
        //if (obj.FindKey("Handled"))
        //   Handled = obj["Handled"].ToBool();
        if (obj.FindKey("Value"))
           Value = (int)obj["Value"].ToInt();
        type = (SignalType)obj["Id"].ToInt();
        if (obj.FindKey("Flags"))
           flags = obj["Flags"].ToInt();
        if (obj.FindKey("Name"))
           Name = obj["Name"].ToStr();
        if (obj.FindKey("ObjectId"))
           ObjectId = obj["ObjectId"].ToInt();
    }
    
    Signal(SignalFlags fl, SignalType id, long objId) {
       this.flags = fl;
       this.ObjectId = objId;
       this.type = id;
       this.SetName(EnumToString(id));
       this.Value = 0;
       this.SetRaiseTime(TimeCurrent());
    }

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
        //Handled = false;
        Value = 0;
        flags = SignalToAuto;
        UseAsFilter = isFilter;
        type = SignalQuiet;    
    }

    virtual bool OnAlert()
    {
       return (!UseAsFilter) && (type != SignalQuiet);
    }
    
    virtual void operator=(const Signal &n) 
    {
        RaiseDateTime = n.RaiseDateTime;
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

