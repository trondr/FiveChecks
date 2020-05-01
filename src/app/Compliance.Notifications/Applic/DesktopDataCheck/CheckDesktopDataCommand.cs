using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic.DesktopDataCheck
{
    public static class CheckDesktopDataCommand
    {
        internal static async Task<Result<ToastNotificationVisibility>> CheckDesktopDataPure(
            Func<Task<DesktopDataInfo>> loadInfo,
            Func<DesktopDataInfo, string, Task<Result<ToastNotificationVisibility>>> showToastNotification,
            Func<Task<Result<ToastNotificationVisibility>>> removeToastNotification)
        {
            var info = await loadInfo().ConfigureAwait(false);
            if (IsNonCompliant(info))
            {
                return await showToastNotification(info, "My Company AS").ConfigureAwait(false);
            }
            var result = await removeToastNotification().ConfigureAwait(false);
            return result;
        }

        internal static bool IsNonCompliant(DesktopDataInfo desktopDataInfo)
        {
            return desktopDataInfo.HasDesktopData;
        }
        
        public static async Task<Result<ToastNotificationVisibility>> CheckDesktopData()
        {
            var groupName = ToastGroups.CheckDesktopData;
            var tag = ToastGroups.CheckDesktopData;
            return await CheckDesktopDataPure(
                () => F.LoadInfo(DesktopData.LoadDesktopDataInfo,info => info.HasDesktopData,ScheduledTasks.ComplianceUserMeasurements,true), 
                (desktopDataInfo, companyName) => DesktopData.ShowDesktopDataToastNotification(desktopDataInfo, tag, groupName),
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