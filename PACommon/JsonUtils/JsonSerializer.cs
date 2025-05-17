using System.Text.Json;
using SysJsonSerializer = System.Text.Json.JsonSerializer;

namespace PACommon.JsonUtils
{
    public static class JsonSerializer
    {
        #region Private fields
        private static readonly JsonSerializerOptions _readerOptions = new()
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };
        private static readonly JsonSerializerOptions _unformatedWriterOptions = new();
        private static readonly JsonSerializerOptions _formatedWriterOptions = new() { WriteIndented = true };
        #endregion

        #region Public functions
        /// <summary>
        /// Turns the json string into a JsonDictionary.
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        /// <exception cref="FormatException">Thrown if the deserialized json is not an object.</exception>
        public static JsonDictionary? DeserializeJson(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return null;
            }
            var jsonElement = SysJsonSerializer.Deserialize<JsonElement>(jsonString, _readerOptions);
            return jsonElement.ValueKind == JsonValueKind.Object
                ? DeserializeJsonObjectEnumerator(jsonElement.EnumerateObject())
                : throw new FormatException("Deserialized json element is not an object.");
        }

        /// <summary>
        /// Serializes the <see cref="JsonDictionary"/> into a string representation.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <param name="format">Wherther to format the json string. This makes the string span multiple lines.</param>
        public static string SerializeJson(JsonDictionary? jsonData, bool format = false)
        {
            var convertedData = SerializeJsonDictionary(jsonData);
            return SysJsonSerializer.Serialize(convertedData, format ? _formatedWriterOptions : _unformatedWriterOptions);
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
            return [.. jsonDict];
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
            return [.. jsonList];
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
