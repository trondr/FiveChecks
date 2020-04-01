using System.Diagnostics;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace Compliance.Notifications.Common
{
    public static class ToastHelper
    {
        public static async Task<Result<int>> RemoveToastNotification(string groupName)
        {
            return await Task.Run(() =>
            {
                DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>("github.com.trondr.Compliance.Notifications");
                DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();
                Logging.DefaultLogger.Info($"Removing notification group '{groupName}'");
                DesktopNotificationManagerCompat.History.RemoveGroup(groupName);
                return new Result<int>(0);
            }).ConfigureAwait(false);
        }
    }
}
