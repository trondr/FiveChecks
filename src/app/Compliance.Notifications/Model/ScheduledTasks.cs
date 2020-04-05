using System;
using System.Security.Principal;
using Microsoft.Win32.TaskScheduler;

namespace Compliance.Notifications.Model
{
    public static class ScheduledTasks
    {
        public const string DiskSpaceComplianceCheckTaskName = "Compliance Notification Check - DiskSpace";
        public const string DiskSpaceComplianceCheckTaskDescription = "DiskSpace Compliance Notification Check at workstation unlock";

        public const string PendingRebootComplianceCheckTaskName = "Compliance Notification Check - Pending Reboot";
        public const string PendingRebootComplianceCheckTaskDescription = "Pending Reboot Compliance Notification Check at workstation unlock";

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

        
    }
}