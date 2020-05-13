using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.ToastTemplates;
using Compliance.Notifications.Resources;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Linq;
using System.Management.Automation;


namespace Compliance.Notifications.Applic.MissingMsUpdatesCheck
{
    public static class MissingMsUpdates
    {
        public static async Task<Result<MissingMsUpdatesInfo>> GetMissingMsUpdatesInfo()
        {
            var previousInfo = await LoadMissingMsUpdatesInfo().ConfigureAwait(false);
            var currentInfo = TryGetMissingUpdates().Try().Match(info => info, exception =>
            {
                Logging.DefaultLogger.Warn($"Failed to get missing updates from class CCM_SoftwareUpdate. {exception.ToExceptionMessage()}");
                return MissingMsUpdatesInfo.Default;
            });
            var updatedInfo = previousInfo.Update(currentInfo);
            if (updatedInfo.Updates.Count > 0)
            {
                SccmClient.TriggerSchedule(SccmAction.ForceUpdateScan).Match(unit => { Logging.DefaultLogger.Info("Successfully triggered update scan.");return Unit.Default;},exception => { Logging.DefaultLogger.Warn("Failed to trigger update scan due to: " + exception.ToExceptionMessage());return Unit.Default;});
                SccmClient.TriggerSchedule(SccmAction.SoftwareUpdatesAgentAssignmentEvaluationCycle).Match(unit => { Logging.DefaultLogger.Info("Successfully triggered update evaluation."); return Unit.Default; }, exception => { Logging.DefaultLogger.Warn("Failed to trigger update evaluation due to: " + exception.ToExceptionMessage()); return Unit.Default; });
            }
            return await Task.FromResult(updatedInfo).ConfigureAwait(false);
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
            return new ActionDismissToastContentInfo(greeting, title, content, content2, action, actionActivationType, strings.MissingMsUpdates_Troubleshooting_ButtonContent, strings.NotNowActionButtonContent, ToastActions.Dismiss, groupName, Option<string>.None, notificationProfile.Value.CompanyName);
        }
        
        private static Try<MissingMsUpdatesInfo> TryGetMissingUpdates() => () =>
        {
            var missingUpdates =
                F.RunPowerShell(new Some<Func<PowerShell, Collection<PSObject>>>(powerShell =>
                    powerShell
                        .AddCommand("Get-WmiObject")
                        .AddParameter("NameSpace", @"ROOT\ccm\ClientSDK")
                        .AddParameter("Class", "CCM_SoftwareUpdate")
                        .Invoke())
                );
            var missingUpdatesInfo = new MissingMsUpdatesInfo();
            foreach (var missingUpdate in missingUpdates)
            {
                dynamic mu = missingUpdate;
                missingUpdatesInfo.Updates.Add(new MsUpdate {ArticleId = mu.ArticleID, Name = mu.Name,Deadline = mu.Deadline, FirstMeasuredMissing = DateTime.Now});
            }
            return new Result<MissingMsUpdatesInfo>(missingUpdatesInfo);
        };
    }
}
