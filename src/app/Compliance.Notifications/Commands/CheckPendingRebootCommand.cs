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
        internal static async Task<Result<int>> CheckPendingRebootPure(Func<Task<PendingRebootInfo>> loadPendingRebootInfo, Func<string, Task<Result<int>>> showToastNotification, Func<Task<Result<int>>> removeToastNotification,Action sendApplicationExitMessage)
        {
            var diskSpaceInfo = await loadPendingRebootInfo().ConfigureAwait(false);
            if (diskSpaceInfo.RebootIsPending)
            {
                return await showToastNotification("My Company AS").ConfigureAwait(false);
            }
            var result = await removeToastNotification().ConfigureAwait(false);
            sendApplicationExitMessage();
            return result;
        }

        public static async Task<Result<int>> CheckPendingReboot()
        {
            var tag = nameof(CheckPendingRebootCommand);
            var groupName = nameof(CheckPendingRebootCommand);
            return await CheckPendingRebootPure(F.LoadPendingRebootInfo, companyName => F.ShowPendingRebootToastNotification(companyName, tag, groupName),() => ToastHelper.RemoveToastNotification(groupName),() => Messenger.Default.Send(new ExitApplicationMessage(groupName))).ConfigureAwait(false);
        }
    }
}