using PACommon;
using PACommon.JsonUtils;
using System.Text.RegularExpressions;

namespace ProgressAdventure.ConfigManagement
{
    /// <summary>
    /// Static class for storing functions for config management.
    /// </summary>
    public static partial class ConfigUtils
    {
        #region Public functions
        #region Reload config functions
        /// <summary>
        /// Reloads the config value from the aggregate of a config file from all given namespaces.
        /// </summary>
        /// <typeparam name="T">The type of the config value.</typeparam>
        /// <param name="configName">The name of the config.</param>
        /// <param name="namespaces">The name of all namespaces to load from.</param>
        /// <param name="vanillaDefaultValue">The default value of the config for if the vanilla config needs to be recreated.</param>
        /// <param name="getStartingValueFunction">The (empty) starting value of the config value.</param>
        /// <param name="appendConfigFunction">The function to aggregate two config values.</param>
        /// <param name="vanillaNamespaceInvalid">If true, it will always recreate the config file from the vanilla namespace.</param>
        /// <returns>The aggregate of the config file from all given namespaces.</returns>
        public static T ReloadConfigsAggregate<T>(
            string configName,
            List<string> namespaces,
            T vanillaDefaultValue,
            Func<T> getStartingValueFunction,
            Func<T, T, T> appendConfigFunction,
            bool vanillaNamespaceInvalid = false
        )
        {
            return ReloadConfigsAggregatePrivate(
                configName,
                namespaces,
                getStartingValueFunction,
                appendConfigFunction,
                (configName) =>
                    PACSingletons.Instance.ConfigManager.TryGetConfigOrRecreate(
                        configName,
                        null,
                        vanillaDefaultValue,
                        vanillaNamespaceInvalid
                    ),
                (configName) =>
                    (PACSingletons.Instance.ConfigManager.TryGetConfig<T>(
                        configName,
                        null,
                        out var configValue
                    ), configValue)
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool)"/> for list values.
        /// </summary>
        /// <typeparam name="T">elements in the list.</typeparam>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool)"/>
        public static List<T> ReloadConfigsAggregate<T>(
            string configName,
            List<string> namespaces,
            List<T> vanillaDefaultValue,
            bool vanillaNamespaceInvalid = false
        )
        {
            return ReloadConfigsAggregatePrivate(
                configName,
                namespaces,
                () => [],
                (aggList, newList) =>
                {
                    foreach (var newItem in newList)
                    {
                        if (!aggList.Contains(newItem))
                        {
                            aggList.Add(newItem);
                        }
                    }
                    return aggList;
                },
                (configName) =>
                    PACSingletons.Instance.ConfigManager.TryGetConfigOrRecreate(
                        configName,
                        null,
                        vanillaDefaultValue,
                        vanillaNamespaceInvalid
                    ),
                (configName) =>
                    (PACSingletons.Instance.ConfigManager.TryGetConfig<List<T>>(
                        configName,
                        null,
                        out var configValue
                    ), configValue)
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool)"/> for <see cref="PACommon.ConfigManagement.AConfigManager.TryGetConfig{TK, TV}(string, string?, out Dictionary{TK, TV}?, Func{string, TK})"/>.
        /// </summary>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool)"/>
        /// <inheritdoc cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigOrRecreate{TK, TV}(string, string?, IDictionary{TK, TV}, Func{TK, string}, Func{string, TK}, bool)"/>
        public static Dictionary<TK, TV> ReloadConfigsAggregate<TK, TV>(
            string configName,
            List<string> namespaces,
            IDictionary<TK, TV> vanillaDefaultValue,
            Func<TK, string> serializeDictionaryKeys,
            Func<string, TK> deserializeDictionaryKeys,
            bool vanillaNamespaceInvalid = false
        )
            where TK : notnull
        {
            return ReloadConfigsAggregatePrivate(
                configName,
                namespaces,
                () => [],
                (aggDict, newDict) =>
                {
                    foreach (var newItem in newDict)
                    {
                        aggDict[newItem.Key] = newItem.Value;
                    }
                    return aggDict;
                },
                (configName) =>
                    PACSingletons.Instance.ConfigManager.TryGetConfigOrRecreate(
                        configName,
                        null,
                        vanillaDefaultValue,
                        serializeDictionaryKeys,
                        deserializeDictionaryKeys,
                        vanillaNamespaceInvalid
                    ),
                (configName) =>
                    (PACSingletons.Instance.ConfigManager.TryGetConfig<TK, TV>(
                        configName,
                        null,
                        out var configValue,
                        deserializeDictionaryKeys
                    ), configValue)
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool)"/> for <see cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigOrRecreate{TK, TV, TVC}(string, string?, IDictionary{TK, TV}, Func{TV, TVC}, Func{TVC, TV}, Func{TK, string}?, Func{string, TK}?, bool)"/>.
        /// </summary>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool)"/>
        /// <inheritdoc cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigOrRecreate{TK, TV, TVC}(string, string?, IDictionary{TK, TV}, Func{TV, TVC}, Func{TVC, TV}, Func{TK, string}?, Func{string, TK}?, bool)"/>
        public static Dictionary<TK, TV> ReloadConfigsAggregate<TK, TV, TVC>(
            string configName,
            List<string> namespaces,
            IDictionary<TK, TV> vanillaDefaultValue,
            Func<TV, TVC> serializeDictionaryValues,
            Func<TVC, TV> deserializeDictionaryValues,
            Func<TK, string>? serializeDictionaryKeys = null,
            Func<string, TK>? deserializeDictionaryKeys = null,
            bool vanillaNamespaceInvalid = false
        )
            where TK : notnull
        {
            return ReloadConfigsAggregatePrivate(
                configName,
                namespaces,
                () => [],
                (aggDict, newDict) =>
                {
                    foreach (var newItem in newDict)
                    {
                        aggDict[newItem.Key] = newItem.Value;
                    }
                    return aggDict;
                },
                (configName) =>
                    PACSingletons.Instance.ConfigManager.TryGetConfigOrRecreate(
                        configName,
                        null,
                        vanillaDefaultValue,
                        serializeDictionaryValues,
                        deserializeDictionaryValues,
                        serializeDictionaryKeys,
                        deserializeDictionaryKeys,
                        vanillaNamespaceInvalid
                    ),
                (configName) =>
                    (PACSingletons.Instance.ConfigManager.TryGetConfig(
                        configName,
                        null,
                        out var configValue,
                        deserializeDictionaryValues,
                        deserializeDictionaryKeys
                    ), configValue)
            );
        }
        #endregion

        #region Config data functions
        /// <summary>
        /// Gets the list of valid config datas.
        /// </summary>
        /// <param name="expectedVersion">The expected version of the config namespaces.<br/>
        /// If null, it doesn't care about the versions.</param>
        public static List<ConfigDataFull> GetValidConfigDatas(string? expectedVersion = Constants.CONFIG_VERSION)
        {
            return Directory.GetDirectories(Constants.CONFIGS_FOLDER_PATH)
                .Select(folder => ConfigDataFull.DeserializeFromFile(Path.GetFileName(folder)))
                .Where(cd => cd is not null && (expectedVersion is null || expectedVersion == cd.configData.Version))
                .Cast<ConfigDataFull>()
                .ToList();
        }

        /// <summary>
        /// Gets the list of valid config folders.
        /// </summary>
        /// <param name="expectedVersion">The expected version of the config namespaces.<br/>
        /// If null, it doesn't care about the versions.</param>
        public static List<string> GetValidNamespaceFolders(string? expectedVersion = Constants.CONFIG_VERSION)
        {
            return GetValidConfigDatas(expectedVersion).Select(cd => cd.folderName).ToList();
        }

        /// <summary>
        /// Gets the loading order data from the loading order file.
        /// </summary>
        /// <param name="validateFolders">Whether to only return loading data for namespaces that actualy exist.</param>
        /// <param name="namespaceExpectedVersion">The expected version to use, if folder validation is enabled.</param>
        public static List<ConfigLoadingData>? GetLoadingOrderData(bool validateFolders = true, string? namespaceExpectedVersion = null)
        {
            JsonDictionary? loadingOrder;
            try
            {
                loadingOrder = PACommon.Tools.LoadJsonFile(
                    Path.Join(Constants.CONFIGS_FOLDER_PATH, Constants.CONFIGS_LOADING_ORDER_FILE_NAME),
                    null,
                    Constants.CONFIG_EXT,
                    false
                );
            }
            catch (Exception ex)
            {
                return null;
            }

            if (loadingOrder is null ||loadingOrder.Keys.Count == 0)
            {
                return [];
            }
            
            var configData = !validateFolders ? null : GetValidConfigDatas(namespaceExpectedVersion);
            var configLoadingDatas = new List<ConfigLoadingData>();
            foreach (var configLoadingDataJson in loadingOrder)
            {
                var configLoadingData = ConfigLoadingData.FromJson(configLoadingDataJson);
                if (
                    configLoadingData is not null &&
                    (configData is null || configData.Any(cd => cd.configData.Namespace == configLoadingData.Namespace))
                )
                {
                    configLoadingDatas.Add(configLoadingData);
                }
            }

            return configLoadingDatas;
        }

        /// <summary>
        /// Sets the loading order data to the loading order file.
        /// </summary>
        /// <param name="loadingData">The loading order data to set.</param>
        public static void SetLoadingOrderData(List<ConfigLoadingData> loadingData)
        {
            var jsonData = new JsonDictionary();
            foreach (var nSpace in loadingData)
            {
                var (namespaceName, namespaceValues) = nSpace.ToJson();
                jsonData.Add(namespaceName, namespaceValues);
            }
            PACommon.Tools.SaveJsonFile(
                jsonData,
                Path.Join(Constants.CONFIGS_FOLDER_PATH, Constants.CONFIGS_LOADING_ORDER_FILE_NAME),
                Constants.CONFIG_EXT,
                true
            );
        }

        /// <summary>
        /// Tries to get the loading order from the file, but if it doesn't exist, it recreates it from the existing namespaces.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if after recreating the loading order, it still can't get it.</exception>
        /// <inheritdoc cref="GetLoadingOrderData(bool, string?)"/>
        public static List<ConfigLoadingData> TryGetLoadingOrderData(
            bool validateNamespaces = true,
            string? namespaceExpectedVersion = null
        )
        {
            var loadingOrder = GetLoadingOrderData(validateNamespaces, namespaceExpectedVersion);
            if (loadingOrder is not null)
            {
                return loadingOrder;
            }

            var namespaceFolders = GetValidNamespaceFolders(namespaceExpectedVersion);
            var nspaces = namespaceFolders.Select(ns => new ConfigLoadingData(ns, true)).ToList();
            SetLoadingOrderData(nspaces);

            return GetLoadingOrderData(validateNamespaces, namespaceExpectedVersion)
                ?? throw new ArgumentNullException("Could not get loading order!");
        }

        /// <summary>
        /// Tries to get the config loading order, from the loading order file or recreates it if it doesn't exist.<br/>
        /// The loaded configs will always contain the vanilla, and will only contain valid configs.
        /// If the loading order that would be returned doesn't reflect the current loading order, it updates the file.
        /// </summary>
        /// <param name="loadingDatas">The returned config loading datas.</param>
        /// <param name="defaultEnabled">Whether newly added configs should be enabled by default.</param>
        /// <param name="defaultEnabledIncludesVanilla">Wether defaultEnabled works the same for the vanilla config, or inverted.</param>
        /// <returns>Whether the vanilla config data file needed to be recreated.</returns>
        /// <exception cref="ArgumentNullException">Thrown if after recreating the loading order, it still can't get it.</exception>
        public static bool TryGetLoadingOrderAndCorrect(
            out List<ConfigLoadingData> loadingDatas,
            bool defaultEnabled = false,
            bool defaultEnabledIncludesVanilla = false
        )
        {
            var configDatas = GetValidConfigDatas(null);

            var vanillaDataRecreated = false;
            if (!configDatas.Any(cd => cd.configData.Namespace == Constants.PA_CONFIGS_NAMESPACE))
            {
                var paNspace = Constants.PA_CONFIGS_NAMESPACE;
                vanillaDataRecreated = true;
                new ConfigData(paNspace, Constants.CONFIG_VERSION).SerializeToFile(paNspace);
                configDatas.Add(new ConfigDataFull(paNspace, new ConfigData(paNspace, "")));
            }

            var loadingOrder = GetLoadingOrderData();
            if (loadingOrder is null)
            {
                var nspaces = configDatas
                    .Where(ns => ns.configData.Namespace != Constants.PA_CONFIGS_NAMESPACE)
                    .Select(ns => new ConfigLoadingData(ns.configData.Namespace, defaultEnabled))
                    .ToList();
                var vanillaConfig = new ConfigLoadingData(
                    configDatas.First(ns => ns.configData.Namespace == Constants.PA_CONFIGS_NAMESPACE).folderName,
                    defaultEnabledIncludesVanilla ? defaultEnabled : !defaultEnabled
                );
                nspaces.Insert(0, vanillaConfig);
                SetLoadingOrderData(nspaces);
                loadingDatas = GetLoadingOrderData() ?? throw new ArgumentNullException("Could not get loading order!");
                return vanillaDataRecreated;
            }

            var changed = false;
            // remove namespaces that don't exist and duplicates
            var loadingOrder2 = new List<ConfigLoadingData>();
            foreach (var loadingConfig in loadingOrder)
            {
                if (
                    configDatas.Any(cd => cd.configData.Namespace == loadingConfig.Namespace) &&
                    !loadingOrder2.Any(lo2 => lo2.Namespace == loadingConfig.Namespace)
                )
                {
                    loadingOrder2.Add(loadingConfig);
                    continue;
                }
                changed = true;
            }
            loadingOrder = loadingOrder2;

            // add missing namespaces
            foreach (var configData in configDatas)
            {
                if (!loadingOrder.Any(ld => ld.Namespace == configData.configData.Namespace))
                {
                    loadingOrder.Add(
                        new ConfigLoadingData(
                            configData.configData.Namespace,
                            configData.configData.Namespace == Constants.PA_CONFIGS_NAMESPACE
                                ? (defaultEnabledIncludesVanilla ? defaultEnabled : !defaultEnabled)
                                : defaultEnabled
                        )
                    );
                    changed = true;
                }
            }

            if (changed)
            {
                SetLoadingOrderData(loadingOrder);
            }

            loadingDatas = loadingOrder;
            return vanillaDataRecreated;
        }
        #endregion
        #endregion

        #region Private functions
        /// <summary>
        /// Reloads the config value from the aggregate of a config file from all avalible namespaces.
        /// </summary>
        /// <typeparam name="T">The type of the config value.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="namespaces">The avalible namespaces, including the default one, in the order of loading.</param>
        /// <param name="getStartingValueFunction">The function to return the staring value of the config value.</param>
        /// <param name="appendConfigFunction">The function to aggregate two config values.</param>
        /// <param name="getConfigVanillaFunction">The GetConfig method to use to get the vanilla config.</param>
        /// <param name="getConfigOtherFunction">The GetConfig method to use to get any non-vanilla config.</param>
        /// <returns>The aggregate of the config file from all avalible namespaces.</returns>
        private static T ReloadConfigsAggregatePrivate<T>(
            string configName,
            List<string> namespaces,
            Func<T> getStartingValueFunction,
            Func<T, T, T> appendConfigFunction,
            Func<string, T> getConfigVanillaFunction,
            Func<string, (bool sucess, T? value)> getConfigOtherFunction
        )
        {
            var aggregateValue = getStartingValueFunction();
            foreach (var nspace in namespaces)
            {
                var configFileSubpath = Path.Join(nspace, configName);
                if (nspace == Constants.PA_CONFIGS_NAMESPACE)
                {
                    var vanillaValue = getConfigVanillaFunction(configFileSubpath);
                    aggregateValue = appendConfigFunction(aggregateValue, vanillaValue);
                }
                else if (PACSingletons.Instance.ConfigManager.ConfigFileExists(configFileSubpath))
                {
                    var (sucess, configValue) = getConfigOtherFunction(configFileSubpath);
                    if (sucess)
                    {
                        aggregateValue = appendConfigFunction(aggregateValue, configValue!);
                    }
                }
            }

            return aggregateValue;
        }

        [GeneratedRegex("^[a-z0-9_]*$")]
        public static partial Regex NamespaceRegex();
        #endregion
    }
}
