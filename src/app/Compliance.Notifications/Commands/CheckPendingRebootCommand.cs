using System;
using System.Threading.Tasks;
using Compliance.Notifications.Common;
using Compliance.Notifications.Module;
using LanguageExt.Common;

namespace Compliance.Notifications.Commands
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
            return await CheckPendingRebootPure(F.LoadPendingRebootInfo, companyName => F.ShowPendingRebootToastNotification(companyName, tag, groupName),() => ToastHelper.RemoveToastNotification(groupName)).ConfigureAwait(false);
        }
    }
}