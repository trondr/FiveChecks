using Compliance.Notifications.Common;
using LanguageExt;

namespace Compliance.Notifications.ComplianceItems.SystemDiskSpace
{
    public class DiskSpaceInfo: Record<DiskSpaceInfo>
    {
        public UDecimal SccmCacheSize { get; set; }
        public UDecimal TotalFreeDiskSpace { get; set; }
        public static DiskSpaceInfo Default => new DiskSpaceInfo() { SccmCacheSize = 0M, TotalFreeDiskSpace = 9999M};
    }
}