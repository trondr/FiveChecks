using Compliance.Notifications.Common;
using Compliance.Notifications.Data;
using LanguageExt;

namespace Compliance.Notifications.Commands.CheckDiskSpace
{
    public class DiskSpaceInfo: Record<DiskSpaceInfo>
    {
        public UDecimal SccmCacheSize { get; set; }
        public UDecimal TotalFreeDiskSpace { get; set; }
        public static DiskSpaceInfo Default => new DiskSpaceInfo() { SccmCacheSize = 0M, TotalFreeDiskSpace = 9999M};
    }
}