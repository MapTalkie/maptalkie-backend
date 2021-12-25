using System;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapTalkie.Domain.Utils.JsonConverters;

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
            w.WritePropertyName("y");
            w.WriteValue(value.Y);
            w.WritePropertyName("x");
            w.WriteValue(value.X);
            w.WritePropertyName("srid");
            w.WriteValue(value.SRID);
            w.WriteEndObject();
        }
    }

    public override Point ReadJson(JsonReader reader, Type objectType, Point? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        try
        {
            return new Point(jObject["x"]!.Value<double>(), jObject["y"]!.Value<double>())
            {
                SRID = jObject["srid"]!.Value<int>()
            };
        }
        catch
        {
            throw new JsonSerializationException(
                "Invalid Point structure - expected keys 'x', 'y' (double) and 'srid' (int)");
        }
    }
}