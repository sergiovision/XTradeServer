using Autofac;
using BusinessObjects;
using FXBusinessLogic.BusinessObjects;
using log4net;
using log4net.Config;
using Topshelf;
using System.Management;
using System;
using System.ServiceProcess;
using System.Reflection;

namespace FXMind.MainServer
{
    /// <summary>
    ///     The server's main entry point.
    /// </summary>
    public static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        public static IContainer Container { get; set; }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MainService>().As<IMainService>().SingleInstance();
            builder.RegisterType<QuartzServer>().AsSelf().SingleInstance();
            builder.RegisterType<BackgroundNotifyLog>().As<INotificationUi>().SingleInstance();
            Container = builder.Build();
        }

        //public static void AlloWDesktopInteraction()
        //{
        //    var service = new System.Management.ManagementObject(string.Format("WIN32_Service.Name='{0}'", Configuration.ServiceName));
        //    try
        //    {
        //        var paramList = new object[11];
        //        paramList[5] = true;//We only need to set DesktopInteract parameter
        //        var output = (int)service.InvokeMethod("Change", paramList);
        //        //if zero is returned then it means change is done.
        //        if (output != 0)
        //            throw new Exception(string.Format("FAILED with code {0}", output));

        //    }
        //    finally
        //    {
        //        service.Dispose();
        //    }

        //}

        /// <summary>
        ///     Main.
        /// </summary>
        public static void Main()
        {
            XmlConfigurator.Configure();
            Log.Info("MainServer Main function started.");

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
                        //server.Initialize();
                        //server.Start();
                        return server;
                    });
                //}


            });
            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}