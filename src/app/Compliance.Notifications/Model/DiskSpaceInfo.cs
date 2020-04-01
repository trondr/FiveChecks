using LanguageExt;

namespace Compliance.Notifications.Model
{
    public class DiskSpaceInfo: Record<DiskSpaceInfo>
    {
        public UDecimal SccmCacheSize { get; set; }
        public UDecimal TotalFreeDiskSpace { get; set; }
        public static DiskSpaceInfo Default => new DiskSpaceInfo { SccmCacheSize = 0M, TotalFreeDiskSpace = 9999M};
    }
}