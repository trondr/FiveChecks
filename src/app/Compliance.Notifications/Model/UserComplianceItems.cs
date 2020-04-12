using System.Collections.Generic;
using Compliance.Notifications.Common;
using Compliance.Notifications.Model.PasswordExpiry;

namespace Compliance.Notifications.Model
{
    public static class UserComplianceItems
    {
        private static readonly MeasureCompliance PasswordExpiryMeasurement = async () =>
            await F.RunUserComplianceItem<PasswordExpiryInfo>(F.GetPasswordExpiryInfo).ConfigureAwait(false);

        /// <summary>
        /// List of all system compliance items.
        /// </summary>
        public static List<MeasureCompliance> Measurements { get; } = new List<MeasureCompliance> { PasswordExpiryMeasurement };
    }
}