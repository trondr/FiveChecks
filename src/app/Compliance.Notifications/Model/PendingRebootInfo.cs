using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using Newtonsoft.Json;

namespace Compliance.Notifications.Model
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
    
    public class RebootSource: Record<RebootSource>
    {
        private RebootSource(string value) { Value = value; }
        public string Value { get; set; }
        public static RebootSource Cbs => new RebootSource("Cbs");
        public static RebootSource Wuau => new RebootSource("Wuau");
        public static RebootSource PendingFileRename => new RebootSource("PendingFileRename");
        public static RebootSource SccmClient => new RebootSource("SccmClient");
        public static RebootSource JoinDomain => new RebootSource("JoinDomain");
        public static RebootSource ComputerNameRename => new RebootSource("ComputerNameRename");
        public static RebootSource RunOnce => new RebootSource("RunOnce");

        public static RebootSource StringToRebootSource(string value)
        {
            switch (value)
            {
                case "Cbs": return RebootSource.Cbs;
                case "Wuau": return RebootSource.Wuau;
                case "PendingFileRename": return RebootSource.PendingFileRename;
                case "SccmClient": return RebootSource.SccmClient;
                case "JoinDomain": return RebootSource.JoinDomain;
                case "ComputerNameRename": return RebootSource.ComputerNameRename;
                case "RunOne": return RebootSource.RunOnce;
                default:
                    throw new ArgumentException($"Invalid reboot source: {value}");
            }
        }
    }
    
    public class RebootSourceJsonConverter : JsonConverter<RebootSource>
    {
        public override void WriteJson(JsonWriter writer, RebootSource value, JsonSerializer serializer)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            writer?.WriteValue(value.Value);
        }

        public override RebootSource ReadJson(JsonReader reader, Type objectType, RebootSource existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return RebootSource.StringToRebootSource(reader.Value as string);
        }

        public override bool CanRead => true;
    }
}