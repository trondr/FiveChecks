using System.Collections.Generic;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.PasswordExpiry;

namespace Compliance.Notifications.Applic
{
    public static class UserComplianceItems
    {
        private static readonly MeasureCompliance PasswordExpiryMeasurement = async () =>
            await F.RunUserComplianceItem<PasswordExpiryInfo>(PasswordExpire.GetPasswordExpiryInfo).ConfigureAwait(false);

        /// <summary>
        /// List of all system compliance items.
        /// </summary>
        public static List<MeasureCompliance> Measurements { get; } = new List<MeasureCompliance> { PasswordExpiryMeasurement };
    }
}