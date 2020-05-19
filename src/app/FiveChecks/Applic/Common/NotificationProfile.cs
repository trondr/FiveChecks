using System;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using System.Threading.Tasks;
using LanguageExt;

namespace FiveChecks.Applic.Common
{
    public class NotificationProfile
    {
        public NotificationProfile(string givenName, string companyName)
        {
            GivenName = givenName;
            CompanyName = companyName;
        }
        public string GivenName { get; }
        public string CompanyName { get; }
    }

    public static class UserProfileOperations
    {
        public static async Task<NotificationProfile> LoadAndSetUserProfile()
        {
            var givenName = 
                (await GetGivenName().ConfigureAwait(false))
                .Match(gName => gName, () => Profile.GetStringProfileValue(Context.User, Option<string>.None, "GiveName", Environment.UserName));
            var companyName = Profile.GetCompanyName();
            var userProfile = new NotificationProfile(givenName, companyName);
            return
                Profile.SetStringProfileValue(Context.User, Option<string>.None, "GiveName", givenName)
                    .Match(unit => userProfile,exception =>
                    {
                        Logging.DefaultLogger.Warn(exception.ToExceptionMessage());
                        return userProfile;
                    });
        }
        
        public static Try<Option<string>> TryGetGivenName() => () =>
        {
            var currentWindowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            using (var pc = new PrincipalContext(ContextType.Domain))
            {
                var up = UserPrincipal.FindByIdentity(pc, currentWindowsPrincipal.Identity.Name);
                return up != null ? up.GivenName : Option<string>.None;
            }
        };

        public static async Task<Option<string>> GetGivenName()
        {
            var result = await Task.Run(() => TryGetGivenName().Try()).ConfigureAwait(false);
            return result.Match(option => option, exception =>
            {
                Logging.DefaultLogger.Warn($"Failed to get user given name from domain. {exception.ToExceptionMessage()}");
                return Option<string>.None;
            });
        }
    }
}
