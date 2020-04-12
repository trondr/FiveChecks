namespace Compliance.Notifications.Model
{
    public class ToastNotificationMessage
    {
        public ToastNotificationMessage(string notificationGroup)
        {
            NotificationGroup = notificationGroup;
        }

        public string NotificationGroup { get; }
    }
}