using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic.SystemUptimeCheck
{
    public static class CheckSystemUptimeCommand
    {
        internal static async Task<Result<ToastNotificationVisibility>> CheckSystemUptimePure(
            Func<Task<SystemUptimeInfo>> loadInfo,
            Func<SystemUptimeInfo,bool> isNonCompliant,
            Func<TimeSpan,string, Task<Result<ToastNotificationVisibility>>> showToastNotification,
            Func<Task<Result<ToastNotificationVisibility>>> removeToastNotification)
        {
            var info = await loadInfo().ConfigureAwait(false);
            if (isNonCompliant(info))
            {
                return await showToastNotification(info.Uptime,"My Company AS").ConfigureAwait(false);
            }
            var result = await removeToastNotification().ConfigureAwait(false);
            return result;
        }

        public static async Task<Result<ToastNotificationVisibility>> CheckSystemUptime(double maxUpTimeHours)
        {
            var groupName = ToastGroups.CheckSystemUptime;
            var tag = ToastGroups.CheckSystemUptime;
            bool IsNonCompliant(SystemUptimeInfo info) => info.Uptime.TotalHours > maxUpTimeHours;
            return await CheckSystemUptimePure(() => F.LoadInfo<SystemUptimeInfo>(SystemUptime.LoadSystemUptimeInfo, IsNonCompliant, ScheduledTasks.ComplianceSystemMeasurements, true),
                IsNonCompliant,
                (uptime,companyName) => SystemUptime.ShowSystemUptimeToastNotification(companyName, tag, groupName, uptime), 
                () => ToastHelper.RemoveToastNotification(groupName)
                ).ConfigureAwait(false);
        }
    }
}