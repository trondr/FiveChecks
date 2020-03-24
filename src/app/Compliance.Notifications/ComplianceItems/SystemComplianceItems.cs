using System;
using System.Threading.Tasks;
using Compliance.Notifications.Common;
using LanguageExt;
using LanguageExt.Common;
using System.Collections.Generic;
using Compliance.Notifications.ComplianceItems.SystemDiskSpace;

namespace Compliance.Notifications.ComplianceItems
{
    public delegate Task<Result<Unit>> MeasureCompliance();
    
    public static class SystemComplianceItems
    {
        private static readonly MeasureCompliance DiskSpaceMeasurement = async () => 
            await F.RunSystemComplianceItem<DiskSpaceInfo>(F.GetDiskSpaceInfo).ConfigureAwait(false);

        /// <summary>
        /// List of all system compliance items.
        /// </summary>
        public static List<MeasureCompliance> Measurements { get; } = new List<MeasureCompliance> {DiskSpaceMeasurement};
    }

    public static class UserComplianceItems
    {
    }
}
