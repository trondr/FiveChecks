using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic.DiskspaceCheck
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
        /// <returns></returns>
        internal static async Task<Result<ToastNotificationVisibility>> CheckDiskSpacePure(
            UDecimal requiredFreeDiskSpace,
            bool subtractSccmCache, 
            Func<Task<DiskSpaceInfo>> loadInfo,
            Func<DiskSpaceInfo, bool> isNonCompliant,
            Func<decimal, string, Task<Result<ToastNotificationVisibility>>> showDiskSpaceToastNotification,
            Func<Task<Result<ToastNotificationVisibility>>> removeDiskSpaceToastNotification)
        {
            var info = await loadInfo().ConfigureAwait(false);
            if (isNonCompliant(info))
            {
                return await showDiskSpaceToastNotification(RequiredCleanupAmount(info, requiredFreeDiskSpace,subtractSccmCache), "My Company AS").ConfigureAwait(false);
            }
            var result = await removeDiskSpaceToastNotification().ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Check disk space compliance.
        /// </summary>
        /// <param name="requiredFreeDiskSpace">Required free disk space in GB.</param>
        /// <param name="subtractSccmCache">When set to true, disk space is compliant if: ((CurrentTotalFreeDiskSpace + CurrentSizeOfSccmCache) - requiredFreeDiskSpace) > 0</param>
        /// <returns></returns>
        public static async Task<Result<ToastNotificationVisibility>> CheckDiskSpace(UDecimal requiredFreeDiskSpace, bool subtractSccmCache)
        {
            var groupName = ToastGroups.CheckDiskSpace;
            var tag = ToastGroups.CheckDiskSpace;
            bool IsNonCompliant(DiskSpaceInfo spaceInfo) => RequiredCleanupAmount(spaceInfo,requiredFreeDiskSpace,subtractSccmCache) > 0;
            return await CheckDiskSpaceCommand.CheckDiskSpacePure(
                    requiredFreeDiskSpace,
                    subtractSccmCache,
                    () => F.LoadInfo<DiskSpaceInfo>(DiskSpace.LoadDiskSpaceResult, IsNonCompliant, ScheduledTasks.ComplianceSystemMeasurements, true),
                    IsNonCompliant,
                    (requiredCleanupAmount, companyName) => DiskSpace.ShowDiskSpaceToastNotification(requiredCleanupAmount, companyName, tag, groupName),
                    () => ToastHelper.RemoveToastNotification(groupName))
                .ConfigureAwait(false);
        }

        private static UDecimal RequiredCleanupAmount(DiskSpaceInfo spaceInfo, UDecimal requiredFreeDiskSpace, bool subtractSccmCache) => requiredFreeDiskSpace - (spaceInfo.TotalFreeDiskSpace + (subtractSccmCache ? spaceInfo.SccmCacheSize : 0));
    }
}
