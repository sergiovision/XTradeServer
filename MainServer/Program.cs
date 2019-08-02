using Autofac;
using BusinessObjects;
using log4net;
using log4net.Config;
using Topshelf;
using System.Management;
using System;
using System.ServiceProcess;
using BusinessLogic;
using Microsoft.AspNet.SignalR;
//using QUIK;

namespace XTrade.MainServer
{
    /// <summary>
    ///     The server's main entry point.
    /// </summary>
    public static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        public static IContainer Container { get; set; }

        public static void RegisterContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new BusinessLogicModule());
            //builder.RegisterModule(new QUIKConnectorModule());
            builder.RegisterModule(new MainServerModule());

            Container = builder.Build();
        }

        /// <summary>
        ///     Main.
        /// </summary>
        public static void Main()
        {
            XmlConfigurator.Configure();

            RegisterContainer();


            var rc = HostFactory.Run(x =>
            {
                x.SetDescription(Configuration.ServiceDescription);
                x.SetDisplayName(Configuration.ServiceDisplayName);
                x.SetServiceName(Configuration.ServiceName);
                x.RunAsLocalSystem();
                //x.RunAsLocalService();
                //x.RunAsNetworkService();

                //if (Environment.UserInteractive && System.Diagnostics.Debugger.IsAttached)
                //{
                //    x.Service<QuartzServer>(factory =>
                //    {
                //        factory.ConstructUsing(name => Container.Resolve<QuartzServer>());
                //        factory.WhenStarted(tc => tc.Start());
                //        factory.WhenStopped(tc => tc.Stop());
                //    });
                //} else
                //{
                x.Service(factory =>
                {
                    var server = Container.Resolve<QuartzServer>();
                    server.Initialize(XTradeConfig.WebPort());
                    //server.Start();
                    return server;
                });
                //}
            });
            var exitCode = (int) Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}