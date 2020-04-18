namespace Compliance.Notifications.Applic.DesktopDataCheck
{
    public class DesktopDataInfo
    {
        public bool HasDesktopData { get; set; }
        public static DesktopDataInfo Default => new DesktopDataInfo { HasDesktopData = false, NumberOfFiles = 0, TotalSizeInBytes = 0L };
        public int NumberOfFiles { get; set; }
        public long TotalSizeInBytes { get; set; }
    }
}