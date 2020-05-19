using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FiveChecks.Applic.Common;
using FiveChecks.Applic.ToastTemplates;
using FiveChecks.Resources;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;

namespace FiveChecks.Applic.PendingRebootCheck
{
    public static class PendingReboot
    {
        public static async Task<Result<PendingRebootInfo>> GetPendingRebootInfo()
        {
            var pendingRebootInfoResults = new List<Result<PendingRebootInfo>>
            {
                await GetCbsRebootPending().ConfigureAwait(false),
                await GetWuauRebootPending().ConfigureAwait(false),
                await GetPendingFileRenameRebootPending().ConfigureAwait(false),
                await GetSccmClientRebootPending().ConfigureAwait(false)
            };
            var success = pendingRebootInfoResults.ToSuccess();
            var pendingRebootInfo = success.Aggregate(PendingRebootInfoExtensions.Update);
            return new Result<PendingRebootInfo>(pendingRebootInfo);
        }

        private static async Task<Result<PendingRebootInfo>> GetWuauRebootPending()
        {
            var rebootPendingRegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired";
            Logging.DefaultLogger.Debug($@"Checking if Windows Update has a pending reboot (Check if key exists: '{rebootPendingRegistryKeyPath}').");
            var rebootIsPending = RegistryOperations.RegistryKeyExists(Registry.LocalMachine, rebootPendingRegistryKeyPath);
            var rebootSource = rebootIsPending ? new List<RebootSource> { RebootSource.Wuau } : new List<RebootSource>();
            var pendingRebootInfo = new PendingRebootInfo { RebootIsPending = rebootIsPending, Sources = rebootSource };
            Logging.DefaultLogger.Info($@"Windows Update pending reboot check result: {pendingRebootInfo.ObjectToString()}");
            return await Task.FromResult(new Result<PendingRebootInfo>(pendingRebootInfo)).ConfigureAwait(false);
        }

        private static async Task<Result<PendingRebootInfo>> GetCbsRebootPending()
        {
            var rebootPendingRegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending";
            Logging.DefaultLogger.Debug($@"Checking if Component Based Servicing has a pending reboot (Check if key exists: '{rebootPendingRegistryKeyPath}').");
            var rebootIsPending = RegistryOperations.RegistryKeyExists(Registry.LocalMachine, rebootPendingRegistryKeyPath);
            var rebootSource = rebootIsPending ? new List<RebootSource> { RebootSource.Cbs } : new List<RebootSource>();
            var pendingRebootInfo = new PendingRebootInfo { RebootIsPending = rebootIsPending, Sources = rebootSource };
            Logging.DefaultLogger.Info($@"Component Based Servicing (CBS) pending reboot check result: {pendingRebootInfo.ObjectToString()}");
            return await Task.FromResult(new Result<PendingRebootInfo>(pendingRebootInfo)).ConfigureAwait(false);
        }

        private static Try<PendingRebootInfo> TryGetSccmClientRebootStatus() => () =>
        {
            dynamic rebootStatus =
                F.RunPowerShell(new Some<Func<PowerShell, Collection<PSObject>>>(powerShell =>
                    powerShell
                        .AddCommand("Invoke-WmiMethod")
                        .AddParameter("NameSpace", @"root\ccm\ClientSDK")
                        .AddParameter("Class", "CCM_ClientUtilities")
                        .AddParameter("Name", "DetermineIfRebootPending")
                        .Invoke())
                ).FirstOrDefault();
            var rebootIsPending = rebootStatus?.RebootPending || rebootStatus?.IsHardRebootPending;
            var rebootSource = rebootIsPending ? new List<RebootSource> { RebootSource.SccmClient } : new List<RebootSource>();
            var pendingRebootInfo = new PendingRebootInfo { RebootIsPending = rebootIsPending, Sources = rebootSource };
            Logging.DefaultLogger.Info($@"Sccm Client pending reboot check result: {pendingRebootInfo.ObjectToString()}");
            return new Result<PendingRebootInfo>(pendingRebootInfo);
        };

        private static async Task<Result<PendingRebootInfo>> GetSccmClientRebootPending()
        {
            Logging.DefaultLogger.Debug($@"Checking if Sccm Client has a pending reboot.");
            var pendingRebootInfoResult =
                TryGetSccmClientRebootStatus()
                    .Try()
                    .Match(
                        info => new Result<PendingRebootInfo>(info),
                        exception =>
                        {
                            Logging.DefaultLogger.Warn($"Getting reboot status from Sccm Client did not succeed. {exception.ToExceptionMessage()}");
                            return new Result<PendingRebootInfo>(exception);
                        }
                        );
            return await Task.FromResult(pendingRebootInfoResult).ConfigureAwait(false);
        }

