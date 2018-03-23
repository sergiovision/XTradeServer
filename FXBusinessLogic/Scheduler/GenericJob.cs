using System;
using System.Threading.Tasks;
using BusinessObjects;
using log4net;
using Quartz;

namespace FXBusinessLogic.Scheduler
{
    public abstract class GenericJob : IJob
    {
        public static INotificationUi s_ownerUI;
        private readonly ILog log;

        private DateTimeOffset runTime;
        protected string strMessage;

        protected GenericJob(ILog l)
        {
            log = l;
        }

        public void SetMessage(string message)
        {
            strMessage = message;
        }

        public bool Begin(IJobExecutionContext context)
        {
            runTime = SystemTime.UtcNow();
            JobKey key = context.JobDetail.Key;
            return false;
        }

        public async void Exit(IJobExecutionContext context)
        {
            DateTimeOffset now = SystemTime.UtcNow();
            TimeSpan duration = now - runTime;
            strMessage += ". For " + (long) duration.TotalMilliseconds + " ms. At " +
                          now.ToString(fxmindConstants.MTDATETIMEFORMAT) + " GMT";
            SchedulerService.LogJob(context, strMessage);
            log.InfoFormat(strMessage);
            if (s_ownerUI != null)
                s_ownerUI.ReloadAllViewsNotification();
            await Task.CompletedTask;
        }

        public abstract Task Execute(IJobExecutionContext context);
    }
}