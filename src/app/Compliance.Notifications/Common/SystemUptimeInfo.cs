using System;

namespace Compliance.Notifications.Common
{
    public class SystemUptimeInfo
    {
        public SystemUptimeInfo() { }
        public TimeSpan Uptime { get; set; }
        public DateTime LastRestart { get; set; }
        public static SystemUptimeInfo Default => new SystemUptimeInfo() { Uptime = TimeSpan.Zero,LastRestart = DateTime.Now};
    }
}