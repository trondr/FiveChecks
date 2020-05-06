using System.Collections.Generic;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.DesktopDataCheck;
using Compliance.Notifications.Applic.PasswordExpiryCheck;

namespace Compliance.Notifications.Applic
{
    public static class UserComplianceItems
    {
        private static readonly MeasureCompliance PasswordExpiryMeasurement = async () =>
            await ComplianceInfo.RunUserComplianceItem<PasswordExpiryInfo>(PasswordExpire.GetPasswordExpiryInfo).ConfigureAwait(false);

        private static readonly MeasureCompliance DesktopDataMeasurement = async () =>
            await ComplianceInfo.RunUserComplianceItem<DesktopDataInfo>(DesktopData.GetDesktopDataInfo).ConfigureAwait(false);

        /// <summary>
        /// List of all system compliance items.
        /// </summary>
        public static List<MeasureCompliance> Measurements { get; } = new List<MeasureCompliance> { PasswordExpiryMeasurement, DesktopDataMeasurement };
    }
}