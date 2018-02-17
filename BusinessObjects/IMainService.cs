using System;
using System.Collections.Generic;
using Autofac;

namespace BusinessObjects
{
    public interface IMainService
    {
        IContainer Container { get; }

        void Init(INotificationUi ui, bool serverMode);

        INotificationUi GetUi();

        void Dispose();

        string GetGlobalProp(string name);

        void SetGlobalProp(string name, string value);

        bool InitScheduler(bool serverMode);

        void RunJobNow(string group, string name);

        string GetJobProp(string group, string name, string prop);

        void SetJobCronSchedule(string group, string name, string cron);

        List<ScheduledJob> GetAllJobsList();

        Dictionary<string, ScheduledJob> GetRunningJobs();

        DateTime? GetJobNextTime(string group, string name);

        DateTime? GetJobPrevTime(string group, string name);

        void PauseScheduler();

        void ResumeScheduler();

        List<double> iCurrencyStrengthAll(string currency, List<string> brokerDates, int iTimeframe);

        List<double> iGlobalSentimentsArray(string symbolName, List<string> brokerDates, int siteId);

        TimeZoneInfo GetBrokerTimeZone();

        void GetAverageLastGlobalSentiments(DateTime date, string symbolStr, out double longPos, out double shortPos);

        bool GetNextNewsEvent(DateTime date, string symbolStr, byte minImportance, ref NewsEventInfo eventInfo);

        bool IsDebug();

        List<Currency> GetCurrencies();

        List<TechIndicator> GetIndicators();

        void SaveCurrency(Currency c);

        void SaveIndicator(TechIndicator i);
    }
}