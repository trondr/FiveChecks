using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using LanguageExt;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic.DesktopDataCheck
{
    public static class CheckDesktopDataCommand
    {
        internal static async Task<Result<ToastNotificationVisibility>> CheckDesktopDataPure(
            Func<Task<DesktopDataInfo>> loadInfo,
            Func<DesktopDataInfo, Task<Result<ToastNotificationVisibility>>> showToastNotification,
            Func<Task<Result<ToastNotificationVisibility>>> removeToastNotification, bool isDisabled)
        {
            if(isDisabled) return await removeToastNotification().ConfigureAwait(false);
            var info = await loadInfo().ConfigureAwait(false);
            if (IsNonCompliant(info))
            {
                return await showToastNotification(info).ConfigureAwait(false);
            }
            return await removeToastNotification().ConfigureAwait(false);
        }

        internal static bool IsNonCompliant(DesktopDataInfo desktopDataInfo)
        {
            return desktopDataInfo.HasDesktopData;
        }
        
        public static async Task<Result<ToastNotificationVisibility>> CheckDesktopData(
            Some<NotificationProfile> userProfile, bool isDisabled)
        {
            var groupName = ToastGroups.CheckDesktopData;
            var tag = ToastGroups.CheckDesktopData;
            var desktopDataCheckIsDisabled = Profile.IsCheckDisabled(isDisabled, typeof(CheckDesktopDataCommand));
            return await CheckDesktopDataPure(
                () => ComplianceInfo.LoadInfo(DesktopData.LoadDesktopDataInfo,info => info.HasDesktopData,ScheduledTasks.ComplianceUserMeasurements,true), 
                (desktopDataInfo) => DesktopData.ShowDesktopDataToastNotification(userProfile.Value, desktopDataInfo, tag, groupName),
                () => ToastHelper.RemoveToastNotification(groupName), desktopDataCheckIsDisabled).ConfigureAwait(false);
        }
    }
}