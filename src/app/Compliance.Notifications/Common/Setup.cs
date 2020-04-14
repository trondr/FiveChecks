using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compliance.Notifications.Model;
using LanguageExt.Common;
using Microsoft.Win32.TaskScheduler;
using Task = System.Threading.Tasks.Task;

namespace Compliance.Notifications.Common
{
    public static class Setup
    {
        public static async Task<Result<int>> Install()
        {
            var res1 = ScheduledTasks.RegisterUserScheduledTask(ScheduledTasks.ComplianceCheckTaskName, ScheduledTasks.ExeFile, ScheduledTasks.ComplianceCheckTaskArguments, ScheduledTasks.ComplianceCheckTaskDescription, new List<Trigger> { ScheduledTasks.UnlockTrigger(), ScheduledTasks.LoginTrigger() })
                .Try()
                .Match(result => new Result<int>(0), exception => new Result<int>(new Exception($"Failed to register task: {ScheduledTasks.ComplianceCheckTaskName}", exception)));

            var res2 = ScheduledTasks.RegisterSystemScheduledTask(ScheduledTasks.ComplianceSystemMeasurementsTaskName, ScheduledTasks.ExeFile, ScheduledTasks.ComplianceSystemMeasurementsTaskArguments, ScheduledTasks.ComplianceSystemMeasurementsTaskDescription)
                .Try()
                .Match(result => new Result<int>(0), exception => new Result<int>(new Exception($"Failed to register task: {ScheduledTasks.ComplianceSystemMeasurementsTaskName}", exception)));

            var res3 = ScheduledTasks.RegisterUserScheduledTask(ScheduledTasks.ComplianceUserMeasurementsTaskName, ScheduledTasks.ExeFile, ScheduledTasks.ComplianceUserMeasurementsTaskArguments, ScheduledTasks.ComplianceUserMeasurementsTaskDescription, new List<Trigger> { ScheduledTasks.HourlyTrigger(), ScheduledTasks.LoginTrigger() })
                .Try()
                .Match(result => new Result<int>(0), exception => new Result<int>(new Exception($"Failed to register task: {ScheduledTasks.ComplianceUserMeasurementsTaskName}", exception)));

            var res4 = ScheduledTasks.RegisterSystemManualTask(ScheduledTasks.FullSystemDiskCleanupTaskName, ScheduledTasks.ExeFile, ScheduledTasks.FullSystemDiskCleanupTaskArguments, ScheduledTasks.FullSystemDiskCleanupTaskDescription)
                .Match(result => new Result<int>(0), exception => new Result<int>(new Exception($"Failed to register task: {ScheduledTasks.FullSystemDiskCleanupTaskName}", exception)));

            var installResult = new List<Result<int>> { res1, res2, res3, res4 }.ToResult().Match(exitCodes => new Result<int>(exitCodes.Sum()), exception => new Result<int>(exception));
            return await Task.FromResult(installResult).ConfigureAwait(false);
        }

        public static async Task<Result<int>> UnInstall()
        {
            var res1 = ScheduledTasks.UnRegisterScheduledTask(ScheduledTasks.ComplianceCheckTaskName).Match(result => new Result<int>(0), exception => new Result<int>(exception));
            var res2 = ScheduledTasks.UnRegisterScheduledTask(ScheduledTasks.ComplianceSystemMeasurementsTaskName).Match(result => new Result<int>(0), exception => new Result<int>(exception));
            var res3 = ScheduledTasks.UnRegisterScheduledTask(ScheduledTasks.ComplianceUserMeasurementsTaskName).Match(result => new Result<int>(0), exception => new Result<int>(exception));
            var res4 = ScheduledTasks.UnRegisterScheduledTask(ScheduledTasks.FullSystemDiskCleanupTaskName).Match(result => new Result<int>(0), exception => new Result<int>(exception));
            var unInstallResult = new List<Result<int>> { res1, res2, res3, res4 }.ToResult().Match(exitCodes => new Result<int>(exitCodes.Sum()), exception => new Result<int>(exception));
            return await Task.FromResult(unInstallResult).ConfigureAwait(false);
        }
    }

}

