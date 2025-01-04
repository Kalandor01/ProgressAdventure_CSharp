using System.Diagnostics.CodeAnalysis;

namespace PACommon.JsonUtils
{
    /// <summary>
    /// Provides tools for correcting json data.
    /// </summary>
    public static class JsonDataCorrecterUtils
    {
        /// <summary>
        /// Reassigns the value in a specified key, to a new key, if the key exists.
        /// </summary>
        /// <param name="jsonData">The json data to correct.</param>
        /// <param name="oldKey">The old name of the key.</param>
        /// <param name="newKey">The new name of the key.</param>
        public static void RenameKeyIfExists(JsonDictionary jsonData, string oldKey, string newKey)
        {
            if (jsonData.TryGetValue(oldKey, out var value))
            {
                jsonData[newKey] = value;
                jsonData.Remove(oldKey);
            }
        }

        /// <summary>
        /// Remaps the values in the specified keys, to new keys, it they exist.
        /// </summary>
        /// <param name="jsonData">The json data to correct.</param>
        /// <param name="remappings">A dictionary pairing up the old keys with the new keys.</param>
        public static void RemapKeysIfExist(JsonDictionary jsonData, Dictionary<string, string> remappings)
        {
            foreach (var remapping in remappings)
            {
                RenameKeyIfExists(jsonData, remapping.Key, remapping.Value);
            }
        }

        /// <summary>
        /// Removes the key from the <see cref="JsonDictionary"/>.
        /// </summary>
        /// <param name="jsonData">The json data to correct.</param>
        /// <param name="jsonKey">The name of the key to remove.</param>
        public static void DeleteKey(JsonDictionary jsonData, string jsonKey)
        {
            jsonData.Remove(jsonKey);
        }

        /// <summary>
        /// Sets the value at the specified key (if the key exists).
        /// </summary>
        /// <param name="jsonData">The json data to correct.</param>
        /// <param name="jsonKey">The name of the key where to set the value.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="onlyIfKeyExists">Whether to set the value even if the key doesn't exist.</param>
        public static void SetValue(JsonDictionary jsonData, string jsonKey, JsonObject? value, bool onlyIfKeyExists = false)
        {
            if (!onlyIfKeyExists || jsonData.ContainsKey(jsonKey))
            {
                jsonData[jsonKey] = value;
            }
        }

        /// <summary>
        /// Sets a set of values at specified keys (if they exist).
        /// </summary>
        /// <param name="jsonData">The json data to correct.</param>
        /// <param name="valuesMap">A map of what values to set to witch key.</param>
        /// <param name="onlyIfKeyExists">Whether to set a value even if the key doesn't exist.</param>
        public static void SetMultipleValues(JsonDictionary jsonData, Dictionary<string, JsonObject?> valuesMap, bool onlyIfKeyExists = false)
        {
            foreach (var item in valuesMap)
            {
                SetValue(jsonData, item.Key, item.Value, onlyIfKeyExists);
            }
        }

        /// <summary>
        /// Tries to parse a json value to a specified type, and then optionaly transforms that value.<br/>
        /// If the value can't be parsed, it logs a json parse error.
        /// </summary>
        /// <typeparam name="T">The class that is being corrected.</typeparam>
        /// <typeparam name="TRes">The type to parse to.<br/>
        /// The allowed types are the same as with the <see cref="Tools.TryParseValueForJsonParsing{T, TRes}(JsonObject?, out TRes, string?, bool, string?, bool)"/> method.</typeparam>
        /// <param name="jsonData">The json data to correct.</param>
        /// <param name="jsonKey">The name of the key, where the value might change.</param>
        /// <param name="transformer">The function to transform the converted value to a <see cref="JsonObject"/>.<br/>
        /// If success is false, the value won't be modified.<br/>
        /// If <paramref name="transformer"/> it's null, <see cref="Tools.ParseToJsonValue{T}(T, bool, bool)"/> will be used instead.</param>
        public static void TransformValue<T, TRes>(
            JsonDictionary jsonData,
            string jsonKey,
            Func<TRes, (bool success, JsonObject? result)>? transformer = null
        )
        {
            if (
                jsonData.TryGetValue(jsonKey, out var valueJson) &&
                Tools.TryParseValueForJsonParsing<T, TRes>(
                    valueJson,
                    out var value,
                    jsonKey,
                    true,
                    $"{typeof(T)} json correction failed",
                    true
                )
            )
            {
                if (transformer is null)
                {
                    jsonData[jsonKey] = Tools.ParseToJsonValue(value);
                    return;
                }

                var (success, result) = transformer(value);
                if (success)
                {
                    jsonData[jsonKey] = result;
                }
            }
        }

        /// <summary>
        /// Tries to parse a json value to a specified type, and then sets multiple values according to a map.<br/>
        /// If the value can't be parsed, it logs a json parse error.
        /// </summary>
        /// <typeparam name="T">The class that is being corrected.</typeparam>
        /// <typeparam name="TRes">The type to parse to.<br/>
        /// The allowed types are the same as with the <see cref="Tools.TryParseValueForJsonParsing{T, TRes}(JsonObject?, out TRes, string?, bool, string?, bool)"/> method.</typeparam>
        /// <typeparam name="TExtra">The type of the extra data from <paramref name="condition"/>.</typeparam>
        /// <param name="jsonData">The json data to correct.</param>
        /// <param name="jsonKey">The name of the key, where the value might change.</param>
        /// <param name="condition">The function to decide if the <paramref name="transformer"/> should run.</param>
        /// <param name="transformer">The function that can return witch keys to set to what value.<br/>
        /// If success is false, the values won't be modified.</param>
        public static void TransformMultipleValues<T, TRes, TExtra>(
            JsonDictionary jsonData,
            string jsonKey,
            Func<TRes, (bool success, TExtra? extraData)> condition,
            Func<TExtra, Dictionary<string, JsonObject?>> transformer
        )
        {
            if (
                jsonData.TryGetValue(jsonKey, out var valueJson) &&
                Tools.TryParseValueForJsonParsing<T, TRes>(
                    valueJson,
                    out var value,
                    jsonKey,
                    true,
                    $"{typeof(T)} json correction failed",
                    true
                )
            )
            {
                var (success, extraData) = condition(value);
                if (success)
                {
                    SetMultipleValues(jsonData, transformer(extraData!));
                }
            }
        }
    }
}
