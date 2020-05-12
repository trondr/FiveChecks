using System;
using System.Collections.Generic;

namespace Compliance.Notifications.Applic.MissingMsUpdatesCheck
{
    public class MissingMsUpdatesInfo
    {
        public List<MsUpdate> Updates { get; internal set; } = new List<MsUpdate>();
        public static MissingMsUpdatesInfo Default => new MissingMsUpdatesInfo();
    }

    public class MsUpdate
    {
        public string ArticleId { get; set; }
        public string Name { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime FirstMeasuredMissing { get; set; }
    }
}
