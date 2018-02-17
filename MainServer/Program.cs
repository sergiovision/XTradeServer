using Autofac;
using BusinessObjects;
using FXBusinessLogic.BusinessObjects;
using log4net;
using log4net.Config;
using Topshelf;

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

        /// <summary>
        ///     Main.
        /// </summary>
        public static void Main()
        {
            XmlConfigurator.Configure();
            Log.Info("MainServer Main function started.");

            RegisterContainer();

            /*
            FXBusinessLogic.BusinessObjects.MainServiceClient mainService = new FXBusinessLogic.BusinessObjects.MainServiceClient();
            mainService.Init();
            double shortPos;
            double longPos;
            mainService.GetLastAverageGlobalSentiments(out longPos, out shortPos);
            mainService.CloseClient();
            */

            //double shortPos = 0;
            // double longPos = 0;
            // MQLBridge.ExportLib.CallMainService(ref shortPos, ref longPos);


            HostFactory.Run(x =>
            {
                /*x.Service<WcfServiceWrapper<MainService, IMainService>>(factory =>
                {
                    factory.ConstructUsing(
                        c => new WcfServiceWrapper<MainService, IMainService>(Configuration.ServiceName));
                    factory.WhenStarted(service => service.Start());
                    factory.WhenStopped(service => service.Stop());
                });
                 */

                x.SetDescription(Configuration.ServiceDescription);
                x.SetDisplayName(Configuration.ServiceDisplayName);
                x.SetServiceName(Configuration.ServiceName);
                //x.RunAsLocalService();
                x.RunAsNetworkService();

                x.Service(factory =>
                {
                    var server = Container.Resolve<QuartzServer>();
                    server.Initialize();
                    //server.Start();
                    return server;
                });
            });
        }
    }
}