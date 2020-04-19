using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.ToastTemplates;
using Compliance.Notifications.Resources;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;
using FileInfo = Pri.LongPath.FileInfo;
using Path = Pri.LongPath.Path;

namespace Compliance.Notifications.Applic.DiskSpaceCheck
{
    public static class DiskSpace
    {
        public static Result<UDecimal> GetFreeDiskSpaceInGigaBytes(string folder)
        {
            if (folder == null) throw new ArgumentNullException(nameof(folder));
            var folderPath = folder.AppendDirectorySeparatorChar();
            return NativeMethods.GetDiskFreeSpaceEx(folderPath, out _, out _, out var lpTotalNumberOfFreeBytes) ?
                new Result<UDecimal>(lpTotalNumberOfFreeBytes / 1024.0M / 1024.0M / 1024.0M) :
                new Result<UDecimal>(new Exception($"Failed to check free disk space: '{folderPath}'"));
        }

        public static async Task<Result<DiskSpaceInfo>> GetDiskSpaceInfo()
        {
            var totalFreeDiskSpace = GetFreeDiskSpaceInGigaBytes(@"c:\");
            Logging.DefaultLogger.Warn("TODO: Calculate path to Sccm Cache");
            var sccmCacheFolderSize = await GetFolderSize(@"c:\windows\ccmcache").ConfigureAwait(false);
            return totalFreeDiskSpace.Match(tfd =>
            {
                return sccmCacheFolderSize.Match(
                    sccmCacheSize => new Result<DiskSpaceInfo>(new DiskSpaceInfo
                    {
                        TotalFreeDiskSpace = tfd,
                        SccmCacheSize = sccmCacheSize
                    }),
                    exception => new Result<DiskSpaceInfo>(exception));
            }, exception => new Result<DiskSpaceInfo>(exception));
        }

        public static Try<FileInfo[]> TryGetFiles(Some<DirectoryInfo> directoryInfo, Some<string> searchPattern) => () =>
             directoryInfo.Value.EnumerateFiles(searchPattern.Value).ToArray();

        public static Try<DirectoryInfo[]> TryGetDirectories(Some<DirectoryInfo> directoryInfo, Some<string> searchPattern) => () =>
            directoryInfo.Value.GetDirectories(searchPattern.Value);

        public static IEnumerable<FileInfo> GetFilesSafe(this DirectoryInfo directory, Some<string> searchPattern, SearchOption searchOption)
        {
            var files = TryGetFiles(directory, searchPattern).Try().Match(fs => fs, exception => Array.Empty<FileInfo>());
            var subDirectories = TryGetDirectories(directory, searchPattern).Try().Match(fs => fs, exception => Array.Empty<DirectoryInfo>());
            var subFiles =
                searchOption == SearchOption.AllDirectories ?
                    subDirectories.SelectMany(info => GetFilesSafe(info, searchPattern, searchOption)).ToArray() :
                    Enumerable.Empty<FileInfo>();
            return files.Concat(subFiles);
        }

        public static Try<UDecimal> TryGetFolderSize(Some<string> folder) => () =>
        {
            var folderSize =
                new DirectoryInfo(folder).GetFilesSafe("*.*", SearchOption.AllDirectories)
                    .Sum(f => f.Length);
            var folderSizeInGb = new UDecimal(folderSize / 1024.0M / 1024.0M / 1024.0M);
            return new Result<UDecimal>(folderSizeInGb);
        };

        public static async Task<Result<UDecimal>> GetFolderSize(Some<string> folder)
        {
            return await Task.Run(() => TryGetFolderSize(folder).Try()).ConfigureAwait(false);
        }

        public static string AppendDirectorySeparatorChar(this string folder)
        {
            if (String.IsNullOrWhiteSpace(folder))
                throw new ArgumentException(strings.ValueCannotBeNullOrWhiteSpace, nameof(folder));

            return !folder.EndsWith($"{Path.DirectorySeparatorChar}", StringComparison.InvariantCulture) ?
                $"{folder}{Path.DirectorySeparatorChar}" :
                folder;
        }

        public static async Task<DiskSpaceInfo> LoadDiskSpaceResult()
        {
            return await F.LoadSystemComplianceItemResultOrDefault(DiskSpaceInfo.Default).ConfigureAwait(false);
        }

        public static async Task<Result<ToastNotificationVisibility>> ShowDiskSpaceToastNotification(decimal requiredCleanupAmount,
            string companyName, string tag, string groupName)
        {
            return await F.ShowToastNotification(async () =>
            {
                var toastContentInfo = await GetCheckDiskSpaceToastContentInfo(requiredCleanupAmount, companyName, groupName).ConfigureAwait(false);
                var toastContent = await ActionDismissToastContent.CreateToastContent(toastContentInfo).ConfigureAwait(true);
                return toastContent;
            }, tag, groupName).ConfigureAwait(false);
        }

        private static async Task<ActionDismissToastContentInfo> GetCheckDiskSpaceToastContentInfo(decimal requiredCleanupAmount, string companyName, string groupName)
        {
            var title = strings.DiskSpaceIsLow_Title;
            var imageUri = new Uri($"https://picsum.photos/364/202?image={F.Rnd.Next(1, 900)}");
            var appLogoImageUri = new Uri("https://unsplash.it/64?image=1005");
            var content = strings.DiskSpaceIsLow_Description;
            var content2 = String.Format(CultureInfo.InvariantCulture, strings.Please_Cleanup_DiskSpace_Text_F0,
                requiredCleanupAmount);
            var action = ToastActions.DiskCleanup;
            var actionActivationType = ToastActivationType.Foreground;
            var greeting = await F.GetGreeting().ConfigureAwait(false);
            return new ActionDismissToastContentInfo(greeting, title, companyName, content, content2,
                imageUri, appLogoImageUri, action, actionActivationType, strings.DiskSpaceIsLow_ActionButton_Content, strings.NotNowActionButtonContent, ToastActions.Dismiss, groupName);
        }
    }
}

