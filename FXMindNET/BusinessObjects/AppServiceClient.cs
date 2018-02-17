using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;

namespace BusinessObjects
{
    public class AppServiceClient : AppService.Iface
    {
        protected TTransport transport;
        protected TProtocol protocol;
        public AppService.Client client;
        public AppServiceClient(string host, short port)
        {
            try
            {
                transport = new TSocket(host, port);
                protocol = new TBinaryProtocol(transport);
                client = new AppService.Client(protocol);
            }
            catch (TApplicationException x)
            {
                Console.WriteLine(x.StackTrace);
            }
            catch (SocketException s)
            {
                Console.WriteLine(s.ToString());
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public string GetGlobalProp(string name)
        {
            string val;
            transport.Open();
            try
            {
                val = client.GetGlobalProp(name);
            }
            finally
            {
                transport.Close();
            }
            return val;
        }

        public void SetGlobalProp(string name, string value)
        {
            transport.Open();
            try
            {
                client.SetGlobalProp(name, value);
            }
            finally
            {
                transport.Close();
            }
        }

        public bool InitScheduler(bool serverMode)
        {
            bool b = false;
            transport.Open();
            try
            {
               b  = client.InitScheduler(serverMode);
            }
            finally
            {
                transport.Close();
            }
            return b;
        }

        public void RunJobNow(string group, string name)
        {
            transport.Open();
            try
            {
                client.RunJobNow(group, name);
            }
            finally
            {
                transport.Close();
            }
        }

        public string GetJobProp(string group, string name, string prop)
        {
            string val;
            transport.Open();
            try
            {
                val = client.GetJobProp(group, name, prop);
            }
            finally
            {
                transport.Close();
            }
            return val;
        }

        public void SetJobCronSchedule(string group, string name, string cron)
        {
            transport.Open();
            try
            {
                client.SetJobCronSchedule(group, name, cron);
            }
            finally
            {
                transport.Close();
            }
        }

        public List<ScheduledJob> GetAllJobsList()
        {
            List<ScheduledJob> list;
            transport.Open();
            try
            {
                list = client.GetAllJobsList();
            }
            finally
            {
                transport.Close();
            }
            return list;
        }

        public Dictionary<string, ScheduledJob> GetRunningJobs()
        {
            Dictionary<string, ScheduledJob> list;
            transport.Open();
            try
            {
                list = client.GetRunningJobs();
            }
            finally
            {
                transport.Close();
            }
            return list;
        }

        public long GetJobNextTime(string group, string name)
        {
            long val;
            transport.Open();
            try
            {
               val = client.GetJobNextTime(group, name);
            }
            finally
            {
                transport.Close();
            }
            return val;
        }

        public long GetJobPrevTime(string group, string name)
        {
            long val;
            transport.Open();
            try
            {
                val = client.GetJobPrevTime(group, name);
            }
            finally
            {
                transport.Close();
            }
            return val;
        }

        public void PauseScheduler()
        {
            transport.Open();
            try
            {
                client.PauseScheduler();
            }
            finally
            {
                transport.Close();
            }
        }

        public void ResumeScheduler()
        {
            transport.Open();
            try
            {
                client.ResumeScheduler();
            }
            finally
            {
                transport.Close();
            }
        }

        public List<CurrencyStrengthSummary> GetCurrencyStrengthSummary(bool recalc, bool bUseLast, long startInterval, long endInterval)
        {
            List<CurrencyStrengthSummary> val;
            transport.Open();
            try
            {
                val = client.GetCurrencyStrengthSummary(recalc, bUseLast, startInterval, endInterval);
            }
            finally
            {
                transport.Close();
            }
            return val;
        }

        public List<Currency> GetCurrencies()
        {
            List<Currency> val;
            transport.Open();
            try
            {
                val = client.GetCurrencies();
            }
            finally
            {
                transport.Close();
            }
            return val;
        }

        public List<TechIndicator> GetIndicators()
        {
            List<TechIndicator> val;
            transport.Open();
            try
            {
                val = client.GetIndicators();
            }
            finally
            {
                transport.Close();
            }
            return val;
        }

        public bool IsDebug()
        {
            bool val;
            transport.Open();
            try
            {
                val = client.IsDebug();
            }
            finally
            {
                transport.Close();
            }
            return val;
        }

        public void SaveCurrency(Currency c)
        {
            transport.Open();
            try
            {
                client.SaveCurrency(c);
            }
            finally
            {
                transport.Close();
            }
        }

        public void SaveIndicator(TechIndicator i)
        {
            transport.Open();
            try
            {
                client.SaveIndicator(i);
            }
            finally
            {
                transport.Close();
            }
        }
    }
}
