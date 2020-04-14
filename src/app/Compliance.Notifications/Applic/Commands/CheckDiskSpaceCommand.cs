using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic.Commands
{
    public static class CheckDiskSpaceCommand
    {
        /// <summary>
        /// Check disk space compliance. Testable version.
        /// </summary>
        /// <param name="requiredFreeDiskSpace">Required free disk space in GB.</param>
        /// <param name="subtractSccmCache">When set to true, disk space is compliant if: ((CurrentTotalFreeDiskSpace + CurrentSizeOfSccmCache) - requiredFreeDiskSpace) > 0</param>
        /// <param name="loadDiskSpaceResult">Load disk space result function</param>
        /// <param name="showDiskSpaceToastNotification"></param>
        /// <param name="removeDiskSpaceToastNotification"></param>
        /// <returns></returns>
        internal static async Task<Result<ToastNotificationVisibility>> CheckDiskSpacePure(UDecimal requiredFreeDiskSpace,
            bool subtractSccmCache, Func<Task<DiskSpaceInfo>> loadDiskSpaceResult,
            Func<decimal, string, Task<Result<ToastNotificationVisibility>>> showDiskSpaceToastNotification,
            Func<Task<Result<ToastNotificationVisibility>>> removeDiskSpaceToastNotification)
        {
            var diskSpaceInfo = await loadDiskSpaceResult().ConfigureAwait(false);
            var requiredCleanupAmount = requiredFreeDiskSpace - (diskSpaceInfo.TotalFreeDiskSpace + (subtractSccmCache ? diskSpaceInfo.SccmCacheSize : 0));
            var isNotCompliant = requiredCleanupAmount > 0;
            if (isNotCompliant)
            {
                return await showDiskSpaceToastNotification(requiredCleanupAmount, "My Company AS").ConfigureAwait(false);
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
            return await CheckDiskSpaceCommand.CheckDiskSpacePure(
                requiredFreeDiskSpace, 
                subtractSccmCache, 
                DiskSpace.LoadDiskSpaceResult, 
                (requiredCleanupAmount, companyName) => DiskSpace.ShowDiskSpaceToastNotification(requiredCleanupAmount, companyName, tag, groupName),
                () => ToastHelper.RemoveToastNotification(groupName)
                )
                .ConfigureAwait(false);
        }
    }
}
