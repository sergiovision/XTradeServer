using Autofac;
using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUIK
{
    public class QUIKConnectorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<QUIKConnector>().As<ITerminalConnector>().SingleInstance();
        }
    }
}