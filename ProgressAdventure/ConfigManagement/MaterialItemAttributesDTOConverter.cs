using PACommon.Extensions;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ConfigManagement
{
    public class MaterialItemAttributesDTOConverter : JsonConverter<MaterialItemAttributesDTO>
    {
        public override MaterialItemAttributesDTO? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            static T ItemArgumentHelper<T>(Dictionary<string, object?> jsonDict, string key)
            {
                return (T)(jsonDict[key]
                    ?? throw new ArgumentNullException($"Couldn't deserialize item {key}", key));
            }

            var jsonDict = reader.GetObjectDictionary(MaterialItemAttrValuesConvert, options);

            return new MaterialItemAttributesDTO(
                ItemArgumentHelper<string>(jsonDict, "display_name"),
                ItemArgumentHelper<MaterialPropertiesDTO>(jsonDict, nameof(MaterialItemAttributesDTO.properties)),
                ItemArgumentHelper<ItemAmountUnit>(jsonDict, nameof(MaterialItemAttributesDTO.unit))
            );
        }

        public override void Write(Utf8JsonWriter writer, MaterialItemAttributesDTO value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("display_name", value.displayName);
            writer.WritePropertyName(nameof(MaterialItemAttributesDTO.unit));
            writer.WriteRawValue(JsonSerializer.Serialize(value.unit, options));
            writer.WritePropertyName(nameof(MaterialItemAttributesDTO.properties));
            writer.WriteRawValue(JsonSerializer.Serialize(value.properties, options));
            writer.WriteEndObject();
        }

        private static object? MaterialItemAttrValuesConvert(ref Utf8JsonReader reader, string key, JsonSerializerOptions options)
        {
            return key switch
            {
                "display_name" => reader.GetString(),
                nameof(MaterialItemAttributesDTO.unit) => JsonSerializer.Deserialize<ItemAmountUnit>(ref reader, options),
                nameof(MaterialItemAttributesDTO.properties) => JsonSerializer.Deserialize<MaterialPropertiesDTO>(ref reader, options),
                _ => throw new ArgumentException("Unknown key!", key),
            };
        }
    }
}
