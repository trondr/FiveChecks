using LanguageExt;
using Pri.LongPath;

namespace Compliance.Notifications.Applic.Common
{
    public class ScheduledTaskInfo
    {
        public ScheduledTaskInfo(Some<string> taskName, Some<string> taskDescription, Some<FileInfo> actionPath, Some<string> actionArguments, int eventId)
        {
            TaskName = taskName;
            TaskDescription = taskDescription;
            ActionPath = actionPath;
            ActionArguments = actionArguments;
            EventId = eventId;
        }
        public string TaskName { get; }
        public string TaskDescription { get; }
        public FileInfo ActionPath { get; }
        public string ActionArguments { get; }
        public int EventId { get; }
    }
}