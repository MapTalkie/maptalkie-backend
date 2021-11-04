using System;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapTalkie.Utils.JsonConverters
{
    public class PointJsonConverter : JsonConverter<Point>
    {
        public override void WriteJson(JsonWriter w, Point? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                w.WriteNull();
            }
            else
            {
                w.WriteStartObject();
                w.WritePropertyName("x");
                w.WriteValue(value.X);
                w.WritePropertyName("y");
                w.WriteValue(value.Y);
                w.WriteEndObject();
            }
        }

        public override Point? ReadJson(JsonReader reader, Type objectType, Point? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;
            var jObject = JObject.FromObject(reader.Value);
            return new Point(jObject["x"]!.Value<double>(), jObject["y"]!.Value<double>());
        }
    }
}