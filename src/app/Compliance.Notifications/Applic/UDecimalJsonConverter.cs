using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Compliance.Notifications.Applic
{
    public class UDecimalJsonConverter: JsonConverter<UDecimal>
    {
        public override void WriteJson(JsonWriter writer, UDecimal value, JsonSerializer serializer)
        {
            writer?.WriteValue((decimal)value);
        }

        public override UDecimal ReadJson(JsonReader reader, Type objectType, UDecimal existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return Convert.ToDecimal(reader.Value, CultureInfo.InvariantCulture);
        }

        public override bool CanRead => true;
    }

   
}
