using PACommon.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PACommon.ConfigManagement.JsonConverters
{
    public class ConsoleKeyInfoConverter : JsonConverter<ConsoleKeyInfo>
    {
        public override ConsoleKeyInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonDict = reader.GetObjectDictionary();

            var key = JsonSerializer.Deserialize<ConsoleKey>($"\"{jsonDict["key"]!}\"", options);
            var modifiers = JsonSerializer.Deserialize<ConsoleModifiers>($"\"{jsonDict["modifiers"]!}\"", options);

            return new ConsoleKeyInfo(
                jsonDict["key_char"]![0],
                key,
                (modifiers & ConsoleModifiers.Shift) != 0,
                (modifiers & ConsoleModifiers.Alt) != 0,
                (modifiers & ConsoleModifiers.Control) != 0
            );
        }

        public override void Write(Utf8JsonWriter writer, ConsoleKeyInfo value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("key");
            writer.WriteRawValue(JsonSerializer.Serialize(value.Key, options));
            writer.WriteString("key_char", value.KeyChar.ToString());
            writer.WritePropertyName("modifiers");
            writer.WriteRawValue(JsonSerializer.Serialize(value.Modifiers, options));
            writer.WriteEndObject();
        }
    }
}
