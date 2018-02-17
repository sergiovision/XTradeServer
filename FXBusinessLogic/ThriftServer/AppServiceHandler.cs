using System.Collections.Generic;
using BusinessObjects;
using FXBusinessLogic.BusinessObjects;
using log4net;

namespace FXBusinessLogic.ThriftServer
{
    internal class AppServiceHandler : AppService.Iface
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AppServiceHandler));

        private readonly MainService fxmind;

        public AppServiceHandler()
        {
            fxmind = MainService.thisGlobal; 
        }

        public string GetGlobalProp(string name)
        {
            return fxmind.GetGlobalProp(name);
        }

        public void SetGlobalProp(string name, string value)
        {
            fxmind.SetGlobalProp(name, value);
        }

        public bool InitScheduler(bool serverMode)
        {
            return fxmind.InitScheduler(serverMode);
        }

        public void RunJobNow(string group, string name)
        {
            fxmind.RunJobNow(group, name);
        }

        public string GetJobProp(string group, string name, string prop)
        {
            return fxmind.GetJobProp(group, name, prop);
        }

        public void SetJobCronSchedule(string group, string name, string cron)
        {
            fxmind.SetJobCronSchedule(group, name, cron);
        }

        public List<ScheduledJob> GetAllJobsList()
        {
            return fxmind.GetAllJobsList();
        }

        public Dictionary<string, ScheduledJob> GetRunningJobs()
        {
            return fxmind.GetRunningJobs();
        }

        public long GetJobNextTime(string group, string name)
        {
            return fxmind.GetJobNextTime(group, name).Value.ToBinary();
        }

        public long GetJobPrevTime(string group, string name)
        {
            return fxmind.GetJobPrevTime(group, name).Value.ToBinary();
        }

        public void PauseScheduler()
        {
            fxmind.PauseScheduler();
        }

        public void ResumeScheduler()
        {
            fxmind.ResumeScheduler();
        }

        public List<CurrencyStrengthSummary> GetCurrencyStrengthSummary(bool recalc, bool bUseLast, long startInterval, long endInterval)
        {
            return new List<CurrencyStrengthSummary>();//fxmind.GetCurrencyStrengthSummary(recalc, bUseLast, startInterval, endInterval);
        }

        public List<Currency> GetCurrencies()
        {
            return fxmind.GetCurrencies();
        }

        public List<TechIndicator> GetIndicators()
        {
            return fxmind.GetIndicators();
        }

        public bool IsDebug()
        {
            return fxmind.IsDebug();
        }

        public void SaveCurrency(Currency c)
        {
            fxmind.SaveCurrency(c);
        }

        public void SaveIndicator(TechIndicator i)
        {
            fxmind.SaveIndicator(i);
        }
    }
}
