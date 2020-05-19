using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Win32.TaskScheduler;
using Task = System.Threading.Tasks.Task;

namespace FiveChecks.Applic.Common
{
    public static class Setup
    {
        public static async Task<Result<int>> Install()
        {
            var res1 = ScheduledTasks.RegisterUserScheduledTask(ScheduledTasks.ComplianceCheck.TaskName, ScheduledTasks.ComplianceCheck.ActionPath, ScheduledTasks.ComplianceCheck.ActionArguments, ScheduledTasks.ComplianceCheck.TaskDescription, new List<Trigger> { ScheduledTasks.UnlockTrigger(), ScheduledTasks.LoginTrigger(), ScheduledTasks.EventTrigger(ScheduledTasks.ComplianceCheck.EventId) })
                .Try()
                .Match(result => new Result<Unit>(Unit.Default), exception => new Result<Unit>(new Exception($"Failed to register task: {ScheduledTasks.ComplianceCheck.TaskName}", exception)));

            var res2 = ScheduledTasks.RegisterSystemScheduledTask(ScheduledTasks.ComplianceSystemMeasurements.TaskName, ScheduledTasks.ComplianceSystemMeasurements.ActionPath, ScheduledTasks.ComplianceSystemMeasurements.ActionArguments, ScheduledTasks.ComplianceSystemMeasurements.TaskDescription, new List<Trigger> { ScheduledTasks.HourlyTrigger(), ScheduledTasks.EventTrigger(ScheduledTasks.ComplianceSystemMeasurements.EventId) })
                .Try()
                .Match(result => new Result<Unit>(Unit.Default), exception => new Result<Unit>(new Exception($"Failed to register task: {ScheduledTasks.ComplianceSystemMeasurements.TaskName}", exception)));

            var res3 = ScheduledTasks.RegisterUserScheduledTask(ScheduledTasks.ComplianceUserMeasurements.TaskName, ScheduledTasks.ComplianceUserMeasurements.ActionPath, ScheduledTasks.ComplianceUserMeasurements.ActionArguments, ScheduledTasks.ComplianceUserMeasurements.TaskDescription, new List<Trigger> { ScheduledTasks.HourlyTrigger(), ScheduledTasks.LoginTrigger(), ScheduledTasks.EventTrigger(ScheduledTasks.ComplianceUserMeasurements.EventId) })
                .Try()
                .Match(result => new Result<Unit>(Unit.Default), exception => new Result<Unit>(new Exception($"Failed to register task: {ScheduledTasks.ComplianceUserMeasurements.TaskName}", exception)));

            //var res4 = ScheduledTasks.RegisterSystemManualTask(ScheduledTasks.FullSystemDiskCleanup.TaskName,
            //        ScheduledTasks.FullSystemDiskCleanup.ActionPath, ScheduledTasks.FullSystemDiskCleanup.ActionArguments,
            //    ScheduledTasks.FullSystemDiskCleanup.TaskDescription, new List<Trigger> { ScheduledTasks.HourlyTrigger(), ScheduledTasks.EventTrigger(ScheduledTasks.FullSystemDiskCleanup.EventId) })
            //    .Match(result => new Result<Unit>(Unit.Default), exception => new Result<Unit>(new Exception($"Failed to register task: {ScheduledTasks.FullSystemDiskCleanup.TaskName}", exception)));

            var res5 = 
                (await WindowsEventLog.CreateEventSource().ConfigureAwait(false))
                .Match(unit => new Result<Unit>(Unit.Default),exception => new Result<Unit>(new Exception("Failed to register event log source.", exception)) );

            var installResult = new List<Result<Unit>> { res1, res2, res3, res5 }.ToResult().Match(exitCodes => new Result<int>(0), exception => new Result<int>(exception));
            return await Task.FromResult(installResult).ConfigureAwait(false);
        }

        public static async Task<Result<int>> UnInstall()
        {
            var res1 = ScheduledTasks.UnRegisterScheduledTask(ScheduledTasks.ComplianceCheck.TaskName).Match(result => new Result<int>(0), exception => new Result<int>(exception));
            var res2 = ScheduledTasks.UnRegisterScheduledTask(ScheduledTasks.ComplianceSystemMeasurements.TaskName).Match(result => new Result<int>(0), exception => new Result<int>(exception));
            var res3 = ScheduledTasks.UnRegisterScheduledTask(ScheduledTasks.ComplianceUserMeasurements.TaskName).Match(result => new Result<int>(0), exception => new Result<int>(exception));
            //var res4 = ScheduledTasks.UnRegisterScheduledTask(ScheduledTasks.FullSystemDiskCleanup.TaskName).Match(result => new Result<int>(0), exception => new Result<int>(exception));
            var unInstallResult = new List<Result<int>> { res1, res2, res3 }.ToResult().Match(exitCodes => new Result<int>(exitCodes.Sum()), exception => new Result<int>(exception));
            return await Task.FromResult(unInstallResult).ConfigureAwait(false);
        }
    }

}

