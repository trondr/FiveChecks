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
            Func<Task<Result<ToastNotificationVisibility>>> removeToastNotification)
        {
            var info = await loadInfo().ConfigureAwait(false);
            if (IsNonCompliant(info))
            {
                return await showToastNotification(info).ConfigureAwait(false);
            }
            var result = await removeToastNotification().ConfigureAwait(false);
            return result;
        }

        internal static bool IsNonCompliant(DesktopDataInfo desktopDataInfo)
        {
            return desktopDataInfo.HasDesktopData;
        }
        
        public static async Task<Result<ToastNotificationVisibility>> CheckDesktopData(Some<NotificationProfile> userProfile)
        {
            var groupName = ToastGroups.CheckDesktopData;
            var tag = ToastGroups.CheckDesktopData;
            return await CheckDesktopDataPure(
                () => F.LoadInfo(DesktopData.LoadDesktopDataInfo,info => info.HasDesktopData,ScheduledTasks.ComplianceUserMeasurements,true), 
                (desktopDataInfo) => DesktopData.ShowDesktopDataToastNotification(userProfile.Value, desktopDataInfo, tag, groupName),
                () => ToastHelper.RemoveToastNotification(groupName)
                ).ConfigureAwait(false);
        }

        public static bool IsDisabled(bool defaultValue)
        {
            var policyCategory = typeof(CheckDesktopDataCommand).GetPolicyCategory();
            return F.PolicyCategoryIsDisabled(policyCategory, defaultValue);
        }
    }
}