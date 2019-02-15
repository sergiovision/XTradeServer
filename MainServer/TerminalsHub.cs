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
    [HubName("terminalsHub")]
    public class TerminalsHub : Hub
    {
        private readonly ITerminalEvents terminal;
        private IMainService mainService;

        public TerminalsHub() //ILifetimeScope lifetimeScope
        {
            // Create a lifetime scope for the hub.
            //_hubLifetimeScope = lifetimeScope.BeginLifetimeScope();
            mainService = Program.Container.Resolve<IMainService>();
            terminal = Program.Container.Resolve<ITerminalEvents>();
        }

        public IEnumerable<PositionInfo> GetAllPositions()
        {
            List<PositionInfo> pos = new List<PositionInfo>();
            try
            {
                pos = terminal.GetAllPositions();
            }
            catch
            {
            }

            return pos;
        }

        /*  protected override void Dispose(bool disposing)
          {
              // Dipose the hub lifetime scope when the hub is disposed.
              if (disposing && _hubLifetimeScope != null)
              {
                  _hubLifetimeScope.Dispose();
              }
  
              base.Dispose(disposing);
          }*/
    }
}