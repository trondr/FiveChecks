using System.Collections.Generic;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.DiskspaceCheck;
using Compliance.Notifications.Applic.PendingRebootCheck;
using Compliance.Notifications.Applic.SystemUptimeCheck;
using LanguageExt;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic
{
    public delegate Task<Result<Unit>> MeasureCompliance();
    
    public static class SystemComplianceItems
    {
        private static readonly MeasureCompliance DiskSpaceMeasurement = async () => 
            await F.RunSystemComplianceItem(DiskSpace.GetDiskSpaceInfo).ConfigureAwait(false);

        private static readonly MeasureCompliance PendingRebootMeasurement = async () =>
            await F.RunSystemComplianceItem(PendingReboot.GetPendingRebootInfo).ConfigureAwait(false);

        private static readonly MeasureCompliance SystemUptimeMeasurement = async () =>
            await F.RunSystemComplianceItem(SystemUptime.GetSystemUptimeInfo).ConfigureAwait(false);

        /// <summary>
        /// List of all system compliance items.
        /// </summary>
        public static List<MeasureCompliance> Measurements { get; } = new List<MeasureCompliance> {DiskSpaceMeasurement, PendingRebootMeasurement, SystemUptimeMeasurement };
    }
}
