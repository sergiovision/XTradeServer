using System;
using System.Threading.Tasks;
using Autofac;
using BusinessObjects;
using FXBusinessLogic.BusinessObjects;
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

                //var jobs2Check = MainService.thisGlobal.GetAllJobsList();
                //setResultMessage("# of jobs to check: " + jobs2Check.size());
                //removeObsoleteJobs(jobs2Check);
                //if (jobs2Check.size() != 0)
                //    addOrModifyJobs(jobs2Check);

                ScheduleJobsStatic();
            }
            catch (Exception ex)
            {
                log.InfoFormat("{0}***{0}Failed: {1}{0}***{0}", Environment.NewLine, ex.Message);
            }
            await Task.CompletedTask;

        }

        protected void ScheduleJobsStatic()
        {
            log.Info("JobSuperviser: ------- Scheduling Jobs -------");

            ScheduleThriftJob<AppServiceServerJob>(fxmindConstants.JOBGROUP_THRIFT, "AppServiceServer",
                fxmindConstants.AppService_PORT, 1);
            ScheduleThriftJob<FXMindMQLServerJob>(fxmindConstants.JOBGROUP_THRIFT, "FXMindMQLServer", fxmindConstants.FXMindMQL_PORT, 5);

            ScheduleJob<OandaRatioJob>(fxmindConstants.JOBGROUP_OPENPOSRATIO, "OandaRatio", "0 0 0/1 ? * MON-FRI *");
            ScheduleJob<MyFXBookRatioJob>(fxmindConstants.JOBGROUP_OPENPOSRATIO, "MyFXBookRatio", "0 0 0/1 ? * MON-FRI *");

            // Disabled
            //ScheduleJob<EToroRatioJob>(fxmindConstants.JOBGROUP_OPENPOSRATIO, "EToroRatioJob", "0 0 0/1 ? * MON-FRI *");
            // Disabled
            //ScheduleJob<ExnessNewsJob>(fxmindConstants.JOBGROUP_NEWS, "ExnessNewsJob", "0 0 9 ? * MON-FRI *");
            ScheduleJob<ForexFactoryNewsJob>(fxmindConstants.JOBGROUP_NEWS, "ForexFactoryNewsJob", "0 0 6 ? * MON-FRI *");
            
            // For testing purposes
            //MainService.thisGlobal.RunJobNow(fxmindConstants.JOBGROUP_NEWS, "ForexFactoryNewsJob");

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