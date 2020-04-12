using System;
using System.Threading.Tasks;
using Compliance.Notifications.Common;
using Compliance.Notifications.Model;
using GalaSoft.MvvmLight.Messaging;
using LanguageExt.Common;

namespace Compliance.Notifications.Commands
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
        /// <param name="sendApplicationExitMessage"></param>
        /// <returns></returns>
        internal static async Task<Result<int>> CheckDiskSpacePure(UDecimal requiredFreeDiskSpace,
            bool subtractSccmCache, Func<Task<DiskSpaceInfo>> loadDiskSpaceResult,
            Func<decimal, string, Task<Result<int>>> showDiskSpaceToastNotification,
            Func<Task<Result<int>>> removeDiskSpaceToastNotification, Action sendApplicationExitMessage)
        {
            var diskSpaceInfo = await loadDiskSpaceResult().ConfigureAwait(false);
            var requiredCleanupAmount = requiredFreeDiskSpace - (diskSpaceInfo.TotalFreeDiskSpace + (subtractSccmCache ? diskSpaceInfo.SccmCacheSize : 0));
            var isNotCompliant = requiredCleanupAmount > 0;
            if (isNotCompliant)
            {
                return await showDiskSpaceToastNotification(requiredCleanupAmount, "My Company AS").ConfigureAwait(false);
            }
            var result = await removeDiskSpaceToastNotification().ConfigureAwait(false);
            sendApplicationExitMessage();
            return result;
        }
        
        /// <summary>
        /// Check disk space compliance.
        /// </summary>
        /// <param name="requiredFreeDiskSpace">Required free disk space in GB.</param>
        /// <param name="subtractSccmCache">When set to true, disk space is compliant if: ((CurrentTotalFreeDiskSpace + CurrentSizeOfSccmCache) - requiredFreeDiskSpace) > 0</param>
        /// <returns></returns>
        public static async Task<Result<int>> CheckDiskSpace(UDecimal requiredFreeDiskSpace, bool subtractSccmCache)
        {
            return await CheckDiskSpaceCommand.CheckDiskSpacePure(requiredFreeDiskSpace, subtractSccmCache, F.LoadDiskSpaceResult, (requiredCleanupAmount, companyName) => F.ShowDiskSpaceToastNotification(requiredCleanupAmount, companyName, nameof(CheckDiskSpaceCommand), nameof(CheckDiskSpaceCommand)),() => ToastHelper.RemoveToastNotification(nameof(CheckDiskSpaceCommand)), () => Messenger.Default.Send(new ExitApplicationMessage())).ConfigureAwait(false);
        }
    }
}
