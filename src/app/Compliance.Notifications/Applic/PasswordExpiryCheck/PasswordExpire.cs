using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.ToastTemplates;
using Compliance.Notifications.Resources;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;

namespace Compliance.Notifications.Applic.PasswordExpiryCheck
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
            else
            {
                MessageBox.Show(strings.PressCtrlAltDeleteToGetToTheWindowsSecurityDialog,
                    strings.ChangePasswordMessageBoxCaption, MessageBoxButton.OK, MessageBoxImage.Information,
                    MessageBoxResult.OK, MessageBoxOptions.None);
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
            var imageUri = new Uri($"https://picsum.photos/364/202?image={F.Rnd.Next(1, 900)}");
            var appLogoImageUri = new Uri("https://unsplash.it/64?image=1005");
            var content = string.Format(CultureInfo.InvariantCulture, strings.PasswordExpiryNotification_Content_F0_F1, passwordExpirationDate.InPeriodFromNow(), passwordExpirationDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            var content2 = strings.PasswordExpiryNotification_Content2;
            var action = ToastActions.ChangePassword;
            var actionActivationType = ToastActivationType.Foreground;
            var greeting = F.GetGreeting(notificationProfile);
            return new ActionDismissToastContentInfo(greeting, title, content, content2, appLogoImageUri, action, actionActivationType, strings.PasswordExpiryNotification_ActionButtonContent, strings.NotNowActionButtonContent, ToastActions.Dismiss, groupName, Option<string>.None, notificationProfile.Value.CompanyName);
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
