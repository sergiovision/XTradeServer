using BusinessObjects;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XTrade.MainServer
{
    public class WebLogManager : IWebLog
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WebLogManager));

        private IHubConnectionContext<dynamic> Clients { get; set; }

        private readonly static object lockObject = new object();
        protected StringBuilder text;

        public WebLogManager()
        {
            Clients = GlobalHost.ConnectionManager.GetHubContext<LogsHub>().Clients;
            text = new StringBuilder($"***Logging started at {DateTime.Now.ToString()}***\n");
        }

        #region Interface Imp

        public string GetAllText()
        {
            return text.ToString();
        }

        public void ClearLog()
        {
            lock (lockObject)
            {
                text.Clear();
                string initMessage = $"***Logging started at {DateTime.Now.ToString()}***\n";
                text.Append(initMessage);
                Clients.All.WritesLog(initMessage);
            }
        }
        public void Log(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            lock (lockObject)
            {
                string msg = DateTime.Now.ToString() + " " + message + "\n";
                text.Append(msg);
                Clients.All.WriteLog(msg);
            }
        }

        /*
        public void Log(string scope, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            lock (lockObject)
            {
                string msg = DateTime.Now.ToString() + " " + message + "\n";
                text.Append(msg);
                Clients.All.WritesScopeLog(scope, msg);
            }
        }
        */



        public void Error(Exception e)
        {
            if (e == null)
                return;
            Clients.All.WriteError(e.ToString());
        }

        public void Info(object message)
        {
            if (message == null)
                return;
            log.Info(message);
        }

        public void Debug(object message)
        {
            if (message == null)
                return;
            log.Debug(message);
        }

        public void Error(string s)
        {
            if (string.IsNullOrEmpty(s))
                return;
            log.Error(s);
        }

        public void Error(string s, Exception e)
        {
            if (string.IsNullOrEmpty(s))
                return;
            log.Error(s, e);
        }

        #endregion
    }
}
