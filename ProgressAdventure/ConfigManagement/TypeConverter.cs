using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ConfigManagement
{
    internal class TypeConverter : JsonConverter<Type>
    {
        public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var typeName = reader.GetString() ?? throw new JsonException($"Type name is null!");
            return PACommon.Utils.GetTypeFromName(typeName) ?? throw new JsonException($"Unknown type name: \"{typeName}\"");
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.FullName);
        }
    }
}
