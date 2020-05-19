using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;

namespace FiveChecks.Applic.MissingMsUpdatesCheck
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

    public static class MissingUpdatesInfoExtensions
    {
        public static MissingMsUpdatesInfo Update(this MissingMsUpdatesInfo previousInfo, Some<MissingMsUpdatesInfo> currentInfo)
        {
            if (previousInfo == null) throw new ArgumentNullException(nameof(previousInfo));
            if(currentInfo.Value.Updates.Count == 0) return MissingMsUpdatesInfo.Default;
            if (previousInfo.Updates.Count == 0) return currentInfo;

            var updatedInfo = MissingMsUpdatesInfo.Default;

            var existingUpdates =
                currentInfo.Value
                    .Updates
                    .Where(update => previousInfo.Updates.Any(msUpdate => update.ArticleId == msUpdate.ArticleId))
                    .Select(currentUpdate =>
                    {
                        var previousUpdate = previousInfo.Updates.First(msUpdate => currentUpdate.ArticleId == msUpdate.ArticleId);
                        return new MsUpdate(){ArticleId = currentUpdate.ArticleId,Deadline = currentUpdate.Deadline, Name = currentUpdate.Name,FirstMeasuredMissing = previousUpdate.FirstMeasuredMissing};
                    });
            var newUpdates = currentInfo.Value.Updates.Where(update => previousInfo.Updates.All(msUpdate => update.ArticleId != msUpdate.ArticleId));
            updatedInfo.Updates.AddRange(existingUpdates);
            updatedInfo.Updates.AddRange(newUpdates);
            return updatedInfo;
            throw new NotImplementedException();
        }
    }
}
