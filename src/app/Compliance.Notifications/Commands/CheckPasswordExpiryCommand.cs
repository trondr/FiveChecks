using System;
using System.Threading.Tasks;
using Compliance.Notifications.Common;
using Compliance.Notifications.Module;
using Compliance.Notifications.Module.PasswordExpiry;
using LanguageExt.Common;

namespace Compliance.Notifications.Commands
{
    public static class CheckPasswordExpiryCommand
    {
        internal static async Task<Result<ToastNotificationVisibility>> CheckPasswordExpiryPure(Func<Task<PasswordExpiryInfo>> loadPasswordExpiryInfo, Func<DateTime,string, Task<Result<ToastNotificationVisibility>>> showToastNotification, Func<Task<Result<ToastNotificationVisibility>>> removeToastNotification)
        {
            var info = await loadPasswordExpiryInfo().ConfigureAwait(false);
            if (info.PasswordExpiryStatus == PasswordExpiryStatus.ExpiringSoon)
            {
                return await showToastNotification(info.PasswordExpiryDate,"My Company AS").ConfigureAwait(false);
            }
            var result = await removeToastNotification().ConfigureAwait(false);
            return result;
        }

        public static async Task<Result<ToastNotificationVisibility>> CheckPasswordExpiry()
        {
            var groupName = ToastGroups.CheckPasswordExpiry;
            var tag = ToastGroups.CheckPasswordExpiry;
            return await CheckPasswordExpiryPure(F.LoadPasswordExpiryInfo, (passwordExpirationDate,companyName) => F.ShowPasswordExpiryToastNotification(passwordExpirationDate, companyName, tag, groupName), () => ToastHelper.RemoveToastNotification(groupName)).ConfigureAwait(false);
        }
    }
}