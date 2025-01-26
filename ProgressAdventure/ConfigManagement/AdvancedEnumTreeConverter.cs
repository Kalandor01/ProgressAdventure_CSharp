using PACommon.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ConfigManagement
{
    public class AdvancedEnumTreeConverter<TEnum> : JsonConverter<EnumTreeValue<TEnum>>
        where TEnum : AdvancedEnumTree<TEnum>
    {
        public override EnumTreeValue<TEnum> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var enumFullName = reader.GetString();
            if (!AdvancedEnumTree<TEnum>.TryGetValue(enumFullName, out var enumValue))
            {
                throw new JsonException($"Unknown enum value name of type {typeof(TEnum)}: \"{enumFullName}\"");
            }
            return enumValue;
        }

        public override void Write(Utf8JsonWriter writer, EnumTreeValue<TEnum> value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.FullName);
        }
    }
}
