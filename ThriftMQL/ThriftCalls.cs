﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BusinessObjects;
using FXBusinessLogic.BusinessObjects.Thrift;
using Microsoft.Win32;
using RGiesecke.DllExport;
namespace ThriftMQL
{
    public class ThriftCalls
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct THRIFT_CLIENT
        {
            public UInt16 port;
            public Int32 Magic;
            public Int32 accountNumber;
            public byte ip0;
            public byte ip1;
            public byte ip2;
            public byte ip3;
        }


        //public static string HostFromClient(THRIFT_CLIENT tc)
        //{
        //    return string.Format("{0}.{1}.{2}.{3}", tc.ip0, tc.ip1, tc.ip2, tc.ip3);
        //}

        //public const string SETTINGS_APPREGKEY = @"SOFTWARE\\FXMind";
        //public const string  = "FXMind.InstallDir";

        public const string LOGFILENAME = @"FXMind.ThriftMQL.log";
        protected static string GlobalErrorMessage;
        public static string _FullFilePath = "";

        public static char[] ParamsSeparator = fxmindConstants.PARAMS_SEPARATOR.ToCharArray();

    /*  public static string RegistryInstallDir
      {
          get
          {
              string result = @"C:\Projects\GitHub\FXMindNET\bin";
              try
              {
                  RegistryKey rk = Registry.LocalMachine.OpenSubKey(SETTINGS_APPREGKEY, false);
                  if (rk != null)
                      result = rk.GetValue("InstallDir")?.ToString();
              }
              catch (Exception e)
              {
                  GlobalErrorMessage = e.ToString();
              }
              return result;
          }
      }*/

    public static string logFilePath
        {
            get
            {
                return _FullFilePath;
            }
        }

        public static void LogWriteLine(string text, ref THRIFT_CLIENT tc)
        {
            try
            {
                File.AppendAllText(logFilePath, $"{DateTime.Now.ToString()} ThriftMQL.dll({tc.Magic}): {text}\n");

            } catch (Exception e)
            {
                GlobalErrorMessage = e.ToString();
            }
        }

        public static void InitDLL(FXMindMQLClient fx)
        {
            DateTime now = DateTime.Now;

            if (String.IsNullOrEmpty(_FullFilePath) && (fx!=null))
            {
                _FullFilePath = fx.client.GetGlobalProperty(fxmindConstants.SETTINGS_PROPERTY_INSTALLDIR) + "\\" + LOGFILENAME;
            }
            Process proc = Process.GetCurrentProcess();
            string path = "";
            if (proc != null)
                path = proc.MainModule.FileName;
            File.AppendAllText(ThriftCalls.logFilePath, $"\nInit ThriftMQL.dll at { now.ToString() } on App: {path} On Port:{fx.Port}\n");
        }

        protected static List<string> StringToList(string str)
        {
            List<string> list = new List<string>();
            if (str == null)
                return list;
            if (str.Length > 0)
            {
                string[] arr = str.Split(ParamsSeparator);
                list = new List<string>(arr);
            }
            return list;
        }

        protected static bool ListToString(ref StringBuilder str, List<string> list )
        {
            //str.Clear();
            int i = 0;
            foreach (var val in list)
            {
                str.Append(val);
                if (i == (list.Count - 1))
                { }
                else 
                    str.Append(ParamsSeparator);
                i++;
                //if (i++ < (list.Count - 1))
                //    str.Append();
            }
            return true;
        }
        protected static void FillParams(ref THRIFT_CLIENT tc, string parameters, Dictionary<string, string> paramsDic)
        {
            if (parameters.Length > 0)
            {
                string[] paramValues = parameters.Split(ParamsSeparator);
                if (paramValues.Length > 1)
                {
                    foreach (var paramValue in paramValues)
                    {
                        string[] oneParamKeyValue = paramValue.Split(new[] { '=' });
                        if (oneParamKeyValue.Length == 2)
                        {
                            paramsDic[oneParamKeyValue[0]] = oneParamKeyValue[1];
                        }
                    }
                }
            }
            paramsDic["magic"] = tc.Magic.ToString();
            paramsDic["account"] = tc.accountNumber.ToString();
        }

        [DllExport("ProcessDoubleData", CallingConvention = CallingConvention.StdCall)]
        public static long ProcessDoubleData([In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]double[] arr, int arr_size,
            [MarshalAs(UnmanagedType.LPWStr)]string parameters, [MarshalAs(UnmanagedType.LPWStr)]string dataStr, ref THRIFT_CLIENT tc)
        {
            List<double> resDblList = null;
            try
            {
                using (var fx = new FXMindMQLClient(tc.port))
                {
                    Dictionary<string, string> paramsDic = new Dictionary<string, string>();
                    FillParams(ref tc, parameters, paramsDic);
                    List<string> list = StringToList(dataStr);
                    resDblList = fx.client.ProcessDoubleData(paramsDic, list);
                    if (resDblList != null)
                    {
                        resDblList.CopyTo(arr);
                    }
                }
            }
            catch (Exception e)
            {
                GlobalErrorMessage = "ProcessDoubleData: " +e.ToString();
                LogWriteLine(GlobalErrorMessage, ref tc);
                resDblList = new List<double>();
                resDblList.Add(-125);
                return 1;
            }
            return 0;
        }

