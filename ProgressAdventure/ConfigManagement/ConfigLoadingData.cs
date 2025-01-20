using PACommon.JsonUtils;

namespace ProgressAdventure.ConfigManagement
{
    public partial class ConfigLoadingData
    {
        public readonly string Namespace;
        public bool Enabled;

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

        public (string key, JsonDictionary value) ToJson()
        {
            return (Namespace, new JsonDictionary { ["enabled"] = Enabled });
        }

        public static ConfigLoadingData? FromJson(KeyValuePair<string, JsonObject?> jsonDataRow)
        {
            var namespaceName = jsonDataRow.Key;
            if (!ConfigUtils.NamespaceRegex().IsMatch(namespaceName))
            {
                PACommon.Tools.LogJsonParseError<ConfigLoadingData>("namespace", "namespace can only be lowercase characters, numbers and \"_\"");
                return null;
            }

            if (
                jsonDataRow.Value is not JsonDictionary jsonData ||
                !PACommon.Tools.TryParseJsonValue<ConfigLoadingData, bool>(jsonData, "enabled", out var enabled, false, true)
            )
            {
                PACommon.Tools.LogJsonParseError<ConfigLoadingData>("enabled", "defaulting to true");
                enabled = true;
            }

            return new ConfigLoadingData(namespaceName, enabled);
        }
    }
}
