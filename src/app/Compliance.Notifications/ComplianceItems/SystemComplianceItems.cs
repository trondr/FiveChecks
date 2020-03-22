using System;
using System.Threading.Tasks;
using Compliance.Notifications.Common;
using LanguageExt;
using LanguageExt.Common;
using System.Linq;
using System.Collections.Generic;
using Compliance.Notifications.Commands.CheckDiskSpace;

namespace Compliance.Notifications.ComplianceItems
{
    public delegate Task<Result<Unit>> MeasureCompliance();

    public static class SystemComplianceItems
    {
        public static MeasureCompliance DiskSpaceMeasurement = async () =>
        {
            Logging.DefaultLogger.Warn("DiskSpaceMeasurement: NOT IMPLEMENTED");
            var diskSpaceInfo = await F.GetDiskSpaceInfo();
            var res = diskSpaceInfo.Match(dsi => F.SaveSystemComplianceItemResult<DiskSpaceInfo>(dsi), exception => Task.FromResult(new Result<Unit>(exception)));
            return await res;
        };

        public static List<MeasureCompliance> Measurements = new List<MeasureCompliance> { DiskSpaceMeasurement };
    }

    public static class UserComplianceItems
    {
    }
}
