using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Compliance.Notifications.Common;
using Compliance.Notifications.Helper;
using Compliance.Notifications.ToastTemplates;
using Microsoft.Toolkit.Uwp.Notifications;
using NCmdLiner.Attributes;
using DesktopNotificationManagerCompat = Compliance.Notifications.Helper.DesktopNotificationManagerCompat;

namespace Compliance.Notifications.Commands
{

    public static class CommandDefinitions
    {
        [Command(Summary = "Show example notification.",Description = "Show example notification.")]
        // ReSharper disable once UnusedMember.Global
        public static async Task<int> ShowNotification(
            [RequiredCommandParameter(Description = "Arguments",AlternativeName = "a", ExampleValue = new[]{"SomeArg1","SomeArg2"})]
            string[] args)
        {
            Logging.GetLogger("ShowNotification").Info($"Compliance.Notifications {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}");
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>("github.com.trondr.Compliance.Notifications");
            DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();

            if (args.Contains(DesktopNotificationManagerCompat.ToastActivatedLaunchArg))
            {
                // Our NotificationActivator code will run after this completes,
                // and will show a window if necessary.
            }
            else
            {
                var title = "Disk space is low!";
                var image = GetRandomPicture();
                var content =
                    "Disk space is critically low. This have consequence for system stability, program installations and update of Windows.";
                var companyName = "My Company AS";
                BindableString content2 = "Please cleanup your disk";
                var action = "ms-settings:storagesense";
                var toastContent =
                    await ActionSnoozeDismissToastContent.CreateToastContent(title, image, companyName, content, content2,
                        action).ConfigureAwait(true);
                // Create the XML document (BE SURE TO REFERENCE WINDOWS.DATA.XML.DOM)
                var doc = new XmlDocument();
                var toastXmlContent = toastContent.GetContent();
                Console.WriteLine(toastXmlContent);
                doc.LoadXml(toastContent.GetContent());
                // And create the toast notification
                var toast = new ToastNotification(doc);
                // And then show it
                DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
                Logging.GetLogger("ShowNotification").Info("Sleeping 5 seconds");
                await Task.Delay(5000).ConfigureAwait(true);
                Logging.GetLogger("ShowNotification").Info("Finished ShowNotification");
            }
            return 0;
        }

        private static readonly Random Rnd = new Random();
        private static string GetRandomPicture()
        {
            var imageNumber = Rnd.Next(1, 900);
            var image = $"https://picsum.photos/364/202?image={imageNumber}";
            return image;
        }
    }
}
