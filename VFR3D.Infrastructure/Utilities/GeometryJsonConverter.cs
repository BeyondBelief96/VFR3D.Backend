using System.Text.Json;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace VFR3D.Infrastructure.Utilities
{
    public class GeometryJsonConverter : System.Text.Json.Serialization.JsonConverter<Geometry>
    {
        private readonly JsonSerializer _serializer = GeoJsonSerializer.Create();

        public override Geometry? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            // Buffer the JSON to a string since GeoJsonSerializer expects a string/TextReader
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var jsonString = jsonDoc.RootElement.GetRawText();
            using var stringReader = new StringReader(jsonString);
            using var jsonReader = new JsonTextReader(stringReader);
            
            return _serializer.Deserialize<Geometry>(jsonReader);
        }

        public override void Write(Utf8JsonWriter writer, Geometry value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            
            _serializer.Serialize(jsonWriter, value);
            
            // Parse and write the JSON to maintain proper structure
            using var jsonDoc = JsonDocument.Parse(stringWriter.ToString());
            jsonDoc.RootElement.WriteTo(writer);
        }
    }
}