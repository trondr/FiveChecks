using System;
using System.Linq;
using System.Threading.Tasks;
using FiveChecks.Applic.Common;
using LanguageExt;
using LanguageExt.Common;

namespace FiveChecks.Applic.MissingMsUpdatesCheck
{
    public static class CheckMissingMsUpdatesCommand
    {
        public static async Task<Result<ToastNotificationVisibility>> CheckMissingMsUpdatesPure(
            Func<Task<MissingMsUpdatesInfo>> loadInfo,
            Func<MissingMsUpdatesInfo, bool> isNonCompliant,
            Func<MissingMsUpdatesInfo, Task<Result<ToastNotificationVisibility>>> showToastNotification,
            Func<Task<Result<ToastNotificationVisibility>>> removeToastNotification, bool isDisabled
            )
        {
            if (loadInfo == null) throw new ArgumentNullException(nameof(loadInfo));
            if (removeToastNotification == null) throw new ArgumentNullException(nameof(removeToastNotification));
            if (isDisabled) return await removeToastNotification().ConfigureAwait(false);
            var info = await loadInfo().ConfigureAwait(false);
            if (isNonCompliant(info))
            {
                return await showToastNotification(info).ConfigureAwait(false);
            }
            return await removeToastNotification().ConfigureAwait(false);
        }
        
        public static async Task<Result<ToastNotificationVisibility>> CheckMissingMsUpdates(
            Some<NotificationProfile> userProfile, int hoursToWaitBeforeNotifyUser, bool isDisabled)
        {
            var category = typeof(CheckMissingMsUpdatesCommand).GetPolicyCategory();
            var policyHoursToWaitBeforeNotifyUser = Profile.GetIntegerPolicyValue(Context.Machine, category, "HoursToWaitBeforeNotifyingUser", (int)hoursToWaitBeforeNotifyUser);

            var groupName = ToastGroups.CheckMissingMsUpdates;
            var tag = ToastGroups.CheckMissingMsUpdates;
            var systemUptimeCheckIsDisabled = Profile.IsCheckDisabled(isDisabled, typeof(CheckMissingMsUpdatesCommand));
            bool IsNonCompliant(MissingMsUpdatesInfo info) => info.Updates.Count > 0 && info.Updates.Any(update => (DateTime.Now - update.FirstMeasuredMissing).TotalHours >= policyHoursToWaitBeforeNotifyUser);
            return await CheckMissingMsUpdatesPure(() => ComplianceInfo.LoadInfo(MissingMsUpdates.LoadMissingMsUpdatesInfo, IsNonCompliant, ScheduledTasks.ComplianceSystemMeasurements, true),
                IsNonCompliant,
                (info) => MissingMsUpdates.ShowMissingUpdatesToastNotification(userProfile, tag, groupName, info),
                () => ToastHelper.RemoveToastNotification(groupName),
                systemUptimeCheckIsDisabled
            ).ConfigureAwait(false);
        }
    }
}
