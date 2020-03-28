using System;
using System.Threading.Tasks;
using Compliance.Notifications.Common;
using Compliance.Notifications.ComplianceItems;
using LanguageExt.Common;

namespace Compliance.Notifications.Commands
{
    public static class CheckPendingRebootCommand
    {
        internal static async Task<Result<int>> CheckPendingRebootF(Func<Task<PendingRebootInfo>> loadPendingRebootInfo, Func<string, Task<Result<int>>> showToastNotification)
        {
            var diskSpaceInfo = await loadPendingRebootInfo().ConfigureAwait(false);
            if (diskSpaceInfo.RebootIsPending)
            {
                return await showToastNotification("My Company AS").ConfigureAwait(false);
            }
            return new Result<int>(0);
        }

        public static async Task<Result<int>> CheckPendingReboot()
        {
            return await CheckPendingRebootF(F.LoadPendingRebootInfo, F.ShowPendingRebootToastNotification).ConfigureAwait(false);
        }
    }
}