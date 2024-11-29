using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ConfigManagement
{
    internal class MaterialItemAttributesDTOConverter : JsonConverter<MaterialItemAttributesDTO>
    {
        public override MaterialItemAttributesDTO? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonDict = reader.GetObjectDictionary();

            var unitStr = jsonDict[nameof(CompoundItemAttributesDTO.unit)]!;
            unitStr = $"\"{unitStr}\"";

            return new MaterialItemAttributesDTO(
                jsonDict["display_name"] ?? throw new ArgumentNullException("display name"),
                JsonSerializer.Deserialize<ItemAmountUnit>(unitStr, options)
            );
        }

        public override void Write(Utf8JsonWriter writer, MaterialItemAttributesDTO value, JsonSerializerOptions options)
        {
            var j = JsonSerializer.Serialize(value.unit, options);
            writer.WriteStartObject();
            writer.WriteString("display_name", value.displayName);
            writer.WritePropertyName("unit");
            writer.WriteRawValue(JsonSerializer.Serialize(value.unit, options));
            writer.WriteEndObject();
        }
    }
}
