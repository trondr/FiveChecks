﻿namespace Compliance.Notifications.Model
{
    public class ExitApplicationMessage
    {
        public ExitApplicationMessage(string notificationGroup)
        {
            NotificationGroup = notificationGroup;
        }

        public string NotificationGroup { get; }
    }
}