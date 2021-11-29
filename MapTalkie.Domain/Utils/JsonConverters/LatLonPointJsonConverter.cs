using System;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapTalkie.Domain.Utils.JsonConverters
{
    public class LatLonPointJsonConverter : JsonConverter<Point>
    {
        public override void WriteJson(JsonWriter w, Point? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                w.WriteNull();
            }
            else
            {
                if (value.SRID != 4326)
                    throw new JsonSerializationException("Failed to serialize a point with SRID != 4326");
                value = MapConvert.ToLatLon(value);

                w.WriteStartObject();
                w.WritePropertyName("lat");
                w.WriteValue(value.Y);
                w.WritePropertyName("lon");
                w.WriteValue(value.X);
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