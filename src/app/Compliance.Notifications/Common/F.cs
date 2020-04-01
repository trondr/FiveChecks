using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Compliance.Notifications.Model;
using Compliance.Notifications.Resources;
using Compliance.Notifications.ToastTemplates;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json;
using Directory = Pri.LongPath.Directory;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;
using FileInfo = Pri.LongPath.FileInfo;
using Path = Pri.LongPath.Path;
using Task = System.Threading.Tasks.Task;

namespace Compliance.Notifications.Common
{
    /// <summary>
    /// Common functions
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

        public static Try<FileInfo[]> TryGetFiles(Some<DirectoryInfo> directoryInfo,Some<string> searchPattern) => () =>
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
            if (string.IsNullOrWhiteSpace(folder))
                throw new ArgumentException(strings.ValueCannotBeNullOrWhiteSpace, nameof(folder));

            return !folder.EndsWith($"{Path.DirectorySeparatorChar}", StringComparison.InvariantCulture) ?
                $"{folder}{Path.DirectorySeparatorChar}" :
                folder;
        }

        public static async Task<DiskSpaceInfo> LoadDiskSpaceResult()
        {
            return await LoadSystemComplianceItemResultOrDefault(DiskSpaceInfo.Default).ConfigureAwait(false);
        }

        public static async Task<PendingRebootInfo> LoadPendingRebootInfo()
        {
            return await LoadSystemComplianceItemResultOrDefault(PendingRebootInfo.Default).ConfigureAwait(false);
        }

        public static async Task<Result<int>> ShowToastNotification(Func<Task<ToastContent>> buildToastContent,
            string tag, string groupName)
        {
            if (buildToastContent == null) throw new ArgumentNullException(nameof(buildToastContent));
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>("github.com.trondr.Compliance.Notifications");
            DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();
            var toastContent = await buildToastContent().ConfigureAwait(false);
            var doc = new XmlDocument();
            var toastXmlContent = toastContent.GetContent();
            Logging.DefaultLogger.Debug(toastXmlContent);
            doc.LoadXml(toastContent.GetContent());
            var toast = new ToastNotification(doc){Tag = tag, Group = groupName};
            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
            return new Result<int>(0);
        }

        private static readonly Random Rnd = new Random();
        public static async Task<Result<int>> ShowDiskSpaceToastNotification(decimal requiredCleanupAmount,
            string companyName, string tag, string groupName)
        {
            return await ShowToastNotification(async () =>
            {
                var toastContentInfo = await GetCheckDiskSpaceToastContentInfo(requiredCleanupAmount, companyName).ConfigureAwait(false);
                var toastContent = await ActionDismissToastContent.CreateToastContent(toastContentInfo).ConfigureAwait(true);
                return toastContent;
            }, tag, groupName).ConfigureAwait(false);
        }

        private static async Task<string> GetGreeting()
        {
            var givenNameResult = await F.GetGivenName().ConfigureAwait(false);
            return givenNameResult.Match(
                    givenName => $"{F.GetGreeting(DateTime.Now)} {givenName}",
                    () => F.GetGreeting(DateTime.Now)
                );
        }
        
        private static async Task<ActionDismissToastContentInfo> GetCheckDiskSpaceToastContentInfo(decimal requiredCleanupAmount, string companyName)
        {
            var title = strings.DiskSpaceIsLow_Title;
            var imageUri = new Uri($"https://picsum.photos/364/202?image={Rnd.Next(1, 900)}");
            var appLogoImageUri = new Uri("https://unsplash.it/64?image=1005");
            var content = strings.DiskSpaceIsLow_Description;
            var content2 = string.Format(CultureInfo.InvariantCulture, strings.Please_Cleanup_DiskSpace_Text_F0,
                requiredCleanupAmount);
            var action = "ms-settings:storagesense";
            var actionActivationType = ToastActivationType.Protocol;
            var greeting = await GetGreeting().ConfigureAwait(false);
            return new ActionDismissToastContentInfo(greeting, title, companyName, content, content2,
                imageUri, appLogoImageUri, action, actionActivationType, strings.DiskSpaceIsLow_ActionButton_Content, strings.NotNowActionButtonContent, "dismiss");
        }

        public static async Task<Result<int>> ShowPendingRebootToastNotification(string companyName, string tag,
            string groupName)
        {
            return await ShowToastNotification(async () =>
            {
                var toastContentInfo = await GetCheckPendingRebootToastContentInfo(companyName).ConfigureAwait(false);
                var toastContent = await ActionDismissToastContent.CreateToastContent(toastContentInfo).ConfigureAwait(true);
                return toastContent;
            }, tag, groupName).ConfigureAwait(false);
        }

        private static async Task<ActionDismissToastContentInfo> GetCheckPendingRebootToastContentInfo(string companyName)
        {
            var title = strings.PendingRebootNotification_Title;
            var imageUri = new Uri($"https://picsum.photos/364/202?image={Rnd.Next(1, 900)}");
            var appLogoImageUri = new Uri("https://unsplash.it/64?image=1005");
            var content = strings.PendingRebootNotification_Content1;
            var content2 = strings.PendingRebootNotification_Content2;
            var action = "restart";
            var actionActivationType = ToastActivationType.Background;
            var greeting = await GetGreeting().ConfigureAwait(false);
            return new ActionDismissToastContentInfo(greeting, title, companyName, content, content2,
                imageUri, appLogoImageUri, action, actionActivationType, strings.PendingRebootNotification_ActionButtonContent, strings.NotNowActionButtonContent, "dismiss");
        }

        public static string GetUserComplianceItemResultFileName<T>()
        {
            var folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),ApplicationInfo.ApplicationName);
            Directory.CreateDirectory(folder);
            return System.IO.Path.Combine(folder, $@"User-{typeof(T).Name}.json");
        }

        public static async Task<Result<Unit>> SaveUserComplianceItemResult<T>(Some<T> complianceItem)
        {
            var fileName = GetUserComplianceItemResultFileName<T>();
            return await SaveComplianceItemResult(complianceItem, fileName).ConfigureAwait(false);
        }

        public static async Task<Result<T>> LoadUserComplianceItemResult<T>()
        {
            var fileName = GetUserComplianceItemResultFileName<T>();
            return await LoadComplianceItemResult<T>(fileName).ConfigureAwait(false);
        }

        public static string GetSystemComplianceItemResultFileName<T>()
        {
            var folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ApplicationInfo.ApplicationName);
            Directory.CreateDirectory(folder);
            return System.IO.Path.Combine(folder, $@"System-{typeof(T).Name}.json");
        }

        public static async Task<Result<Unit>> SaveSystemComplianceItemResult<T>(Some<T> complianceItem)
        {
            var fileName = GetSystemComplianceItemResultFileName<T>();
            return await SaveComplianceItemResult(complianceItem, fileName).ConfigureAwait(false);
        }

        public static Try<Unit> DeleteFile(Some<string> fileName) => () =>
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
            return Unit.Default;
        };

        public static Result<Unit> ClearSystemComplianceItemResult<T>()
        {
            var fileName = GetSystemComplianceItemResultFileName<T>();
            return  DeleteFile(fileName).Try();
        }

        public static async Task<Result<T>> LoadSystemComplianceItemResult<T>()
        {
            var fileName = GetSystemComplianceItemResultFileName<T>();
            return await LoadComplianceItemResult<T>(fileName).ConfigureAwait(false);
        }

        public static async Task<T> LoadSystemComplianceItemResultOrDefault<T>(T defaultValue)
        {
            var fileName = GetSystemComplianceItemResultFileName<T>();
            return (await LoadComplianceItemResult<T>(fileName).ConfigureAwait(false)).Match(arg => arg, exception =>
            {
                Logging.DefaultLogger.Warn($"Could not load '{typeof(T)}' so returning default value. Load error: {exception.ToExceptionMessage()}");
                return defaultValue;
            });
        }

        public static async Task<Result<Unit>> SaveComplianceItemResult<T>(Some<T> complianceItem, Some<string> fileName)
        {
            return await TrySave(complianceItem, fileName).Try().ConfigureAwait(false);
        }

        private static TryAsync<Unit> TrySave<T>(Some<T> complianceItem, Some<string> fileName) => async () =>
        {
            using (var sw = new StreamWriter(fileName))
            {
                var json = JsonConvert.SerializeObject(complianceItem.Value, new UDecimalJsonConverter(), new RebootSourceJsonConverter());
                await sw.WriteAsync(json).ConfigureAwait(false);
                return new Result<Unit>(Unit.Default);
            }
        };
        
        public static async Task<Result<T>> LoadComplianceItemResult<T>(Some<string> fileName)
        {
            return await TryLoad<T>(fileName).Try().ConfigureAwait(false);
        }
        private static TryAsync<T> TryLoad<T>(Some<string> fileName) => async () =>
        {
            using (var sr = new StreamReader(fileName))
            {
                var json = await sr.ReadToEndAsync().ConfigureAwait(false);
                var item = JsonConvert.DeserializeObject<T>(json,new UDecimalJsonConverter(),new RebootSourceJsonConverter());
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

        public static async Task<Result<Unit>> RunSystemComplianceItem<T>(Func<Task<Result<T>>> measureCompliance)
        {
            if (measureCompliance == null) throw new ArgumentNullException(nameof(measureCompliance));
            if (!SystemComplianceItemIsActive<T>()) return new Result<Unit>();
            var info = await measureCompliance().ConfigureAwait(false);
            var res = await info.Match(dsi => F.SaveSystemComplianceItemResult<T>(dsi), exception => Task.FromResult(new Result<Unit>(exception))).ConfigureAwait(false);
            return res;
        }

        private static bool SystemComplianceItemIsActive<T>()
        {
            Logging.DefaultLogger.Warn("TODO: Implement Check if compliance item is activated. Default is true. When implemented this enables support for disabling a system compliance item.");
            return true;
        }

        public static string ToExceptionMessage(this Exception ex)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (ex is AggregateException aggregateException)
            {
                return $"{aggregateException.GetType().Name}: {aggregateException.Message}" + Environment.NewLine + string.Join(Environment.NewLine,aggregateException.InnerExceptions.Select(exception => exception.ToExceptionMessage()).ToArray());
            }
            return ex.InnerException != null
                    ? $"{ex.GetType().Name}: {ex.Message}" + Environment.NewLine + ex.InnerException.ToExceptionMessage()
                    : $"{ex.GetType().Name}: {ex.Message}";
        }

        private static Try<Unit> RegisterSystemScheduledTask(Some<string> taskName, Some<FileInfo> exeFile,
            Some<string> arguments, Some<string> taskDescription) => () =>
        {
            using (var ts = TaskService.Instance)
            {
                using (var td = ts.NewTask())
                {
                    td.RegistrationInfo.Description = taskDescription.Value;
                    td.Actions.Add(new ExecAction(exeFile.Value.FullName, arguments.Value, exeFile.Value.Directory.FullName));
                    td.Triggers.Add(ScheduledTasks.HourlyTrigger());
                    //Allow users to run scheduled task : (A;;0x1200a9;;;BU)
                    td.RegistrationInfo.SecurityDescriptorSddlForm =
                        "O:BAG:SYD:AI(A;;FR;;;SY)(A;;0x1200a9;;;BU)(A;ID;0x1f019f;;;BA)(A;ID;0x1f019f;;;SY)(A;ID;FA;;;BA)";
                    td.Principal.UserId = "SYSTEM";
                    td.Principal.RunLevel = TaskRunLevel.Highest;
                    ts.RootFolder.RegisterTaskDefinition(taskName.Value, td);
                }
            }
            return new Result<Unit>(Unit.Default);
        };

        private static Try<Unit> RegisterUserScheduledTask(Some<string> taskName, Some<FileInfo> exeFile,
            Some<string> arguments, Some<string> taskDescription, Some<Trigger> trigger) => () =>
        {
            using (var ts = TaskService.Instance)
            {
                using (var td = ts.NewTask())
                {
                    td.RegistrationInfo.Description = taskDescription.Value;
                    td.Actions.Add(new ExecAction(exeFile.Value.FullName, arguments.Value, exeFile.Value.Directory.FullName));
                    td.Triggers.Add(trigger.Value);
                    td.Principal.GroupId = ScheduledTasks.BuiltInUsers();
                    td.Principal.RunLevel = TaskRunLevel.LUA;
                    ts.RootFolder.RegisterTaskDefinition(taskName.Value, td);
                }
            }
            return new Result<Unit>(Unit.Default);
        };

        public static async Task<Result<int>> Install()
        {
            var exeFile = Assembly.GetExecutingAssembly().Location;
            
            var checkTaskResult = RegisterUserScheduledTask(ScheduledTasks.ComplianceCheckTaskName, new FileInfo(exeFile),"CheckDiskSpace /subtractSccmCache=True /requiredFreeDiskSpace=40", ScheduledTasks.ComplianceCheckTaskDescription, ScheduledTasks.UnlockTrigger())
                .Try()
                .Match(result => new Result<int>(0),exception => new Result<int>(new Exception($"Failed to register task: {ScheduledTasks.ComplianceCheckTaskName}", exception)));
            
            var systemTaskResult = RegisterSystemScheduledTask(ScheduledTasks.ComplianceSystemMeasurementsTaskName, new FileInfo(exeFile), "MeasureSystemComplianceItems", ScheduledTasks.ComplianceSystemMeasurementsTaskDescription)
                .Try()
                .Match(result => new Result<int>(0), exception => new Result<int>(new Exception($"Failed to register task: {ScheduledTasks.ComplianceSystemMeasurementsTaskName}", exception)));

            var userTaskResult = RegisterUserScheduledTask(ScheduledTasks.ComplianceUserMeasurementsTaskName, new FileInfo(exeFile), "MeasureUserComplianceItems", ScheduledTasks.ComplianceUserMeasurementsTaskDescription, ScheduledTasks.HourlyTrigger())
                .Try()
                .Match(result => new Result<int>(0), exception => new Result<int>(new Exception($"Failed to register task: {ScheduledTasks.ComplianceUserMeasurementsTaskName}", exception)));

            var installResult = new List<Result<int>> { checkTaskResult, systemTaskResult, userTaskResult }.ToResult().Match(exitCodes => new Result<int>(exitCodes.Sum()), exception => new Result<int>(exception));
            return await Task.FromResult(installResult).ConfigureAwait(false);
        }

        private static Try<Result<Unit>> UnRegisterScheduledTask(Some<string> taskName) => () =>
        {
            var task = new Option<Microsoft.Win32.TaskScheduler.Task>(
                TaskService.Instance.AllTasks.Where(t => t.Name == taskName));
            return task.Match(t =>
            {
                TaskService.Instance.RootFolder.DeleteTask(t.Name, false);
                return new Result<Unit>(Unit.Default);
            }, () => new Result<Unit>(Unit.Default));
        };

        public static async Task<Result<int>> UnInstall()
        {
            var res1 = UnRegisterScheduledTask(ScheduledTasks.ComplianceCheckTaskName).Try().Match(result => new Result<int>(0), exception => new Result<int>(exception));
            var res2 = UnRegisterScheduledTask(ScheduledTasks.ComplianceSystemMeasurementsTaskName).Try().Match(result => new Result<int>(0), exception => new Result<int>(exception));
            var res3 = UnRegisterScheduledTask(ScheduledTasks.ComplianceUserMeasurementsTaskName).Try().Match(result => new Result<int>(0), exception => new Result<int>(exception));
            var unInstallResult =  new List<Result<int>> {res1,res2,res3}.ToResult().Match(exitCodes => new Result<int>(exitCodes.Sum()), exception => new Result<int>(exception));
            return await Task.FromResult(unInstallResult).ConfigureAwait(false);
        }

        //Source: https://github.com/WindowsNotifications/desktop-toasts
        public static async Task<Option<Uri>> DownloadImageToDisk(Some<Uri> httpImage)
        {
            // Toasts can live for up to 3 days, so we cache images for up to 3 days.
            // Note that this is a very simple cache that doesn't account for space usage, so
            // this could easily consume a lot of space within the span of 3 days.

            try
            {
                if (DesktopNotificationManagerCompat.CanUseHttpImages)
                {
                    return httpImage.Value;
                }

                var directory = Directory.CreateDirectory(Path.GetTempPath() + "github.com.trondr.Compliance.Notifications");
                foreach (var d in directory.EnumerateDirectories())
                {
                    if (d.CreationTimeUtc.Date < DateTime.UtcNow.Date.AddDays(-3))
                    {
                        d.Delete(true);
                    }
                }


                var dayDirectory = directory.CreateSubdirectory($"{DateTime.UtcNow.Day}");
                string imagePath = dayDirectory.FullName + "\\" + (uint)httpImage.Value.GetHashCode() + ".jpg";

                if (File.Exists(imagePath))
                {
                    return new Uri("file://" + imagePath);
                }

                using (var c = new HttpClient())
                {
                    using (var stream = await c.GetStreamAsync(httpImage.Value).ConfigureAwait(false))
                    {
                        using (var fileStream = File.OpenWrite(imagePath))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                }
                return new Uri("file://" + imagePath);
            }
            catch (HttpRequestException) { return Option<Uri>.None; }
        }

        public static async Task<string> DownloadImage(Some<Uri> httpImage)
        {
            var image = await DownloadImageToDisk(httpImage).ConfigureAwait(false);
            return image.Match(uri => uri.LocalPath, () => "");
        }
        
        public static Try<Option<string>> TryGetGivenName() => () =>
        {
            var currentWindowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            using (var pc = new PrincipalContext(ContextType.Domain))
            {
                var up = UserPrincipal.FindByIdentity(pc, currentWindowsPrincipal.Identity.Name);
                return up != null ? up.GivenName : Option<string>.None;
            }
        };

        public static async Task<Option<string>> GetGivenName()
        {
            var result = await Task.Run(() => TryGetGivenName().Try()).ConfigureAwait(false);
            return result.Match(option => option, exception =>
            {
                Logging.DefaultLogger.Error($"Failed to get user given name. {exception.ToExceptionMessage()}");
                return Option<string>.None;
            });
        }

        public static string GetGreeting(DateTime now)
        {
            var hourOfDay = now.Hour;
            if (hourOfDay > 0 && hourOfDay < 12)
                return strings.GoodMorning;
            if (hourOfDay >= 12 && hourOfDay < 16)
                return strings.GoodAfterNoon;
            return strings.GoodEvening;
        }

        public static async Task<Result<PendingRebootInfo>> GetPendingRebootInfo()
        {
            var pendingRebootInfoResults = new List<Result<PendingRebootInfo>>
            {
                await GetCbsRebootPending().ConfigureAwait(false),
                await GetWuauRebootPending().ConfigureAwait(false),
                await GetPendingFileRenameRebootPending().ConfigureAwait(false),
                await GetSccmClientRebootPending().ConfigureAwait(false)
            };
            var success = pendingRebootInfoResults.ToSuccess();
            var pendingRebootInfo = success.Aggregate(PendingRebootInfoExtensions.Update);
            return new Result<PendingRebootInfo>(pendingRebootInfo);
        }

        private static async Task<Result<PendingRebootInfo>> GetWuauRebootPending()
        {
            var rebootPendingRegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired";
            Logging.DefaultLogger.Debug($@"Checking if Windows Update has a pending reboot (Check if key exists: '{rebootPendingRegistryKeyPath}').");
            var rebootIsPending = RegistryKeyExists(Registry.LocalMachine, rebootPendingRegistryKeyPath);
            var rebootSource = rebootIsPending ? new List<RebootSource> { RebootSource.Wuau } : new List<RebootSource>();
            var pendingRebootInfo = new PendingRebootInfo { RebootIsPending = rebootIsPending, Source = rebootSource };
            Logging.DefaultLogger.Info($@"Windows Update pending reboot check result: {pendingRebootInfo.ObjectToString()}");
            return await Task.FromResult(new Result<PendingRebootInfo>(pendingRebootInfo)).ConfigureAwait(false);
        }

        private static async Task<Result<PendingRebootInfo>> GetCbsRebootPending()
        {
            var rebootPendingRegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending";
            Logging.DefaultLogger.Debug($@"Checking if Component Based Servicing has a pending reboot (Check if key exists: '{rebootPendingRegistryKeyPath}').");
            var rebootIsPending = RegistryKeyExists(Registry.LocalMachine, rebootPendingRegistryKeyPath);
            var rebootSource = rebootIsPending ? new List<RebootSource> { RebootSource.Cbs } : new List<RebootSource>();
            var pendingRebootInfo = new PendingRebootInfo { RebootIsPending = rebootIsPending, Source = rebootSource };
            Logging.DefaultLogger.Info($@"Component Based Servicing (CBS) pending reboot check result: {pendingRebootInfo.ObjectToString()}");
            return await Task.FromResult(new Result<PendingRebootInfo>(pendingRebootInfo)).ConfigureAwait(false);
        }

        private static Try<PendingRebootInfo> TryGetSccmClientRebootStatus() => () =>
        {
            dynamic rebootStatus =
                RunPowerShell(new Some<Func<PowerShell, Collection<PSObject>>>(powerShell =>
                    powerShell
                        .AddCommand("Invoke-WmiMethod")
                        .AddParameter("NameSpace", @"root\ccm\ClientSDK")
                        .AddParameter("Class", "CCM_ClientUtilities")
                        .AddParameter("Name", "DetermineIfRebootPending")
                        .Invoke())
                ).FirstOrDefault();
            var rebootIsPending = rebootStatus?.RebootPending || rebootStatus?.IsHardRebootPending;
            var rebootSource = rebootIsPending ? new List<RebootSource> { RebootSource.SccmClient } : new List<RebootSource>();
            var pendingRebootInfo = new PendingRebootInfo { RebootIsPending = rebootIsPending, Source = rebootSource };
            Logging.DefaultLogger.Info($@"Sccm Client pending reboot check result: {pendingRebootInfo.ObjectToString()}");
            return new Result<PendingRebootInfo>(pendingRebootInfo);
        };

        private static async Task<Result<PendingRebootInfo>> GetSccmClientRebootPending()
        {
            var pendingRebootInfoResult = 
                TryGetSccmClientRebootStatus()
                    .Try()
                    .Match(
                        info => new Result<PendingRebootInfo>(info), 
                        exception =>
                            {
                                Logging.DefaultLogger.Warn($"Getting reboot status from Sccm Client did not succeed. {exception.ToExceptionMessage()}");
                                return new Result<PendingRebootInfo>(exception);
                            }
                        );
            return await Task.FromResult(pendingRebootInfoResult).ConfigureAwait(false);
        }
        
        private static async Task<Result<PendingRebootInfo>> GetPendingFileRenameRebootPending()
        {
            var rebootPendingRegistryKeyPath = @"SYSTEM\CurrentControlSet\Control\Session Manager";
            var rebootPendingRegistryValueName = "PendingFileRenameOperations";
            var rebootIsPending = MultiStringRegistryValueExistsAndHasStrings(Registry.LocalMachine, rebootPendingRegistryKeyPath,
                rebootPendingRegistryValueName);
            var rebootSource = rebootIsPending ? new List<RebootSource> { RebootSource.PendingFileRename } : new List<RebootSource>();
            var pendingRebootInfo = new PendingRebootInfo { RebootIsPending = rebootIsPending, Source = rebootSource };
            Logging.DefaultLogger.Info($@"Pending file rename operation pending reboot check result: {pendingRebootInfo.ObjectToString()}");
            return await Task.FromResult(new Result<PendingRebootInfo>(pendingRebootInfo)).ConfigureAwait(false);
        }

        private static bool MultiStringRegistryValueExistsAndHasStrings(Some<RegistryKey> baseKey, Some<string> subKeyPath, Some<string> valueName)
        {
            using (var key = baseKey.Value.OpenSubKey(subKeyPath.Value))
            {
                var value = key?.GetValue(valueName);
                if (value == null) return false;
                if (value is string[] stringArray)
                {
                    return stringArray.Length > 0;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a registry key exists.
        /// </summary>
        /// <param name="baseKey">Example: Registry.LocalMachine</param>
        /// <param name="subKeyPath"></param>
        /// <returns></returns>
        public static bool RegistryKeyExists(Some<RegistryKey> baseKey, string subKeyPath)
        {
            using (var key = baseKey.Value.OpenSubKey(subKeyPath))
            {
                return key != null;
            }
        }
        
        public static IEnumerable<T> ToSuccess<T>(this IEnumerable<Result<T>> results)
        {
            var resultArray = results.ToArray();
            //Log any faulted results
            resultArray.Where(result => result.IsFaulted).Select(result =>result.IfFail(exception =>
            {
                Logging.DefaultLogger.Warn($"Failed to produce {typeof(T).Name} result. Error: {exception.GetType().Name}: {exception.ToExceptionMessage()}" );
            })).ToArr();
            //Return the successful results
            return
                resultArray
                    .Where(result => result.IsSuccess)
                    .Select(result => result.Match(v => v, exception => throw exception))
                    .ToArray();
        }

        public static Collection<PSObject> RunPowerShell(Some<Func<PowerShell, Collection<PSObject>>> action)
        {
            using (var powerShell = PowerShell.Create(RunspaceMode.NewRunspace))
            {
                return action.Value(powerShell);
            }
        }

        public static string ObjectToString(this object data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var objectType = data.GetType();
            if (objectType == typeof(string))
            {
                return $"\"{data}\"";
            }
            if (objectType.IsPrimitive || !objectType.IsClass)
            {
                return data.ToString();
            }
            return JsonConvert.SerializeObject(data);
        }

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
