using System;
using Newtonsoft.Json;

namespace FiveChecks.Applic.PendingRebootCheck
{
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