        [DllExport("ProcessStringData", CallingConvention = CallingConvention.StdCall)]
        public static long ProcessStringData([In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder str, [MarshalAs(UnmanagedType.LPWStr)]string parameters, ref THRIFT_CLIENT tc)
        {
            try
            {
                using (var fx = new FXMindMQLClient(tc.port))
                {
                    List<string> list = StringToList(str.ToString());
                    Dictionary<string, string> paramsDic = new Dictionary<string, string>();
                    FillParams(ref tc, parameters, paramsDic);
                    list = fx.client.ProcessStringData(paramsDic, list);
                    if (list.Count > 0)
                    {
                        ListToString(ref str, list);
                        return list.Count;
                    }
                }
            }
            catch (Exception e)
            {
                GlobalErrorMessage = "ProcessStringData: " + e.ToString();
                LogWriteLine(GlobalErrorMessage, ref tc);
                //client = null;
                str.Append("Error");
                return 1;
            }
            return 0;
        }

        [DllExport("IsServerActive", CallingConvention = CallingConvention.StdCall)]
        public static long IsServerActive(ref THRIFT_CLIENT tc)
        {
            try
            {
                long ret = 0;
                using (var fx = new FXMindMQLClient(tc.port))
                {
                    Dictionary<string, string> paramsDic = new Dictionary<string, string>();
                    paramsDic["magic"] = tc.Magic.ToString();
                    paramsDic["account"] = tc.accountNumber.ToString();
                    ret = fx.client.IsServerActive(paramsDic);
                }
                return ret;
            }
            catch (Exception e)
            {
                //client = null;
                GlobalErrorMessage = "IsServerActive: " + e.ToString();
                LogWriteLine(GlobalErrorMessage, ref tc);
                return 0;
            }
        }

        [DllExport("PostStatusMessage", CallingConvention = CallingConvention.StdCall)]
        public static void PostStatusMessage(ref THRIFT_CLIENT tc, [MarshalAs(UnmanagedType.LPWStr)]string message)
        {
            try         
            {
                using (var fx = new FXMindMQLClient(tc.port))
                {
                    Dictionary<string, string> paramsDic = new Dictionary<string, string>();
                    paramsDic["magic"] = tc.Magic.ToString();
                    paramsDic["account"] = tc.accountNumber.ToString();
                    paramsDic["message"] = message;
                    fx.client.PostStatusMessage(paramsDic);
                }

            }
            catch (Exception e)
            {
                //client = null;
                GlobalErrorMessage = "PostStatusMessage: " + e.ToString();
                LogWriteLine(GlobalErrorMessage, ref tc);
            }
        }

        [DllExport("CloseClient", CallingConvention = CallingConvention.StdCall)]
        public static void CloseClient(ref THRIFT_CLIENT tc)
        {
            try
            {
                //if (client != null)
                //{
                //    client.Dispose();
                //    client = null;
                    GC.Collect();
                //}
            }
            catch (Exception e)
            {
                //client = null;
                GlobalErrorMessage = "CloseClient: " + e.ToString();
                LogWriteLine(GlobalErrorMessage, ref tc);
            }
        }

        [DllExport("GetGlobalProperty", CallingConvention = CallingConvention.StdCall)]
        public static long GetGlobalProperty([In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder RetValue, [MarshalAs(UnmanagedType.LPWStr)]string PropName, ref THRIFT_CLIENT tc)
        {
            try
            {
                using (var fx = new FXMindMQLClient(tc.port))
                {
                    RetValue.Append(fx.client.GetGlobalProperty(PropName));
                }
                return RetValue.Length;
            }
            catch (Exception e)
            {
                GlobalErrorMessage = "GetGlobalProperty: " + e.ToString();
                LogWriteLine(GlobalErrorMessage, ref tc);
                return -1;
            }
        }

        [DllExport("InitExpert", CallingConvention = CallingConvention.StdCall)]
        public static long InitExpert([MarshalAs(UnmanagedType.LPWStr)]string ChartTimeFrame, [MarshalAs(UnmanagedType.LPWStr)]string Symbol,
            [MarshalAs(UnmanagedType.LPWStr)]string EAName, ref THRIFT_CLIENT tc)
        {
            try
            {
                long res = 0;
                using (var fx = new FXMindMQLClient(tc.port))
                {
                    res = fx.client.InitExpert(tc.accountNumber, ChartTimeFrame, Symbol, EAName);
                }
                return res;
            }
            catch (Exception e)
            {
                GlobalErrorMessage = "InitExpert: " + e.ToString();
                LogWriteLine(GlobalErrorMessage, ref tc);
                return -1;
            }
        }

        [DllExport("DeInitExpert", CallingConvention = CallingConvention.StdCall)]
        public static void DeInitExpert([In, MarshalAs(UnmanagedType.I4)]int Reason, ref THRIFT_CLIENT tc)
        {
            try
            {
                using (var fx = new FXMindMQLClient(tc.port))
                {
                    fx.client.DeInitExpert(Reason, tc.Magic);
                }
            }
            catch (Exception e)
            {
                GlobalErrorMessage = "DeInitExpert: " + e.ToString();
                LogWriteLine(GlobalErrorMessage, ref tc);
            }
        }

        [DllExport("SaveExpert", CallingConvention = CallingConvention.StdCall)]
        public static void SaveExpert([MarshalAs(UnmanagedType.LPWStr)]string ActiveOrdersList, ref THRIFT_CLIENT tc)
        {
            try
            {
                using (var fx = new FXMindMQLClient(tc.port))
                {
                    fx.client.SaveExpert(tc.Magic, ActiveOrdersList);
                }
            }
            catch (Exception e)
            {
                GlobalErrorMessage = "SaveExpert: " + e.ToString();
                LogWriteLine(GlobalErrorMessage, ref tc);
            }
        }


    }
}
