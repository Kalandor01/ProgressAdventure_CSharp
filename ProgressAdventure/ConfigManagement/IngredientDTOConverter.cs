using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;

namespace ProgressAdventure.ConfigManagement
{
    internal class IngredientDTOConverter : JsonConverter<IngredientDTO>
    {
        public override IngredientDTO? ReadJson(JsonReader reader, Type objectType, IngredientDTO? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = JObject.Load(reader);

            var itemTypeString = value[nameof(IngredientDTO.itemType)].Value<string>();
            var itemType = ItemUtils.ParseItemType(itemTypeString) ?? throw new ArgumentNullException("item type");
            var unitInt = value[nameof(IngredientDTO.unit)].Value<int?>();
            var materialInt = value[nameof(IngredientDTO.material)].Value<int?>();
            var converters = ConfigManager.Instance.GetConvertersNonInclusive<IngredientDTOConverter>();
            Material? material = null;
            if (materialInt is not null)
            {
                material = JsonConvert.DeserializeObject<Material>(materialInt.ToString(), converters);
            }
            ItemAmountUnit? unit = null;
            if (unitInt is not null)
            {
                unit = JsonConvert.DeserializeObject<ItemAmountUnit>(unitInt.ToString(), converters);
            }
            return new IngredientDTO(
                itemType,
                material,
                value[nameof(IngredientDTO.amount)].Value<double>(),
                unit
            );
        }

        public override void WriteJson(JsonWriter writer, IngredientDTO? value, JsonSerializer serializer)
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                Converters = ConfigManager.Instance.GetConvertersNonInclusive<IngredientDTOConverter>(),
            };
            JsonSerializer.Create(jsonSerializerSettings).Serialize(writer, value);
        }
    }
}
