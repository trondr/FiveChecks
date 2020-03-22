using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Compliance.Notifications.Commands.CheckDiskSpace;
using Compliance.Notifications.Helper;
using Compliance.Notifications.Resources;
using Compliance.Notifications.ToastTemplates;
using LanguageExt.Common;

namespace Compliance.Notifications.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class F
    {
        public static string AppendNameToFileName(this string fileName, string name)
        {
            return string.Concat(
                    Path.GetFileNameWithoutExtension(fileName),
                    ".", 
                    name, 
                    Path.GetExtension(fileName)
                    );
        }

        public static Result<decimal> GetFreeDiskSpaceInGigaBytes(string folder)
        {
            if (folder == null) throw new ArgumentNullException(nameof(folder));
            var folderPath = folder.AppendDirectorySeparatorChar();
            return NativeMethods.GetDiskFreeSpaceEx(folderPath, out _, out _, out var lpTotalNumberOfFreeBytes) ? 
                new Result<decimal>(lpTotalNumberOfFreeBytes/1024.0M/1024.0M/1024.0M) : 
                new Result<decimal>(new Exception($"Failed to check free disk space: '{folderPath}'"));
        }

        public static string AppendDirectorySeparatorChar(this string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
                throw new ArgumentException(Resource_Strings.ValueCannotBeNullOrWhiteSpace, nameof(folder));

            return !folder.EndsWith($"{Path.DirectorySeparatorChar}", StringComparison.InvariantCulture) ?
                $"{folder}{Path.DirectorySeparatorChar}" :
                folder;
        }

        public static DiskSpaceInfo LoadDiskSpaceResult(decimal requiredFreeDiskSpace, bool subtractSccmCache)
        {
            Logging.DefaultLogger.Warn("LoadDiskSpaceResult: NOT IMPLEMENTED");
            return new DiskSpaceInfo(){TotalFreeDiskSpace = 10, SccmCacheSize = 20};
        }

        private static readonly Random Rnd = new Random();
        public static async Task<Result<int>> ShowDiskSpaceToastNotification(decimal requiredCleanupAmount, string companyName)
        {
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>("github.com.trondr.Compliance.Notifications");
            DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();
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
