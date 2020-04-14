using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;

namespace Compliance.Notifications.Applic
{
    public class PendingRebootInfo : Record<PendingRebootInfo>
    {
        public bool RebootIsPending { get; set; }

        public List<RebootSource> Source { get; internal set; } = new List<RebootSource>();

        public static PendingRebootInfo Default => new PendingRebootInfo() { RebootIsPending = false};
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
                    Source = new List<RebootSource>(org.RebootIsPending? org.Source : new List<RebootSource>())
                };
            return new PendingRebootInfo
            {
                RebootIsPending = true, 
                Source = new List<RebootSource>(org.RebootIsPending? org.Source.Concat(add.Source): add.Source)
            };
        }
    }
}