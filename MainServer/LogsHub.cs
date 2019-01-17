using Autofac;
using BusinessObjects;
using log4net.Core;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XTrade.MainServer
{
    [HubName("LogsHub")]
    public class LogsHub : Hub
    {
        private readonly IWebLog weblog;
        IMainService mainService;

        public LogsHub() // ILifetimeScope lifetimeScope
        {
            // Create a lifetime scope for the hub.
            //_hubLifetimeScope = lifetimeScope.BeginLifetimeScope();
            mainService = Program.Container.Resolve<IMainService>();
            weblog = Program.Container.Resolve<IWebLog>();
        }

        public string GetAllText()
        {
            return weblog.GetAllText();
        }

        public void ClearLog()
        {
            // clear
            weblog.ClearLog();
        }


    }
}
