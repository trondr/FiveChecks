using Compliance.Notifications.Data;

namespace Compliance.Notifications.Commands.CheckDiskSpace
{
    public class DiskSpaceInfo
    {
        public UDecimal SccmCacheSize { get; set; }
        public UDecimal TotalFreeDiskSpace { get; set; }
    }
}