        private static async Task<Result<PendingRebootInfo>> GetPendingFileRenameRebootPending()
        {
            Logging.DefaultLogger.Debug($@"Checking if Pending File Rename Operations has a pending reboot.");
            var category = Profile.GetPolicyCategory(typeof(CheckPendingRebootCommand));
            var pendingFileRenameOperationsSubCategory = category + "\\PendingFileRenameOperations";
            var pendingFileRenameOperationsExcludePatternsSubCategory = pendingFileRenameOperationsSubCategory + "\\PendingFileRenameOperations\\ExcludePatterns";

            var excludeRenameTargets = Profile.GetBooleanPolicyValue(Context.Machine, pendingFileRenameOperationsSubCategory, "ExcludeRenameTargets", false);
            var excludeDeleteTargets = Profile.GetBooleanPolicyValue(Context.Machine, pendingFileRenameOperationsSubCategory, "ExcludeDeleteTargets", false);
            var excludePatterns = Profile.GetBooleanPolicyValue(Context.Machine, pendingFileRenameOperationsSubCategory, "ExcludePatterns", false);
            var excludePatternsArray =
                excludePatterns ? 
                    Profile.GetPolicyStringValueNames(Context.Machine, pendingFileRenameOperationsExcludePatternsSubCategory).ToRegExPatterns().ToArray() : 
                    Array.Empty<Regex>();
            var pendingFileRenameOperations = 
                    PendingFileRenameOperationExtensions.GetPendingFileRenameOperations()
                    .Exclude(excludeRenameTargets, excludeDeleteTargets, excludePatternsArray)
                    .Select(operation => operation.ToDto())
                    .ToArray();
            Logging.DefaultLogger.Debug($"#Pending File Rename Operations: {pendingFileRenameOperations.Length}");
            var rebootIsPending = pendingFileRenameOperations.Length > 0;
            var rebootSource = rebootIsPending ? new List<RebootSource> { RebootSource.PendingFileRenameOperations } : new List<RebootSource>();
            var pendingRebootInfo = new PendingRebootInfo { RebootIsPending = rebootIsPending, Sources = rebootSource };
            pendingRebootInfo.PendingFileRenameOperations.AddRange(pendingFileRenameOperations);
            Logging.DefaultLogger.Info($@"Pending file rename operation pending reboot check result: {pendingRebootInfo.ObjectToString()}");
            return await Task.FromResult(new Result<PendingRebootInfo>(pendingRebootInfo)).ConfigureAwait(false);
        }


        public static async Task<Result<ToastNotificationVisibility>> ShowPendingRebootToastNotification(Some<NotificationProfile> notificationProfile, PendingRebootInfo info, string tag, string groupName)
        {
            return await ToastHelper.ShowToastNotification(async () =>
            {
                var toastContentInfo = GetCheckPendingRebootToastContentInfo(notificationProfile, info, groupName);
                var toastContent = await ActionDismissToastContent.CreateToastContent(toastContentInfo).ConfigureAwait(true);
                return toastContent;
            }, tag, groupName).ConfigureAwait(false);
        }

        private static ActionDismissToastContentInfo GetCheckPendingRebootToastContentInfo(Some<NotificationProfile> notificationProfile, PendingRebootInfo info, string groupName)
        {
            var title = strings.PendingRebootNotification_Title;
            var content = strings.PendingRebootNotification_Content1;
            var content2 = strings.PendingRebootNotification_Content2;
            var action = ToastActions.Restart;
            var actionActivationType = ToastActivationType.Foreground;
            var greeting = F.GetGreeting(notificationProfile);
            Option<string> content3 = string.Format(CultureInfo.InvariantCulture,strings.PendingRebootNotification_Source_F0, info.ToSourceDescription());
            return new ActionDismissToastContentInfo(greeting, title, content, content2, action, actionActivationType, strings.PendingRebootNotification_ActionButtonContent, strings.NotNowActionButtonContent, ToastActions.Dismiss, groupName, content3, notificationProfile.Value.CompanyName);
        }

        public static async Task<PendingRebootInfo> LoadPendingRebootInfo()
        {
            return await ComplianceInfo.LoadSystemComplianceItemResultOrDefault(PendingRebootInfo.Default).ConfigureAwait(false);
        }
    }
}
