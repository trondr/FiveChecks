using System;
using System.Threading.Tasks;
using Compliance.Notifications.Common;
using LanguageExt.Common;

namespace Compliance.Notifications.Commands.CheckDiskSpace
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
        /// <returns></returns>
        internal static async Task<Result<int>> CheckDiskSpaceF(decimal requiredFreeDiskSpace, bool subtractSccmCache, Func<decimal, bool, DiskSpaceInfo> loadDiskSpaceResult, Func<decimal, string, Task<Result<int>>> showDiskSpaceToastNotification)
        {
            if (loadDiskSpaceResult == null) throw new ArgumentNullException(nameof(loadDiskSpaceResult));
            var diskSpaceInfo = loadDiskSpaceResult(requiredFreeDiskSpace, subtractSccmCache);
            var requiredCleanupAmount = requiredFreeDiskSpace - (diskSpaceInfo.TotalFreeDiskSpace + (subtractSccmCache ? diskSpaceInfo.SccmCacheSize : 0));
            var isNotCompliant = requiredCleanupAmount > 0;
            if (isNotCompliant)
            {
                return await showDiskSpaceToastNotification(requiredCleanupAmount, "My Company AS").ConfigureAwait(false);
            }
            return new Result<int>(0);
        }
        
        /// <summary>
        /// Check disk space compliance.
        /// </summary>
        /// <param name="requiredFreeDiskSpace">Required free disk space in GB.</param>
        /// <param name="subtractSccmCache">When set to true, disk space is compliant if: ((CurrentTotalFreeDiskSpace + CurrentSizeOfSccmCache) - requiredFreeDiskSpace) > 0</param>
        /// <returns></returns>
        public static async Task<Result<int>> CheckDiskSpace(decimal requiredFreeDiskSpace, bool subtractSccmCache)
        {
            return await CheckDiskSpaceCommand.CheckDiskSpaceF(requiredFreeDiskSpace, subtractSccmCache,(requiredFreeDiskSpace2, subtractSccmCache2) => F.LoadDiskSpaceResult(requiredFreeDiskSpace2, subtractSccmCache2), F.ShowDiskSpaceToastNotification).ConfigureAwait(false);
        }
    }

    public class DiskSpaceInfo
    {
        public decimal SccmCacheSize { get; set; }
        public decimal TotalFreeDiskSpace { get; set; }
    }
}
