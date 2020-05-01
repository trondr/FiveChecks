using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using LanguageExt;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic.PasswordExpiryCheck
{
    public static class CheckPasswordExpiryCommand
    {
        internal static async Task<Result<ToastNotificationVisibility>> CheckPasswordExpiryPure(Func<Task<PasswordExpiryInfo>> loadInfo, Func<PasswordExpiryInfo, bool> isNonCompliant, Func<DateTime, Task<Result<ToastNotificationVisibility>>> showToastNotification, Func<Task<Result<ToastNotificationVisibility>>> removeToastNotification)
        {
            var info = await loadInfo().ConfigureAwait(false);
            if (isNonCompliant(info))
            {
                return await showToastNotification(info.PasswordExpiryDate).ConfigureAwait(false);
            }
            var result = await removeToastNotification().ConfigureAwait(false);
            return result;
        }

        public static async Task<Result<ToastNotificationVisibility>> CheckPasswordExpiry(Some<NotificationProfile> userProfile)
        {
            var groupName = ToastGroups.CheckPasswordExpiry;
            var tag = ToastGroups.CheckPasswordExpiry;
            bool IsNonCompliant(PasswordExpiryInfo info) => info.PasswordExpiryStatus == PasswordExpiryStatus.ExpiringSoon;
            return await CheckPasswordExpiryPure(
                () => F.LoadInfo<PasswordExpiryInfo>(PasswordExpire.LoadPasswordExpiryInfo, IsNonCompliant, ScheduledTasks.ComplianceUserMeasurements, true),
                IsNonCompliant, 
                (passwordExpirationDate) => PasswordExpire.ShowPasswordExpiryToastNotification(userProfile,passwordExpirationDate, tag, groupName), 
                () => ToastHelper.RemoveToastNotification(groupName)
                ).ConfigureAwait(false);
        }

        public static bool IsDisabled(bool defaultValue)
        {
            var policyCategory = typeof(CheckPasswordExpiryCommand).GetPolicyCategory();
            return F.PolicyCategoryIsDisabled(policyCategory, defaultValue);
        }
    }
}