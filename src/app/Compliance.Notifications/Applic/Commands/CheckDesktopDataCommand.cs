using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic.Commands
{
    public static class CheckDesktopDataCommand
    {
        internal static async Task<Result<ToastNotificationVisibility>> CheckDesktopDataPure(
            Func<Task<DesktopDataInfo>> loadInfo,
            Func<DesktopDataInfo, string, Task<Result<ToastNotificationVisibility>>> showToastNotification,
            Func<Task<Result<ToastNotificationVisibility>>> removeToastNotification)
        {
            var info = await loadInfo().ConfigureAwait(false);
            if (info.HasDesktopData)
            {
                return await showToastNotification(info, "My Company AS").ConfigureAwait(false);
            }
            var result = await removeToastNotification().ConfigureAwait(false);
            return result;
        }


        public static async Task<Result<ToastNotificationVisibility>> CheckDesktopData()
        {
            var groupName = ToastGroups.CheckDesktopData;
            var tag = ToastGroups.CheckDesktopData;
            return await CheckDesktopDataPure(DesktopData.LoadDesktopDataInfo, (desktopDataInfo, companyName) => DesktopData.ShowDesktopDataToastNotification(desktopDataInfo, companyName, tag, groupName),() => ToastHelper.RemoveToastNotification(groupName)).ConfigureAwait(false);
        }
    }
}