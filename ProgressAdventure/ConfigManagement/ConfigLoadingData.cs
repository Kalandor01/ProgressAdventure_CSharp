using PACommon.JsonUtils;

namespace ProgressAdventure.ConfigManagement
{
    /// <summary>
    /// Class for storing the data for a config in the config loading order file.
    /// </summary>
    public class ConfigLoadingData
    {
        /// <summary>
        /// The namespace for a config.
        /// </summary>
        public readonly string Namespace;
        /// <summary>
        /// If the config is enabled.
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// <inheritdoc cref="ConfigLoadingData" path="//summary"/>
        /// </summary>
        /// <param name="namespaceName"><inheritdoc cref="Namespace" path="//summary"/></param>
        /// <param name="enabled"><inheritdoc cref="Enabled" path="//summary"/></param>
        /// <exception cref="ArgumentException">Thrown if the namespace is invalid.</exception>
        public ConfigLoadingData(string namespaceName, bool enabled)
        {
            Namespace = ConfigUtils.NamespaceRegex().IsMatch(namespaceName)
                ? namespaceName
                : throw new ArgumentException("Invalid namespace name", nameof(namespaceName));
            Enabled = enabled;
        }

        public override string? ToString()
        {
            return $"{Namespace}: {(Enabled ? "enabled" : "disabled")}";
        }

        /// <inheritdoc cref="IJsonReadable.ToJson"/>
        public (string key, JsonDictionary value) ToJson()
        {
            return (Namespace, new JsonDictionary { ["enabled"] = Enabled });
        }

        /// <inheritdoc cref="IJsonConvertable{TSelf}.FromJson(JsonDictionary?, string, out TSelf)"/>
        public static bool FromJson(KeyValuePair<string, JsonObject?> objectJson, out ConfigLoadingData? convertedObject)
        {
            convertedObject = null;
            var namespaceName = objectJson.Key;
            if (!ConfigUtils.NamespaceRegex().IsMatch(namespaceName))
            {
                PACommon.Tools.LogJsonParseError("namespace", "namespace can only be lowercase characters, numbers and \"_\"");
                return false;
            }

            var success = true;
            if (
                objectJson.Value is not JsonDictionary jsonData ||
                !PACommon.Tools.TryParseJsonValue<bool>(jsonData, "enabled", out var enabled, false, true)
            )
            {
                PACommon.Tools.LogJsonParseError("enabled", "defaulting to true");
                enabled = true;
                success = false;
            }

            convertedObject = new ConfigLoadingData(namespaceName, enabled);
            return success;
        }
    }
}
