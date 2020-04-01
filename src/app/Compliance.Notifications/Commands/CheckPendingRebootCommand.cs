using System;
using System.Threading.Tasks;
using Compliance.Notifications.Common;
using Compliance.Notifications.Model;
using GalaSoft.MvvmLight.Messaging;
using LanguageExt.Common;

namespace Compliance.Notifications.Commands
{
    public static class CheckPendingRebootCommand
    {
        internal static async Task<Result<int>> CheckPendingRebootF(Func<Task<PendingRebootInfo>> loadPendingRebootInfo, Func<string, Task<Result<int>>> showToastNotification, Func<Task<Result<int>>> removeToastNotification)
        {
            var diskSpaceInfo = await loadPendingRebootInfo().ConfigureAwait(false);
            if (diskSpaceInfo.RebootIsPending)
            {
                return await showToastNotification("My Company AS").ConfigureAwait(false);
            }
            var result = await removeToastNotification().ConfigureAwait(false);
            Messenger.Default.Send(new ExitApplicationMessage());
            return result;
        }

        public static async Task<Result<int>> CheckPendingReboot()
        {
            return await CheckPendingRebootF(F.LoadPendingRebootInfo, companyName => F.ShowPendingRebootToastNotification(companyName, nameof(CheckPendingRebootCommand), nameof(CheckPendingRebootCommand)),() => F.RemoveToastNotification(nameof(CheckPendingRebootCommand))).ConfigureAwait(false);
        }
    }
}