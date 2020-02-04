using System;
using System.Collections.Generic;
using Autofac;

namespace BusinessObjects
{
    public interface IMainService : IDataService
    {
        IContainer Container { get; }

        void Init(IContainer container);

        void Dispose();

        bool InitScheduler(bool bServerMode);

        void RunJobNow(string group, string name);

        void StopJobNow(string group, string name);

        string GetJobProp(string group, string name, string prop);

        void SetJobCronSchedule(string group, string name, string cron);

        List<ScheduledJobInfo> GetAllJobsList();

        Dictionary<string, ScheduledJobInfo> GetRunningJobs();

        DateTime? GetJobNextTime(string group, string name);

        DateTime? GetJobPrevTime(string group, string name);

        void PauseScheduler();

        void ResumeScheduler();

        TimeZoneInfo GetBrokerTimeZone();

        bool GetNextNewsEvent(DateTime date, string symbolStr, byte minImportance, ref NewsEventInfo eventInfo);

        List<NewsEventInfo> GetTodayNews(DateTime date, string symbolStr, byte minImportance, int tzoffset = 0);

        bool IsDebug();

        ExpertInfo InitExpert(ExpertInfo expert);

        ExpertInfo InitTerminal(ExpertInfo expert);

        // void SaveExpert(ExpertInfo expert);

        void DeInitExpert(ExpertInfo expert);

        void DeInitTerminal(ExpertInfo expert);
        
        int DeleteHistoryOrders(string filePath);

        void DeployToTerminals(string sourceFolder);

        string DeployToAccount(int id);

        List<Wallet> GetWalletBalanceRange(int WalletId, DateTime from, DateTime to);

        bool UpdateAccountState(AccountState accState);

        SignalInfo ListenSignal(long ReciverObj, long flags);

        void PostSignalTo(SignalInfo signal);

        SignalInfo SendSignal(SignalInfo expert);

        void SubscribeToSignals(long objectId);

        SignalInfo CreateSignal(SignalFlags flags, long ObjectId, EnumSignals Id);

        List<Rates> GetRates(bool IsReread);
    }
}