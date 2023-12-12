using Newtonsoft.Json;

namespace ProgressAdventure.ConfigManagement
{
    internal class EnumConverter : JsonConverter<Enum>
    {
        public override Enum? ReadJson(JsonReader reader, Type objectType, Enum? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (!(
                Enum.TryParse(objectType, value?.ToString(), out var res) &&
                Enum.IsDefined(objectType, res)
            ))
            {
                throw new JsonException($"Unknown enum name: \"{value}\"");
            }
            return (Enum?)res;
        }

        public override void WriteJson(JsonWriter writer, Enum? value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }
    }
}
