using System;
using System.Linq;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using LanguageExt;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic.PendingRebootCheck
{
    public static class CheckPendingRebootCommand
    {
        internal static async Task<Result<ToastNotificationVisibility>> CheckPendingRebootPure(Func<Task<PendingRebootInfo>> loadInfo, Func<PendingRebootInfo, bool> isNonCompliant, Func<PendingRebootInfo,string, Task<Result<ToastNotificationVisibility>>> showToastNotification, Func<Task<Result<ToastNotificationVisibility>>> removeToastNotification, bool isDisabled)
        {
            if(isDisabled) return await removeToastNotification().ConfigureAwait(false);
            var info = await loadInfo().ConfigureAwait(false);
            if (isNonCompliant(info))
            {
                return await showToastNotification(info,"My Company AS").ConfigureAwait(false);
            }
            return await removeToastNotification().ConfigureAwait(false);
        }

        public static async Task<Result<ToastNotificationVisibility>> CheckPendingReboot(Some<NotificationProfile> userProfile, bool isDisabled)
        {
            var groupName = ToastGroups.CheckPendingReboot;
            var tag = ToastGroups.CheckPendingReboot;
            var isPendingRebootCheckDisabled = F.IsCheckDisabled(isDisabled, typeof(CheckPendingRebootCommand));

            bool IsNonCompliant(PendingRebootInfo info)
            {
                var newInfo = info.RemoveRebootSources(RebootSource.AllSources.Where(source => source.IsDisabled()));
                return newInfo.RebootIsPending;
            }

            return await CheckPendingRebootPure(
                () => F.LoadInfo<PendingRebootInfo>(PendingReboot.LoadPendingRebootInfo, IsNonCompliant, ScheduledTasks.ComplianceSystemMeasurements, true),
                IsNonCompliant,
                (info,companyName) => PendingReboot.ShowPendingRebootToastNotification(userProfile, info, tag, groupName),
                () => ToastHelper.RemoveToastNotification(groupName),
                isPendingRebootCheckDisabled
                ).ConfigureAwait(false);
        }
    }
}