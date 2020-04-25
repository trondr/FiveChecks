using System;
using System.Globalization;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.ToastTemplates;
using Compliance.Notifications.Resources;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Compliance.Notifications.Applic.SystemUptimeCheck
{
    public static class SystemUptime
    {
        /// <summary>
        /// Get the system uptime
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetSystemUptime()
        {
            var millisecondsSinceLastRestart = (long)NativeMethods.GetTickCount64();
            var ticksSinceLastRestart = millisecondsSinceLastRestart * TimeSpan.TicksPerMillisecond;
            return new TimeSpan(ticksSinceLastRestart);
        }

        /// <summary>
        /// Get the time of the last restart.
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLastRestartTime()
        {
            return DateTime.Now.Add(-GetSystemUptime());
        }

        public static async Task<Result<SystemUptimeInfo>> GetSystemUptimeInfo()
        {
            SystemUptimeInfo systemUptimeInfo = new SystemUptimeInfo() { Uptime = GetSystemUptime(), LastRestart = GetLastRestartTime() };
            return await Task.FromResult(new Result<SystemUptimeInfo>(systemUptimeInfo)).ConfigureAwait(false);
        }

        public static async Task<Result<ToastNotificationVisibility>> ShowSystemUptimeToastNotification(string companyName, string tag, string groupName, TimeSpan systemUptime)
        {
            return await F.ShowToastNotification(async () =>
            {
                var toastContentInfo = await GetCheckSystemUptimeToastContentInfo(companyName, groupName, systemUptime).ConfigureAwait(false);
                var toastContent = await ActionDismissToastContent.CreateToastContent(toastContentInfo).ConfigureAwait(true);
                return toastContent;
            }, tag, groupName).ConfigureAwait(false);
        }

        private static async Task<ActionDismissToastContentInfo> GetCheckSystemUptimeToastContentInfo(
            string companyName, string groupName, TimeSpan systemUptime)
        {
            var title = strings.SystemUptime_Title;
            var imageUri = new Uri($"https://picsum.photos/364/202?image={F.Rnd.Next(1, 900)}");
            var appLogoImageUri = new Uri("https://unsplash.it/64?image=1005");
            var content = string.Format(CultureInfo.InvariantCulture, strings.SystemUptimeContent_F0, systemUptime.ToReadableString());
            var content2 = strings.SystemUptimeContent2;
            var action = ToastActions.Restart;
            var actionActivationType = ToastActivationType.Foreground;
            var greeting = await F.GetGreeting().ConfigureAwait(false);
            return new ActionDismissToastContentInfo(greeting, title, companyName, content, content2,
                imageUri, appLogoImageUri, action, actionActivationType, strings.SystemUptime_Action_Button_Content, strings.NotNowActionButtonContent, ToastActions.Dismiss, groupName, Option<string>.None);
        }

        public static async Task<SystemUptimeInfo> LoadSystemUptimeInfo()
        {
            return await F.LoadSystemComplianceItemResultOrDefault(SystemUptimeInfo.Default).ConfigureAwait(false);
        }
        
    }
}
