using System;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapTalkie.Domain.Utils.JsonConverters
{
    public class PolygonJsonConverter : JsonConverter<Polygon>
    {
        public override void WriteJson(JsonWriter w, Polygon? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                w.WriteNull();
            }
            else
            {
                w.WriteStartObject();
                w.WritePropertyName("points");
                w.WriteStartArray();
                foreach (var point in value.Shell.Coordinates)
                {
                    w.WriteStartObject();
                    w.WritePropertyName("x");
                    w.WriteValue(point.X);
                    w.WritePropertyName("y");
                    w.WriteValue(point.Y);
                    w.WriteEndObject();
                }

                w.WriteEndArray();
                w.WriteEndObject();
            }
        }

        public override Polygon? ReadJson(JsonReader reader, Type objectType, Polygon? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            try
            {
                var jObject = JObject.Load(reader);
                var points = jObject["points"]!.Value<JArray>();
                var coords = new Coordinate[points!.Count];
                for (var i = 0; i < points.Count; i++)
                {
                    var pObject = points[i].Value<JObject>()!;
                    coords[i] = new Coordinate(pObject["x"]!.Value<double>(), pObject["y"]!.Value<double>());
                }

                return new Polygon(new LinearRing(coords));
            }
            catch (Exception e)
            {
                throw new JsonReaderException("Invalid Polygon object");
            }
        }
    }
}