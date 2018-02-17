using System;
using BusinessObjects;
using FXBusinessLogic.Scheduler;
using log4net;
using Quartz;

namespace FXBusinessLogic.Thrift
{
    [DisallowConcurrentExecution]
    public class AppServiceServerJob : GenericJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (AppServiceServerJob));
        private AppServiceServer ts;

        public AppServiceServerJob()
            : base(log)
        {
            ts = new AppServiceServer(fxmindConstants.AppService_PORT);
        }

        public override void Execute(IJobExecutionContext context)
        {
            try
            {
                if (Begin(context))
                {
                    SetMessage("Job Locked");
                    Exit(context);
                    return;
                }
                SetMessage("AppServiceServerJob listening endpoint localhost:" + fxmindConstants.AppService_PORT);
                SchedulerService.LogJob(context, strMessage);
                AppServiceServer.Run();
                SetMessage("AppServiceServerJob Finished.");
                Exit(context);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }
    }
}