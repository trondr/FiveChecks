using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic.Commands
{
    public static class CheckPendingRebootCommand
    {
        internal static async Task<Result<ToastNotificationVisibility>> CheckPendingRebootPure(Func<Task<PendingRebootInfo>> loadPendingRebootInfo, Func<string, Task<Result<ToastNotificationVisibility>>> showToastNotification, Func<Task<Result<ToastNotificationVisibility>>> removeToastNotification)
        {
            var diskSpaceInfo = await loadPendingRebootInfo().ConfigureAwait(false);
            if (diskSpaceInfo.RebootIsPending)
            {
                return await showToastNotification("My Company AS").ConfigureAwait(false);
            }
            var result = await removeToastNotification().ConfigureAwait(false);
            return result;
        }

        public static async Task<Result<ToastNotificationVisibility>> CheckPendingReboot()
        {
            var groupName = ToastGroups.CheckPendingReboot;
            var tag = ToastGroups.CheckPendingReboot;
            return await CheckPendingRebootPure(PendingReboot.LoadPendingRebootInfo, companyName => PendingReboot.ShowPendingRebootToastNotification(companyName, tag, groupName),() => ToastHelper.RemoveToastNotification(groupName)).ConfigureAwait(false);
        }
    }
}