using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using LanguageExt;

namespace Compliance.Notifications.Applic.PendingRebootCheck
{
    public class PendingRebootInfo : Record<PendingRebootInfo>
    {
        public bool RebootIsPending { get; set; }

        public List<RebootSource> Sources { get; internal set; } = new List<RebootSource>();

        public static PendingRebootInfo Default => new PendingRebootInfo() { RebootIsPending = false};

        public string ToSourceDescription()
        {
            return string.Join(",", Sources);
        }
    }

    public static class PendingRebootInfoExtensions
    {
        public static PendingRebootInfo Update(this PendingRebootInfo org, PendingRebootInfo add)
        {
            if (org == null) throw new ArgumentNullException(nameof(org));
            if (add == null) throw new ArgumentNullException(nameof(add));
            if (!add.RebootIsPending)
                return new PendingRebootInfo
                {
                    RebootIsPending = org.RebootIsPending, 
                    Sources = new List<RebootSource>(org.RebootIsPending? org.Sources : new List<RebootSource>())
                };
            return new PendingRebootInfo
            {
                RebootIsPending = true, 
                Sources = new List<RebootSource>(org.RebootIsPending? org.Sources.Concat(add.Sources): add.Sources)
            };
        }

        public static PendingRebootInfo RemoveSource(this PendingRebootInfo org, RebootSource rebootSource)
        {
            if (org == null) throw new ArgumentNullException(nameof(org));
            var sources = org.Sources.Where(source => source.Value != rebootSource.Value).ToList();
            return new PendingRebootInfo {RebootIsPending = sources.Count > 0,Sources = sources};
        }

        public static PendingRebootInfo RemoveRebootSources(this PendingRebootInfo org, IEnumerable<RebootSource> rebootSources)
        {
            if (rebootSources == null) throw new ArgumentNullException(nameof(rebootSources));
            var rebootSourceList = rebootSources.ToList();
            var firstRebootSource = rebootSourceList.FirstOrDefault();
            var remainingRebootSources = rebootSourceList.Tail();
            return firstRebootSource != null ? org.RemoveSource(firstRebootSource).RemoveRebootSources(remainingRebootSources) : org;
        }
    }
}