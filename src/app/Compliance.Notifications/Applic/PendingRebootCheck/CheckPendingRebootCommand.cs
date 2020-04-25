using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic.PendingRebootCheck
{
    public static class CheckPendingRebootCommand
    {
        internal static async Task<Result<ToastNotificationVisibility>> CheckPendingRebootPure(Func<Task<PendingRebootInfo>> loadInfo, Func<PendingRebootInfo, bool> isNonCompliant, Func<PendingRebootInfo,string, Task<Result<ToastNotificationVisibility>>> showToastNotification, Func<Task<Result<ToastNotificationVisibility>>> removeToastNotification)
        {
            var info = await loadInfo().ConfigureAwait(false);
            if (isNonCompliant(info))
            {
                return await showToastNotification(info,"My Company AS").ConfigureAwait(false);
            }
            var result = await removeToastNotification().ConfigureAwait(false);
            return result;
        }

        public static async Task<Result<ToastNotificationVisibility>> CheckPendingReboot()
        {
            var groupName = ToastGroups.CheckPendingReboot;
            var tag = ToastGroups.CheckPendingReboot;
            bool IsNonCompliant(PendingRebootInfo info) => info.RebootIsPending;
            return await CheckPendingRebootPure(
                () => F.LoadInfo<PendingRebootInfo>(PendingReboot.LoadPendingRebootInfo, IsNonCompliant, ScheduledTasks.ComplianceSystemMeasurements, true),
                IsNonCompliant,
                (info,companyName) => PendingReboot.ShowPendingRebootToastNotification(info, companyName, tag, groupName),
                () => ToastHelper.RemoveToastNotification(groupName)
                ).ConfigureAwait(false);
        }
    }
}