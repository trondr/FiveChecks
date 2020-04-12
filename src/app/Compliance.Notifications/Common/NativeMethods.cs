using System.Runtime.InteropServices;
using System.Text;

namespace Compliance.Notifications.Common
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,out ulong lpFreeBytesAvailable,out ulong lpTotalNumberOfBytes,out ulong lpTotalNumberOfFreeBytes);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(SystemMetric smIndex);
    }

    public enum SystemMetric : int
    {
        /// <summary>
        /// This system metric is used in a Terminal Services environment. If the calling process is associated with a Terminal Services 
        /// client session, the return value is nonzero. If the calling process is associated with the Terminal Services console session, 
        /// the return value is 0. 
        /// Windows Server 2003 and Windows XP:  The console session is not necessarily the physical console. 
        /// For more information, see WTSGetActiveConsoleSessionId.
        /// </summary>
        SmRemoteSession = 0x1000,
    }
}
