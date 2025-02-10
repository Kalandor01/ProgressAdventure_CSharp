using PACommon.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ConfigManagement
{
    public class AdvancedEnumConverter<TEnum> : JsonConverter<EnumValue<TEnum>>
        where TEnum : AdvancedEnum<TEnum>
    {
        public override EnumValue<TEnum> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var enumName = reader.GetString();
            if (!AdvancedEnum<TEnum>.TryGetValue(enumName, out var enumValue))
            {
                throw new JsonException($"Unknown enum value name of type {typeof(TEnum)}: \"{enumName}\"");
            }
            return enumValue;
        }

        public override void Write(Utf8JsonWriter writer, EnumValue<TEnum> value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Name);
        }
    }
}
