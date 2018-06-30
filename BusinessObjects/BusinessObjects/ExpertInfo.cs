/**
 * Autogenerated by Thrift Compiler (0.11.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Thrift;
using Thrift.Collections;
using System.Runtime.Serialization;
using Thrift.Protocol;
using Thrift.Transport;

namespace BusinessObjects
{

  #if !SILVERLIGHT
  [Serializable]
  #endif
  public partial class ExpertInfo : TBase
  {
    private long _Account;
    private long _MagicNumber;
    private string _ChartTimeFrame;
    private string _Symbol;
    private string _EAName;
    private List<string> _orderTicketsToLoad;

    public long Account
    {
      get
      {
        return _Account;
      }
      set
      {
        __isset.Account = true;
        this._Account = value;
      }
    }

    public long MagicNumber
    {
      get
      {
        return _MagicNumber;
      }
      set
      {
        __isset.MagicNumber = true;
        this._MagicNumber = value;
      }
    }

    public string ChartTimeFrame
    {
      get
      {
        return _ChartTimeFrame;
      }
      set
      {
        __isset.ChartTimeFrame = true;
        this._ChartTimeFrame = value;
      }
    }

    public string Symbol
    {
      get
      {
        return _Symbol;
      }
      set
      {
        __isset.Symbol = true;
        this._Symbol = value;
      }
    }

    public string EAName
    {
      get
      {
        return _EAName;
      }
      set
      {
        __isset.EAName = true;
        this._EAName = value;
      }
    }

    public List<string> OrderTicketsToLoad
    {
      get
      {
        return _orderTicketsToLoad;
      }
      set
      {
        __isset.orderTicketsToLoad = true;
        this._orderTicketsToLoad = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool Account;
      public bool MagicNumber;
      public bool ChartTimeFrame;
      public bool Symbol;
      public bool EAName;
      public bool orderTicketsToLoad;
    }

    public ExpertInfo() {
    }

    public void Read (TProtocol iprot)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        TField field;
        iprot.ReadStructBegin();
        while (true)
        {
          field = iprot.ReadFieldBegin();
          if (field.Type == TType.Stop) { 
            break;
          }
          switch (field.ID)
          {
            case 1:
              if (field.Type == TType.I64) {
                Account = iprot.ReadI64();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 2:
              if (field.Type == TType.I64) {
                MagicNumber = iprot.ReadI64();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 3:
              if (field.Type == TType.String) {
                ChartTimeFrame = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 4:
              if (field.Type == TType.String) {
                Symbol = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 5:
              if (field.Type == TType.String) {
                EAName = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 6:
              if (field.Type == TType.List) {
                {
                  OrderTicketsToLoad = new List<string>();
                  TList _list4 = iprot.ReadListBegin();
                  for( int _i5 = 0; _i5 < _list4.Count; ++_i5)
                  {
                    string _elem6;
                    _elem6 = iprot.ReadString();
                    OrderTicketsToLoad.Add(_elem6);
                  }
                  iprot.ReadListEnd();
                }
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            default: 
              TProtocolUtil.Skip(iprot, field.Type);
              break;
          }
          iprot.ReadFieldEnd();
        }
        iprot.ReadStructEnd();
      }
      finally
      {
        iprot.DecrementRecursionDepth();
      }
    }

    public void Write(TProtocol oprot) {
      oprot.IncrementRecursionDepth();
      try
      {
        TStruct struc = new TStruct("ExpertInfo");
        oprot.WriteStructBegin(struc);
        TField field = new TField();
        if (__isset.Account) {
          field.Name = "Account";
          field.Type = TType.I64;
          field.ID = 1;
          oprot.WriteFieldBegin(field);
          oprot.WriteI64(Account);
          oprot.WriteFieldEnd();
        }
        if (__isset.MagicNumber) {
          field.Name = "MagicNumber";
          field.Type = TType.I64;
          field.ID = 2;
          oprot.WriteFieldBegin(field);
          oprot.WriteI64(MagicNumber);
          oprot.WriteFieldEnd();
        }
        if (ChartTimeFrame != null && __isset.ChartTimeFrame) {
          field.Name = "ChartTimeFrame";
          field.Type = TType.String;
          field.ID = 3;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(ChartTimeFrame);
          oprot.WriteFieldEnd();
        }
        if (Symbol != null && __isset.Symbol) {
          field.Name = "Symbol";
          field.Type = TType.String;
          field.ID = 4;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Symbol);
          oprot.WriteFieldEnd();
        }
        if (EAName != null && __isset.EAName) {
          field.Name = "EAName";
          field.Type = TType.String;
          field.ID = 5;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(EAName);
          oprot.WriteFieldEnd();
        }
        if (OrderTicketsToLoad != null && __isset.orderTicketsToLoad) {
          field.Name = "orderTicketsToLoad";
          field.Type = TType.List;
          field.ID = 6;
          oprot.WriteFieldBegin(field);
          {
            oprot.WriteListBegin(new TList(TType.String, OrderTicketsToLoad.Count));
            foreach (string _iter7 in OrderTicketsToLoad)
            {
              oprot.WriteString(_iter7);
            }
            oprot.WriteListEnd();
          }
          oprot.WriteFieldEnd();
        }
        oprot.WriteFieldStop();
        oprot.WriteStructEnd();
      }
      finally
      {
        oprot.DecrementRecursionDepth();
      }
    }

    public override string ToString() {
      StringBuilder __sb = new StringBuilder("ExpertInfo(");
      bool __first = true;
      if (__isset.Account) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Account: ");
        __sb.Append(Account);
      }
      if (__isset.MagicNumber) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("MagicNumber: ");
        __sb.Append(MagicNumber);
      }
      if (ChartTimeFrame != null && __isset.ChartTimeFrame) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("ChartTimeFrame: ");
        __sb.Append(ChartTimeFrame);
      }
      if (Symbol != null && __isset.Symbol) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Symbol: ");
        __sb.Append(Symbol);
      }
      if (EAName != null && __isset.EAName) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("EAName: ");
        __sb.Append(EAName);
      }
      if (OrderTicketsToLoad != null && __isset.orderTicketsToLoad) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("OrderTicketsToLoad: ");
        __sb.Append(OrderTicketsToLoad);
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}