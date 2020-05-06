using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LanguageExt;

namespace Compliance.Notifications.Applic.Common
{
    public static class DoubleCheck
    {
        private static readonly ConcurrentDictionary<string, DateTime> DoubleCheckTimeStamps = new ConcurrentDictionary<string, DateTime>();
        private static readonly TimeSpan DoubleCheckThreshold = new TimeSpan(0, 0, 60);

        public static bool ShouldRunDoubleCheckAction(Some<string> actionName)
        {
            if (!DoubleCheckTimeStamps.ContainsKey(actionName.Value))
            {
                DoubleCheckTimeStamps.AddOrUpdate(actionName.Value,DateTime.MinValue,(s, time) => DateTime.MinValue);
            }
            var timeSinceLastDoubleCheck = DateTime.Now - DoubleCheckTimeStamps[actionName.Value];
            var doDoubleCheck = timeSinceLastDoubleCheck > DoubleCheckThreshold;
            return doDoubleCheck;
        }

        public static void TimeStampDoubleCheckAction(Some<string> actionName)
        {
            DoubleCheckTimeStamps.AddOrUpdate(actionName.Value, DateTime.Now, (s, time) => DateTime.Now);
        }
    }
}
