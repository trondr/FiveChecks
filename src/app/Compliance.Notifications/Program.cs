using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Compliance.Notifications.Helper;
using Microsoft.Toolkit.Uwp.Notifications;
using DesktopNotificationManagerCompat = Compliance.Notifications.Helper.DesktopNotificationManagerCompat;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Compliance.Notifications.ToastTemplates;
using Microsoft.QueryStringDotNET;

namespace Compliance.Notifications
{
    class Program
    {
        static Random _random = new Random();
        
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine($"Compliance.Notifications {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}");
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>("github.com.trondr.Compliance.Notifications");
            DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();

            if (args.Contains(DesktopNotificationManagerCompat.TOAST_ACTIVATED_LAUNCH_ARG))
            {
                // Our NotificationActivator code will run after this completes,
                // and will show a window if necessary.
            }
            else
            {
                var title = "Disk space is low!";
                var image = GetRandomPicture();
                var content = "Disk space is critically low. This have consequence for system stability, program installations and update of Windows.";
                var companyName = "My Company AS";
                BindableString content2 = "Please cleanup your disk";
                var action = "ms-settings:storagesense";
                var toastContent = await ActionSnoozeDismissToastContent.CreateToastContent(title, image, companyName, content, content2, action);
                // Create the XML document (BE SURE TO REFERENCE WINDOWS.DATA.XML.DOM)
                var doc = new XmlDocument();
                var toastXmlContent = toastContent.GetContent();
                Console.WriteLine(toastXmlContent);
                doc.LoadXml(toastContent.GetContent());
                // And create the toast notification
                var toast = new ToastNotification(doc);
                // And then show it
                DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
                System.Threading.Thread.Sleep(5000);
            }
            return 0;
        }

        private static string GetRandomPicture()
        {
            var imageNumber = _random.Next(1, 900);
            var image = $"https://picsum.photos/364/202?image={imageNumber}";
            return image;
        }
    }
}
