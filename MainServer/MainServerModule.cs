using Autofac;
using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XTrade.MainServer
{
    public class MainServerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // builder.RegisterType<MainService>().As<IMainService>().SingleInstance();
            builder.RegisterType<QuartzServer>().AsSelf().SingleInstance();
            builder.RegisterType<PositionsManager>().As<ITerminalEvents>().SingleInstance();
            builder.RegisterType<WebLogManager>().As<IWebLog>().SingleInstance();
            // builder.RegisterType<BackgroundNotifyLog>().As<INotificationUi>().SingleInstance();
            // builder.RegisterHubs(Assembly.GetExecutingAssembly());
            // builder.RegisterType<PositionsTester>().As<ITerminalEvents>().SingleInstance();
        }
    }
}