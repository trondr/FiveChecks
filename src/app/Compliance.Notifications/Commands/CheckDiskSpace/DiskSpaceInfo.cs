using Compliance.Notifications.Common;
using Compliance.Notifications.Data;

namespace Compliance.Notifications.Commands.CheckDiskSpace
{
    public class DiskSpaceInfo
    {
        public UDecimal SccmCacheSize { get; set; }
        public UDecimal TotalFreeDiskSpace { get; set; }
        public static DiskSpaceInfo Default => new DiskSpaceInfo() { SccmCacheSize = 0M,TotalFreeDiskSpace = 9999M};
    }
}