using LanguageExt;

namespace Compliance.Notifications.Module.PasswordExpiry
{
    public class UserPasswordExpiryStatusInfo: Record<UserPasswordExpiryStatusInfo>
    {
        public UserPasswordExpiryStatusInfo(Some<UserPasswordInfo> userPasswordInfo, bool isRemoteSession, PasswordExpiryStatus passwordExpiryStatus)
        {
            UserPasswordInfo = userPasswordInfo;
            IsRemoteSession = isRemoteSession;
            PasswordExpiryStatus = passwordExpiryStatus;
        }
        public UserPasswordInfo UserPasswordInfo { get; }
        public bool IsRemoteSession { get; }
        public PasswordExpiryStatus PasswordExpiryStatus { get; }
    }
}