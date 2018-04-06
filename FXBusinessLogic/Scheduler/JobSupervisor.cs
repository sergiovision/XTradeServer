using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using System.Linq;
using BusinessObjects;
using FXBusinessLogic.BusinessObjects;
using FXBusinessLogic.fx_mind;
using FXBusinessLogic.Thrift;
using log4net;
using Quartz;
using com.fxmind.manager.jobs;
using DevExpress.Xpo;

namespace FXBusinessLogic.Scheduler
{
    // JobSupervisor responsible for sheduling and manage all jobs in quartz
    internal class JobSupervisor : IJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(JobSupervisor));
        protected IScheduler sched;
        protected IJobDetail thisJobDetail;

        public JobSupervisor()
        {
            log.Debug("JobSuperviser c-tor");
        }

        public  async Task Execute(IJobExecutionContext context)
        {
            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                thisJobDetail = context.JobDetail;
                sched = context.Scheduler;
                log.Info("JobSuperviser: ------- Scheduling Jobs -------");

                ScheduleThriftJob<AppServiceServerJob>(fxmindConstants.JOBGROUP_THRIFT, "AppServiceServer",
                    fxmindConstants.AppService_PORT, 1);
                ScheduleThriftJob<FXMindMQLServerJob>(fxmindConstants.JOBGROUP_THRIFT, "FXMindMQLServer", fxmindConstants.FXMindMQL_PORT, 5);

                IEnumerable<DBJobs> jobs2Check = MainService.thisGlobal.GetDBActiveJobsList(session);
                UnscheduleObsoleteJobs(jobs2Check);
                addOrModifyJobs(jobs2Check);

                // For testing purposes
                //MainService.thisGlobal.RunJobNow("SYSTEM", "OandaRatioJob");

                //ScheduleJobsStatic();
            }
            catch (Exception ex)
            {
                log.InfoFormat("{0}***{0}Failed: {1}{0}***{0}", Environment.NewLine, ex.Message);
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
            await Task.CompletedTask;

        }

        protected void UnscheduleObsoleteJobs(IEnumerable<DBJobs> jobs2Check)
        {
            try
            {
                var jobs = jobs2Check.Select<DBJobs, JobKey>(d => getJobKeyForJobDescription(d)).Where(d=> !d.Equals(thisJobDetail.Key));
                if (jobs != null)
                    MainService.thisGlobal.UnsheduleJobs(jobs);
            }
            catch (SchedulerException e)
            {
                log.Error("method: UnscheduleObsoleteJobs: unable to remove the jobs.", e);
            }
        }

        /**
         * From jobs2Check (a fresh list of JobDescription objects), addOrModifyJobs checks if jobs must be added or modified.<p>
         * 
         * @param jobs2Check list of JobDescription objects
         */
        protected void addOrModifyJobs(IEnumerable<DBJobs> jobs2Check)
        {
            foreach (var job in jobs2Check)
            {
                try
                {
                    if (!ScheduleJob(job.CLASSPATH, job.GRP, job.NAME, job.CRON))
                    {
                        if (MainService.thisGlobal.DeleteJob(getJobKeyForJobDescription(job)))
                            ScheduleJob(job.CLASSPATH, job.GRP, job.NAME, job.CRON);
                    }
                }
                catch (SchedulerException e)
                {
                    log.Error("method: addOrModifyJobs: error when retrieving a jobDetail with this jobKey: " + job.NAME, e);
                }
            }
        }

        public JobKey getJobKeyForJobDescription(ScheduledJob aJobDescription)
        {
            return new JobKey(aJobDescription.Name, aJobDescription.Group);
        }

        public JobKey getJobKeyForJobDescription(DBJobs aJobDescription)
        {
            return new JobKey(aJobDescription.NAME, aJobDescription.GRP);
        }

        protected void ScheduleJobsStatic()
        {
            ScheduleJob(typeof(OandaRatioJob).FullName, fxmindConstants.JOBGROUP_OPENPOSRATIO, "OandaRatio", "0 0 0/1 ? * MON-FRI *");
            ScheduleJob(typeof(MyFXBookRatioJob).FullName, fxmindConstants.JOBGROUP_OPENPOSRATIO, "MyFXBookRatio", "0 0 0/1 ? * MON-FRI *");

            // Disabled
            //ScheduleJob<EToroRatioJob>(fxmindConstants.JOBGROUP_OPENPOSRATIO, "EToroRatioJob", "0 0 0/1 ? * MON-FRI *");
            // Disabled
            //ScheduleJob<ExnessNewsJob>(fxmindConstants.JOBGROUP_NEWS, "ExnessNewsJob", "0 0 9 ? * MON-FRI *");
            ScheduleJob(typeof(ForexFactoryNewsJob).FullName, fxmindConstants.JOBGROUP_NEWS, "ForexFactoryNewsJob", "0 0 6 ? * MON-FRI *");
           
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

        public bool ScheduleJob(string typeClassName, string group, string name, string cron)  // where TJobType : GenericJob, new()
        {
            Type type = Type.GetType(typeClassName);
            if (type == null)
                return false;
            IJobDetail job = JobBuilder.Create(type)
                .WithIdentity(name, group)
                .UsingJobData("Lock", "false")
                .StoreDurably(true)
                .Build();
            var exists = sched.CheckExists(job.Key);
            if (exists.Result)
                return false;
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
            return true;
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