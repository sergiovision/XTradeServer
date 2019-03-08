using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using System.Linq;
using BusinessObjects;
using Quartz;
using System.Diagnostics;
using System.IO;
using BusinessLogic.BusinessObjects;
using BusinessLogic.Scheduler;
using Newtonsoft.Json;
using System.Threading;

namespace BusinessLogic.Jobs
{
    internal class TestJob : GenericJob
    {
        public static Random NotRandom = new Random();
        protected IScheduler sched;
        protected IJobDetail thisJobDetail;


        public TestJob()
        {
            log.Debug("TestJob c-tor");
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            if (Begin(context))
            {
                SetMessage("Job Locked");
                Exit(context);
                return;
            }

            try
            {
                thisJobDetail = context.JobDetail;
                sched = context.Scheduler;

                // MainService.thisGlobal.ClearPositions();
                //PositionInfo position = new PositionInfo();
                //position.Lots = 1;
                //position.Type = 0;
                //position.Symbol = "BRENT";

                //SignalInfo signal_Order = MainService.thisGlobal.CreateSignal(SignalFlags.Cluster, 1, EnumSignals.SIGNAL_MARKET_MANUAL_ORDER);
                //signal_Order.Value = 0;


                //long ticket = NotRandom.Next() * 1000;

                //PositionInfo pos = new PositionInfo { Symbol = "GOLD", Ticket = ticket, Lots = 0.01, Profit = new decimal(5) };

                //var termNotify = MainService.thisGlobal.Container.Resolve<ITerminalEvents>();
                //if (termNotify != null)
                //   termNotify.InsertPosition(pos);

                SignalInfo signal_History =
                    MainService.thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_DEALS_HISTORY);
                signal_History.Value = 0;
                MainService.thisGlobal.PostSignalTo(signal_History);

                //                while(!context.CancellationToken.IsCancellationRequested)
                //              {
                //                Thread.Sleep(500);
                //          }


                SetMessage("TestJob Finished.");
            }
            catch (Exception ex)
            {
                SetMessage($"ERROR: {ex}");
            }

            Exit(context);
            await Task.CompletedTask;
        }
    }
}