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

  /// <summary>
  /// Structs are the basic complex data structures. They are comprised of fields
  /// which each have an integer identifier, a type, a symbolic name, and an
  /// optional default value.
  /// 
  /// Fields can be declared "optional", which ensures they will not be included
  /// in the serialized output if they aren't set.  Note that this requires some
  /// manual management in some languages.
  /// </summary>
  #if !SILVERLIGHT
  [Serializable]
  #endif
  public partial class ScheduledJob : TBase
  {
    private bool _isRunning;
    private string _Group;
    private string _Name;
    private string _Log;
    private string _Schedule;
    private long _PrevTime;
    private long _NextTime;

    public bool IsRunning
    {
      get
      {
        return _isRunning;
      }
      set
      {
        __isset.isRunning = true;
        this._isRunning = value;
      }
    }

    public string Group
    {
      get
      {
        return _Group;
      }
      set
      {
        __isset.@Group = true;
        this._Group = value;
      }
    }

    public string Name
    {
      get
      {
        return _Name;
      }
      set
      {
        __isset.Name = true;
        this._Name = value;
      }
    }

    public string Log
    {
      get
      {
        return _Log;
      }
      set
      {
        __isset.Log = true;
        this._Log = value;
      }
    }

    public string Schedule
    {
      get
      {
        return _Schedule;
      }
      set
      {
        __isset.Schedule = true;
        this._Schedule = value;
      }
    }

    public long PrevTime
    {
      get
      {
        return _PrevTime;
      }
      set
      {
        __isset.PrevTime = true;
        this._PrevTime = value;
      }
    }

    public long NextTime
    {
      get
      {
        return _NextTime;
      }
      set
      {
        __isset.NextTime = true;
        this._NextTime = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool isRunning;
      public bool @Group;
      public bool Name;
      public bool Log;
      public bool Schedule;
      public bool PrevTime;
      public bool NextTime;
    }

    public ScheduledJob() {
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
              if (field.Type == TType.Bool) {
                IsRunning = iprot.ReadBool();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 2:
              if (field.Type == TType.String) {
                Group = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 3:
              if (field.Type == TType.String) {
                Name = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 4:
              if (field.Type == TType.String) {
                Log = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 5:
              if (field.Type == TType.String) {
                Schedule = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 6:
              if (field.Type == TType.I64) {
                PrevTime = iprot.ReadI64();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 7:
              if (field.Type == TType.I64) {
                NextTime = iprot.ReadI64();
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
        TStruct struc = new TStruct("ScheduledJob");
        oprot.WriteStructBegin(struc);
        TField field = new TField();
        if (__isset.isRunning) {
          field.Name = "isRunning";
          field.Type = TType.Bool;
          field.ID = 1;
          oprot.WriteFieldBegin(field);
          oprot.WriteBool(IsRunning);
          oprot.WriteFieldEnd();
        }
        if (Group != null && __isset.@Group) {
          field.Name = "Group";
          field.Type = TType.String;
          field.ID = 2;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Group);
          oprot.WriteFieldEnd();
        }
        if (Name != null && __isset.Name) {
          field.Name = "Name";
          field.Type = TType.String;
          field.ID = 3;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Name);
          oprot.WriteFieldEnd();
        }
        if (Log != null && __isset.Log) {
          field.Name = "Log";
          field.Type = TType.String;
          field.ID = 4;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Log);
          oprot.WriteFieldEnd();
        }
        if (Schedule != null && __isset.Schedule) {
          field.Name = "Schedule";
          field.Type = TType.String;
          field.ID = 5;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Schedule);
          oprot.WriteFieldEnd();
        }
        if (__isset.PrevTime) {
          field.Name = "PrevTime";
          field.Type = TType.I64;
          field.ID = 6;
          oprot.WriteFieldBegin(field);
          oprot.WriteI64(PrevTime);
          oprot.WriteFieldEnd();
        }
        if (__isset.NextTime) {
          field.Name = "NextTime";
          field.Type = TType.I64;
          field.ID = 7;
          oprot.WriteFieldBegin(field);
          oprot.WriteI64(NextTime);
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
      StringBuilder __sb = new StringBuilder("ScheduledJob(");
      bool __first = true;
      if (__isset.isRunning) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("IsRunning: ");
        __sb.Append(IsRunning);
      }
      if (Group != null && __isset.@Group) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Group: ");
        __sb.Append(Group);
      }
      if (Name != null && __isset.Name) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Name: ");
        __sb.Append(Name);
      }
      if (Log != null && __isset.Log) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Log: ");
        __sb.Append(Log);
      }
      if (Schedule != null && __isset.Schedule) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Schedule: ");
        __sb.Append(Schedule);
      }
      if (__isset.PrevTime) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("PrevTime: ");
        __sb.Append(PrevTime);
      }
      if (__isset.NextTime) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("NextTime: ");
        __sb.Append(NextTime);
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}
