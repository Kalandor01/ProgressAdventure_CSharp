using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;

namespace ProgressAdventure.ConfigManagement
{
    internal class MaterialItemAttributesDTOConverter : JsonConverter<MaterialItemAttributesDTO>
    {
        public override MaterialItemAttributesDTO? ReadJson(JsonReader reader, Type objectType, MaterialItemAttributesDTO? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = JObject.Load(reader);
            var unitStr = value[nameof(MaterialItemAttributesDTO.unit)].Value<string>();
            unitStr = $"\"{unitStr}\"";
            var converters = ConfigManager.GetConvertersNonInclusive<MaterialItemAttributesDTOConverter>();
            return new MaterialItemAttributesDTO(
                value[nameof(MaterialItemAttributesDTO.displayName)].Value<string>(),
                JsonConvert.DeserializeObject<ItemAmountUnit>(unitStr, converters)
            );
        }

        public override void WriteJson(JsonWriter writer, MaterialItemAttributesDTO? value, JsonSerializer serializer)
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                Converters = ConfigManager.GetConvertersNonInclusive<MaterialItemAttributesDTOConverter>(),
            };
            JsonSerializer.Create(jsonSerializerSettings).Serialize(writer, value);
        }
    }
}
