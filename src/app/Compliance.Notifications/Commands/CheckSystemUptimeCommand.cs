using System;
using System.Threading.Tasks;
using Compliance.Notifications.Common;
using Compliance.Notifications.Module;
using LanguageExt.Common;

namespace Compliance.Notifications.Commands
{
    public static class CheckSystemUptimeCommand
    {
        internal static async Task<Result<ToastNotificationVisibility>> CheckSystemUptimePure(double maxUpTimeHours,
            Func<Task<SystemUptimeInfo>> loadInfo,
            Func<TimeSpan,string, Task<Result<ToastNotificationVisibility>>> showToastNotification,
            Func<Task<Result<ToastNotificationVisibility>>> removeToastNotification)
        {
            var info = await loadInfo().ConfigureAwait(false);
            if (info.Uptime.TotalHours > maxUpTimeHours)
            {
                return await showToastNotification(info.Uptime,"My Company AS").ConfigureAwait(false);
            }
            var result = await removeToastNotification().ConfigureAwait(false);
            return result;
        }

        public static async Task<Result<ToastNotificationVisibility>> CheckSystemUptime(double maxUpTimeDays)
        {
            var groupName = ToastGroups.CheckSystemUptime;
            var tag = ToastGroups.CheckSystemUptime;
            return await CheckSystemUptimePure(maxUpTimeDays, F.LoadSystemUptimeInfo, (uptime,companyName) => F.ShowSystemUptimeToastNotification(companyName, tag, groupName, uptime), () => ToastHelper.RemoveToastNotification(groupName)).ConfigureAwait(false);
        }
    }
}