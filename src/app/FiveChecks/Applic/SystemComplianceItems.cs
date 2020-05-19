using System.Collections.Generic;
using System.Threading.Tasks;
using FiveChecks.Applic.Common;
using FiveChecks.Applic.DiskSpaceCheck;
using FiveChecks.Applic.MissingMsUpdatesCheck;
using FiveChecks.Applic.PendingRebootCheck;
using FiveChecks.Applic.SystemUptimeCheck;
using LanguageExt;
using LanguageExt.Common;

namespace FiveChecks.Applic
{
    public delegate Task<Result<Unit>> MeasureCompliance();
    
    public static class SystemComplianceItems
    {
        private static readonly MeasureCompliance DiskSpaceMeasurement = async () => 
            await ComplianceInfo.RunSystemComplianceItem(DiskSpace.GetDiskSpaceInfo).ConfigureAwait(false);

        private static readonly MeasureCompliance PendingRebootMeasurement = async () =>
            await ComplianceInfo.RunSystemComplianceItem(PendingReboot.GetPendingRebootInfo).ConfigureAwait(false);

        private static readonly MeasureCompliance SystemUptimeMeasurement = async () =>
            await ComplianceInfo.RunSystemComplianceItem(SystemUptime.GetSystemUptimeInfo).ConfigureAwait(false);

        private static readonly MeasureCompliance MissingMsUpdatesMeasurement = async () =>
            await ComplianceInfo.RunSystemComplianceItem(MissingMsUpdates.GetMissingMsUpdatesInfo).ConfigureAwait(false);

        /// <summary>
        /// List of all system compliance items.
        /// </summary>
        public static List<MeasureCompliance> Measurements { get; } = new List<MeasureCompliance> {DiskSpaceMeasurement, PendingRebootMeasurement, SystemUptimeMeasurement, MissingMsUpdatesMeasurement };
    }
}
