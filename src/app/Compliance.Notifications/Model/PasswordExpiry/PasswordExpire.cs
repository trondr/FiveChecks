using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using Compliance.Notifications.Common;
using Compliance.Notifications.Resources;
using LanguageExt;
using Microsoft.Win32;

namespace Compliance.Notifications.Model.PasswordExpiry
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

        private static string GetDomainControllerName()
        {
            using (var domain = Domain.GetCurrentDomain())
            {
                using (var domainController = domain.FindDomainController())
                {
                    return domainController.Name;
                }
            }
        }

        public static DateTime GetPasswordExpirationDate(string userId)
        {
            var passwordNeverExpires = GetPasswordNeverExpires(userId);
            if (passwordNeverExpires) return DateTime.MaxValue;
            var domainControllerName = GetDomainControllerName();
            using (var userDirectoryEntry = new DirectoryEntry($"WinNT://{domainControllerName}/{userId},user"))
            {
                var nativeUser = userDirectoryEntry.NativeObject as ActiveDs.IADsUser;
                return nativeUser?.PasswordExpirationDate ?? DateTime.MaxValue;
            }
        }

        public static double GetExpiryWarningDays()
        {
            var passwordExpiryWarningDays = RegistryOperations.GetRegistryValue(Registry.LocalMachine,
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "PasswordExpiryWarning", 0d);
            return passwordExpiryWarningDays.Match(o => Convert.ToDouble(o), () => 0d);
        }

        public static UserPasswordInfo GetUserPasswordInfo(Some<string> userId)
        {
            var passwordExpirationDate = GetPasswordExpirationDate(userId);
            var userPasswordInfo = new UserPasswordInfo(userId, passwordExpirationDate);
            return userPasswordInfo;
        }

        public static async Task<UserPasswordExpiryStatusInfo> GetPasswordExpiryStatusF(Some<string> userId, Func<string,UserPasswordInfo> getUserPasswordInfo, Func<bool> getIsRemoteSession,Func<DateTime> getNow, Func<double> getExpiryWarningDays)
        {
            if (getUserPasswordInfo == null) throw new ArgumentNullException(nameof(getUserPasswordInfo));
            if (getIsRemoteSession == null) throw new ArgumentNullException(nameof(getIsRemoteSession));
            return await Task<UserPasswordExpiryStatusInfo>.Run(() =>
                {
                    var userPasswordInfo = getUserPasswordInfo(userId);
                    if (userPasswordInfo.PasswordExpirationDate == DateTime.MaxValue)
                        return new UserPasswordExpiryStatusInfo(userPasswordInfo, getIsRemoteSession(), PasswordExpiryStatus.NotExpiring);

                    var passwordExpiryTimeSpan = userPasswordInfo.PasswordExpirationDate - getNow();
                    if (passwordExpiryTimeSpan.TotalDays < 0)
                        return new UserPasswordExpiryStatusInfo(userPasswordInfo, getIsRemoteSession(), PasswordExpiryStatus.HasExpired);

                    if (getExpiryWarningDays() > passwordExpiryTimeSpan.TotalDays)
                        return new UserPasswordExpiryStatusInfo(userPasswordInfo, getIsRemoteSession(), PasswordExpiryStatus.ExpiringSoon);

                    return new UserPasswordExpiryStatusInfo(userPasswordInfo, getIsRemoteSession(), PasswordExpiryStatus.NotExpiring);
                }).ConfigureAwait(false);
        }

        public static async Task<UserPasswordExpiryStatusInfo> GetPasswordExpiryStatus(Some<string> userId)
        {
            return await GetPasswordExpiryStatusF(userId, _ => GetUserPasswordInfo(userId), () => GetIsRemoteSession(),
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
    }
}
