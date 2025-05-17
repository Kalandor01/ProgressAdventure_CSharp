using PACommon.JsonUtils;
using System.Diagnostics.CodeAnalysis;

namespace ProgressAdventure.ConfigManagement
{
    public class LoadedConfigData : IJsonConvertable<LoadedConfigData>
    {
        public readonly string Namespace;
        public readonly string Version;

        public LoadedConfigData(string @namespace, string version)
        {
            Namespace = ConfigUtils.NamespaceRegex().IsMatch(@namespace)
                ? @namespace
                : throw new ArgumentException("Invalid namespace name", nameof(@namespace));
            Version = version;
        }

        public override string? ToString()
        {
            return $"\"{Namespace}\": {Version}";
        }

        #region JsonConvert
        public JsonDictionary ToJson()
        {
            return new JsonDictionary
            {
                [Constants.JsonKeys.ConfigData.NAMESPACE] = Namespace,
                [Constants.JsonKeys.ConfigData.VERSION] = Version,
            };
        }

        public static bool FromJsonWithoutCorrection(
            JsonDictionary configJson,
            string fileVersion,
            [NotNullWhen(true)] ref LoadedConfigData? convertedObject
        )
        {
            if (
                !PACommon.Tools.TryParseJsonValue<string>(configJson, Constants.JsonKeys.ConfigData.NAMESPACE, out var namespaceName, isCritical: true) ||
                !ConfigUtils.NamespaceRegex().IsMatch(namespaceName)
            )
            {
                PACommon.Tools.LogJsonParseError(nameof(namespaceName), $"invalid namespace name: \"{namespaceName}\"", true);
                return false;
            }

            if (!PACommon.Tools.TryParseJsonValue<string>(configJson, Constants.JsonKeys.ConfigData.VERSION, out var version, isCritical: true))
            {
                return false;
            }

            convertedObject = new LoadedConfigData(namespaceName, version);
            return true;
        }
        #endregion

        /// <summary>
        /// Tries to create a <see cref="LoadedConfigData"/> object.
        /// </summary>
        /// <param name="namespace">The loaded config namespace.</param>
        /// <param name="version">The loaded config version.</param>
        /// <param name="configData">The created <see cref="LoadedConfigData"/>.</param>
        /// <returns>If the creation was successful.</returns>
        public static bool TryCreate(string @namespace, string version, [NotNullWhen(true)] out LoadedConfigData? configData)
        {
            try
            {
                configData = new LoadedConfigData(@namespace, version);
                return true;
            }
            catch (Exception ex)
            {
                configData = null;
                return false;
            }
        }
    }
}
