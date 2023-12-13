using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;

namespace ProgressAdventure.ConfigManagement
{
    internal class CompoundItemAttributesDTOConverter : JsonConverter<CompoundItemAttributesDTO>
    {
        public override CompoundItemAttributesDTO? ReadJson(JsonReader reader, Type objectType, CompoundItemAttributesDTO? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = JObject.Load(reader);

            var itemTypeString = value[nameof(CompoundItemAttributesDTO.typeName)].Value<string>();
            var itemType = ItemUtils.ParseItemType(itemTypeString) ?? throw new ArgumentNullException("item type");
            var unitStr = value[nameof(CompoundItemAttributesDTO.unit)].Value<string>();
            unitStr = $"\"{unitStr}\"";

            var converters = ConfigManager.Instance.GetConvertersNonInclusive<CompoundItemAttributesDTOConverter>();
            return new CompoundItemAttributesDTO(
                itemType,
                value[nameof(CompoundItemAttributesDTO.displayName)].Value<string>(),
                JsonConvert.DeserializeObject<ItemAmountUnit>(unitStr, converters)
            );
        }

        public override void WriteJson(JsonWriter writer, CompoundItemAttributesDTO? value, JsonSerializer serializer)
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                Converters = ConfigManager.Instance.GetConvertersNonInclusive<CompoundItemAttributesDTOConverter>(),
            };
            JsonSerializer.Create(jsonSerializerSettings).Serialize(writer, value);
        }
    }
}
