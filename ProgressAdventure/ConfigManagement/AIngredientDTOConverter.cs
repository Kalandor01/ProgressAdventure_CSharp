using PACommon.Enums;
using PACommon.Extensions;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ConfigManagement
{
    public class AIngredientDTOConverter : JsonConverter<AIngredientDTO>
    {
        public override AIngredientDTO? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            static T ItemArgumentHelper<T>(Dictionary<string, object?> jsonDict, string key, bool nullable = false)
            {
                var value = jsonDict[key];
                if (nullable && value is null)
                {
                    return default;
                }

                return (T)(jsonDict[key] ?? throw new ArgumentNullException($"Couldn't deserialize item {key}", key));
            }

            var jsonDict = reader.GetObjectDictionary(MaterialItemAttrValuesConvert, options);

            var itemType = ItemArgumentHelper<EnumTreeValue<ItemType>>(jsonDict, "item_type");
            var material = ItemArgumentHelper<EnumValue<Material>?>(jsonDict, "material", true);
            var amount = ItemArgumentHelper<double>(jsonDict, "amount");
            var unit = ItemArgumentHelper<ItemAmountUnit?>(jsonDict, "unit", true);

            if (itemType == ItemUtils.MATERIAL_ITEM_TYPE)
            {
                return new NonSolidIngredientDTO(
                    material,
                    amount,
                    unit
                );
            }

            return new IngredientDTO(
                itemType,
                material,
                amount,
                unit
            );
        }

        public override void Write(Utf8JsonWriter writer, AIngredientDTO value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("item_type");
            writer.WriteRawValue(JsonSerializer.Serialize(value.itemType, options));
            writer.WritePropertyName(nameof(AIngredientDTO.material));
            writer.WriteRawValue(JsonSerializer.Serialize(value.material, options));
            writer.WriteNumber(nameof(AIngredientDTO.amount), value.amount);
            writer.WritePropertyName(nameof(AIngredientDTO.unit));
            writer.WriteRawValue(JsonSerializer.Serialize(value.unit, options));
            writer.WriteEndObject();
        }

        private static object? MaterialItemAttrValuesConvert(ref Utf8JsonReader reader, string key, JsonSerializerOptions options)
        {
            return key switch
            {
                "item_type" => JsonSerializer.Deserialize<EnumTreeValue<ItemType>>(ref reader, options),
                "material" => JsonSerializer.Deserialize<EnumValue<Material>?>(ref reader, options),
                "amount" => reader.GetDouble(),
                "unit" => JsonSerializer.Deserialize<ItemAmountUnit?>(ref reader, options),
                _ => throw new ArgumentException("Unknown key!", key),
            };
        }
    }
}
