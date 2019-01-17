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
using System.Threading;

namespace BusinessLogic.Jobs
{
    internal class ConnectQUIKJob : GenericJob
    {
        protected IScheduler sched;
        protected IJobDetail thisJobDetail;

        public ConnectQUIKJob()
        {
            log.Debug("ConnectQUIKJob c-tor");
        }

        protected static string strPath = "";
        protected ITerminalConnector connector;
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

                connector = MainService.thisGlobal.Container.Resolve<ITerminalConnector>();

                var terminals = MainService.thisGlobal.GetTerminals().Where(x => x.Broker.Contains("QUIK"));
                Terminal toTerminal = null;
                if ((terminals != null) && terminals.Count() > 0)
                {
                    toTerminal = terminals.FirstOrDefault();
                }

                if (connector.Connect(toTerminal))
                {
                    RunProccessor(context);
                }
                connector.Dispose();
                SetMessage($"ConnectQUIKJob Finished.");
            }
            catch (Exception ex)
            {
                SetMessage($"ERROR: {ex.ToString()}");
            }
            Exit(context);
            await Task.CompletedTask;

        }

        protected SignalInfo ListenSignals(out IExpert quikExpert)
        {
            var advisers = connector.GetRunningAdvisers();
            foreach(var expert in advisers)
            {
                SignalInfo signal = MainService.thisGlobal.ListenSignal(expert.Key, (long)SignalFlags.Expert);
                if (signal != null)
                {
                    quikExpert = expert.Value;
                    return signal;
                }
            }
            quikExpert = null;
            return null;
        }

        public void RunProccessor(IJobExecutionContext context)
        {
            while(!connector.IsStopped() )
            {                    
                Thread.Sleep(100);
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                IExpert expert = null;
                SignalInfo signal = ListenSignals(out expert);
                if (signal != null)
                {
                    switch((EnumSignals)signal.Id)
                    {
                        case EnumSignals.SIGNAL_ACTIVE_ORDERS:
                            {
                                // MainService.thisGlobal.UpdatePositions(signal.ObjectId, connector.GetActivePositions());
                            }
                            break;
                        case EnumSignals.SIGNAL_MARKET_EXPERT_ORDER:
                        case EnumSignals.SIGNAL_MARKET_MANUAL_ORDER:
                        case EnumSignals.SIGNAL_MARKET_FROMPENDING_ORDER:
                            {
                                //PositionInfo position = JsonConvert.DeserializeObject<PositionInfo>(signal.Data);
                                connector.MarketOrder(signal, expert);
                            }
                            break;
                        case EnumSignals.SIGNAL_CHECK_BALANCE:
                            {

                            }
                            break;
                    }
                }
            }
        }

    }
}
 