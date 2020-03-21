using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Compliance.Notifications.Common;
using Compliance.Notifications.Helper;
using Compliance.Notifications.Resources;
using Compliance.Notifications.ToastTemplates;
using LanguageExt.Common;
using DesktopNotificationManagerCompat = Compliance.Notifications.Helper.DesktopNotificationManagerCompat;

namespace Compliance.Notifications.Commands.CheckDiskSpace
{
    public static class CheckDiskSpaceCommand
    {

        /// <summary>
        /// Check disk space compliance.
        /// </summary>
        /// <param name="requiredFreeDiskSpace">Required free disk space in GB.</param>
        /// <param name="subtractSccmCache">When set to true, disk space is compliant if: ((CurrentTotalFreeDiskSpace + CurrentSizeOfSccmCache) - requiredFreeDiskSpace) > 0</param>
        /// <returns></returns>
        public static async Task<Result<int>> CheckDiskSpace(int requiredFreeDiskSpace, bool subtractSccmCache)
        {
            Logging.GetLogger("ShowNotification").Info($"Compliance.Notifications {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}");
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>("github.com.trondr.Compliance.Notifications");
            DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();

            var requiredCleanupAmount = 10.5M; //TODO: Test value to be calculated
            await ShowDiskSpaceToastNotification(requiredCleanupAmount, "My Company AS").ConfigureAwait(false);
            
            return new Result<int>(0);
        }
        private static readonly Random Rnd = new Random();

        public static async Task<Result<int>> ShowDiskSpaceToastNotification(decimal requiredCleanupAmount, string companyName)
        {
            var title = Resource_Strings.DiskSpaceIsLow_Title;
            var imageUri = new Uri($"https://picsum.photos/364/202?image={Rnd.Next(1, 900)}");
            var appLogoImageUri = new Uri("https://unsplash.it/64?image=1005");
            var content = Resource_Strings.DiskSpaceIsLow_Description;
            var content2 = string.Format(CultureInfo.InvariantCulture, Resource_Strings.Please_Cleanup_DiskSpace_Text_F0, requiredCleanupAmount);
            var action = "ms-settings:storagesense";
            var toastContentInfo = new ActionSnoozeDismissToastContentInfo(title, companyName, content, content2, action, imageUri, appLogoImageUri);
            var toastContent = await ActionSnoozeDismissToastContent.CreateToastContent(toastContentInfo).ConfigureAwait(true);
            var doc = new XmlDocument();
            var toastXmlContent = toastContent.GetContent();
            Console.WriteLine(toastXmlContent);
            doc.LoadXml(toastContent.GetContent());
            var toast = new ToastNotification(doc);
            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
            return new Result<int>(0);
        }
    }
}
