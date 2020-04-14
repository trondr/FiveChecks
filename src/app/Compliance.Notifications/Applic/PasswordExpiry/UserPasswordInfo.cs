using System;
using LanguageExt;

namespace Compliance.Notifications.Applic.PasswordExpiry
{
    public class UserPasswordInfo: Record<UserPasswordInfo>
    {
        public UserPasswordInfo(Some<string> userId, Some<DateTime> passwordExpirationDate)
        {
            UserId = userId;
            PasswordExpirationDate = passwordExpirationDate;
        }

        public string UserId { get; }
        public DateTime PasswordExpirationDate { get; }
    }
}