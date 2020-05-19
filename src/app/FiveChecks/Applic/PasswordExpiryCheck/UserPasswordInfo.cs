using System;
using LanguageExt;

namespace FiveChecks.Applic.PasswordExpiryCheck
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