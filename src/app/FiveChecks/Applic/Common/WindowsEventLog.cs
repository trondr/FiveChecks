using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace FiveChecks.Applic.Common
{
    public static class WindowsEventLog
    {
        public const string EventLogName = "Application";
        public const string EventSourceName = "FiveChecks";
        public const int UserComplianceItemEventId = 11;
        public const int SystemComplianceItemEventId = 12;

        public static async Task<Result<Unit>> CreateEventSource()
        {
            return await Task.Run(() =>
            {
                if (!EventLog.SourceExists(EventSourceName))
                {
                    EventLog.CreateEventSource(EventSourceName, EventLogName);
                }
                return new Result<Unit>(Unit.Default);
            }).ConfigureAwait(false);
        }

        public static async Task<Result<Unit>> WriteEventLog(string message, EventLogEntryType type, int eventId)
        {
            return await F.AsyncTryFunc(async () =>
            {
                using (var applicationEventLog = new EventLog(EventLogName) { Source = EventSourceName })
                {
                    applicationEventLog.WriteEntry(message, type, eventId);
                    return await Task.FromResult(new Result<Unit>(Unit.Default)).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }
    }
}