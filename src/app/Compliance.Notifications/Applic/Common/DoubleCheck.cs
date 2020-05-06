using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using LanguageExt;

namespace Compliance.Notifications.Applic.Common
{
    public static class DoubleCheck
    {
        private static readonly ConcurrentDictionary<string, DateTime> DoubleCheckTimeStamps = new ConcurrentDictionary<string, DateTime>();
        private static readonly TimeSpan DoubleCheckThreshold = new TimeSpan(0, 0, 60);

        public static bool ShouldRunDoubleCheckAction(Some<string> actionName)
        {
            return ShouldRunDoubleCheckPure(actionName,DoubleCheckTimeStamps.ToImmutableDictionary(),DateTime.Now, DoubleCheckThreshold);
        }

        public static bool ShouldRunDoubleCheckPure(Some<string> actionName, Some<ImmutableDictionary<string, DateTime>> doubleCheckTimeStamps, DateTime now, TimeSpan threshold)
        {
            if (!doubleCheckTimeStamps.Value.ContainsKey(actionName.Value))
            {
                return true;
            }
            var timeSinceLastDoubleCheck = now - doubleCheckTimeStamps.Value[actionName.Value];
            var doDoubleCheck = timeSinceLastDoubleCheck > threshold;
            return doDoubleCheck;
        }

        public static void TimeStampDoubleCheckAction(Some<string> actionName)
        {
            DoubleCheckTimeStamps.AddOrUpdate(actionName.Value, DateTime.Now, (s, time) => DateTime.Now);
        }
    }
}
