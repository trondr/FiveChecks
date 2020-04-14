using System.Threading.Tasks;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic.Common
{
    public static class ToastHelper
    {
        public static async Task<Result<ToastNotificationVisibility>> RemoveToastNotification(string groupName)
        {
            return await Task.Run(() =>
            {
                DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>("github.com.trondr.Compliance.Notifications");
                DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();
                Logging.DefaultLogger.Info($"Removing notification group '{groupName}'");
                DesktopNotificationManagerCompat.History.RemoveGroup(groupName);
                return new Result<ToastNotificationVisibility>(ToastNotificationVisibility.Hide);
            }).ConfigureAwait(false);
        }
    }
}
