using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

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

        public static async Task<Result<Unit>> RunDoubleCheck(Some<string> actionName, Func<Task<Result<Unit>>> doubleCheckAction, bool doubleCheck)
        {
            if (doubleCheckAction == null) throw new ArgumentNullException(nameof(doubleCheckAction));
            if (doubleCheck)
            {
                TimeStampDoubleCheckAction(actionName);
                return await doubleCheckAction().ConfigureAwait(false);
            }
            return new Result<Unit>(Unit.Default);
        }

        private static void TimeStampDoubleCheckAction(Some<string> actionName)
        {
            DoubleCheckTimeStamps.AddOrUpdate(actionName.Value, DateTime.Now, (s, time) => DateTime.Now);
        }
    }
}
