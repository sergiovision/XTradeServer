using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using System.Linq;
using BusinessObjects;
using log4net;
using Quartz;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using System.Diagnostics;
using FXBusinessLogic;
using System.IO;
using FXBusinessLogic.BusinessObjects;
using FXBusinessLogic.Scheduler;

namespace com.fxmind.manager.jobs
{
    // ClearExpertsHistoryJob 
    internal class ClearExpertsHistoryJob : GenericJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TerminalMonitoringJob));
        protected IScheduler sched;
        protected IJobDetail thisJobDetail;
        
        public ClearExpertsHistoryJob()
            : base(log)
        {
            log.Debug("ClearExpertsHistoryJob c-tor");

        }

        protected static string strPath = "";

        public override async Task Execute(IJobExecutionContext context)
        {
            if (Begin(context))
            {
                SetMessage("Job Locked");
                Exit(context);
                return;
            }

            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                thisJobDetail = context.JobDetail;
                sched = context.Scheduler;
                if (MainService.thisGlobal.IsDebug())
                    log.Info("ClearExpertsHistoryJob: Cleaning History in all *.set files");

                string fileDir = MainService.thisGlobal.GetGlobalProp(fxmindConstants.SETTINGS_PROPERTY_MTCOMMONFILES);
                int ordersDeleted = 0;
                var setFiles = Directory.EnumerateFiles(fileDir, "*.set", SearchOption.AllDirectories);
                int filesCount = 0;
                if (setFiles != null)
                {
                    filesCount = setFiles.Count();
                    foreach (string currentFile in setFiles)
                    {
                        ordersDeleted += MainService.thisGlobal.DeleteHistoryOrders(currentFile);
                    }
                }
                SetMessage($"ClearExpertsHistoryJob : Files Processed: {filesCount} Deleted Orders: {ordersDeleted} ");

            }
            catch (Exception ex)
            {
                SetMessage($"ERROR: {ex.ToString()}");
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
            Exit(context);
            await Task.CompletedTask;

        }

    }
}
 