using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapTalkie.Utils.JsonConverters
{
    public class IdToStringConverter : JsonConverter<long>
    {
        public override void WriteJson(JsonWriter writer, long value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }

        public override long ReadJson(JsonReader reader, Type objectType, long existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            JToken jt = JToken.ReadFrom(reader);

            return jt.Value<long>();
        }
    }
}