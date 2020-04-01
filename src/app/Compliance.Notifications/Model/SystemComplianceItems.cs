using System.Collections.Generic;
using System.Threading.Tasks;
using Compliance.Notifications.Common;
using LanguageExt;
using LanguageExt.Common;

namespace Compliance.Notifications.Model
{
    public delegate Task<Result<Unit>> MeasureCompliance();
    
    public static class SystemComplianceItems
    {
        private static readonly MeasureCompliance DiskSpaceMeasurement = async () => 
            await F.RunSystemComplianceItem(F.GetDiskSpaceInfo).ConfigureAwait(false);

        private static readonly MeasureCompliance PendingRebootMeasurement = async () =>
            await F.RunSystemComplianceItem(F.GetPendingRebootInfo).ConfigureAwait(false);

        /// <summary>
        /// List of all system compliance items.
        /// </summary>
        public static List<MeasureCompliance> Measurements { get; } = new List<MeasureCompliance> {DiskSpaceMeasurement, PendingRebootMeasurement };
    }

    public static class UserComplianceItems
    {
    }
}
