using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace ProgressAdventure.ConfigManagement
{
    /// <summary>
    /// Static class for storing functions for config management.
    /// </summary>
    public static partial class ConfigUtils
    {
        private static List<string>? _loadingNamespaces;
        public static ReadOnlyCollection<string>? LoadingNamespaces
        {
            get => _loadingNamespaces?.AsReadOnly();
        }
        public static string? CurrentlyLoadingNamespace { get; private set; }

        #region Public functions
        #region Reload config functions
        /// <summary>
        /// Reloads the config value from the aggregate of a config file from all given namespaces.
        /// </summary>
        /// <typeparam name="T">The type of the config value.</typeparam>
        /// <param name="configName">The name of the config.</param>
        /// <param name="namespaceFolders">The name of all config folders to load from.</param>
        /// <param name="vanillaDefaultValue">The default value of the config for if the vanilla config needs to be recreated.</param>
        /// <param name="getStartingValueFunction">The (empty) starting value of the config value.</param>
        /// <param name="appendConfigFunction">The function to aggregate two config values.</param>
        /// <param name="vanillaNamespaceInvalid">If true, it will always recreate the config file from the vanilla namespace.</param>
        /// <param name="showProgressIndentation">If not null, shows the progress of loading the configs on the console.</param>
        /// <returns>The aggregate of the config file from all given namespaces.</returns>
        public static T ReloadConfigsAggregate<T>(
            string configName,
            List<string> namespaceFolders,
            T vanillaDefaultValue,
            Func<T> getStartingValueFunction,
            Func<T, T, T> appendConfigFunction,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null
        )
        {
            return ReloadConfigsAggregatePrivate(
                configName,
                namespaceFolders,
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
                    ), configValue),
                showProgressIndentation
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool, int?)"/> for list values.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool, int?)"/>
        public static List<T> ReloadConfigsAggregateList<T>(
            string configName,
            List<string> namespaceFolders,
            List<T> vanillaDefaultValue,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null
        )
        {
            return ReloadConfigsAggregatePrivate(
                configName,
                namespaceFolders,
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
                    ), configValue),
                showProgressIndentation
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool, int?)"/> for <see cref="AdvancedEnum{TSelf}"/> values.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="isNamespacedValues">Whether the enum values should be namespaced.</param>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool, int?)"/>
        public static void ReloadConfigsAggregateAdvancedEnum<TEnum>(
            string configName,
            List<string> namespaceFolders,
            List<EnumValue<TEnum>> vanillaDefaultValue,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null,
            bool isNamespacedValues = false
        )
            where TEnum : AdvancedEnum<TEnum>
        {
            var vanillaDefaultActualValue = vanillaDefaultValue.Select(v => v.Name).ToList();
            ReloadConfigsAggregatePrivate(
                configName,
                namespaceFolders,
                () =>
                {
                    AdvancedEnum<TEnum>.Clear();
                    return [];
                },
                (aggList, newList) =>
                {
                    foreach (var newItem in newList ?? [])
                    {
                        AdvancedEnum<TEnum>.TryAddValue(newItem, out _);
                    }
                    return aggList;
                },
                (configName) =>
                {
                    var recreated = false;
                    while (true)
                    {
                        var configValues = PACSingletons.Instance.ConfigManager.TryGetConfigOrRecreate(
                            configName,
                            null,
                            vanillaDefaultActualValue,
                            vanillaNamespaceInvalid
                        );
                        var result = CheckEnumConfigNamespacedValues(configValues, configName, isNamespacedValues);
                        if (result.success)
                        {
                            return result.configValues;
                        }
                        if (recreated)
                        {
                            throw new ArgumentException("Invalid AdvancedEnum values after recreating", nameof(vanillaDefaultValue));
                        }
                        recreated = true;
                    }
                },
                (configName) =>
                {
                    if (
                        !PACSingletons.Instance.ConfigManager.TryGetConfig<List<string>>(
                            configName,
                            null,
                            out var configValue
                        )
                    )
                    {
                        return (false, null);
                    }

                    return CheckEnumConfigNamespacedValues(configValue, configName, isNamespacedValues);
                },
                showProgressIndentation
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool, int?)"/> for <see cref="AdvancedEnum{TSelf}"/> values.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="isNamespacedValues">Whether the enum values should be namespaced.</param>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool, int?)"/>
        public static void ReloadConfigsAggregateAdvancedEnumTree<TEnum>(
            string configName,
            List<string> namespaceFolders,
            List<EnumTreeValue<TEnum>> vanillaDefaultValue,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null,
            bool isNamespacedValues = false
        )
            where TEnum : AdvancedEnumTree<TEnum>
        {
            var vanillaDefaultActualValue = vanillaDefaultValue.Select(v => v.FullName).ToList();
            ReloadConfigsAggregatePrivate(
                configName,
                namespaceFolders,
                () =>
                {
                    AdvancedEnumTree<TEnum>.Clear();
                    return [];
                },
                (aggList, newList) =>
                {
                    foreach (var newItem in newList ?? [])
                    {
                        AdvancedEnumTree<TEnum>.TryAddValue(newItem, out _, true);
                    }
                    return aggList;
                },
                (configName) =>
                {
                    var recreated = false;
                    while (true)
                    {
                        var configValues = PACSingletons.Instance.ConfigManager.TryGetConfigOrRecreate(
                            configName,
                            null,
                            vanillaDefaultActualValue,
                            vanillaNamespaceInvalid
                        );
                        var result = CheckEnumConfigNamespacedValues(configValues, configName, isNamespacedValues);
                        if (result.success)
                        {
                            return result.configValues;
                        }
                        if (recreated)
                        {
                            throw new ArgumentException("Invalid AdvancedEnumTree values after recreating", nameof(vanillaDefaultValue));
                        }
                        recreated = true;
                    }
                },
                (configName) =>
                {
                    if (
                        !PACSingletons.Instance.ConfigManager.TryGetConfig<List<string>>(
                            configName,
                            null,
                            out var configValue
                        )
                    )
                    {
                        return (false, null);
                    }

                    return CheckEnumConfigNamespacedValues(configValue, configName, isNamespacedValues);
                },
                showProgressIndentation
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool, int?)"/> for <see cref="PACommon.ConfigManagement.AConfigManager.TryGetConfig{TK, TV}(string, string?, out Dictionary{TK, TV}?, Func{string, TK})"/>.
        /// </summary>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool, int?)"/>
        /// <inheritdoc cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigOrRecreate{TK, TV}(string, string?, IDictionary{TK, TV}, Func{TK, string}, Func{string, TK}, bool)"/>
        public static Dictionary<TK, TV> ReloadConfigsAggregateDict<TK, TV>(
            string configName,
            List<string> namespaceFolders,
            Dictionary<TK, TV> vanillaDefaultValue,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null
        )
            where TK : notnull
        {
            return ReloadConfigsAggregatePrivate(
                configName,
                namespaceFolders,
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
                        vanillaNamespaceInvalid
                    ),
                (configName) =>
                    (PACSingletons.Instance.ConfigManager.TryGetConfig<Dictionary<TK, TV>>(
                        configName,
                        null,
                        out var configValue
                    ), configValue),
                showProgressIndentation
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool, int?)"/> for <see cref="PACommon.ConfigManagement.AConfigManager.TryGetConfig{TK, TV}(string, string?, out Dictionary{TK, TV}?, Func{string, TK})"/>.
        /// </summary>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool, int?)"/>
        /// <inheritdoc cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigOrRecreate{TK, TV}(string, string?, IDictionary{TK, TV}, Func{TK, string}, Func{string, TK}, bool)"/>
        public static Dictionary<TK, TV> ReloadConfigsAggregateDict<TK, TV>(
            string configName,
            List<string> namespaceFolders,
            IDictionary<TK, TV> vanillaDefaultValue,
            Func<TK, string> serializeDictionaryKeys,
            Func<string, TK> deserializeDictionaryKeys,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null
        )
            where TK : notnull
        {
            return ReloadConfigsAggregatePrivate(
                configName,
                namespaceFolders,
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
                    ), configValue),
                showProgressIndentation
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool, int?)"/> for <see cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigOrRecreate{TK, TV, TVC}(string, string?, IDictionary{TK, TV}, Func{TV, TVC}, Func{TVC, TV}, Func{TK, string}?, Func{string, TK}?, bool)"/>.
        /// </summary>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{string}, T, Func{T}, Func{T, T, T}, bool, int?)"/>
        /// <inheritdoc cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigOrRecreate{TK, TV, TVC}(string, string?, IDictionary{TK, TV}, Func{TV, TVC}, Func{TVC, TV}, Func{TK, string}?, Func{string, TK}?, bool)"/>
        public static Dictionary<TK, TV> ReloadConfigsAggregateDict<TK, TV, TVC>(
            string configName,
            List<string> namespaceFolders,
            IDictionary<TK, TV> vanillaDefaultValue,
            Func<TV, TVC> serializeDictionaryValues,
            Func<TVC, TV> deserializeDictionaryValues,
            Func<TK, string>? serializeDictionaryKeys = null,
            Func<string, TK>? deserializeDictionaryKeys = null,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null
        )
            where TK : notnull
        {
            return ReloadConfigsAggregatePrivate(
                configName,
                namespaceFolders,
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
                    ), configValue),
                showProgressIndentation
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
            PACommon.Tools.RecreateFolder(Constants.CONFIGS_FOLDER_PATH, "configs");
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
            PACommon.Tools.RecreateFolder(Constants.CONFIGS_FOLDER_PATH, "configs");
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
            PACommon.Tools.RecreateFolder(Constants.CONFIGS_FOLDER_PATH, "configs");
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
            if (!configDatas.Any(cd => cd.configData.Namespace == Constants.VANILLA_CONFIGS_NAMESPACE))
            {
                var paNspace = Constants.VANILLA_CONFIGS_NAMESPACE;
                vanillaDataRecreated = true;
                new ConfigData(paNspace, Constants.CONFIG_VERSION).SerializeToFile(paNspace);
                configDatas.Add(new ConfigDataFull(paNspace, new ConfigData(paNspace, "")));
            }

            var loadingOrder = GetLoadingOrderData();
            if (loadingOrder is null)
            {
                var nspaces = configDatas
                    .Where(ns => ns.configData.Namespace != Constants.VANILLA_CONFIGS_NAMESPACE)
                    .Select(ns => new ConfigLoadingData(ns.configData.Namespace, defaultEnabled))
                    .ToList();
                var vanillaConfig = new ConfigLoadingData(
                    configDatas.First(ns => ns.configData.Namespace == Constants.VANILLA_CONFIGS_NAMESPACE).folderName,
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
                            configData.configData.Namespace == Constants.VANILLA_CONFIGS_NAMESPACE
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

        /// <summary>
        /// Tries to correct a string to be namespaced using the currently loaded namespace(s).
        /// </summary>
        /// <param name="str">The maybe namespaced string.</param>
        /// <param name="namespacedString">The namespaced string, or the same if the string was null or whitespace.</param>
        /// <param name="logChange">Whether to log if the namespaced string is changed to be valid.</param>
        /// <returns>If the string was able to be correctly namespaced.</returns>
        public static bool TryGetNamepsacedString(string str, out string namespacedString, bool logChange = true)
        {
            namespacedString = str;
            if (
                LoadingNamespaces is null ||
                CurrentlyLoadingNamespace is null ||
                string.IsNullOrWhiteSpace(str)
            )
            {
                return false;
            }

            var nsSepIndex = str.IndexOf(Constants.NAMESPACE_SEPARATOR_CHAR);
            var defaultNamepsace = Constants.DEFAULT_NAMESPACE_IS_CURRENT_NAMESPACE
                ? CurrentlyLoadingNamespace
                : Constants.VANILLA_CONFIGS_NAMESPACE;
            if (nsSepIndex == -1)
            {
                namespacedString = $"{defaultNamepsace}{Constants.NAMESPACE_SEPARATOR_CHAR}{str}";
                if (logChange)
                {
                    PACSingletons.Instance.Logger.Log(
                        "Namespaced string created",
                        $"while loading from: {CurrentlyLoadingNamespace}, \"{str}\" -> \"{namespacedString}\"",
                        LogSeverity.DEBUG
                    );
                }
                return true;
            }
            else if (
                nsSepIndex >= str.Length - 1 ||
                string.IsNullOrWhiteSpace(str[(nsSepIndex + 1)..])
            )
            {
                return false;
            }

            var nspace = str[..nsSepIndex];
            if (
                string.IsNullOrWhiteSpace(nspace) ||
                !NamespaceRegex().IsMatch(nspace) ||
                !LoadingNamespaces.Contains(nspace)
            )
            {
                namespacedString = defaultNamepsace + str[nsSepIndex..];
                if (logChange)
                {
                    PACSingletons.Instance.Logger.Log(
                        "Namespaced string changed",
                        $"while loading from: {CurrentlyLoadingNamespace}, \"{str}\" -> \"{namespacedString}\"",
                        LogSeverity.WARN
                    );
                }
            }
            return true;
        }

        /// <summary>
        /// Tries to correct a string to be namespaced using the currently loaded namespace(s).
        /// </summary>
        /// <param name="str">The maybe namespaced string.</param>
        /// <param name="logChange">Whether to log if the namespaced string is changed to be valid.</param>
        /// <returns>The namespaced string, or the same string if namespacing failed.</returns>
        public static string GetNamepsacedString(string str, bool logChange = true)
        {
            TryGetNamepsacedString(str, out var nsString, logChange);
            return nsString;
        }

        /// <summary>
        /// Adds a namespace to a string.
        /// </summary>
        /// <param name="str">The original string.</param>
        /// <param name="nspace">The namespace to add.</param>
        /// <returns>The namespaced string.</returns>
        public static string MakeNamespacedString(string str, string nspace = Constants.VANILLA_CONFIGS_NAMESPACE)
        {
            return $"{nspace}{Constants.NAMESPACE_SEPARATOR_CHAR}{str}";
        }
        #endregion

        #region Private functions

        private static (bool success, List<string>? configValues) CheckEnumConfigNamespacedValues(
            List<string> configValues,
            string configName,
            bool isNamespaceEnum
        )
        {
            if (!isNamespaceEnum)
            {
                return (configValues.All(cv => !string.IsNullOrWhiteSpace(cv)), configValues);
            }

            var namespacedValues = new List<string>();
            foreach (var value in configValues)
            {
                if (!TryGetNamepsacedString(value, out var namespacedValue))
                {
                    PACSingletons.Instance.Logger.Log(
                        "Invalid namespaced enum value in config",
                        $"while loading \"{configName}\" from: \"{CurrentlyLoadingNamespace}\", value: \"{value}\""
                    );
                    return (false, null);
                }
                namespacedValues.Add(namespacedValue);
            }
            return (true, namespacedValues);
        }

        private static void LogConfigLoadingBegin(string configPath, string configFolder, int? showProgressIndentation)
        {
            var fullConfigPath = Path.GetRelativePath(PACommon.Constants.ROOT_FOLDER, PACSingletons.Instance.ConfigManager.GetConfigFilePath(configPath));
            PACSingletons.Instance.Logger.Log(
                "Reloading from config file",
                $"\"{fullConfigPath}\"",
                LogSeverity.DEBUG
            );

            if (showProgressIndentation is not null)
            {
                Console.Write(new string(' ', (int)showProgressIndentation * 4) + configFolder);
            }
        }

        private static void LogConfigLoadingEnd(bool success, int? showProgressIndentation)
        {
            if (!success)
            {
                PACSingletons.Instance.Logger.Log(
                    "Reloading from config file failed",
                    $"refer to above log",
                    LogSeverity.DEBUG
                );
            }

            if (showProgressIndentation is not null)
            {
                Console.WriteLine(success ? "" : ": FALIED!");
            }
        }

        /// <summary>
        /// Reloads the config value from the aggregate of a config file from all avalible namespaces.
        /// </summary>
        /// <typeparam name="T">The type of the config value.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="namespaceFolders">The avalible config folders, including the default one, in the order of loading.</param>
        /// <param name="getStartingValueFunction">The function to return the staring value of the config value.</param>
        /// <param name="appendConfigFunction">The function to aggregate two config values.</param>
        /// <param name="getConfigVanillaFunction">The GetConfig method to use to get the vanilla config.</param>
        /// <param name="getConfigOtherFunction">The GetConfig method to use to get any non-vanilla config.</param>
        /// <returns>The aggregate of the config file from all avalible namespaces.</returns>
        private static T ReloadConfigsAggregatePrivate<T>(
            string configName,
            List<string> namespaceFolders,
            Func<T> getStartingValueFunction,
            Func<T, T, T> appendConfigFunction,
            Func<string, T> getConfigVanillaFunction,
            Func<string, (bool sucess, T? value)> getConfigOtherFunction,
            int? showProgressIndentation
        )
        {
            var configNameFull = $"{configName}.{Constants.CONFIG_EXT}";
            PACSingletons.Instance.Logger.Log(
                "Loading config file",
                $"\"{configNameFull}\"",
                LogSeverity.DEBUG
            );

            if (showProgressIndentation is not null)
            {
                Console.WriteLine(new string(' ', (int)showProgressIndentation * 4) + $"Loading file \"{Path.GetFileName(configNameFull)}\" from config:");
            }
            showProgressIndentation = showProgressIndentation + 1 ?? null;

            _loadingNamespaces = [];
            var aggregateValue = getStartingValueFunction();
            foreach (var folder in namespaceFolders)
            {
                _loadingNamespaces.Add(folder);
                CurrentlyLoadingNamespace = folder;
                var configFileSubpath = Path.Join(folder, configName);
                if (folder == Constants.VANILLA_CONFIGS_NAMESPACE)
                {
                    LogConfigLoadingBegin(configFileSubpath, folder, showProgressIndentation);
                    var vanillaValue = getConfigVanillaFunction(configFileSubpath);
                    aggregateValue = appendConfigFunction(aggregateValue, vanillaValue);
                    LogConfigLoadingEnd(true, showProgressIndentation);
                }
                else if (PACSingletons.Instance.ConfigManager.ConfigFileExists(configFileSubpath))
                {
                    LogConfigLoadingBegin(configFileSubpath, folder, showProgressIndentation);
                    var (sucess, configValue) = getConfigOtherFunction(configFileSubpath);
                    LogConfigLoadingEnd(sucess, showProgressIndentation);
                    if (sucess)
                    {
                        aggregateValue = appendConfigFunction(aggregateValue, configValue!);
                    }
                }
                CurrentlyLoadingNamespace = null;
            }
            _loadingNamespaces = null;

            return aggregateValue;
        }

        [GeneratedRegex("^[a-z0-9_]*$")]
        public static partial Regex NamespaceRegex();
        #endregion
    }
}
