using System;
using MapTalkie.Common.Utils;
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
                if (value.SRID == 3857 || value.SRID == 0 || value.SRID == 4326)
                {
                    value = MapConvert.ToLatLon(value);

                    w.WritePropertyName("lat");
                    w.WriteValue(value.Y);
                    w.WritePropertyName("lon");
                    w.WriteValue(value.X);
                }
                else
                {
                    w.WritePropertyName("y");
                    w.WriteValue(value.Y);
                    w.WritePropertyName("x");
                    w.WriteValue(value.X);
                    w.WritePropertyName("srid");
                    w.WriteValue(value.SRID);
                }

                w.WriteEndObject();
            }
        }

        public override Point ReadJson(JsonReader reader, Type objectType, Point? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            try
            {
                return new Point(jObject["lon"]!.Value<double>(), jObject["lat"]!.Value<double>())
                {
                    SRID = 4326
                };
            }
            catch
            {
                throw new JsonSerializationException(
                    "Invalid Point structure - expected keys 'lat' and 'lon' of type double");
            }
        }
    }
}