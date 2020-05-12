using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.ToastTemplates;
using Compliance.Notifications.Resources;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Linq;


namespace Compliance.Notifications.Applic.MissingMsUpdatesCheck
{
    public static class MissingMsUpdates
    {
        public static async Task<Result<MissingMsUpdatesInfo>> GetMissingMsUpdatesInfo()
        {
            Logging.DefaultLogger.Warn("TODO: Implement missing MS updates calculation.");

            var march = new DateTime(2020,03,20);
            var firstMeasuredMissing = new DateTime(2020, 03, 29);
            return await Task.FromResult(new Result<MissingMsUpdatesInfo>(new MissingMsUpdatesInfo {Updates = new List<MsUpdate>
            {
                new MsUpdate {ArticleId = "4538156",Name = "2020-02 Cumulative Update for .NET Framework 3.5, 4.7.2 and 4.8 for Windows 10 Version 1809 for x64 (KB4538156)", Deadline = march,FirstMeasuredMissing = firstMeasuredMissing},
                new MsUpdate {ArticleId = "4494174",Name = "2020-01 Update for Windows 10 Version 1809 for x64-based Systems (KB4494174)",Deadline = march,FirstMeasuredMissing = firstMeasuredMissing},
                new MsUpdate {ArticleId = "4549947",Name = "2020-04 Servicing Stack Update for Windows 10 Version 1809 for x64-based Systems (KB4549947)",Deadline = march,FirstMeasuredMissing = firstMeasuredMissing},
                new MsUpdate {ArticleId = "4549949",Name = "2020-04 Cumulative Update for Windows 10 Version 1809 for x64-based Systems (KB4549949)",Deadline = march,FirstMeasuredMissing = firstMeasuredMissing},
                new MsUpdate {ArticleId = "3104046",Name = "Office 365 Client Update - Semi-annual Channel Version 1908 for x64 based Edition (Build 11929.20708)",Deadline = march,FirstMeasuredMissing = firstMeasuredMissing}
            }
            })).ConfigureAwait(false);
        }

        public static async Task<MissingMsUpdatesInfo> LoadMissingMsUpdatesInfo()
        {
            return await ComplianceInfo.LoadSystemComplianceItemResultOrDefault(MissingMsUpdatesInfo.Default).ConfigureAwait(false);
        }

        public static async Task<Result<ToastNotificationVisibility>> ShowMissingUpdatesToastNotification(Some<NotificationProfile> userProfile, string tag, string groupName, MissingMsUpdatesInfo info)
        {
            return await ToastHelper.ShowToastNotification(async () =>
            {
                var toastContentInfo = GetCheckMissingMsUpdatesToastContentInfo(userProfile, groupName, info);
                var toastContent = await ActionDismissToastContent.CreateToastContent(toastContentInfo).ConfigureAwait(true);
                return toastContent;
            }, tag, groupName).ConfigureAwait(false);
        }

        private static ActionDismissToastContentInfo GetCheckMissingMsUpdatesToastContentInfo(Some<NotificationProfile> notificationProfile, string groupName, MissingMsUpdatesInfo info)
        {
            var title = strings.MissingMsUpdatesTitle;
            var imageUri = new Uri($"https://picsum.photos/364/202?image={F.Rnd.Next(1, 900)}");
            var appLogoImageUri = new Uri("https://unsplash.it/64?image=1005");
            var content = string.Format(CultureInfo.InvariantCulture, strings.MissingUpdatesContent_F0, info.Updates.Count);
            var missingUpdateKbs = string.Join(Environment.NewLine, info.Updates.Select(u => "\tKB" + u.ArticleId).ToArray()) + Environment.NewLine;
            var content2 = string.Format(CultureInfo.InvariantCulture, strings.MissingUpdatesContent2_F0, Environment.NewLine + missingUpdateKbs);
            var action = ToastActions.TroubleShootWindowsUpdate;
            var actionActivationType = ToastActivationType.Foreground;
            var greeting = F.GetGreeting(notificationProfile);
            return new ActionDismissToastContentInfo(greeting, title, content, content2,
                imageUri, appLogoImageUri, action, actionActivationType, strings.MissingMsUpdates_Troubleshooting_ButtonContent, strings.NotNowActionButtonContent, ToastActions.Dismiss, groupName, Option<string>.None, notificationProfile.Value.CompanyName);
        }
    }
}
