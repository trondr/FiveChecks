using System;

namespace Compliance.Notifications.Common
{
    public class SystemUptimeInfo
    {
        public SystemUptimeInfo() { }
        public TimeSpan Uptime { get; set; }
        public DateTime LastRestart { get; set; }
    }
}