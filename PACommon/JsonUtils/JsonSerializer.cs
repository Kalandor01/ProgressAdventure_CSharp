using System.Text.Json;
using SysJsonSerializer = System.Text.Json.JsonSerializer;

namespace PACommon.JsonUtils
{
    public static class JsonSerializer
    {
        #region Public functions
        /// <summary>
        /// Turns the json string into a JsonDictionary.
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        public static JsonDictionary? DeserializeJson(string jsonString)
        {
            var rootElement = SysJsonSerializer.Deserialize<JsonElement?>(jsonString);
            return rootElement is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object
                ? DeserializeJsonObjectEnumerator(jsonElement.EnumerateObject())
                : null;
        }

        public static string SerializeJson(JsonDictionary? jsonData)
        {
            var convertedData = SerializeJsonDictionary(jsonData);
            return SysJsonSerializer.Serialize(convertedData);
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
                                        ? new JsonValue(guidValue)
                                        : (element.TryGetDateTime(out var dateTimeValue)
                                            ? new JsonValue(dateTimeValue)
                                            : (TimeSpan.TryParse(element.ToString(), out var timeSpanValue)
                                                ? new JsonValue(timeSpanValue)
                                                : new JsonValue(element.ToString())
                                            )
                                        ),
                JsonValueKind.Number => element.TryGetInt64(out var longValue)
                                        ? new JsonValue(longValue)
                                        : (element.TryGetUInt64(out var ulongValue)
                                            ? new JsonValue(ulongValue)
                                            : (element.TryGetDouble(out var doubleValue)
                                                ? new JsonValue(doubleValue)
                                                : new JsonValue(element.ToString(), true)
                                            )
                                        ),
                JsonValueKind.True => new JsonValue(true),
                JsonValueKind.False => new JsonValue(false),
                _ => null,
            };
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Turns the JsonElement object enumerator into a JsonDictionary.
        /// </summary>
        /// <param name="objectEnumerator">The object enumerator containing the JsonElements.</param>
        private static JsonDictionary DeserializeJsonObjectEnumerator(JsonElement.ObjectEnumerator objectEnumerator)
        {
            var jsonDict = new Dictionary<string, JsonObject?>();
            foreach (var property in objectEnumerator)
            {
                var jsonValue = DeserializeJsonElement(property.Value);
                jsonDict.Add(property.Name, jsonValue);
            }
            return new JsonDictionary(jsonDict);
        }

        /// <summary>
        /// Turns the JsonElements array enumerator into a JsonArray. 
        /// </summary>
        /// <param name="jsonListEnumerator">The json array enumerator.</param>
        private static JsonArray DeserializeJsonArrayEnumerator(JsonElement.ArrayEnumerator jsonListEnumerator)
        {
            var jsonList = new List<JsonObject?>();
            foreach (var element in jsonListEnumerator)
            {
                jsonList.Add(DeserializeJsonElement(element));
            }
            return new JsonArray(jsonList);
        }

        private static Dictionary<string, object?>? SerializeJsonDictionary(JsonDictionary? jsonObject)
        {
            return jsonObject?.ToDictionary(k => k.Key, v => SerializeJsonObject(v.Value));
        }

        private static object? SerializeJsonObject(JsonObject? jsonObject)
        {
            if (jsonObject is null)
            {
                return null;
            }
            if (jsonObject is JsonDictionary jsonDictionary)
            {
                return SerializeJsonDictionary(jsonDictionary);
            }
            if (jsonObject is JsonArray jsonArray)
            {
                return jsonArray.Select(SerializeJsonObject);
            }
            return jsonObject.Value;
        }
        #endregion
    }
}
