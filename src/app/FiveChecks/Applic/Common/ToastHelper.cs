using System;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using GalaSoft.MvvmLight.Messaging;
using LanguageExt.Common;
using Microsoft.Toolkit.Uwp.Notifications;

namespace FiveChecks.Applic.Common
{
    public static class ToastHelper
    {
        internal static string Aumid = $"{ApplicationInfo.ApplicationProductName}";

        public static async Task<Result<ToastNotificationVisibility>> ShowToastNotification(Func<Task<ToastContent>> buildToastContent, string tag, string groupName)
        {
            if (buildToastContent == null) throw new ArgumentNullException(nameof(buildToastContent));
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>(ToastHelper.Aumid);
            DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();
            var toastContent = await buildToastContent().ConfigureAwait(false);
            var doc = new XmlDocument();
            var toastXmlContent = toastContent.GetContent();
            Logging.DefaultLogger.Debug(toastXmlContent);
            doc.LoadXml(toastContent.GetContent());
            var toast = new ToastNotification(doc) { Tag = tag, Group = groupName };
            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
            Messenger.Default.Send(new RegisterToastNotificationMessage(groupName));
            return new Result<ToastNotificationVisibility>(ToastNotificationVisibility.Show);
        }

        public static async Task<Result<ToastNotificationVisibility>> RemoveToastNotification(string groupName)
        {
            return await Task.Run(() =>
            {
                DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>(Aumid);
                DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();
                Logging.DefaultLogger.Info($"Removing notification group '{groupName}'");
                DesktopNotificationManagerCompat.History.RemoveGroup(groupName);
                Messenger.Default.Send(new UnRegisterToastNotificationMessage(groupName));
                return new Result<ToastNotificationVisibility>(ToastNotificationVisibility.Hide);
            }).ConfigureAwait(false);
        }
    }
}
