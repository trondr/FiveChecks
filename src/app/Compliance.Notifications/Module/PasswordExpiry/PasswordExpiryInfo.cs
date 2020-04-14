using System;
using LanguageExt;

namespace Compliance.Notifications.Module.PasswordExpiry
{
    public class PasswordExpiryInfo : Record<PasswordExpiryInfo>
    {
        public static PasswordExpiryInfo Default => new PasswordExpiryInfo { PasswordExpiryStatus = PasswordExpiryStatus.NotExpiring, PasswordExpiryDate = DateTime.MaxValue, IsRemoteSession = false};

        public PasswordExpiryInfo(DateTime passwordExpiryDate, PasswordExpiryStatus passwordExpiryStatus, bool isRemoteSession)
        {
            PasswordExpiryDate = passwordExpiryDate;
            PasswordExpiryStatus = passwordExpiryStatus;
            IsRemoteSession = isRemoteSession;
        }
        private PasswordExpiryInfo(){}

        public DateTime PasswordExpiryDate { get; set; }
        public PasswordExpiryStatus PasswordExpiryStatus { get; set; }
        public bool IsRemoteSession { get; set; }
    }
}