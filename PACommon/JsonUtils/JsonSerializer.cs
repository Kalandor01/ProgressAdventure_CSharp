using System.Collections;
using System.Text.Json;
using SysJsonSerializer = System.Text.Json.JsonSerializer;

namespace PACommon.JsonUtils
{
    public static class JsonSerializer
    {
        #region Public functions
        /// <summary>
        /// Turns the json string into a dictionary.
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        public static Dictionary<string, object?>? DeserializeJson(string jsonString)
        {
            var rootElement = SysJsonSerializer.Deserialize<JsonElement?>(jsonString);
            return rootElement is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object
                ? DeserializeJsonObjectEnumerator(jsonElement.EnumerateObject())
                : null;
        }

        public static string SerializeJson(IDictionary? jsonData)
        {
            return SysJsonSerializer.Serialize(jsonData);
        }

        /// <summary>
        /// Returns the value of the JsonElement.
        /// </summary>
        /// <param name="element">The JsonElement to deserialize.</param>
        public static JsonObject? DeserializeJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => DeserializeJsonObjectEnumerator(element.EnumerateObject()),
                JsonValueKind.Array => DeserializeJsonArrayEnumerator(element.EnumerateArray()),
                JsonValueKind.String => element.TryGetGuid(out var guidValue)
                                        ? guidValue
                                        : (element.TryGetDateTime(out var dateTimeValue)
                                            ? dateTimeValue
                                            : element.GetString()
                                        ),
                JsonValueKind.Number => element.TryGetInt64(out var longValue)
                                        ? longValue
                                        : (element.TryGetUInt64(out var ulongValue)
                                            ? ulongValue
                                            : element.GetDouble()
                                        ),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null,
            };
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Turns the JsonElement object enumerator into a dictionary of elements names and values.
        /// </summary>
        /// <param name="objectEnumerator">The object enumerator containing the JsonElements.</param>
        private static Dictionary<string, JsonObject?> DeserializeJsonObjectEnumerator(JsonElement.ObjectEnumerator objectEnumerator)
        {
            var jsonDict = new Dictionary<string, object?>();
            foreach (var property in objectEnumerator)
            {
                var jsonValue = DeserializeJsonElement(property.Value);
                jsonDict.Add(property.Name, jsonValue);
            }
            return jsonDict;
        }

        /// <summary>
        /// Turns the JsonElements array enumerator into a list of the elements values. 
        /// </summary>
        /// <param name="jsonListEnumerator">The json array enumerator.</param>
        private static List<JsonObject?> DeserializeJsonArrayEnumerator(JsonElement.ArrayEnumerator jsonListEnumerator)
        {
            // TODO: custom DTO for storing json (like JsonElment but more primitive)
            var jsonList = new List<object?>();
            foreach (var element in jsonListEnumerator)
            {
                jsonList.Add(DeserializeJsonElement(element));
            }
            return jsonList;
        }
        #endregion
    }
}
