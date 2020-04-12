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
        internal static async Task<Result<int>> CheckPasswordExpiryPure(Func<Task<PasswordExpiryInfo>> loadPasswordExpiryInfo, Func<DateTime,string, Task<Result<int>>> showToastNotification, Func<Task<Result<int>>> removeToastNotification, Action sendApplicationExit)
        {
            var info = await loadPasswordExpiryInfo().ConfigureAwait(false);
            if (info.PasswordExpiryStatus == PasswordExpiryStatus.ExpiringSoon)
            {
                return await showToastNotification(info.PasswordExpiryDate,"My Company AS").ConfigureAwait(false);
            }
            var result = await removeToastNotification().ConfigureAwait(false);
            sendApplicationExit();
            return result;
        }

        public static async Task<Result<int>> CheckPasswordExpiry()
        {
            return await CheckPasswordExpiryPure(F.LoadPasswordExpiryInfo, (passwordExpirationDate,companyName) => F.ShowPasswordExpiryToastNotification(passwordExpirationDate, companyName, nameof(CheckPasswordExpiryCommand), nameof(CheckPasswordExpiryCommand)), () => ToastHelper.RemoveToastNotification(nameof(CheckPasswordExpiryCommand)),() => Messenger.Default.Send(new ExitApplicationMessage(nameof(CheckPasswordExpiryCommand)))).ConfigureAwait(false);
        }
    }
}