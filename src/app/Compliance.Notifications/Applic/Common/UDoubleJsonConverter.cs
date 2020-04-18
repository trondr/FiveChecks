using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Compliance.Notifications.Applic.Common
{
    public class UDoubleJsonConverter: JsonConverter<UDouble>
    {
        public override void WriteJson(JsonWriter writer, UDouble value, JsonSerializer serializer)
        {
            writer?.WriteValue(value);
        }

        public override UDouble ReadJson(JsonReader reader, Type objectType, UDouble existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return Convert.ToDouble(reader.Value, CultureInfo.InvariantCulture);
        }

        public override bool CanRead => true;
    }

   
}
