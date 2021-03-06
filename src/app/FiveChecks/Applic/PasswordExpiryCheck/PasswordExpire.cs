﻿using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using FiveChecks.Applic.Common;
using FiveChecks.Applic.ToastTemplates;
using FiveChecks.Resources;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;

namespace FiveChecks.Applic.PasswordExpiryCheck
{
    public static class PasswordExpire
    {
        public static bool GetIsRemoteSession()
        {
            var remoteSession = NativeMethods.GetSystemMetrics(SystemMetric.SmRemoteSession);
            return remoteSession > 0;
        }

        public static bool GetPasswordNeverExpires(string userId)
        {
            using (var context = new PrincipalContext(ContextType.Domain))
            {
                using (var user = UserPrincipal.FindByIdentity(context, userId))
                {
                    return user != null && user.PasswordNeverExpires;
                }
            }
        }

        private static Result<string> GetDomainControllerName()
        {
            try
            {
                using (var domain = Domain.GetCurrentDomain())
                {
                    using (var domainController = domain.FindDomainController())
                    {
                        return domainController.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is ActiveDirectoryObjectNotFoundException || ex is ActiveDirectoryOperationException || ex is ObjectDisposedException)
                {
                    return new Result<string>(new Exception("Failed to locate a domain controller.", ex));
                }
                throw;
            }
        }

        public static DateTime GetPasswordExpirationDate(string userId)
        {
            var passwordNeverExpires = GetPasswordNeverExpires(userId);
            if (passwordNeverExpires) return DateTime.MaxValue;
            var domainControllerName = GetDomainControllerName();
            return domainControllerName.Match(dc =>
            {
                using (var userDirectoryEntry = new DirectoryEntry($"WinNT://{dc}/{userId},user"))
                {
                    var nativeUser = userDirectoryEntry.NativeObject as ActiveDs.IADsUser;
                    return nativeUser?.PasswordExpirationDate ?? DateTime.MaxValue;
                }
            }, exception =>
            {
                Logging.DefaultLogger.Warn(exception.Message);
                return DateTime.MaxValue;
            });
        }

        public static double GetExpiryWarningDays()
        {
            var passwordExpiryWarningDays = RegistryOperations.GetRegistryValue(Registry.LocalMachine,
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "PasswordExpiryWarning", 0d);
            return passwordExpiryWarningDays.Match(o => Convert.ToDouble(o,CultureInfo.InvariantCulture), () => 0d);
        }

        public static UserPasswordInfo GetUserPasswordInfo(Some<string> userId)
        {
            var passwordExpirationDate = GetPasswordExpirationDate(userId);
            var userPasswordInfo = new UserPasswordInfo(userId, passwordExpirationDate);
            return userPasswordInfo;
        }

        public static async Task<UserPasswordExpiryStatusInfo> GetPasswordExpiryStatusPure(Some<string> userId, Func<string,UserPasswordInfo> getUserPasswordInfo, Func<bool> getIsRemoteSession,Func<DateTime> getNow, Func<double> getExpiryWarningDays)
        {
            if (getUserPasswordInfo == null) throw new ArgumentNullException(nameof(getUserPasswordInfo));
            if (getIsRemoteSession == null) throw new ArgumentNullException(nameof(getIsRemoteSession));
            return await Task.Run(() =>
                {
                    var userPasswordInfo = getUserPasswordInfo(userId);
                    if (userPasswordInfo.PasswordExpirationDate == DateTime.MaxValue)
                        return new UserPasswordExpiryStatusInfo(userPasswordInfo, getIsRemoteSession(), PasswordExpiryStatus.NotExpiring);
                    var now = getNow();
                    var passwordExpiryTimeSpan = userPasswordInfo.PasswordExpirationDate - now;
                    if (passwordExpiryTimeSpan.TotalDays < 0)
                        return new UserPasswordExpiryStatusInfo(userPasswordInfo, getIsRemoteSession(), PasswordExpiryStatus.HasExpired);

                    if (getExpiryWarningDays() > passwordExpiryTimeSpan.TotalDays)
                        return new UserPasswordExpiryStatusInfo(userPasswordInfo, getIsRemoteSession(), PasswordExpiryStatus.ExpiringSoon);

                    return new UserPasswordExpiryStatusInfo(userPasswordInfo, getIsRemoteSession(), PasswordExpiryStatus.NotExpiring);
                }).ConfigureAwait(false);
        }

        public static async Task<UserPasswordExpiryStatusInfo> GetPasswordExpiryStatus(Some<string> userId)
        {
            return await GetPasswordExpiryStatusPure(userId, _ => GetUserPasswordInfo(userId), () => GetIsRemoteSession(),
                () => DateTime.Now, () => GetExpiryWarningDays()).ConfigureAwait(false);
        }
        
        public static void ShowWindowsSecurityDialog(bool isRemoteSession)
        {
            if (isRemoteSession)
            {
                var shellAppType = Type.GetTypeFromProgID("Shell.Application");
                var shell = Activator.CreateInstance(shellAppType);
                shellAppType.InvokeMember("WindowsSecurity", System.Reflection.BindingFlags.InvokeMethod, null, shell,
                    null, CultureInfo.InvariantCulture);
            }
        }

        public static async Task<Result<ToastNotificationVisibility>> ShowPasswordExpiryToastNotification(Some<NotificationProfile> userProfile, DateTime passwordExpirationDate, string tag, string groupName)
        {
            return await ToastHelper.ShowToastNotification(async () =>
            {
                var toastContentInfo = GetCheckPasswordExpiryToastContentInfo(userProfile, passwordExpirationDate, groupName);
                var toastContent = await ActionDismissToastContent.CreateToastContent(toastContentInfo).ConfigureAwait(true);
                return toastContent;
            }, tag, groupName).ConfigureAwait(false);
        }

        private static ActionDismissToastContentInfo GetCheckPasswordExpiryToastContentInfo(Some<NotificationProfile> notificationProfile,
            DateTime passwordExpirationDate, string groupName)
        {
            var title = strings.PasswordExpiryNotification_Title;
            var content = string.Format(CultureInfo.InvariantCulture, strings.PasswordExpiryNotification_Content_F0_F1, passwordExpirationDate.InPeriodFromNow(), passwordExpirationDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            var isRemoteSessions = PasswordExpire.GetIsRemoteSession();
            var content2 = isRemoteSessions? strings.PasswordExpiryNotification_Content2: strings.PasswordExpiryNotification_Content2 + Environment.NewLine + Environment.NewLine +  strings.PressCtrlAltDeleteToGetToTheWindowsSecurityDialog;
            var action = isRemoteSessions? ToastActions.ChangePassword: string.Empty;
            var actionActivationType = ToastActivationType.Foreground;
            var greeting = F.GetGreeting(notificationProfile);
            return new ActionDismissToastContentInfo(greeting, title, content, content2, action, actionActivationType, isRemoteSessions ? strings.PasswordExpiryNotification_ActionButtonContent:string.Empty, strings.NotNowActionButtonContent, ToastActions.Dismiss, groupName, Option<string>.None, notificationProfile.Value.CompanyName);
        }

        public static async Task<Result<PasswordExpiryInfo>> GetPasswordExpiryInfo()
        {
            if(!F.IsOnline()) return new Result<PasswordExpiryInfo>(new Exception("Cannot contact domain controller to check password expiry"));
            var userPasswordExpiryInfo = await PasswordExpire.GetPasswordExpiryStatus(Environment.UserName).ConfigureAwait(false);
            var passwordExpiryInfo = new PasswordExpiryInfo(userPasswordExpiryInfo.UserPasswordInfo.PasswordExpirationDate, userPasswordExpiryInfo.PasswordExpiryStatus, userPasswordExpiryInfo.IsRemoteSession);
            return new Result<PasswordExpiryInfo>(passwordExpiryInfo);
        }

        public static async Task<PasswordExpiryInfo> LoadPasswordExpiryInfo()
        {
            return await ComplianceInfo.LoadUserComplianceItemResultOrDefault(PasswordExpiryInfo.Default).ConfigureAwait(false);
        }
    }
}
