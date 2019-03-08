using System;
using Autofac;
using BusinessObjects;
using log4net;
using Microsoft.Owin.Hosting;
using Topshelf;

namespace XTrade.MainServer
{
    /// <summary>
    ///     The main server logic.
    /// </summary>
    public class QuartzServer : ServiceControl, IQuartzServer
    {
        //public static QuartzServer quartz;
        private static readonly ILog Log = LogManager.GetLogger(typeof(QuartzServer));
        private IDisposable webapi;
        private short webport;
        private IMainService xtradeServer;

        public QuartzServer()
        {
            Initialize(XTradeConfig.WebPort());
        }

        /// <summary>
        ///     Starts this instance, delegates to scheduler.
        /// </summary>
        public virtual void Start()
        {
            try
            {
                // scheduler.Start();
                xtradeServer.Init(Program.Container);
                if (webport <= 0)
                    throw new Exception("Port should be set for WEBAPI!!!");
                webapi = WebApp.Start<Startup>($"http://*:{webport}");
            }
            catch (Exception ex)
            {
                Log.Fatal(string.Format("Scheduler start failed: {0}", ex.Message), ex);
                throw;
            }

            //Log.Info("Scheduler started successfully");
        }

        /// <summary>
        ///     Stops this instance, delegates to scheduler.
        /// </summary>
        public virtual void Stop()
        {
            try
            {
                if (xtradeServer != null)
                    xtradeServer.Dispose();
                if (webapi != null)
                    webapi.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Scheduler stop failed: {0}", ex.Message), ex);
                throw;
            }

            Log.Info("Scheduler shutdown complete");
        }

        /// <summary>
        ///     Pauses all activity in scheduler.
        /// </summary>
        public virtual void Pause()
        {
            xtradeServer.PauseScheduler();
        }

        /// <summary>
        ///     Resumes all activity in server.
        /// </summary>
        public void Resume()
        {
            xtradeServer.ResumeScheduler();
        }

        /// <summary>
        ///     TopShelf's method delegated to <see cref="Start()" />.
        /// </summary>
        public bool Start(HostControl hostControl)
        {
            Start();
            return true;
        }

        /// <summary>
        ///     TopShelf's method delegated to <see cref="Stop()" />.
        /// </summary>
        public bool Stop(HostControl hostControl)
        {
            Stop();
            return true;
        }

        /// <summary>
        ///     Initializes the instance of the <see cref="QuartzServer" /> class.
        /// </summary>
        public virtual void Initialize(short WEBAPIPORT)
        {
            try
            {
                webport = WEBAPIPORT;
                xtradeServer = Program.Container.Resolve<IMainService>();
            }
            catch (Exception e)
            {
                Log.Error("Server initialization failed:" + e.Message, e);
                throw;
            }
        }


        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            xtradeServer.Dispose(); // no-op for now
        }

        /// <summary>
        ///     TopShelf's method delegated to <see cref="Pause()" />.
        /// </summary>
        public bool Pause(HostControl hostControl)
        {
            Pause();
            return true;
        }

        /// <summary>
        ///     TopShelf's method delegated to <see cref="Resume()" />.
        /// </summary>
        public bool Continue(HostControl hostControl)
        {
            Resume();
            return true;
        }
    }
}