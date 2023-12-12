using Newtonsoft.Json;
using ProgressAdventure.ItemManagement;

namespace ProgressAdventure.ConfigManagement
{
    internal class ItemTypeIDConverter : JsonConverter<ItemTypeID>
    {
        public override ItemTypeID ReadJson(JsonReader reader, Type objectType, ItemTypeID existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if(ItemUtils.TryParseItemType(reader.Value?.ToString(), out var itemType))
            {
                throw new JsonException($"Unknown item type name: \"{reader.Value}\"");
            }
            return itemType;
        }

        public override void WriteJson(JsonWriter writer, ItemTypeID value, JsonSerializer serializer)
        {
            writer.WriteValue(ItemUtils.ItemIDToTypeName(value));
        }
    }
}
