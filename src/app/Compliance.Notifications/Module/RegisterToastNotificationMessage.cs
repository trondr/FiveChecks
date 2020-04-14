namespace Compliance.Notifications.Module
{
    public class RegisterToastNotificationMessage
    {
        public RegisterToastNotificationMessage(string notificationGroup)
        {
            NotificationGroup = notificationGroup;
        }

        public string NotificationGroup { get; }
    }

    public class UnRegisterToastNotificationMessage
    {
        public UnRegisterToastNotificationMessage(string notificationGroup)
        {
            NotificationGroup = notificationGroup;
        }

        public string NotificationGroup { get; }
    }
}