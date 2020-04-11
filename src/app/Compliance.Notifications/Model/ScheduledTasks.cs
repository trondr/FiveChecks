using System;
using System.Security.Principal;
using Compliance.Notifications.Common;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Win32.TaskScheduler;
using Pri.LongPath;
using System.Linq;

namespace Compliance.Notifications.Model
{
    public static class ScheduledTasks
    {
        public const string ComplianceCheckTaskName = "Compliance Notification Check";
        public const string ComplianceCheckTaskDescription = "Compliance Notification Check at workstation unlock";
        public const string ComplianceCheckArguments = "CheckCompliance /requiredFreeDiskSpace=\"40\" /subtractSccmCache=\"True\" /disableDiskSpaceCheck=\"False\" /disablePendingRebootCheck=\"False\" /userInterfaceCulture=\"nb-NO\"";

        public const string ComplianceSystemMeasurementsTaskName = "Compliance System Measurement";
        public const string ComplianceSystemMeasurementsTaskDescription = "Measurement system compliance hourly";

        public const string ComplianceUserMeasurementsTaskName = "Compliance User Measurement";
        public const string ComplianceUserMeasurementsTaskDescription = "Measurement user compliance hourly";

        public const string FullSystemDiskCleanupTaskName = "Compliance Full System Disk Cleanup";
        public const string FullSystemDiskCleanupDescription = "Compliance Full System Disk Cleanup";

        public static Func<Trigger> UnlockTrigger => () => new SessionStateChangeTrigger(TaskSessionStateChangeType.SessionUnlock);

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


        public static Result<Unit> RegisterSystemManualTask(Some<string> taskName, Some<FileInfo> exeFile, Some<string> arguments, Some<string> taskDescription)
        {
            return F.TryFunc(() =>
            {
                using (var ts = TaskService.Instance)
                {
                    using (var td = ts.NewTask())
                    {
                        td.RegistrationInfo.Description = taskDescription.Value;
                        td.Actions.Add(new ExecAction(exeFile.Value.FullName, arguments.Value, exeFile.Value.Directory.FullName));
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