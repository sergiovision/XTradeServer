using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using System.Linq;
using Quartz;
using System.Diagnostics;
using BusinessLogic;
using System.IO;
using BusinessLogic.BusinessObjects;
using BusinessLogic.Repo;
using BusinessObjects;

namespace BusinessLogic.Jobs
{
    // TerminalMonitoringJob starts and monitors terminals 
    internal class TerminalMonitoringJob : IJob
    {
        protected IScheduler sched;
        protected IJobDetail thisJobDetail;
        protected ProcessImpersonation procUtil;
        protected IWebLog log;
        public TerminalMonitoringJob()
        {
            log = MainService.thisGlobal.Container.Resolve<IWebLog>();
            log.Debug("TerminalMonitoringJob c-tor");
            procUtil = MainService.thisGlobal.Container.Resolve<ProcessImpersonation>();
        }

        protected static string strPath = "";

        public  async Task Execute(IJobExecutionContext context)
        {

            try
            {
                thisJobDetail = context.JobDetail;
                sched = context.Scheduler;
                if (MainService.thisGlobal.IsDebug())
                    log.Info("TerminalMonitoringJob: ------- Monitor Terminals -------");

                DataService dataService = MainService.thisGlobal.Container.Resolve<DataService>();

                IEnumerable<Terminal> results = dataService.GetTerminals().Where(x => (x.Disabled == false) && (x.Stopped == false)); //.ExecuteNativeQuery<DBTerminal>(mCheckQuery);
                foreach (var resRow in results)
                {
                    var oPath = resRow.FullPath;
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
            await Task.CompletedTask;
        }
    }
}
 