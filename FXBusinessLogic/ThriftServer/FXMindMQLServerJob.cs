using System;
using System.Threading.Tasks;
using BusinessObjects;
using FXBusinessLogic.Scheduler;
using log4net;
using Quartz;

namespace FXBusinessLogic.Thrift
{
    [DisallowConcurrentExecution]
    public class FXMindMQLServerJob : GenericJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FXMindMQLServerJob));
        private FXMindMQLServer ts;

        public FXMindMQLServerJob()
            : base(log)
        {
            ts = new FXMindMQLServer();
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (Begin(context))
                {
                    SetMessage("Job Locked");
                    Exit(context);
                    return;
                }

                FXMindMQLServer.Run();
                SetMessage("FXMindMQLServerJob Finished.");
                Exit(context);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            await Task.CompletedTask;

        }
    }
}