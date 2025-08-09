using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;

namespace ProgressAdventure.ConfigManagement
{
    /// <summary>
    /// Static class for storing functions for config management.
    /// </summary>
    public static partial class ConfigUtils
    {
        /// <summary>
        /// The currently loading namespaces.
        /// </summary>
        private static List<string>? _loadingNamespaces;

        /// <summary>
        /// <inheritdoc cref="_loadingNamespaces" path="//summary"/>
        /// </summary>
        public static ReadOnlyCollection<string>? LoadingNamespaces => _loadingNamespaces?.AsReadOnly();

        /// <summary>
        /// The name of the currently loading namespace.
        /// </summary>
        public static string? CurrentlyLoadingNamespace { get; private set; }

        /// <summary>
        /// The list of currently enabled configs. (needs to be manualy updated using <see cref="UpdateEnabledConfigDatas(out bool)"/>)
        /// </summary>
        private static List<ConfigData>? _enabledConfigDatas;

        /// <summary>
        /// <inheritdoc cref="_enabledConfigDatas" path="//summary"/>
        /// </summary>
        public static ReadOnlyCollection<ConfigData> EnabledConfigDatas => _enabledConfigDatas?.AsReadOnly() ?? UpdateEnabledConfigDatas(out _);

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
        /// <param name="comment">The comment to write before the recreated json data.</param>
        /// <returns>The aggregate of the config file from all given namespaces.</returns>
        public static T ReloadConfigsAggregate<T>(
            string configName,
            List<(string folderName, string namespaceName)> namespaceFolders,
            T vanillaDefaultValue,
            Func<T> getStartingValueFunction,
            Func<T, T, T> appendConfigFunction,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null,
            string? comment = null
        )
        {
            return ReloadConfigsAggregatePrivate(
                configName,
                namespaceFolders,
                getStartingValueFunction,
                (aggregate, newValue, removeValue) => appendConfigFunction(aggregate, newValue),
                (configName) =>
                    (PACSingletons.Instance.ConfigManager.TryGetConfigOrRecreate(
                        configName,
                        null,
                        vanillaDefaultValue,
                        vanillaNamespaceInvalid,
                        comment
                    ), default!),
                (configName) =>
                    (PACSingletons.Instance.ConfigManager.TryGetConfig<T>(
                        configName,
                        null,
                        out var configValue
                    ), configValue, default),
                showProgressIndentation
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{ValueTuple{string, string}}, T, Func{T}, Func{T, T, T}, bool, int?, string?)"/> for list values.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{ValueTuple{string, string}}, T, Func{T}, Func{T, T, T}, bool, int?, string?)"/>
        public static List<T> ReloadConfigsAggregateList<T>(
            string configName,
            List<(string folderName, string namespaceName)> namespaceFolders,
            List<T> vanillaDefaultValue,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null,
            string? comment = null
        )
        {
            var valueTypeIsNullable = Nullable.GetUnderlyingType(typeof(T)) is not null;
            return ReloadConfigsAggregatePrivate(
                configName,
                namespaceFolders,
                () => [],
                (aggList, newList, removeList) =>
                {
                    if (!valueTypeIsNullable && newList.Any(ni => ni is null))
                    {
                        PACSingletons.Instance.Logger.Log("Config error", $"config list value was null instead of {typeof(T).FullName}", LogSeverity.WARN);
                        return aggList;
                    }

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
                    (PACSingletons.Instance.ConfigManager.TryGetConfigOrRecreate(
                        configName,
                        null,
                        vanillaDefaultValue,
                        vanillaNamespaceInvalid,
                        comment
                    ), []),
                (configName) =>
                    (PACSingletons.Instance.ConfigManager.TryGetConfig<List<T>>(
                        configName,
                        null,
                        out var configValue
                    ), configValue, new List<T>()),
                showProgressIndentation
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{ValueTuple{string, string}}, T, Func{T}, Func{T, T, T}, bool, int?, string?)"/> for <see cref="AdvancedEnum{TSelf}"/> values.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="isNamespacedValues">Whether the enum values should be namespaced.</param>
        /// <param name="removeValueBeggining">The string that should be at the beggining of a value, to signify that that value should be removed.</param>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{ValueTuple{string, string}}, T, Func{T}, Func{T, T, T}, bool, int?, string?)"/>
        public static void ReloadConfigsAggregateAdvancedEnum<TEnum>(
            string configName,
            List<(string folderName, string namespaceName)> namespaceFolders,
            List<EnumValue<TEnum>> vanillaDefaultValue,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null,
            bool isNamespacedValues = false,
            string removeValueBeggining = Constants.CONFIG_REMOVE_BEGGINING,
            string? comment = null
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
                (aggList, newList, removeList) =>
                {
                    foreach (var newItem in newList)
                    {
                        AdvancedEnum<TEnum>.TryAddValue(newItem, out _);
                    }
                    foreach (var removeItem in removeList)
                    {
                        AdvancedEnum<TEnum>.TryRemoveValue(removeItem);
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
                            vanillaNamespaceInvalid,
                            comment
                        );
                        var (success, addedValues, removedValues) = CheckEnumConfigNamespacedValues(configValues, configName, isNamespacedValues, removeValueBeggining);
                        if (success)
                        {
                            return (addedValues!, removedValues!);
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
                        return (false, null, null);
                    }

                    return CheckEnumConfigNamespacedValues(configValue, configName, isNamespacedValues, removeValueBeggining);
                },
                showProgressIndentation
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{ValueTuple{string, string}}, T, Func{T}, Func{T, T, T}, bool, int?, string?)"/> for <see cref="AdvancedEnum{TSelf}"/> values.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="isNamespacedValues">Whether the enum values should be namespaced.</param>
        /// <param name="removeValueBeggining">The string that should be at the beggining of a value, to signify that that value should be removed.</param>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{ValueTuple{string, string}}, T, Func{T}, Func{T, T, T}, bool, int?, string?)"/>
        public static void ReloadConfigsAggregateAdvancedEnumTree<TEnum>(
            string configName,
            List<(string folderName, string namespaceName)> namespaceFolders,
            List<EnumTreeValue<TEnum>> vanillaDefaultValue,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null,
            bool isNamespacedValues = false,
            string removeValueBeggining = Constants.CONFIG_REMOVE_BEGGINING,
            string? comment = null
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
                (aggList, newList, removeList) =>
                {
                    foreach (var newItem in newList)
                    {
                        AdvancedEnumTree<TEnum>.TryAddValue(newItem, out _, true);
                    }
                    foreach (var removeItem in removeList)
                    {
                        AdvancedEnumTree<TEnum>.TryRemoveValue(removeItem);
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
                            vanillaNamespaceInvalid,
                            comment
                        );
                        var (success, addedValues, removedValues) = CheckEnumConfigNamespacedValues(configValues, configName, isNamespacedValues, removeValueBeggining);
                        if (success)
                        {
                            return (addedValues!, removedValues!);
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
                        return (false, null, null);
                    }

                    return CheckEnumConfigNamespacedValues(configValue, configName, isNamespacedValues, removeValueBeggining);
                },
                showProgressIndentation
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{ValueTuple{string, string}}, T, Func{T}, Func{T, T, T}, bool, int?, string?)"/> for <see cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigDict{TK, TV}(string, string?, out Dictionary{TK, TV}?, Func{string, TK})"/>.
        /// </summary>
        /// <param name="removeKeyBeggining">The string that should be at the beggining of a key, to signify that that key should be removed.</param>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{ValueTuple{string, string}}, T, Func{T}, Func{T, T, T}, bool, int?, string?)"/>
        /// <inheritdoc cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigOrRecreateDict{TK, TV}(string, string?, IDictionary{TK, TV}, Func{TK, string}, Func{string, TK}, bool, string?)"/>
        public static Dictionary<string, TV> ReloadConfigsAggregateDict<TV>(
            string configName,
            List<(string folderName, string namespaceName)> namespaceFolders,
            Dictionary<string, TV> vanillaDefaultValue,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null,
            string removeKeyBeggining = Constants.CONFIG_REMOVE_BEGGINING,
            string? comment = null
        )
        {
            return ReloadConfigsAggregateDictPrivate(
                configName,
                namespaceFolders,
                configName =>
                    PACSingletons.Instance.ConfigManager.TryGetConfigOrRecreate(
                        configName,
                        null,
                        vanillaDefaultValue,
                        vanillaNamespaceInvalid,
                        comment
                    ),
                (configName, deserializeKeysFunction) =>
                    (PACSingletons.Instance.ConfigManager.TryGetConfigDict<string, TV>(
                        configName,
                        null,
                        out var configValue,
                        deserializeKeysFunction
                    ), configValue),
                null,
                showProgressIndentation,
                removeKeyBeggining
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{ValueTuple{string, string}}, T, Func{T}, Func{T, T, T}, bool, int?, string?)"/> for <see cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigDict{TK, TV}(string, string?, out Dictionary{TK, TV}?, Func{string, TK})"/>.
        /// </summary>
        /// <param name="removeKeyBeggining">The string that should be at the beggining of a key, to signify that that key should be removed.</param>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{ValueTuple{string, string}}, T, Func{T}, Func{T, T, T}, bool, int?, string?)"/>
        /// <inheritdoc cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigOrRecreateDict{TK, TV}(string, string?, IDictionary{TK, TV}, Func{TK, string}, Func{string, TK}, bool, string?)"/>
        public static Dictionary<TK, TV> ReloadConfigsAggregateDict<TK, TV>(
            string configName,
            List<(string folderName, string namespaceName)> namespaceFolders,
            IDictionary<TK, TV> vanillaDefaultValue,
            Func<TK, string> serializeDictionaryKeys,
            Func<string, TK> deserializeDictionaryKeys,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null,
            string removeKeyBeggining = Constants.CONFIG_REMOVE_BEGGINING,
            string? comment = null
        )
            where TK : notnull
        {
            return ReloadConfigsAggregateDictPrivate(
                configName,
                namespaceFolders,
                configName =>
                    PACSingletons.Instance.ConfigManager.TryGetConfigOrRecreateDict(
                        configName,
                        null,
                        vanillaDefaultValue,
                        serializeDictionaryKeys,
                        deserializeDictionaryKeys,
                        vanillaNamespaceInvalid,
                        comment
                    ),
                (configName, deserializeKeysFunction) =>
                    (PACSingletons.Instance.ConfigManager.TryGetConfigDict<TK, TV>(
                        configName,
                        null,
                        out var configValue,
                        deserializeKeysFunction
                    ), configValue),
                deserializeDictionaryKeys,
                showProgressIndentation,
                removeKeyBeggining
            );
        }

        /// <summary>
        /// <see cref="ReloadConfigsAggregate{T}(string, List{ValueTuple{string, string}}, T, Func{T}, Func{T, T, T}, bool, int?, string?)"/> for <see cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigOrRecreateDict{TK, TV, TVC}(string, string?, IDictionary{TK, TV}, Func{TV, TVC}, Func{TVC, TV}, Func{TK, string}?, Func{string, TK}?, bool, string?)"/>.
        /// </summary>
        /// <param name="removeKeyBeggining">The string that should be at the beggining of a key, to signify that that key should be removed.</param>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{ValueTuple{string, string}}, T, Func{T}, Func{T, T, T}, bool, int?, string?)"/>
        /// <inheritdoc cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigOrRecreateDict{TK, TV, TVC}(string, string?, IDictionary{TK, TV}, Func{TV, TVC}, Func{TVC, TV}, Func{TK, string}?, Func{string, TK}?, bool, string?)"/>
        public static Dictionary<TK, TV> ReloadConfigsAggregateDict<TK, TV, TVC>(
            string configName,
            List<(string folderName, string namespaceName)> namespaceFolders,
            IDictionary<TK, TV> vanillaDefaultValue,
            Func<TV, TVC> serializeDictionaryValues,
            Func<TVC, TV> deserializeDictionaryValues,
            Func<TK, string>? serializeDictionaryKeys = null,
            Func<string, TK>? deserializeDictionaryKeys = null,
            bool vanillaNamespaceInvalid = false,
            int? showProgressIndentation = null,
            string removeKeyBeggining = Constants.CONFIG_REMOVE_BEGGINING,
            string? comment = null
        )
            where TK : notnull
        {
            return ReloadConfigsAggregateDictPrivate(
                configName,
                namespaceFolders,
                configName =>
                    PACSingletons.Instance.ConfigManager.TryGetConfigOrRecreateDict(
                        configName,
                        null,
                        vanillaDefaultValue,
                        serializeDictionaryValues,
                        deserializeDictionaryValues,
                        serializeDictionaryKeys,
                        deserializeDictionaryKeys,
                        vanillaNamespaceInvalid,
                        comment
                    ),
                (configName, deserializeKeysFunction) =>
                    (PACSingletons.Instance.ConfigManager.TryGetConfigDict(
                        configName,
                        null,
                        out var configValue,
                        deserializeDictionaryValues,
                        deserializeKeysFunction
                    ), configValue),
                deserializeDictionaryKeys,
                showProgressIndentation,
                removeKeyBeggining
            );
        }
        #endregion

        #region Config data functions
        /// <summary>
        /// Gets the list of valid config datas.
        /// </summary>
        /// <param name="expectedVersion">The expected version of the config namespaces.<br/>
        /// If null, it doesn't care about the versions.</param>
        public static List<ConfigData> GetValidConfigDatas(string? expectedVersion = Constants.CONFIG_FORMAT_VERSION)
        {
            PACommon.Tools.RecreateFolder(Constants.CONFIGS_FOLDER_PATH, "configs");
            return [.. Directory.GetDirectories(Constants.CONFIGS_FOLDER_PATH)
                .Select(folder => ConfigData.DeserializeFromFile(Path.GetFileName(folder)))
                .Where(cd => cd is not null && (expectedVersion is null || expectedVersion == cd.Format))
                .Cast<ConfigData>()];
        }

        /// <summary>
        /// Gets the list of valid config folders.
        /// </summary>
        /// <param name="expectedVersion">The expected version of the config namespaces.<br/>
        /// If null, it doesn't care about the versions.</param>
        public static List<string> GetValidNamespaceFolders(string? expectedVersion = Constants.CONFIG_FORMAT_VERSION)
        {
            return [.. GetValidConfigDatas(expectedVersion).Select(cd => cd.FolderName)];
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
                var success = ConfigLoadingData.FromJson(configLoadingDataJson, out var configLoadingData);
                if (
                    configLoadingData is not null &&
                    (configData is null || configData.Any(cd => cd.Namespace == configLoadingData.Namespace))
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
        /// <returns>If the vanilla config is valid/needed to be recreated.</returns>
        /// <exception cref="ArgumentNullException">Thrown if after recreating the loading order, it still can't get it.</exception>
        public static bool TryGetLoadingOrderAndCorrect(
            out List<ConfigLoadingData> loadingDatas,
            bool defaultEnabled = false,
            bool defaultEnabledIncludesVanilla = false
        )
        {
            var configDatas = GetValidConfigDatas(null);

            var vanillaIsInvalid = false;
            var vanillaConfigData = configDatas.FirstOrDefault(cd => cd.Namespace == Constants.VANILLA_CONFIGS_NAMESPACE);
            if (
                vanillaConfigData?.Format != Constants.CONFIG_FORMAT_VERSION ||
                vanillaConfigData?.Version != Constants.VANILLA_CONFIG_VERSION
            )
            {
                var paNspace = Constants.VANILLA_CONFIGS_NAMESPACE;
                vanillaIsInvalid = true;
                new ConfigData(paNspace, paNspace, Constants.CONFIG_FORMAT_VERSION, Constants.VANILLA_CONFIG_VERSION).SerializeToFile();
                if (vanillaConfigData is null)
                {
                    configDatas.Add(new ConfigData(paNspace, paNspace, "", ""));
                }
            }

            var loadingOrder = GetLoadingOrderData();
            if (loadingOrder is null)
            {
                var nspaces = configDatas
                    .Where(ns => ns.Namespace != Constants.VANILLA_CONFIGS_NAMESPACE)
                    .Select(ns => new ConfigLoadingData(ns.Namespace, defaultEnabled))
                    .ToList();
                var vanillaConfig = new ConfigLoadingData(
                    configDatas.First(ns => ns.Namespace == Constants.VANILLA_CONFIGS_NAMESPACE).FolderName,
                    defaultEnabledIncludesVanilla ? defaultEnabled : !defaultEnabled
                );
                nspaces.Insert(0, vanillaConfig);
                SetLoadingOrderData(nspaces);
                loadingDatas = GetLoadingOrderData() ?? throw new ArgumentNullException("Could not get loading order!");
                return vanillaIsInvalid;
            }

            var changed = false;
            // remove namespaces that don't exist and duplicates
            var loadingOrder2 = new List<ConfigLoadingData>();
            foreach (var loadingConfig in loadingOrder)
            {
                if (
                    configDatas.Any(cd => cd.Namespace == loadingConfig.Namespace) &&
                    loadingOrder2.All(lo2 => lo2.Namespace != loadingConfig.Namespace))
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
                if (loadingOrder.All(ld => ld.Namespace != configData.Namespace))
                {
                    loadingOrder.Add(
                        new ConfigLoadingData(
                            configData.Namespace,
                            configData.Namespace == Constants.VANILLA_CONFIGS_NAMESPACE
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
            return vanillaIsInvalid;
        }

        /// <summary>
        /// Gets all dependency errors with the currently enabled configs.
        /// </summary>
        /// <param name="loadingOrder">The current loading order.</param>
        /// <param name="configDatas">The config datas.</param>
        /// <returns>A dictionary, where the keys are the incorrect config namespace names and the value signals the problem:<br/>
        /// - If the list is null, it means that there is no config data associated with this config.<br/>
        /// - Otherwise, each element of the list is an invalid depedency. The tape depends of the invalidType:<br/>
        ///     - -1: missing namespace<br/>
        ///     - 0: disabled namespace<br/>
        ///     - 1: dependency loaded before dependant</returns>
        public static Dictionary<string, List<(string dependency, int invalidType)>?> ValidateConfigDependencies(
            List<ConfigLoadingData> loadingOrder,
            List<ConfigData> configDatas
        )
        {
            var invalids = new Dictionary<string, List<(string dependency, int invalidType)>?>();
            for (var x = 0; x < loadingOrder.Count; x++)
            {
                var loadedConfig = loadingOrder[x];
                var configData = configDatas.FirstOrDefault(c => c.Namespace == loadedConfig.Namespace);
                if (configData is null)
                {
                    invalids.Add(loadedConfig.Namespace, null);
                    continue;
                }
                if (
                    !loadedConfig.Enabled ||
                    configData.Dependencies.Count == 0
                )
                {
                    continue;
                }

                var badDepends = new List<(string dependency, int invalidType)>();
                foreach (var dependency in configData.Dependencies)
                {
                    if (loadingOrder.FirstOrDefault(lo => lo.Namespace == dependency) is not ConfigLoadingData dependencyLoading)
                    {
                        badDepends.Add((dependency, -1));
                    }
                    else if (!dependencyLoading.Enabled)
                    {
                        badDepends.Add((dependency, 0));
                    }
                    else if (loadingOrder.IndexOf(loadedConfig) < loadingOrder.IndexOf(dependencyLoading))
                    {
                        badDepends.Add((dependency, 1));
                    }
                }
                if (badDepends.Count > 0)
                {
                    invalids.Add(configData.Namespace, badDepends);
                }
            }
            return invalids;
        }

        /// <summary>
        /// Gets and updates the list of currently enabled config datas.
        /// </summary>
        /// <param name="vanillaInvalid">If the vanilla config is valid/needed to be recreated.</param>
        public static ReadOnlyCollection<ConfigData> UpdateEnabledConfigDatas(out bool vanillaInvalid)
        {
            vanillaInvalid = TryGetLoadingOrderAndCorrect(out var loadingOrder);
            var configDatas = GetValidConfigDatas(null);
            var datas = loadingOrder
                .Where(lo => lo.Enabled)
                .Select(lo => configDatas.First(cd => cd.Namespace == lo.Namespace))
                .ToList();
            _enabledConfigDatas = datas;
            return datas.AsReadOnly();
        }

        /// <summary>
        /// Gets the difference between a list of <see cref="LoadedConfigData"/>s and the currently loaded configs.
        /// </summary>
        public static ConfigDiff GetConfigDiff(IList<LoadedConfigData>? lastLoadedConfigs)
        {
            var currentlyLoadedConfigs = EnabledConfigDatas;
            if (lastLoadedConfigs is null || lastLoadedConfigs.Count == 0)
            {
                return new ConfigDiff
                {
                    added = [.. currentlyLoadedConfigs],
                    removed = [],
                    versionChanged = [],
                    orderChanged = [],
                };
            }

            if (currentlyLoadedConfigs.Count == 0)
            {
                return new ConfigDiff
                {
                    added = [],
                    removed = [.. lastLoadedConfigs],
                    versionChanged = [],
                    orderChanged = [],
                };
            }

            var added = currentlyLoadedConfigs.ToList();
            var removed = new List<LoadedConfigData>();
            var versionChange = new List<(ConfigData config, string oldVersion)>();
            var orderChange = new List<(ConfigData config, int oldIndex, int newIndex)>();

            var prewIndex = -1;
            var index = -1;
            var prewConfig = currentlyLoadedConfigs[0];
            foreach (var configData in lastLoadedConfigs)
            {
                index++;
                var foundConfig = currentlyLoadedConfigs.FirstOrDefault(cc => configData.Namespace == cc.Namespace);
                if (foundConfig is null)
                {
                    removed.Add(configData);
                    continue;
                }

                added.Remove(foundConfig);
                if (foundConfig.Version != configData.Version)
                {
                    versionChange.Add((foundConfig, configData.Version));
                }

                var newIndex = currentlyLoadedConfigs.IndexOf(foundConfig);
                if (prewIndex >= newIndex)
                {
                    orderChange.Add((prewConfig, index, newIndex));
                }

                prewIndex = newIndex;
                prewConfig = foundConfig;
            }

            return new ConfigDiff
            {
                added = [.. added],
                removed = [.. removed],
                versionChanged = [.. versionChange],
                orderChanged = [.. orderChange],
            };
        }
        #endregion

        #region Namespacing functions
        /// <summary>
        /// Tries to correct a string to be namespaced using the given namespace.
        /// </summary>
        /// <param name="str">The maybe namespaced string.</param>
        /// <param name="defaultNamespace">The namespace to write if there is no namespace.</param>
        /// <param name="namespacedString">The namespaced string, or the same if the string was null or whitespace.</param>
        /// <param name="isConfigLoading">Whether to get the list of valid namespaces from config loading, or currently loaded namespaces.</param>
        /// <param name="logChange">Whether to log if the namespaced string is changed to be valid.</param>
        /// <param name="throwOnInvalidNamespace">Whether to throw and exception if the namespaced string is invalid.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="throwOnInvalidNamespace"/> is true and the namespaced string is invalid.</exception>
        /// <returns>If the string was able to be correctly namespaced.</returns>
        private static bool TryGetNamespacedStringPrivate(
            string str,
            string defaultNamespace,
            out string namespacedString,
            bool isConfigLoading,
            bool logChange = true,
            bool throwOnInvalidNamespace = false
        )
        {
            namespacedString = str;
            if (
                (isConfigLoading && LoadingNamespaces is null) ||
                string.IsNullOrWhiteSpace(defaultNamespace) ||
                string.IsNullOrWhiteSpace(str)
            )
            {
                if (throwOnInvalidNamespace)
                {
                    throw new ArgumentException("No namespaced string or default namespace is null.", nameof(str));
                }
                return false;
            }

            var nsSepIndex = str.IndexOf(Constants.NAMESPACE_SEPARATOR_CHAR);
            if (nsSepIndex == -1)
            {
                namespacedString = $"{defaultNamespace}{Constants.NAMESPACE_SEPARATOR_CHAR}{str}";
                if (logChange)
                {
                    PACSingletons.Instance.Logger.Log(
                        "Namespaced string created",
                        $"{(isConfigLoading ? $"while loading from: {defaultNamespace}, " : "")}\"{str}\" -> \"{namespacedString}\"",
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
                if (throwOnInvalidNamespace)
                {
                    throw new ArgumentException("No text after namespace in namespaced string.", nameof(str));
                }
                return false;
            }

            var nspace = str[..nsSepIndex];
            if (!string.IsNullOrWhiteSpace(nspace) &&
                NamespaceRegex().IsMatch(nspace) &&
                (!isConfigLoading || (LoadingNamespaces is not null && LoadingNamespaces.Contains(nspace))) &&
                (isConfigLoading || EnabledConfigDatas.Any(c => c.Namespace == nspace))) return true;
            if (throwOnInvalidNamespace)
            {
                throw new ArgumentException("Invalid/non-existent namespace.", nameof(str));
            }

            namespacedString = defaultNamespace + str[nsSepIndex..];
            if (logChange)
            {
                PACSingletons.Instance.Logger.Log(
                    "Namespaced string changed",
                    $"{(isConfigLoading ? $"while loading from: {defaultNamespace}, " : "")}\"{str}\" -> \"{namespacedString}\"",
                    LogSeverity.WARN
                );
            }
            return true;
        }

        /// <inheritdoc cref="TryGetNamespacedStringPrivate(string, string, out string, bool, bool, bool)"/>
        public static bool TryGetNamespacedString(
            string str,
            out string namespacedString,
            bool logChange = true,
            bool throwOnInvalidNamespace = false
        )
        {
            var defaultNamepsace = Constants.DEFAULT_NAMESPACE_IS_CURRENT_NAMESPACE
                ? CurrentlyLoadingNamespace
                : Constants.VANILLA_CONFIGS_NAMESPACE;
            return TryGetNamespacedStringPrivate(str, defaultNamepsace!, out namespacedString, true, logChange, throwOnInvalidNamespace);
        }

        /// <inheritdoc cref="TryGetNamespacedStringPrivate(string, string, out string, bool, bool, bool)"/>
        public static bool TryGetNamespacedString(
            string str,
            string defaultNamespace,
            out string namespacedString,
            bool logChange = true,
            bool throwOnInvalidNamespace = false
        )
        {
            return TryGetNamespacedStringPrivate(str, defaultNamespace, out namespacedString, false, logChange, throwOnInvalidNamespace);
        }

        /// <summary>
        /// Tries to correct a string to be namespaced using the currently loaded namespace(s).
        /// </summary>
        /// <param name="str">The maybe namespaced string.</param>
        /// <param name="logChange">Whether to log if the namespaced string is changed to be valid.</param>
        /// <param name="throwOnInvalidNamespace">Whether to throw and exception if the namespaced string is invalid.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="throwOnInvalidNamespace"/> is true and the namespaced string is invalid.</exception>
        /// <returns>The namespaced string, or the same string if namespacing failed.</returns>
        public static string GetNameapacedString(
            string str,
            bool logChange = true,
            bool throwOnInvalidNamespace = true
        )
        {
            TryGetNamespacedString(str, out var nsString, logChange, throwOnInvalidNamespace);
            return nsString;
        }

        /// <summary>
        /// Tries to correct a string to be namespaced using the given namespace.
        /// </summary>
        /// <param name="str">The maybe namespaced string.</param>
        /// <param name="namespaceName">The namespace to write if there is no namespace.</param>
        /// <param name="logChange">Whether to log if the namespaced string is changed to be valid.</param>
        /// <returns>The namespaced string, or the same string if namespacing failed.</returns>
        public static string GetSpecificNamespacedString(
            string str,
            string namespaceName = Constants.VANILLA_CONFIGS_NAMESPACE,
            bool logChange = true
        )
        {
            TryGetNamespacedString(str, namespaceName, out var nsString, logChange);
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

        /// <summary>
        /// Removes the namespace from a namespaced string.
        /// </summary>
        /// <param name="namespacedString">The namespaced string.</param>
        /// <returns>The namespaced string or the original string if it wasn't namespaced.</returns>
        public static string RemoveNamespace(string namespacedString)
        {
            var sepIndex = namespacedString.IndexOf(Constants.NAMESPACE_SEPARATOR_CHAR);
            return sepIndex != -1 ? namespacedString[(sepIndex + 1)..] : namespacedString;
        }
        #endregion
        #endregion

        #region Private functions

        private static (bool success, List<string>? addedValues, List<string>? removedValues) CheckEnumConfigNamespacedValues(
            List<string> configValues,
            string configName,
            bool isNamespaceEnum,
            string removeValueBeggining
        )
        {
            var addedValues = new List<string>();
            var removedValues = new List<string>();
            if (!isNamespaceEnum)
            {
                var isNoneEmpty = true;
                foreach (var value in configValues)
                {
                    if (value.StartsWith(removeValueBeggining))
                    {
                        var rawValue = value[removeValueBeggining.Length..];
                        isNoneEmpty &= !string.IsNullOrWhiteSpace(rawValue);
                        removedValues.Add(rawValue);
                    }
                    else
                    {
                        isNoneEmpty &= !string.IsNullOrWhiteSpace(value);
                        addedValues.Add(value);
                    }
                }
                return (isNoneEmpty, addedValues, removedValues);
            }

            foreach (var value in configValues)
            {
                var isRemoveValue = value.StartsWith(removeValueBeggining);
                var rawValue = isRemoveValue ? value[removeValueBeggining.Length..] : value;
                string namespacedValue;
                try
                {
                    TryGetNamespacedString(rawValue, out namespacedValue, throwOnInvalidNamespace: true);
                }
                catch (Exception ex)
                {
                    PACSingletons.Instance.Logger.Log(
                        "Invalid namespaced enum value in config",
                        $"while loading \"{configName}\" from: \"{CurrentlyLoadingNamespace}\", value: \"{value}\", error: \"{ex.Message}\"",
                        LogSeverity.ERROR
                    );
                    return (false, null, null);
                }

                if (isRemoveValue)
                {
                    removedValues.Add(namespacedValue);
                }
                else
                {
                    addedValues.Add(namespacedValue);
                }
            }
            return (true, addedValues, removedValues);
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
                PACSingletons.Instance.ConsoleProxy.Write(new string(' ', (int)showProgressIndentation * 4) + configFolder);
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
                PACSingletons.Instance.ConsoleProxy.WriteLine(success ? "" : $": {Tools.StylizedText("FALIED!", Constants.Colors.RED)}");
            }
        }

        /// <summary>
        /// Reloads the config value from the aggregate of a config file from all avalible namespaces.
        /// </summary>
        /// <typeparam name="T">The type of the config value.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="namespaceFolders">The avalible config folders, including the default one, in the order of loading.</param>
        /// <param name="getStartingValueFunction">The function to return the staring value of the config value.</param>
        /// <param name="changeConfigFunction">The function to aggregate two config values, and remove the third from the result.</param>
        /// <param name="getConfigVanillaFunction">The GetConfig method to use to get the vanilla config.</param>
        /// <param name="getConfigOtherFunction">The GetConfig method to use to get any non-vanilla config.</param>
        /// <returns>The aggregate of the config file from all avalible namespaces.</returns>
        private static T ReloadConfigsAggregatePrivate<T>(
            string configName,
            List<(string folderName, string namespaceName)> namespaceFolders,
            Func<T> getStartingValueFunction,
            Func<T, T, T, T> changeConfigFunction,
            Func<string, (T add, T remove)> getConfigVanillaFunction,
            Func<string, (bool sucess, T? addValue, T? removeValue)> getConfigOtherFunction,
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
                PACSingletons.Instance.ConsoleProxy.WriteLine(new string(' ', (int)showProgressIndentation * 4) + $"Loading file \"{Path.GetFileName(configNameFull)}\" from config:");
            }
            showProgressIndentation = showProgressIndentation + 1 ?? null;

            _loadingNamespaces = [];
            var aggregateValue = getStartingValueFunction();
            foreach (var (folderName, namespaceName) in namespaceFolders)
            {
                _loadingNamespaces.Add(namespaceName);
                CurrentlyLoadingNamespace = namespaceName;
                var configFileSubpath = Path.Join(folderName, configName);
                if (namespaceName == Constants.VANILLA_CONFIGS_NAMESPACE)
                {
                    LogConfigLoadingBegin(configFileSubpath, folderName, showProgressIndentation);
                    var (add, remove) = getConfigVanillaFunction(configFileSubpath);
                    aggregateValue = changeConfigFunction(aggregateValue, add, remove);
                    LogConfigLoadingEnd(true, showProgressIndentation);
                }
                else if (PACSingletons.Instance.ConfigManager.ConfigFileExists(configFileSubpath))
                {
                    LogConfigLoadingBegin(configFileSubpath, folderName, showProgressIndentation);
                    var (sucess, addValue, removeValue) = getConfigOtherFunction(configFileSubpath);
                    LogConfigLoadingEnd(sucess, showProgressIndentation);
                    if (sucess)
                    {
                        aggregateValue = changeConfigFunction(aggregateValue, addValue!, removeValue!);
                    }
                }
                CurrentlyLoadingNamespace = null;
            }
            _loadingNamespaces = null;

            return aggregateValue;
        }

        /// <summary>
        /// Reloads the config value from the aggregate of a config file from all avalible namespaces.
        /// </summary>
        /// <param name="getConfigOtherFunction">The GetConfig method to use to get any non-vanilla config.<br/>
        /// The second argument is the modified deserialize keys function.</param>
        /// <returns>The aggregate of the config file from all avalible namespaces.</returns>
        /// /// <param name="removeKeyBeggining">The string that should be at the beggining of a key, to signify that that key should be removed.</param>
        /// <inheritdoc cref="ReloadConfigsAggregate{T}(string, List{ValueTuple{string, string}}, T, Func{T}, Func{T, T, T}, bool, int?, string?)"/>
        /// <inheritdoc cref="PACommon.ConfigManagement.AConfigManager.TryGetConfigOrRecreateDict{TK, TV, TVC}(string, string?, IDictionary{TK, TV}, Func{TV, TVC}, Func{TVC, TV}, Func{TK, string}?, Func{string, TK}?, bool, string?)"/>
        private static Dictionary<TK, TV> ReloadConfigsAggregateDictPrivate<TK, TV>(
            string configName,
            List<(string folderName, string namespaceName)> namespaceFolders,
            Func<string, Dictionary<TK, TV>> getConfigVanillaFunction,
            Func<string, Func<string, TK>, (bool success, Dictionary<TK, TV>? result)> getConfigOtherFunction,
            Func<string, TK>? deserializeDictionaryKeys,
            int? showProgressIndentation,
            string removeKeyBeggining
        )
            where TK : notnull
        {
            var valueTypeIsNullable = Nullable.GetUnderlyingType(typeof(TV)) is not null;
            return ReloadConfigsAggregatePrivate(
                configName,
                namespaceFolders,
                () => [],
                (aggDict, newDict, removeDict) =>
                {
                    if (!valueTypeIsNullable && newDict.Any(ni => ni.Value is null && !removeDict.ContainsKey(ni.Key)))
                    {
                        PACSingletons.Instance.Logger.Log("Config error", $"config dictionary value was null instead of {typeof(TV).FullName}", LogSeverity.WARN);
                        return aggDict;
                    }

                    foreach (var newItem in newDict)
                    {
                        aggDict[newItem.Key] = newItem.Value;
                    }
                    foreach (var removeItem in removeDict)
                    {
                        aggDict.Remove(removeItem.Key);
                    }
                    return aggDict;
                },
                (configName) =>
                    (getConfigVanillaFunction(configName), new Dictionary<TK, TV>()),
                (configName) =>
                {
                    var removeValues = new Dictionary<TK, TV>();
                    var (success, result) = getConfigOtherFunction(configName, key =>
                    {
                        var isRemoveKey = key.StartsWith(removeKeyBeggining);
                        if (isRemoveKey)
                        {
                            key = key[removeKeyBeggining.Length..];
                        }
                        var desKey = deserializeDictionaryKeys is null ? (TK)(object)key : deserializeDictionaryKeys(key);
                        if (isRemoveKey)
                        {
                            removeValues[desKey] = default;
                        }
                        return desKey;
                    });
                    return (success, result, removeValues);
                },
                showProgressIndentation
            );
        }

        [GeneratedRegex("^[a-z0-9_]*$")]
        public static partial Regex NamespaceRegex();
        #endregion
    }
}
