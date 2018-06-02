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

namespace com.fxmind.manager.jobs
{
    // TerminalMonitoringJob starts and monitors terminals 
    internal class TerminalMonitoringJob : IJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TerminalMonitoringJob));
        protected IScheduler sched;
        protected IJobDetail thisJobDetail;
        protected string mCheckQuery;
        protected ProcessImpersonation procUtil;

        public TerminalMonitoringJob()
        {
            log.Debug("TerminalMonitoringJob c-tor");
            mCheckQuery = @"SELECT DISTINCT term.*
                                FROM terminal term
                                INNER JOIN adviser adv ON term.ID = adv.TERMINAL_ID
                            WHERE adv.DISABLED = false "; //AND adv.RUNNING = true

            procUtil = new ProcessImpersonation(log);
        }

        protected static string strPath = "";

        public  async Task Execute(IJobExecutionContext context)
        {

            Session session = FXConnectionHelper.GetNewSession();
            try
            {
                thisJobDetail = context.JobDetail;
                sched = context.Scheduler;
                if (MainService.thisGlobal.IsDebug())
                    log.Info("TerminalMonitoringJob: ------- Monitor Terminals -------");

                SelectedData data = session.ExecuteQuery(mCheckQuery);
                foreach (var resRow in data.ResultSet[0].Rows)
                {
                    var oPath = resRow.Values[3];
                    if (oPath != null)
                    {
                        strPath = oPath as string;
                        string appName = Path.GetFileNameWithoutExtension(strPath);
                        Process[] processlist = Process.GetProcessesByName(appName);
                        if ((processlist == null) || (processlist.Length == 0))
                        {
                            procUtil.ExecuteAppAsLoggedOnUser(strPath, "");
                        } else
                        {
                            var procL = processlist.Where(d => d.MainModule.FileName.Equals(strPath, StringComparison.InvariantCultureIgnoreCase));
                            if ((procL == null) || (procL.Count() == 0))
                            {
                                procUtil.ExecuteAppAsLoggedOnUser(strPath, "");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"TerminalMonitoringJob Failed: {ex.ToString()}");
            }
            finally
            {
                session.Disconnect();
                session.Dispose();
            }
            await Task.CompletedTask;

        }

    }
}
 