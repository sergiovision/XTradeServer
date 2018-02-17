using Autofac;
using BusinessObjects;
using log4net;

namespace FXMind.MainServer
{
    public class BackgroundNotifyLog : INotificationUi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (BackgroundNotifyLog));

        public BackgroundNotifyLog()
        {
            Log.Info("BackgroundNotifyLog initialized");
        }

        public void LogStatus(string statMessage)
        {
            Log.Info(statMessage);
        }

        public void InitProgressNotification(int min, int max)
        {
            Log.Info("Progress changed to " + min + " of " + max);
        }

        public void UpdateProgressNotification()
        {
        }

        public void ReloadAllViewsNotification()
        {
        }

        public void UpdateData(object data)
        {
        }

        public IContainer GetContainer()
        {
            return Program.Container;
        }
    }
}