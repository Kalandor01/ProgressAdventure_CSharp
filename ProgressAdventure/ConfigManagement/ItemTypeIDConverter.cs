using ProgressAdventure.ItemManagement;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ConfigManagement
{
    internal class ItemTypeIDConverter : JsonConverter<ItemTypeID>
    {
        public override ItemTypeID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (!ItemUtils.TryParseItemType(value, out var itemType))
            {
                throw new JsonException($"Unknown item type name: \"{value}\"");
            }
            return itemType;
        }

        public override void Write(Utf8JsonWriter writer, ItemTypeID value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(ItemUtils.ItemIDToTypeName(value));
        }
    }
}
