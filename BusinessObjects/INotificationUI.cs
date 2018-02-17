using Autofac;

namespace BusinessObjects
{
    public interface INotificationUi
    {
        void LogStatus(string statMessage);

        void InitProgressNotification(int min, int max);

        void UpdateProgressNotification();

        void ReloadAllViewsNotification();

        void UpdateData(object data);

        IContainer GetContainer();
    }
}