using System;
using System.Threading.Tasks;
using Autofac;
using BusinessObjects;
using FXBusinessLogic.News;
using FXBusinessLogic.PosRatio;
using FXBusinessLogic.Thrift;
using log4net;
using Quartz;

namespace FXBusinessLogic.Scheduler
{
    // JobSupervisor responsible for sheduling and manage all jobs in quartz
    internal class JobSupervisor : IJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(JobSupervisor));
        protected IScheduler sched;

        public JobSupervisor()
        {
            log.Debug("JobSuperviser c-tor");
        }

        public  async Task Execute(IJobExecutionContext context)
        {
            try
            {
                sched = context.Scheduler;
                SchedulingAllJobs();
            }
            catch (Exception ex)
            {
                log.InfoFormat("{0}***{0}Failed: {1}{0}***{0}", Environment.NewLine, ex.Message);
            }
            await Task.CompletedTask;

        }

        protected void SchedulingAllJobs()
        {
            log.Info("JobSuperviser: ------- Scheduling Jobs -------");

            ScheduleThriftJob<AppServiceServerJob>(fxmindConstants.JOBGROUP_THRIFT, "AppServiceServer",
                fxmindConstants.AppService_PORT, 1);
            //ScheduleThriftJob<FXMindMQLServerJob>(fxmindConstants.JOBGROUP_THRIFT, "FXMindMQLServer", fxmindConstants.FXMindMQL_PORT, 5);

            ScheduleJob<OandaRatioJob>(fxmindConstants.JOBGROUP_OPENPOSRATIO, "OandaRatio", "0 0 0/1 ? * MON-FRI *");
            ScheduleJob<MyFXBookRatioJob>(fxmindConstants.JOBGROUP_OPENPOSRATIO, "MyFXBookRatio",
                "0 0 0/1 ? * MON-FRI *");
            ScheduleJob<EToroRatioJob>(fxmindConstants.JOBGROUP_OPENPOSRATIO, "EToroRatioJob", "0 0 0/1 ? * MON-FRI *");

            ScheduleJob<NewsParseJob>(fxmindConstants.JOBGROUP_NEWS, "NewsParseJob", "0 0 9 ? * MON-FRI *");
            // runs every 4 hours by defailt

            /*
            //ScheduleJobWithParam<TechnicalDetailsJob>(fxmindConstants.JOBGROUP_TECHDETAIL, "TechDetails_Min1",
            //    fxmindConstants.CRON_MANUAL, "TimeFrame", "60");
            ScheduleJobWithParam<TechnicalDetailsJob>(fxmindConstants.JOBGROUP_TECHDETAIL, "TechDetails_Min5",
                fxmindConstants.CRON_MANUAL, "TimeFrame", "300"); //"0 0/5 * ? * MON-FRI *"
            ScheduleJobWithParam<TechnicalDetailsJob>(fxmindConstants.JOBGROUP_TECHDETAIL, "TechDetails_Min15",
                fxmindConstants.CRON_MANUAL, "TimeFrame", "900"); //"0 0/15 * ? * MON-FRI *"
            ScheduleJobWithParam<TechnicalDetailsJob>(fxmindConstants.JOBGROUP_TECHDETAIL, "TechDetails_Min30",
                fxmindConstants.CRON_MANUAL, "TimeFrame", "1800"); //"0 0/30 * ? * MON-FRI *"
            ScheduleJobWithParam<TechnicalDetailsJob>(fxmindConstants.JOBGROUP_TECHDETAIL, "TechDetails_Hourly",
                "0 0 0/1 ? * MON-FRI *", "TimeFrame", "3600"); //
            //ScheduleJobWithParam<TechnicalDetailsJob>(fxmindConstants.JOBGROUP_TECHDETAIL, "TechDetails_5Hourly",
            //    fxmindConstants.CRON_MANUAL, "TimeFrame", "18000"); "0 0 0/5 ? * MON-FRI *"
            ScheduleJobWithParam<TechnicalDetailsJob>(fxmindConstants.JOBGROUP_TECHDETAIL, "TechDetails_Daily",
                fxmindConstants.CRON_MANUAL, "TimeFrame", "86400");//"0 0 9 ? * MON-FRI *"
            ScheduleJobWithParam<TechnicalDetailsJob>(fxmindConstants.JOBGROUP_TECHDETAIL, "TechDetails_Monthly",
                fxmindConstants.CRON_MANUAL, "TimeFrame", "month"); //"0 0 9 ? * MON-FRI *"
            */

            log.Info("JobSuperviser: ------- Jobs Scheduled -------");
        }

        protected void SetTimeZoneForTrigger(ICronTrigger trigger)
        {
            if (GenericJob.s_ownerUI != null)
            {
                IContainer container = GenericJob.s_ownerUI.GetContainer();
                if (container != null)
                {
                    var fxmind = container.Resolve<IMainService>();
                    if (fxmind != null)
                    {
                        TimeZoneInfo tz = fxmind.GetBrokerTimeZone();
                        trigger.TimeZone = tz;
                    }
                }
            }
        }

        public void ScheduleJob<TJobType>(string group, string name, string cron)
            where TJobType : GenericJob, new()
        {
            IJobDetail job = JobBuilder.Create<TJobType>()
                .WithIdentity(name, group)
                .UsingJobData("Lock", "false")
                .StoreDurably(true)
                .Build();
            var exists = sched.CheckExists(job.Key);
            if (exists.Result) return;
            string triggerName = name + "Trigger";
            SchedulerService.SetJobDataMap(job.Key, job.JobDataMap);
            var trigger = (ICronTrigger) TriggerBuilder.Create()
                .WithIdentity(triggerName, group)
                .WithCronSchedule(cron)
                //.WithPriority(1)
                .Build();
            SetTimeZoneForTrigger(trigger);

            var result  = sched.ScheduleJob(job, trigger);
            DateTimeOffset ft = result.Result;

            log.Info(job.Key + " scheduled at: " + ft.ToUniversalTime() + " and repeat on cron: " +
                     trigger.CronExpressionString);
        }

        public void ScheduleJobWithParam<TJobType>(string group, string name, string cron, string param, string value)
            where TJobType : GenericJob, new()
        {
            IJobDetail job = JobBuilder.Create<TJobType>()
                .WithIdentity(name, group)
                .UsingJobData("Lock", "false")
                .UsingJobData(param, value)
                .StoreDurably(true)
                .Build();
            var exists = sched.CheckExists(job.Key);
            if (exists.Result) return;
            string triggerName = name + "Trigger";
            SchedulerService.SetJobDataMap(job.Key, job.JobDataMap);
            var trigger = (ICronTrigger) TriggerBuilder.Create()
                .WithIdentity(triggerName, group)
                .WithCronSchedule(cron)
                //.WithPriority(1)
                .Build();

            SetTimeZoneForTrigger(trigger);

            var result = sched.ScheduleJob(job, trigger);
            DateTimeOffset ft = result.Result;

            log.Info(job.Key + " scheduled at: " + ft.ToUniversalTime() + " and repeat on cron: " +
                     trigger.CronExpressionString);
        }

        public void ScheduleThriftJob<TJobType>(string group, string name, short port, int timeoutsec)
            where TJobType : GenericJob, new()
        {
            IJobDetail job = JobBuilder.Create<TJobType>()
                .WithIdentity(name, group)
                .UsingJobData("port", port.ToString())
                .StoreDurably(true)
                .Build();
            var exists = sched.CheckExists(job.Key);
            if (exists.Result) 
                sched.DeleteJob(job.Key);
            string triggerName = name + "Trigger";
            SchedulerService.SetJobDataMap(job.Key, job.JobDataMap);
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(triggerName, group)
                //.WithPriority(10)
                .StartAt(DateTime.Now.AddSeconds(timeoutsec))
                .Build();

            var result = sched.ScheduleJob(job, trigger);
            DateTimeOffset ft = result.Result;

            log.Info(job.Key + " scheduled to start at " + ft);
        }
    }
}