using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FiveChecks.Applic.PendingRebootCheck;
using LanguageExt;
using LanguageExt.Common;
using Newtonsoft.Json;
using Directory = Pri.LongPath.Directory;

namespace FiveChecks.Applic.Common
{
    public static class ComplianceInfo
    {
        public static string GetUserComplianceItemResultFileName<T>()
        {
            var folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationInfo.ApplicationName);
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

        public static Result<Unit> ClearSystemComplianceItemResult<T>()
        {
            var fileName = GetSystemComplianceItemResultFileName<T>();
            return F.DeleteFile(fileName).Try();
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

        public static async Task<T> LoadUserComplianceItemResultOrDefault<T>(T defaultValue)
        {
            var fileName = GetUserComplianceItemResultFileName<T>();
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
                Logging.DefaultLogger.Info($"Saving {typeof(T).Name}: {json}");
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
                Logging.DefaultLogger.Info($"Loading {typeof(T).Name}: {json}");
                var item = JsonConvert.DeserializeObject<T>(json, new UDecimalJsonConverter(), new RebootSourceJsonConverter());
                return new Result<T>(item);
            }
        };

        public static async Task<Result<Unit>> RunSystemComplianceItem<T>(Func<Task<Result<T>>> measureCompliance)
        {
            if (measureCompliance == null) throw new ArgumentNullException(nameof(measureCompliance));
            if (!SystemComplianceItemIsActive<T>()) return new Result<Unit>(Unit.Default);
            var info = await measureCompliance().ConfigureAwait(false);
            var res = await info.Match(dsi => SaveSystemComplianceItemResult<T>(dsi), exception => Task.FromResult(new Result<Unit>(exception))).ConfigureAwait(false);
            return res;
        }

        public static async Task<Result<Unit>> RunUserComplianceItem<T>(Func<Task<Result<T>>> measureCompliance)
        {
            if (measureCompliance == null) throw new ArgumentNullException(nameof(measureCompliance));
            if (!UserComplianceItemIsActive<T>()) return new Result<Unit>(Unit.Default);
            var info = await measureCompliance().ConfigureAwait(false);
            var res = await info.Match(dsi => SaveUserComplianceItemResult<T>(dsi), exception => Task.FromResult(new Result<Unit>(exception))).ConfigureAwait(false);
            return res;
        }

        private static bool SystemComplianceItemIsActive<T>()
        {
            //Logging.DefaultLogger.Warn($"TODO: Implement Check if compliance item '{typeof(T)}' is activated. Default is true. When implemented this enables support for disabling a system compliance item.");
            return !Profile.IsMeasurementDisabled(false, typeof(T));
        }

        private static bool UserComplianceItemIsActive<T>()
        {
            //Logging.DefaultLogger.Warn($"TODO: Implement Check if compliance item '{typeof(T)}' is activated. Default is true. When implemented this enables support for disabling a user compliance item.");
            return !Profile.IsMeasurementDisabled(false, typeof(T));
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
            var results = processTasks.Select(task => task.Result);
            return results.ToResult().Match(units => new Result<Unit>(Unit.Default), exception => new Result<Unit>(exception));
        }

        /// <summary>
        /// Load compliance measurement and check for non-compliance. If non-compliance and double check is true, trigger a new measurement to make sure it is still non-compliant.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="loadInfo"></param>
        /// <param name="isNonCompliant"></param>
        /// <param name="scheduledTask"></param>
        /// <param name="doubleCheck"></param>
        /// <returns></returns>
        public static async Task<T> LoadInfo<T>(Func<Task<T>> loadInfo, Func<T, bool> isNonCompliant, Some<ScheduledTaskInfo> scheduledTask, bool doubleCheck)
        {
            if (loadInfo == null) throw new ArgumentNullException(nameof(loadInfo));

            Func<bool> checkIfDoubleCheckShouldBeRun = () => DoubleCheck.ShouldRunDoubleCheckAction(scheduledTask.Value.TaskName);
            Func<Task<Result<Unit>>> doubleCheckAction = async () =>
            {
                return await DoubleCheck.RunDoubleCheck(scheduledTask.Value.TaskName, async () => await ScheduledTasks.RunScheduledTask(scheduledTask, true).ConfigureAwait(false), doubleCheck).ConfigureAwait(false);
            };
            return await LoadInfoPure(loadInfo, isNonCompliant, checkIfDoubleCheckShouldBeRun, doubleCheckAction).ConfigureAwait(false);
        }

        public static async Task<T> LoadInfoPure<T>(Func<Task<T>> loadInfo, Func<T, bool> isNonCompliant, Func<bool> doDoubleCheck, Func<Task<Result<Unit>>> doubleCheckAction)
        {
            if (loadInfo == null) throw new ArgumentNullException(nameof(loadInfo));
            var info = await loadInfo().ConfigureAwait(false);
            var doubleCheck = doDoubleCheck();
            var isNotCompliant = isNonCompliant(info);
            if (!isNotCompliant || !doubleCheck) return info;
            var doubleCheckResult = await doubleCheckAction().ConfigureAwait(false);
            return
                await doubleCheckResult.Match(
                        async unit =>
                        {
                            var doubleCheckedInfo = await loadInfo().ConfigureAwait(false);
                            return doubleCheckedInfo;
                        },
                        async exception =>
                        {
                            Logging.DefaultLogger.Error($"Failed to run a double check of '{typeof(T)}' non-compliance result. {exception.ToExceptionMessage()}");
                            return await Task.FromResult(info).ConfigureAwait(false);
                        })
                    .ConfigureAwait(false);
        }
    }
}
