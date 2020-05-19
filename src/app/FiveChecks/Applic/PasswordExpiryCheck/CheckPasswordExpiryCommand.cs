using System;
using System.Threading.Tasks;
using FiveChecks.Applic.Common;
using LanguageExt;
using LanguageExt.Common;

namespace FiveChecks.Applic.PasswordExpiryCheck
{
    public static class CheckPasswordExpiryCommand
    {
        internal static async Task<Result<ToastNotificationVisibility>> CheckPasswordExpiryPure(Func<Task<PasswordExpiryInfo>> loadInfo, Func<PasswordExpiryInfo, bool> isNonCompliant, Func<DateTime, Task<Result<ToastNotificationVisibility>>> showToastNotification, Func<Task<Result<ToastNotificationVisibility>>> removeToastNotification, bool isDisabled)
        {
            if(isDisabled) return await removeToastNotification().ConfigureAwait(false);
            var info = await loadInfo().ConfigureAwait(false);
            if (isNonCompliant(info))
            {
                return await showToastNotification(info.PasswordExpiryDate).ConfigureAwait(false);
            }
            return await removeToastNotification().ConfigureAwait(false);
        }

        public static async Task<Result<ToastNotificationVisibility>> CheckPasswordExpiry(Some<NotificationProfile> userProfile, bool isDisabled)
        {
            var groupName = ToastGroups.CheckPasswordExpiry;
            var tag = ToastGroups.CheckPasswordExpiry;
            var passwordExpiryCheckIsDisabled = Profile.IsCheckDisabled(isDisabled, typeof(CheckPasswordExpiryCommand));
            bool IsNonCompliant(PasswordExpiryInfo info) => info.PasswordExpiryStatus == PasswordExpiryStatus.ExpiringSoon;
            return await CheckPasswordExpiryPure(
                () => ComplianceInfo.LoadInfo<PasswordExpiryInfo>(PasswordExpire.LoadPasswordExpiryInfo, IsNonCompliant, ScheduledTasks.ComplianceUserMeasurements, true),
                IsNonCompliant, 
                (passwordExpirationDate) => PasswordExpire.ShowPasswordExpiryToastNotification(userProfile,passwordExpirationDate, tag, groupName), 
                () => ToastHelper.RemoveToastNotification(groupName),
                passwordExpiryCheckIsDisabled
                ).ConfigureAwait(false);
        }
    }
}