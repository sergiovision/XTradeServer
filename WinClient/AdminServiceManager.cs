//using Common.Logging;

using System;
using System.ServiceProcess;
using log4net;
using Microsoft.Win32;

namespace FXMind.WinClient
{
    public class AdminServiceManager
    {
        public const string ServiceName = "FXMindMainServer";
        public const int TimeoutMilliseconds = 10000;
        private static readonly ILog log = LogManager.GetLogger(typeof (AdminServiceManager));

        public static string GetCurrentServiceStatus()
        {
            string EnabledStatus = "Enabled";
            string RunningStatus = "Stopped";
            try
            {
                //2 - Automatic
                //3 - Manual
                //4 - Disabled

                string stringRegKey = @"SYSTEM\CurrentControlSet\Services\";
                stringRegKey += ServiceName;
                RegistryKey key = Registry.LocalMachine.OpenSubKey(stringRegKey, true);
                if (key == null)
                {
                    log.Error("Registry key for the service: " + ServiceName + " doesn't exist");
                    return "Error Retrieve Service Status";
                }
                object value = key.GetValue("Start");
                if (value != null)
                {
                    switch (value.ToString())
                    {
                        case "2":
                            EnabledStatus = "Enabled";
                            break;
                        case "3":
                            EnabledStatus = "Enabled";
                            break;
                        case "4":
                            EnabledStatus = "Disabled";
                            break;
                    }
                }

                var service = new ServiceController(ServiceName);
                switch (service.Status)
                {
                    case ServiceControllerStatus.ContinuePending:
                        RunningStatus = "ContinuePending";
                        break;
                    case ServiceControllerStatus.PausePending:
                        RunningStatus = "PausePending";
                        break;
                    case ServiceControllerStatus.Paused:
                        RunningStatus = "Paused";
                        break;
                    case ServiceControllerStatus.Running:
                        RunningStatus = "Running";
                        break;
                    case ServiceControllerStatus.StartPending:
                        RunningStatus = "StartPending";
                        break;
                    case ServiceControllerStatus.StopPending:
                        RunningStatus = "StopPending";
                        break;
                    case ServiceControllerStatus.Stopped:
                        RunningStatus = "Stopped";
                        break;
                }
            }
            catch (Exception e)
            {
                return "Error retrieving status for service " + e;
            }
            return EnabledStatus + " and " + RunningStatus;
        }

        public static bool StartService()
        {
            var service = new ServiceController(ServiceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(TimeoutMilliseconds);
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                log.Info("Service STARTED.");
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                // ...
            }
            return false;
        }

        public static bool StopService()
        {
            var service = new ServiceController(ServiceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(TimeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                log.Info("Service STOPPED.");
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return false;
        }

        public static bool RestartService()
        {
            var service = new ServiceController(ServiceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(TimeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(TimeoutMilliseconds - (millisec2 - millisec1));

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                log.Info("Service RESTARTED.");
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return false;
        }

        public static bool DisableService()
        {
            // values:
            //2 - Automatic
            //3 - Manual
            //4 - Disabled
            string stringRegKey = @"SYSTEM\CurrentControlSet\Services\";
            stringRegKey += ServiceName;
            RegistryKey key = Registry.LocalMachine.OpenSubKey(stringRegKey, true);
            if (key == null)
            {
                log.Error("Registry key for the service: " + ServiceName + " doesn't exist");
                return false;
            }
            key.SetValue("Start", 4);
            log.Info("Service DISABLED.");
            return true;
        }

        public static bool EnableService()
        {
            // values:
            //2 - Automatic
            //3 - Manual
            //4 - Disabled
            string stringRegKey = @"SYSTEM\CurrentControlSet\Services\";
            stringRegKey += ServiceName;
            RegistryKey key = Registry.LocalMachine.OpenSubKey(stringRegKey, true);
            if (key == null)
            {
                log.Error("Registry key for the service: " + ServiceName + " doesn't exist");
                return false;
            }
            key.SetValue("Start", 2);
            log.Info("Service ENABLED.");
            return true;
        }
    }
}