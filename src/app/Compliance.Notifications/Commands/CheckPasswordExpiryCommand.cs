using System;
using System.Threading.Tasks;
using Compliance.Notifications.Common;
using Compliance.Notifications.Model;
using Compliance.Notifications.Model.PasswordExpiry;
using GalaSoft.MvvmLight.Messaging;
using LanguageExt.Common;

namespace Compliance.Notifications.Commands
{
    public static class CheckPasswordExpiryCommand
    {
        internal static async Task<Result<int>> CheckPendingRebootF(Func<Task<PasswordExpiryInfo>> loadPasswordExpiryInfo, Func<DateTime,string, Task<Result<int>>> showToastNotification, Func<Task<Result<int>>> removeToastNotification)
        {
            var info = await loadPasswordExpiryInfo().ConfigureAwait(false);
            if (info.PasswordExpiryStatus == PasswordExpiryStatus.ExpiringSoon)
            {
                return await showToastNotification(info.PasswordExpiryDate,"My Company AS").ConfigureAwait(false);
            }
            var result = await removeToastNotification().ConfigureAwait(false);
            Messenger.Default.Send(new ExitApplicationMessage());
            return result;
        }

        public static async Task<Result<int>> CheckPasswordExpiry()
        {
            return await CheckPendingRebootF(F.LoadPasswordExpiryInfo, (passwordExpirationDate,companyName) => F.ShowPasswordExpiryToastNotification(passwordExpirationDate, companyName, nameof(CheckPasswordExpiryCommand), nameof(CheckPasswordExpiryCommand)), () => ToastHelper.RemoveToastNotification(nameof(CheckPasswordExpiryCommand))).ConfigureAwait(false);
        }
    }
}