using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.System;
using Windows.UI.Notifications;
using Compliance.Notifications.Commands.CheckDiskSpace;
using Compliance.Notifications.ComplianceItems;
using Compliance.Notifications.Data;
using Compliance.Notifications.Helper;
using Compliance.Notifications.Resources;
using Compliance.Notifications.ToastTemplates;
using LanguageExt;
using LanguageExt.Common;
using Newtonsoft.Json;

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

        public static Result<UDecimal> GetFreeDiskSpaceInGigaBytes(string folder)
        {
            if (folder == null) throw new ArgumentNullException(nameof(folder));
            var folderPath = folder.AppendDirectorySeparatorChar();
            return NativeMethods.GetDiskFreeSpaceEx(folderPath, out _, out _, out var lpTotalNumberOfFreeBytes) ? 
                new Result<UDecimal>(lpTotalNumberOfFreeBytes/1024.0M/1024.0M/1024.0M) : 
                new Result<UDecimal>(new Exception($"Failed to check free disk space: '{folderPath}'"));
        }

        public static async Task<Result<DiskSpaceInfo>> GetDiskSpaceInfo()
        {
            var totalFreeDiskSpace = GetFreeDiskSpaceInGigaBytes(@"c:\");
            Logging.DefaultLogger.Warn("GetDiskSpaceInfo: NOT IMPLEMENTED");
            var sccmCacheFolderSize = await GetFolderSize(@"c:\temp").ConfigureAwait(false);
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

        public static async Task<Result<UDecimal>> GetFolderSize(Some<string> folder)
        {
            Logging.DefaultLogger.Warn("GetFolderSize: Need better error handling when Access Denied.");
            return await Task.Run(() =>
            {
                var folderSize = 
                    Pri.LongPath.Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                        .Sum(s => new FileInfo(s).Length);
                return new Result<UDecimal>(new UDecimal(folderSize/1024.0M/1024.0M/1024.0M));
            }).ConfigureAwait(false);
        }

        public static string AppendDirectorySeparatorChar(this string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
                throw new ArgumentException(Resource_Strings.ValueCannotBeNullOrWhiteSpace, nameof(folder));

            return !folder.EndsWith($"{Path.DirectorySeparatorChar}", StringComparison.InvariantCulture) ?
                $"{folder}{Path.DirectorySeparatorChar}" :
                folder;
        }

        public static DiskSpaceInfo LoadDiskSpaceResult()
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

        public static string GetUserComplianceItemResultFileName<T>()
        {
            var folder = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),ApplicationInfo.ApplicationName);
            Directory.CreateDirectory(folder);
            return System.IO.Path.Combine(folder, $@"User-{typeof(T).Name}.json");
        }

        public static async Task<Result<Unit>> SaveUserComplianceItemResult<T>(Some<T> complianceItem)
        {
            var fileName = GetUserComplianceItemResultFileName<T>();
            return await SaveComplianceItemResult<T>(complianceItem, fileName).ConfigureAwait(false);
        }

        public static async Task<Result<T>> LoadUserComplianceItemResult<T>()
        {
            var fileName = GetUserComplianceItemResultFileName<T>();
            return await LoadComplianceItemResult<T>(fileName).ConfigureAwait(false);
        }

        public static string GetSystemComplianceItemResultFileName<T>()
        {
            var folder = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ApplicationInfo.ApplicationName);
            Directory.CreateDirectory(folder);
            return System.IO.Path.Combine(folder, $@"System-{typeof(T).Name}.json");
        }

        public static async Task<Result<Unit>> SaveSystemComplianceItemResult<T>(Some<T> complianceItem)
        {
            var fileName = GetSystemComplianceItemResultFileName<T>();
            return await SaveComplianceItemResult<T>(complianceItem, fileName).ConfigureAwait(false);
        }

        public static async Task<Result<T>> LoadSystemComplianceItemResult<T>()
        {
            var fileName = GetSystemComplianceItemResultFileName<T>();
            return await LoadComplianceItemResult<T>(fileName).ConfigureAwait(false);
        }
        
        public static async Task<Result<Unit>> SaveComplianceItemResult<T>(Some<T> complianceItem, Some<string> fileName)
        {
            return await TrySave(complianceItem, fileName)().ConfigureAwait(false);
        }
        private static TryAsync<Unit> TrySave<T>(Some<T> complianceItem, Some<string> fileName) => async () =>
        {
            using (var sw = new StreamWriter(fileName))
            {
                var json = JsonConvert.SerializeObject(complianceItem.Value, new UDecimalJsonConverter());
                await sw.WriteAsync(json).ConfigureAwait(false);
                return new Result<Unit>(Unit.Default);
            }
        };
        
        public static async Task<Result<T>> LoadComplianceItemResult<T>(Some<string> fileName)
        {
            return await TryLoad<T>(fileName)().ConfigureAwait(false);
        }
        private static TryAsync<T> TryLoad<T>(Some<string> fileName) => async () =>
        {
            using (var sr = new StreamReader(fileName))
            {
                var json = await sr.ReadToEndAsync().ConfigureAwait(false);
                var item = JsonConvert.DeserializeObject<T>(json,new UDecimalJsonConverter());
                return new Result<T>(item);
            }
        };

        public static Result<IEnumerable<T>> ToResult<T>(this IEnumerable<Result<T>> results)
        {
            var e = new Exception(string.Empty);
            var resultArray = results.ToArray();
            var exceptions =
                resultArray
                    .Where(result => !result.IsSuccess)
                    .Select(result => result.Match(arg => e, exception => exception))
                    .ToArray();
            var values =
                resultArray
                    .Where(result => result.IsSuccess)
                    .Select(result => result.Match(v => v, exception => throw exception))
                    .ToArray();

            return exceptions.Length == 0
                ? new Result<IEnumerable<T>>(values)
                : new Result<IEnumerable<T>>(new AggregateException(exceptions));
        }

        public static async Task<Result<Unit>> ExecuteComplianceMeasurements(this List<MeasureCompliance> measurements)
        {
            var tasks = measurements.Select(async compliance => await compliance.Invoke().ConfigureAwait(false)).ToList();
            var processTasks = tasks.ToList();
            while (processTasks.Count > 0)
            {
                var firstFinishedTask = await Task.WhenAny(processTasks.ToArray()).ConfigureAwait(false);
                processTasks.Remove(firstFinishedTask);
                await firstFinishedTask.ConfigureAwait(false);
            }
            var results = tasks.Select(task => task.Result);
            return results.ToResult().Match(units => new Result<Unit>(Unit.Default), exception => new Result<Unit>(exception));
        }

        public static string ToExceptionMessage(this Exception ex)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (ex is AggregateException aggregateException)
            {

                return aggregateException.Message + Environment.NewLine + string.Join(Environment.NewLine,aggregateException.InnerExceptions.Select(exception => exception.ToExceptionMessage()).ToArray());
            }

            return ex.InnerException != null
                    ? ex.Message + Environment.NewLine + ex.InnerException.ToExceptionMessage()
                    : ex.Message;
        }
    }
}
