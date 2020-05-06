using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using LanguageExt;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic.DiskSpaceCheck
{
    public static class CheckDiskSpaceCommand
    {
        /// <summary>
        /// Check disk space compliance. Testable version.
        /// </summary>
        /// <param name="requiredFreeDiskSpace">Required free disk space in GB.</param>
        /// <param name="subtractSccmCache">When set to true, disk space is compliant if: ((CurrentTotalFreeDiskSpace + CurrentSizeOfSccmCache) - requiredFreeDiskSpace) > 0</param>
        /// <param name="loadInfo">Load disk space result function</param>
        /// <param name="isNonCompliant">function calculating non-compliance</param>
        /// <param name="showDiskSpaceToastNotification"></param>
        /// <param name="removeDiskSpaceToastNotification"></param>
        /// <param name="isDisabled"></param>
        /// <returns></returns>
        internal static async Task<Result<ToastNotificationVisibility>> CheckDiskSpacePure(
            UDecimal requiredFreeDiskSpace,
            bool subtractSccmCache, 
            Func<Task<DiskSpaceInfo>> loadInfo,
            Func<DiskSpaceInfo, bool> isNonCompliant,
            Func<decimal, Task<Result<ToastNotificationVisibility>>> showDiskSpaceToastNotification,
            Func<Task<Result<ToastNotificationVisibility>>> removeDiskSpaceToastNotification, bool isDisabled)
        {
            if(isDisabled) return await removeDiskSpaceToastNotification().ConfigureAwait(false);
            var info = await loadInfo().ConfigureAwait(false);
            if (isNonCompliant(info))
            {
                return await showDiskSpaceToastNotification(RequiredCleanupAmount(info, requiredFreeDiskSpace,subtractSccmCache)).ConfigureAwait(false);
            }
            return await removeDiskSpaceToastNotification().ConfigureAwait(false);
        }

        /// <summary>
        /// Check disk space compliance.
        /// </summary>
        /// <param name="notificationProfile"></param>
        /// <param name="requiredFreeDiskSpace">Required free disk space in GB.</param>
        /// <param name="subtractSccmCache">When set to true, disk space is compliant if: ((CurrentTotalFreeDiskSpace + CurrentSizeOfSccmCache) - requiredFreeDiskSpace) > 0</param>
        /// <param name="isDisabled"></param>
        /// <returns></returns>
        public static async Task<Result<ToastNotificationVisibility>> CheckDiskSpace(Some<NotificationProfile> notificationProfile, UDecimal requiredFreeDiskSpace, bool subtractSccmCache, bool isDisabled)
        {
            var category = typeof(CheckDiskSpaceCommand).GetPolicyCategory();
            var policyRequiredFreeDiskSpace = Profile.GetIntegerPolicyValue(Context.Machine, category, "RequiredFreeDiskSpace", (int)requiredFreeDiskSpace);
            var policySubtractSccmCache = Profile.GetBooleanPolicyValue(Context.Machine, category, "SubtractSccmCache", subtractSccmCache);
            var diskSpaceCheckIsDisabled = F.IsCheckDisabled(isDisabled, typeof(CheckDiskSpaceCommand));

            var groupName = ToastGroups.CheckDiskSpace;
            var tag = ToastGroups.CheckDiskSpace;
            bool IsNonCompliant(DiskSpaceInfo spaceInfo) => RequiredCleanupAmount(spaceInfo, policyRequiredFreeDiskSpace, policySubtractSccmCache) > 0;
            return await CheckDiskSpaceCommand.CheckDiskSpacePure(
                    policyRequiredFreeDiskSpace,
                    policySubtractSccmCache,
                    () => F.LoadInfo<DiskSpaceInfo>(DiskSpace.LoadDiskSpaceResult, IsNonCompliant, ScheduledTasks.ComplianceSystemMeasurements, true),
                    IsNonCompliant,
                    (requiredCleanupAmount) => DiskSpace.ShowDiskSpaceToastNotification(notificationProfile, requiredCleanupAmount, tag, groupName),
                    () => ToastHelper.RemoveToastNotification(groupName), diskSpaceCheckIsDisabled)
                .ConfigureAwait(false);
        }

        private static decimal RequiredCleanupAmount(DiskSpaceInfo spaceInfo, UDecimal requiredFreeDiskSpace, bool subtractSccmCache) => requiredFreeDiskSpace - (spaceInfo.TotalFreeDiskSpace + (subtractSccmCache ? spaceInfo.SccmCacheSize : 0));
    }
}
