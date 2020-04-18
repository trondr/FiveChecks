using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Win32.TaskScheduler;
using Pri.LongPath;

namespace Compliance.Notifications.Applic.Common
{
    public static class ScheduledTasks
    {
        public const string ComplianceCheckTaskName = "Compliance Notification Check";
        public const string ComplianceCheckTaskDescription = "Compliance Notification Check at workstation unlock";
        public const string ComplianceCheckTaskArguments = "CheckCompliance /requiredFreeDiskSpace=\"40\" /subtractSccmCache=\"True\" /maxUptimeHours=\"168\" /disableDiskSpaceCheck=\"False\" /disablePendingRebootCheck=\"False\" /disableSystemUptimeCheck=\"False\" /userInterfaceCulture=\"nb-NO\"";

        public const string ComplianceSystemMeasurementsTaskName = "Compliance System Measurement";
        public const string ComplianceSystemMeasurementsTaskDescription = "Measurement system compliance hourly";
        public const string ComplianceSystemMeasurementsTaskArguments = "MeasureSystemComplianceItems";

        public const string ComplianceUserMeasurementsTaskName = "Compliance User Measurement";
        public const string ComplianceUserMeasurementsTaskDescription = "Measurement user compliance hourly";
        public const string ComplianceUserMeasurementsTaskArguments = "MeasureUserComplianceItems";

        public const string FullSystemDiskCleanupTaskName = "Compliance Full System Disk Cleanup";
        public const string FullSystemDiskCleanupTaskDescription = "Compliance Full System Disk Cleanup";
        public const string FullSystemDiskCleanupTaskArguments = "RunFullSystemDiskCleanup";

        public static FileInfo ExeFile { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location);

        public static Func<Trigger> UnlockTrigger => () => new SessionStateChangeTrigger(TaskSessionStateChangeType.SessionUnlock);

        public static Func<Trigger> LoginTrigger => () => new LogonTrigger();

        public static Func<Trigger> HourlyTrigger => () => new DailyTrigger {Repetition = new RepetitionPattern(new TimeSpan(0, 1, 0, 0, 0), new TimeSpan(1, 0, 0, 0))};

        public static Func<string> BuiltInUsers => () =>
        {
            var sid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            var account = (NTAccount)sid.Translate(typeof(NTAccount));
            return account.Value;
        };


        public static Try<Unit> RegisterSystemScheduledTask(Some<string> taskName, Some<FileInfo> exeFile,
    Some<string> arguments, Some<string> taskDescription) => () =>
    {
        using (var ts = TaskService.Instance)
        {
            using (var td = ts.NewTask())
            {
                td.RegistrationInfo.Description = taskDescription.Value;
                td.Actions.Add(new ExecAction($"\"{exeFile.Value.FullName}\"", arguments.Value, exeFile.Value.Directory.FullName));
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


        public static Result<Unit> RegisterSystemManualTask(Some<string> taskName, Some<FileInfo> exeFile, Some<string> arguments, Some<string> taskDescription)
        {
            return F.TryFunc(() =>
            {
                using (var ts = TaskService.Instance)
                {
                    using (var td = ts.NewTask())
                    {
                        td.RegistrationInfo.Description = taskDescription.Value;
                        td.Actions.Add(new ExecAction($"\"{exeFile.Value.FullName}\"", arguments.Value, exeFile.Value.Directory.FullName));
                        //Allow users to run scheduled task : (A;;0x1200a9;;;BU)
                        td.RegistrationInfo.SecurityDescriptorSddlForm =
                            "O:BAG:SYD:AI(A;;FR;;;SY)(A;;0x1200a9;;;BU)(A;ID;0x1f019f;;;BA)(A;ID;0x1f019f;;;SY)(A;ID;FA;;;BA)";
                        td.Principal.UserId = "SYSTEM";
                        td.Principal.RunLevel = TaskRunLevel.Highest;
                        ts.RootFolder.RegisterTaskDefinition(taskName.Value, td);
                    }
                }
                return new Result<Unit>(Unit.Default);
            });
        }


        public static Try<Unit> RegisterUserScheduledTask(Some<string> taskName, Some<FileInfo> exeFile,
            Some<string> arguments, Some<string> taskDescription, Some<List<Trigger>> triggers) => () =>
            {
                using (var ts = TaskService.Instance)
                {
                    using (var td = ts.NewTask())
                    {
                        td.RegistrationInfo.Description = taskDescription.Value;
                        td.Actions.Add(new ExecAction($"\"{exeFile.Value.FullName}\"", arguments.Value, exeFile.Value.Directory.FullName));
                        foreach (var trigger in triggers.Value)
                        {
                            td.Triggers.Add(trigger);
                        }
                        td.Principal.GroupId = ScheduledTasks.BuiltInUsers();
                        td.Principal.RunLevel = TaskRunLevel.LUA;
                        ts.RootFolder.RegisterTaskDefinition(taskName.Value, td);
                    }
                }
                return new Result<Unit>(Unit.Default);
            };

        public static Result<Unit> UnRegisterScheduledTask(Some<string> taskName)
        {
            return F.TryFunc(() =>
            {
                var task = new Option<Microsoft.Win32.TaskScheduler.Task>(TaskService.Instance.AllTasks.Where(t => t.Name == taskName));
                return task.Match(t =>
                {
                    TaskService.Instance.RootFolder.DeleteTask(t.Name, false);
                    return new Result<Unit>(Unit.Default);
                }, () => new Result<Unit>(Unit.Default));
            });
        }
    }
}