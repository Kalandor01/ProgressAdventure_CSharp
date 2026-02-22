using System.IO.Compression;
using System.Reflection;
using JetBrains.Annotations;
using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using PACommon.SettingsManagement;
using PACommon.TestUtils;
using PAExtras;
using ProgressAdventure;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.EntityManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.SettingsManagement;
using ProgressAdventure.WorldManagement;
using ProgressAdventure.WorldManagement.Content;
using Attribute = ProgressAdventure.Enums.Attribute;
using PACConstants = PACommon.Constants;
using PAConstants = ProgressAdventure.Constants;
using PACTools = PACommon.Tools;
using PATools = ProgressAdventure.Tools;
using Utils = PACommon.Utils;

namespace ProgressAdventureTests
{
    public static class Tests
    {
        #region Tests
        #region Dictionary/Enum tests
        #region Entity
        /// <summary>
        /// Checks if the EntityUtils, facing to movement vector dictionary contains all required keys and correct values.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? EntityUtilsFacingToMovementVectorDictionaryCheck()
        {
            var requiredKeys = Enum.GetValues<Facing>();
            IDictionary<Facing, (int x, int y)> checkedDictionary;

            try
            {
                checkedDictionary = Utils.GetInternalPropertyFromStaticClass<IDictionary<Facing, (int x, int y)>>(typeof(EntityUtils), "FacingToMovementVectorMap");
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"\n\tExeption because of (outdated?) test structure in {nameof(EntityUtils)}: " + ex);
            }

            var existingValues = new List<(int x, int y)>();

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (checkedDictionary.TryGetValue(key, out var value))
                {
                    if (
                        existingValues.Contains(value)
                    )
                    {
                        errorMessages.Add($"The dictionary already contains the value \"{value}\", associated with \"{key}\".");
                    }
                    else
                    {
                        if (
                            value.x < -1 || value.x > 1 ||
                            value.y < -1 || value.y > 1
                        )
                        {
                            errorMessages.Add($"The value associated to \"{key}\" is wrong.");
                            continue;
                        }
                        existingValues.Add(value);
                    }
                }
                else
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                }
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }
        
        /// <summary>
        /// Checks if the EntityUtils, attributes stat change dictionary contains all required keys and correct values.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? EntityUtilsAttributeStatsChangeDictionaryCheck()
        {
            var requiredKeys = Attribute.GetValues();
            IDictionary<EnumValue<Attribute>, (double maxHp, double attack, double defence, double agility)> checkedDictionary;

            try
            {
                checkedDictionary = Utils.GetInternalPropertyFromStaticClass<IDictionary<EnumValue<Attribute>, (double maxHp, double attack, double defence, double agility)>>(typeof(EntityUtils), "AttributeStatChangeMap");
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"\n\tExeption because of (outdated?) test structure in {nameof(EntityUtils)}: " + ex);
            }

            var existingValues = new List<(double maxHp, double attack, double defence, double agility)>();

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (checkedDictionary.TryGetValue(key, out var value))
                {
                    if (existingValues.Contains(value))
                    {
                        errorMessages.Add($"The dictionary already contains the value \"{value}\", associated with \"{key}\".");
                    }
                    else
                    {
                        existingValues.Add(value);
                    }
                }
                else
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                }
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }
        
        /// <summary>
        /// Checks if the EntityUtils, entity properties map dictionary contains all required keys and correct values.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? EntityUtilsEntityPropertiesMapDictionaryCheck()
        {
            var requiredKeys = EntityType.GetValues();
            IDictionary<EnumValue<EntityType>, EntityPropertiesDTO> checkedDictionary;

            try
            {
                checkedDictionary = Utils.GetInternalPropertyFromStaticClass<IDictionary<EnumValue<EntityType>, EntityPropertiesDTO>>(
                    typeof(EntityUtils), "EntityPropertiesMap"
                );
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"\n\tExeption because of (outdated?) test structure in {nameof(EntityUtils)}: " + ex);
            }

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.TryGetValue(key, out var property))
                {
                    errorMessages.Add($"The dictionary doesn't contain the key: \"{key}\".");
                    continue;
                }

                if (property is null)
                {
                    errorMessages.Add($"The dictionary value at key \"{key}\" is null.");
                    continue;
                }

                if (
                    string.IsNullOrWhiteSpace(PASingletons.Instance.Localizer.GetLocalizedString(property.displayName)) ||
                    property.maxHp.baseValue - property.maxHp.negativeFluctuation < 0 ||
                    property.attack.baseValue - property.attack.negativeFluctuation < 0 ||
                    property.defence.baseValue - property.defence.negativeFluctuation < 0 ||
                    property.agility.baseValue - property.agility.negativeFluctuation < 0 ||
                    property.teamChangeChange > 1 ||
                    property.teamChangeChange < 0 ||
                    property.attributeChances.Any(ac =>
                        ac.positiveAttributeChance + ac.negativeAttributeChance > 1 ||
                        ac.positiveAttributeChance < 0 ||
                        ac.positiveAttributeChance < 0
                    )
                )
                {
                    errorMessages.Add($"Incorrect property value associated with \"{key}\".");
                }
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }
        #endregion

        #region Items
        /// <summary>
        /// Checks if the ItemUtils, material item attributes dictionary contains all required keys and correct values.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? ItemUtilsMaterialItemAttributesDictionaryCheck()
        {
            var requiredKeys = Material.GetValues();
            var checkedDictionary = ItemUtils.MaterialItemAttributes;

            var existingValues = new List<EnumValue<Material>>();

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.TryGetValue(key, out var _))
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                    continue;
                }
                existingValues.Add(key);
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }

        /// <summary>
        /// Checks if the ItemUtils, compound item attributes dictionary contains all required keys and correct values.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? ItemUtilsCompoundItemAttributesDictionaryCheck()
        {
            var requiredKeys = ItemType.GetAllValues().Where(v => ItemType.GetValues(v).Count == 0).ToList();
            var checkedDictionary = ItemUtils.CompoundItemAttributes;

            var existingValues = new List<string>();

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.TryGetValue(key, out var _))
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                    continue;
                }

                if (existingValues.Contains(key.FullName))
                {
                    errorMessages.Add($"The dictionary already contains the type name \"{key.FullName}\".");
                    continue;
                }

                existingValues.Add(key.FullName);
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }

        /// <summary>
        /// Checks if the ItemUtils, item deffinitions are correct.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? ItemUtilsItemDeffinitionsCheck()
        {
            var checkedDictionary = ItemUtils.DeffinitionItemRecipes;

            var errorMessages = new List<string>();
            PACSingletons.Instance.ConsoleProxy.WriteLine();
            foreach (var deffinitions in checkedDictionary)
            {
                if (deffinitions.Value is null)
                {
                    errorMessages.Add($"The deffinitions list at item type \"{deffinitions.Key}\" is null.");
                    continue;
                }
                if (deffinitions.Value.Count == 0)
                {
                    errorMessages.Add($"The deffinitions list at item type \"{deffinitions.Key}\" is empty.");
                    continue;
                }
                if (!ItemUtils.CompoundItemAttributes.TryGetValue(deffinitions.Key, out var compoundItem))
                {
                    errorMessages.Add($"There is no compound item attribute for the item type \"{deffinitions.Key}\".");
                    continue;
                }

                var prop = compoundItem.properties;
                BigDecimal? maxVolume = null;
                if (
                    prop.dimensionX is not null &&
                    prop.dimensionY is not null &&
                    prop.dimensionZ is not null
                )
                {
                    maxVolume = (BigDecimal)prop.dimensionX * prop.dimensionY * prop.dimensionZ;
                }

                foreach (var deffinition in deffinitions.Value)
                {
                    if (deffinition is null)
                    {
                        errorMessages.Add($"A deffinition in the deffinitions list at item type \"{deffinitions.Key}\" is null.");
                        continue;
                    }

                    var isClose = false;
                    if (
                        maxVolume is not null &&
                        deffinition.volume > maxVolume
                    )
                    {
                        isClose = true;
                        var diff = deffinition.volume - maxVolume;
                        if (maxVolume / 1e15 < diff)
                        {
                            errorMessages.Add($"A deffinition in the deffinitions list at item type \"{deffinitions.Key}\" has a bigger volume than it's dimensions would allow: {maxVolume} (+{diff}).");
                            continue;
                        }
                    }
                    PACSingletons.Instance.ConsoleProxy.WriteLine($"\t{(isClose ? " !!!" : "")} \"{deffinitions.Key}\" Diff: {(maxVolume is null ? "[VARIABLE]" : $"{maxVolume} ({deffinition.volume - maxVolume})")}");
                }
            }

            PACSingletons.Instance.ConsoleProxy.Write("Results...");
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }

        /// <summary>
        /// Checks if the ItemUtils, compound item attributes dictionary contains all required keys and correct values.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? ItemUtilsItemRecipesDictionaryCheck()
        {
            var checkedDictionary = ItemUtils.ItemRecipes;

            var errorMessages = new List<string>();
            foreach (var itemRecipes in checkedDictionary)
            {
                if (itemRecipes.Value is null)
                {
                    errorMessages.Add($"The recipes list at item type \"{itemRecipes.Key}\" is null.");
                    continue;
                }
                if (itemRecipes.Value.Count == 0)
                {
                    errorMessages.Add($"The recipes list at item type \"{itemRecipes.Key}\" is empty.");
                    continue;
                }
                if (!ItemUtils.CompoundItemAttributes.TryGetValue(itemRecipes.Key, out var compoundItem))
                {
                    errorMessages.Add($"There is no compound item attribute for the item type \"{itemRecipes.Key}\".");
                    continue;
                }

                foreach (var itemRecipe in itemRecipes.Value)
                {
                    if (itemRecipe is null)
                    {
                        errorMessages.Add($"A recipe in the recipes list at item type \"{itemRecipes.Key}\" is null.");
                        continue;
                    }
                    if (
                        compoundItem.unit == ItemAmountUnit.AMOUNT &&
                        itemRecipe.resultAmount % 1 != 0
                    )
                    {
                        errorMessages.Add($"A recipe in the recipes list at item type \"{itemRecipes.Key}\" has a {nameof(itemRecipe.resultAmount)} that isn't an integer, but the item type that will be created can only have an integer amount.");
                    }
                }
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }
        #endregion

        #region Settings
        /// <summary>
        /// Checks if the SettingsUtils, action type attributes dictionary contains all required keys and correct values.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? SettingsUtilsActionTypeAttributesDictionaryCheck()
        {
            var requiredKeys = ActionType.GetValues();
            var checkedDictionary = SettingsUtils.ActionTypeAttributes;

            var existingResponses = new List<string>();

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.TryGetValue(key, out var value))
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                    continue;
                }

                if (existingResponses.Contains(value.response))
                {
                    errorMessages.Add($"The dictionary already contains the value \"{value}\", associated with \"{key}\".");
                    continue;
                }

                existingResponses.Add(value.response);

                if (value.defaultKeys.Count == 0)
                {
                    errorMessages.Add($"The action type doesn't have any keys at the key \"{key}\".");
                }
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }

        /// <summary>
        /// Checks if the SettingsUtils, special key name dictionary contains all required keys and correct values.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? SettingsUtilsSpecialKeyNameDictionaryCheck()
        {
            var checkedDictionary = KeybindUtils.specialKeyNameMap;

            var existingKeys = new List<ConsoleKey>();
            var existingValues = new List<string>();

            var errorMessages = new List<string>();
            foreach (var element in checkedDictionary)
            {
                if (existingKeys.Contains(element.Key))
                {
                    errorMessages.Add($"The dictionary already contains the key \"{element.Key}\".");
                    continue;
                }

                existingKeys.Add(element.Key);

                if (existingValues.Contains(element.Value))
                {
                    errorMessages.Add($"The dictionary already contains the value \"{element.Value}\", associated with \"{element.Key}\".");
                    continue;
                }

                existingValues.Add(element.Value);
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }

        /// <summary>
        /// Checks if the SettingsUtils, setting value type map dictionary contains all required keys and correct values.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? SettingsUtilsSettingValueTypeMapDictionaryCheck()
        {
            var requiredKeys = Enum.GetValues<SettingsKey>();
            var checkedDictionary = Utils.GetInternalFieldFromNonStaticClass<Dictionary<SettingsKey, JsonObjectType>, ISettings>(
                PASingletons.Instance.Settings,
                "_settingValueTypeMap"
            );

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.ContainsKey(key))
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                }
            }
            
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }

        /// <summary>
        /// Checks if the SettingsUtils, <see cref="SettingsUtils.GetDefaultSettings"/> dictionary contains all required keys and correct values.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? SettingsUtilsDefaultSettingsDictionaryCheck()
        {
            var requiredKeys = Enum.GetValues<SettingsKey>();
            var checkedDictionary = SettingsUtils.GetDefaultSettings();

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.TryGetValue(key.ToString(), out var value))
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                    continue;
                }

                if (value is null)
                {
                    errorMessages.Add($"The ignore map in the dictionary at \"{key}\" is null.");
                }
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }
        #endregion

        #region World
        /// <summary>
        /// Checks if the WorldUtils, tile noise offsets dictionary contains all required keys and correct values.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? WorldUtilsTileNoiseOffsetsDictionaryCheck()
        {
            var requiredKeys = Enum.GetValues<TileNoiseType>();
            IDictionary<TileNoiseType, double> checkedDictionary;

            try
            {
                checkedDictionary = Utils.GetInternalPropertyFromStaticClass<IDictionary<TileNoiseType, double>>(typeof(WorldUtils), "TileNoiseOffsets");
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"\n\tExeption because of (outdated?) test structure in {nameof(WorldUtils)}: " + ex);
            }

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.ContainsKey(key))
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                }
            }
            
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }

        /// <summary>
        /// Checks if the WorldUtils, content type map dictionary contains all required keys and correct values.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? WorldUtilsContentTypeMapDictionaryCheck()
        {
            // get all classes that directly implement BaseContent directly
            var paAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.GetName().Name == nameof(ProgressAdventure));

            var requiredKeys1 = TerrainType.GetValues();
            var requiredKeys2 = StructureType.GetValues();

            var terrainType = typeof(TerrainContent);
            var structureType = typeof(StructureContent);
            var unfilteredClassObjs1 = paAssembly.GetTypes().Where(terrainType.IsAssignableFrom);
            var requiredTypeClassess1 = unfilteredClassObjs1.Where(type => type is { IsAbstract: false, IsInterface: false }).ToList();
            var unfilteredClassObjs2 = paAssembly.GetTypes().Where(structureType.IsAssignableFrom);
            var requiredTypeClassess2 = unfilteredClassObjs2.Where(type => type is { IsAbstract: false, IsInterface: false }).ToList();

            var foundTypeClasses1 = new List<Type>();
            var foundTypeClasses2 = new List<Type>();

            IDictionary<EnumValue<TerrainType>, TerrainTypePropertiesDTO> checkedDictionary1;
            IDictionary<EnumValue<StructureType>, StructureTypePropertiesDTO> checkedDictionary2;

            try
            {
                checkedDictionary1 = Utils.GetInternalPropertyFromStaticClass<IDictionary<EnumValue<TerrainType>, TerrainTypePropertiesDTO>>(typeof(WorldUtils), "TerrainTypeMap");
                checkedDictionary2 = Utils.GetInternalPropertyFromStaticClass<IDictionary<EnumValue<StructureType>, StructureTypePropertiesDTO>>(typeof(WorldUtils), "StructureTypeMap");
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"\n\tExeption because of (outdated?) test structure in {nameof(WorldUtils)}: " + ex);
            }

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys1)
            {
                if (!checkedDictionary1.TryGetValue(key, out var props))
                {
                    errorMessages.Add($"The terrain dictionary doesn't contain a value for \"{key}\".");
                    continue;
                }

                if (props is null)
                {
                    errorMessages.Add($"The value of the terrain dictionary at \"{key}\" is null.");
                    continue;
                }

                if (!PASingletons.Instance.Localizer.TryGetLocalizedString(props.displayName, out var localizedString))
                {
                    errorMessages.Add($"The display name of the value of the terrain dictionary at \"{key}\" is an invalid localization key.");
                }

                if (string.IsNullOrWhiteSpace(localizedString))
                {
                    errorMessages.Add($"The display name of the value of the terrain dictionary at \"{key}\" is empty.");
                }

                if (!requiredTypeClassess1.Contains(props.matchingType))
                {
                    errorMessages.Add($"The match type of the value of the terrain dictionary at \"{key}\" is invalid.");
                    continue;
                }

                if (foundTypeClasses1.Contains(props.matchingType))
                {
                    errorMessages.Add($"The match type of the value of the terrain dictionary at \"{key}\" was already used.");
                    continue;
                }
                foundTypeClasses1.Add(props.matchingType);
            }
            foreach (var key in requiredKeys2)
            {
                if (!checkedDictionary2.TryGetValue(key, out var props))
                {
                    errorMessages.Add($"The structure dictionary doesn't contain a value for \"{key}\".");
                    continue;
                }

                if (props is null)
                {
                    errorMessages.Add($"The value of the structure dictionary at \"{key}\" is null.");
                    continue;
                }

                if (!PASingletons.Instance.Localizer.TryGetLocalizedString(props.displayName, out var localizedString))
                {
                    errorMessages.Add($"The display name of the value of the structure dictionary at \"{key}\" is an invalid localization key.");
                }

                if (string.IsNullOrWhiteSpace(localizedString))
                {
                    errorMessages.Add($"The display name of the value of the structure dictionary at \"{key}\" is empty.");
                }

                if (!requiredTypeClassess2.Contains(props.matchingType))
                {
                    errorMessages.Add($"The match type of the value of the structure dictionary at \"{key}\" is invalid.");
                    continue;
                }

                if (foundTypeClasses2.Contains(props.matchingType))
                {
                    errorMessages.Add($"The match type of the value of the structure dictionary at \"{key}\" was already used.");
                    continue;
                }
                foundTypeClasses2.Add(props.matchingType);
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }

        /// <summary>
        /// Checks if the WorldUtils, content type property map dictionary contains all required keys and correct values.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? WorldUtilsContentTypePropertyMapDictionaryCheck()
        {
            var paAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.GetName().Name == nameof(ProgressAdventure));

            // get all classes that directly implement BaseContent directly
            var baseContentType1 = typeof(TerrainContent);
            var huh = paAssembly.GetTypes().Where(t => t.BaseType == baseContentType1);
            var filteredTypes1 = paAssembly.GetTypes()
                .Where(type => type is { IsAbstract: false, IsInterface: false } && type.BaseType == baseContentType1);
            var baseContentType2 = typeof(StructureContent);
            var filteredTypes2 = paAssembly.GetTypes()
                .Where(type => type is { IsAbstract: false, IsInterface: false } && type.BaseType == baseContentType2);

            var requiredKeys1 = filteredTypes1.ToList();
            var requiredKeys2 = filteredTypes2.ToList();

            IDictionary<Type, Dictionary<TileNoiseType, double>> checkedDictionary1;
            IDictionary<Type, Dictionary<TileNoiseType, double>> checkedDictionary2;
            try
            {
                checkedDictionary1 = Utils.GetInternalPropertyFromStaticClass<IDictionary<Type, Dictionary<TileNoiseType, double>>>(typeof(WorldUtils), "TerrainTypePropertyMap");
                checkedDictionary2 = Utils.GetInternalPropertyFromStaticClass<IDictionary<Type, Dictionary<TileNoiseType, double>>>(typeof(WorldUtils), "StructureTypePropertyMap");
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"\n\tExeption because of (outdated?) test structure in {nameof(WorldUtils)}: " + ex);
            }

            if (WorldUtilsContentTypePropertyMapDictionaryCheckPrivate(requiredKeys1, checkedDictionary1) is { } result1)
            {
                return result1;
            }
            return WorldUtilsContentTypePropertyMapDictionaryCheckPrivate(requiredKeys2, checkedDictionary2);
        }
        #endregion
        #endregion

        #region Exists and loadable tests
        /// <summary>
        /// Checks if all material enums values can be turned into material items.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? AllMaterialItemTypesExistAndLoadable()
        {
            const int itemAmount = 3;

            var allItems = new List<MaterialItem>();

            // all item IDs can turn into items
            var errorMessages = new List<string>();
            foreach (var material in Material.GetValues())
            {
                MaterialItem item;
                try
                {
                    item = new MaterialItem(material, amount: itemAmount);
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Couldn't create item from type \"{material}\": " + ex);
                    continue;
                }

                allItems.Add(item);
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            // items loadable from json and are the same as before load
            errorMessages = [];
            foreach (var item in allItems)
            {
                MaterialItem loadedItem;

                try
                {
                    var itemJson = item.ToJson();
                    var success = PACTools.TryFromJson(itemJson, PAConstants.SAVE_VERSION, out AItem? loadedAItem);

                    if (loadedAItem is null || !success)
                    {
                        throw new ArgumentNullException(item.Type.ToString());
                    }

                    loadedItem = (MaterialItem)loadedAItem;
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Loading item from json failed for \"{item.Type}\": " + ex);
                    continue;
                }

                if (
                    loadedItem.Type == item.Type &&
                    loadedItem.Material == item.Material &&
                    loadedItem.Amount == item.Amount &&
                    loadedItem.DisplayName == item.DisplayName &&
                    loadedItem.Unit == item.Unit &&
                    loadedItem.Mass == item.Mass &&
                    loadedItem.Volume == item.Volume
                )
                {
                    continue;
                }

                errorMessages.Add($"Original item, and item loaded from json are not the same for \"{item.Type}\"");
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }

        /// <summary>
        /// Checks if all item IDs can be turned into compound items.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? AllCompoundItemTypesExistAndLoadable()
        {
            const int itemAmount = 3;

            var allItems = new List<CompoundItem>();

            // all item IDs can turn into items
            var errorMessages = new List<string>();
            foreach (var itemType in ItemType.GetAllValues().Where(v => ItemType.GetValues(v).Count == 0))
            {
                if (itemType == ItemType.Misc.MATERIAL)
                {
                    continue;
                }

                var attributes = ItemUtils.CompoundItemAttributes[itemType];
                CompoundItem item;
                try
                {
                    item = ItemUtils.CreateCompoundItem(itemType, amount: itemAmount);
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Couldn't create item from type \"{itemType}\": " + ex);
                    continue;
                }

                allItems.Add(item);
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            // items loadable from json and are the same as before load
            errorMessages = [];
            foreach (var item in allItems)
            {
                CompoundItem loadedItem;

                try
                {
                    var itemJson = item.ToJson();
                    var success = PACTools.TryFromJson(itemJson, PAConstants.SAVE_VERSION, out AItem? loadedAItem);

                    if (loadedAItem is null || !success)
                    {
                        throw new ArgumentNullException(item.Type.ToString());
                    }

                    loadedItem = (CompoundItem)loadedAItem;
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Loading item from json failed for \"{item.Type}\": " + ex);
                    continue;
                }

                if (
                    loadedItem.Type == item.Type &&
                    loadedItem.Material == item.Material &&
                    loadedItem.Amount == item.Amount &&
                    loadedItem.DisplayName == item.DisplayName &&
                    loadedItem.Unit == item.Unit &&
                    loadedItem.Mass == item.Mass &&
                    loadedItem.Volume == item.Volume &&
                    item.Parts.All(part => loadedItem.Parts.Any(part2 => part.Equals(part2)))
                )
                {
                    continue;
                }

                errorMessages.Add($"Original item, and item loaded from json are not the same for \"{item.Type}\"");
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }

        /// <summary>
        /// Checks if all entities have a type name, and can be loaded from json.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? AllEntitiesLoadable()
        {
            RandomStates.Initialize();

            IDictionary<EnumValue<EntityType>, EntityPropertiesDTO> entitypropertiesMap;

            try
            {
                entitypropertiesMap = Utils.GetInternalPropertyFromStaticClass<IDictionary<EnumValue<EntityType>, EntityPropertiesDTO>>(
                    typeof(EntityUtils),
                    "EntityPropertiesMap"
                );
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"\n\tExeption because of (outdated?) test structure in {nameof(EntityUtils)}: " + ex);
            }

            var entities = new List<Entity>();

            // check if entity exists and loadable from jsom
            var errorMessages = new List<string>();

            const string testSaveName = "test save";
            (long, long)? nullTestPos = null;
            var testPos = (15, 38);
            var testPos2 = (68, 29);
            (long, long)? nullableTestPos = testPos;

            PATools.DeleteSave(testSaveName);
            World.Initialize();
            World.TryGetTileAll(testPos, out var testTile, testSaveName);
            World.TryGetTileAll(testPos2, out var testTile2, testSaveName);
            foreach (var entityType in EntityType.GetValues())
            {
                // get entity type properties
                if (!entitypropertiesMap.TryGetValue(entityType, out var properties))
                {
                    errorMessages.Add("Entity type has no properties in entity properties map");
                    continue;
                }

                var defEntityJson = new JsonDictionary
                {
                    ["type"] = entityType,
                };

                Entity? entity;

                try
                {
                    PACSingletons.Instance.Logger.Log("Beggining mock entity creation", "ignore \"json is null\" warnings", LogSeverity.OTHER);
                    PACTools.TryFromJsonExtra(defEntityJson, nullTestPos, PAConstants.SAVE_VERSION, out entity);
                    PACSingletons.Instance.Logger.Log("Mock entity creation ended", "", LogSeverity.OTHER);

                    if (entity is null)
                    {
                        throw new ArgumentNullException(entityType.Name);
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Entity creation from default json failed for \"{entityType}\": " + ex);
                    continue;
                }

                entities.Add(entity);

                if (!entity.AddPosition(testPos))
                {
                    errorMessages.Add($"Entity type cannot be added to a Tile: \"{entityType}\"");
                }
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            // entities loadable from json and are the same as before load
            errorMessages = [];
            foreach (var entity in entities)
            {
                Entity? loadedEntity;

                try
                {
                    var entityJson = entity.ToJson();
                    var success = PACTools.TryFromJsonExtra(entityJson, nullableTestPos, PAConstants.SAVE_VERSION, out loadedEntity);

                    if (loadedEntity is null || !success)
                    {
                        throw new ArgumentNullException(entity.GetType().ToString());
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Loading entity from json failed for \"{entity.GetType()}\": " + ex);
                    continue;
                }

                if (
                    loadedEntity.type == entity.type &&
                    loadedEntity.FullName == entity.FullName &&
                    loadedEntity.MaxHp == entity.MaxHp &&
                    loadedEntity.CurrentHp == entity.CurrentHp &&
                    loadedEntity.Attack == entity.Attack &&
                    loadedEntity.Defence == entity.Defence &&
                    loadedEntity.Agility == entity.Agility &&
                    loadedEntity.Position == entity.Position &&
                    loadedEntity.originalTeam == entity.originalTeam &&
                    loadedEntity.currentTeam == entity.currentTeam &&
                    loadedEntity.attributes.SequenceEqual(entity.attributes) &&
                    loadedEntity.drops.SequenceEqual(entity.drops) &&
                    (
                        (loadedEntity.TryGetInventory() is null && entity.TryGetInventory() is null) ||
                        (
                            loadedEntity.TryGetInventory() is  { } loadedInv &&
                            entity.TryGetInventory() is  { } entityInv &&
                            loadedInv.items.SequenceEqual(entityInv.items)
                        )
                    )
                )
                {
                    if (!entity.MovePosition(testPos2))
                    {
                        errorMessages.Add($"Entity type cannot be moved to another Tile: \"{entity.type}\"");
                    }
                    if (!entity.RemovePosition())
                    {
                        errorMessages.Add($"Entity type cannot be removed from a Tile: \"{entity.type}\"");
                    }
                    continue;
                }

                errorMessages.Add($"Original entity, and entity loaded from json are not the same for \"{entity.type}\"");
            }
            
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }
        #endregion

        #region Other tests
        /// <summary>
        /// Checks if all objects that implement IJsonConvertable can be converted to and from json.<br/>
        /// ONLY CHECKS FOR SUCCESFUL CONVERSION. NOT IF THE RESULTING OBJECT HAS THE SAME VALUES FOR ATTRIBUTES OR NOT!
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? BasicJsonConvertTest()
        {
            // list of classes that implement "IJsonConvertable<T>"!
            var testObjects = new List<IJsonReadable>
            {
                new LoadedConfigData("test_namespace", "v123"),
                new CompoundItem(ItemType.Weapon.SWORD, [new MaterialItem(Material.CLOTH)]),
                new Inventory(),
                new MaterialItem(Material.FLINT),
                new ActionKey(ActionType.ESCAPE, [new ConsoleKeyInfo()]),
                new Keybinds(),
                new Chunk((1, 1)),
                RandomStates.Initialize(),
                SaveData.Initialize("test", initialiseRandomGenerators: false),
                PACTools.FromJson<DisplaySaveData>(DisplaySaveData.ToJsonFromSaveData(SaveData.Instance), PAConstants.SAVE_VERSION)
                ?? throw new ArgumentNullException(nameof(DisplaySaveData), $"{nameof(DisplaySaveData)} from json should not be null."),
            };

            RandomStates.Initialize();

            // get all classes that implement IJsonConvertable<T>
            var jsonConvertableType = typeof(IJsonConvertable<>);
            var paAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.GetName().Name == nameof(ProgressAdventure));
            var unfilteredTypes = paAssembly.GetTypes().Where(jsonConvertableType.IsGenericAssignableFromType);
            var filteredTypes = unfilteredTypes
                .Where(type => type is { IsAbstract: false, IsInterface: false })
                .ToList();

            //check if all mocked classes are correct and present
            if (filteredTypes.Count != testObjects.Count)
            {
                var diff = testObjects.Count - filteredTypes.Count;
                return new TestResultDTO(LogSeverity.FAIL, $"\n\tThere are {Math.Abs(diff)} {(diff > 0 ? "more" : "less")} test objects in the test objects list than there should be.");
            }

            var errorMessages = new List<string>();
            foreach (var testObject in testObjects)
            {
                if (filteredTypes.All(type => type != testObject.GetType()))
                {
                    errorMessages.Add($"The {testObject.GetType()} type object should not be in the test objects list.");
                }
            }
            
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }
            
            //to/from json
            var errorMessages2 = new List<string>();
            foreach (var testObject in testObjects)
            {
                var objJson = testObject.ToJson();
                var genericConvertableTypes = testObject.GetType().GetInterfaces();
                var genericConvertableType = genericConvertableTypes.First(t => t.Name == jsonConvertableType.Name);
                var method = genericConvertableType?.GetMethod("FromJson", BindingFlags.Static | BindingFlags.Public);
                var parameters = new object?[] { objJson, PAConstants.SAVE_VERSION, null };
                
                try
                {
                    var succObj = method?.Invoke(null, parameters);

                    if (parameters[2] is null || succObj is not bool success || !success)
                    {
                        errorMessages2.Add($"Couldn't convert the json representation of {testObject.GetType()} back without {(parameters[2] is null ? "errors" : "warnings")}.");
                    }
                }
                catch (Exception ex)
                {
                    errorMessages2.Add($"FromJson method invokation threw an exception at the {testObject.GetType()} type object.");
                }
            }
            
            if (errorMessages2.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages2));
            }

            return new TestResultDTO(LogSeverity.PASS, "Not fully implemented!");
        }

        /// <summary>
        /// Checks if all objects that implement IJsonConvertableExtra can be converted to and from json.<br/>
        /// ONLY CHECKS FOR SUCCESFUL CONVERSION. NOT IF THE RESULTING OBJECT HAS THE SAME VALUES FOR ATTRIBUTES OR NOT!
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? BasicJsonConvertExtraTest()
        {
            // extra data for classes that implement "IJsonConvertableExra<T, TExtra>"!
            const string testConfigFolderName = "test config";
            (long x, long y) testPostition = (1537, 269);
            var testChunkRandom = Chunk.GetChunkRandom(testPostition);
            (long, long)? entityTestPos = (17, 68);

            // list of classes that implement "IJsonConvertableExra<T, TExtra>"!
            var testObjects = new List<(IJsonReadable obj, object? extraData)>
            {
                (
                    new ConfigData(testConfigFolderName, "test_namespace", "v123", "1.57", ["pa", "test_depend"]),
                    testConfigFolderName
                ),
                (
                    new PopulationManager(
                        new Dictionary<EnumValue<EntityType>, int>
                        {
                            [EntityType.CAVEMAN] = 23,
                            [EntityType.ELF] = 5,
                            [EntityType.HUMAN] = 36,
                            [EntityType.DEMON] = 12,
                        },
                        [
                            new Entity(EntityType.PLAYER),
                            new Entity(EntityType.DEMON),
                            new Entity(EntityType.DEMON),
                            new Entity(EntityType.ELF),
                            new Entity(EntityType.CAVEMAN),
                            new Entity(EntityType.DRAGON),
                        ],
                        testPostition,
                        testChunkRandom
                    ),
                    (testChunkRandom, testPostition)
                ),
                (
                    new Tile(testPostition.x, testPostition.y, testChunkRandom),
                    (testChunkRandom, testPostition)
                ),
                (
                    new Entity(EntityType.PLAYER),
                    entityTestPos
                ),
            };

            RandomStates.Initialize();

            // get all classes that implement IJsonConvertableExtra<T>
            var jsonConvertableType = typeof(IJsonConvertableExtra<,>);
            var paAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == nameof(ProgressAdventure));
            var unfilteredTypes = paAssembly.GetTypes().Where(jsonConvertableType.IsGenericAssignableFromType);
            var filteredTypes = unfilteredTypes.Where(type => type is { IsAbstract: false, IsInterface: false }).ToList();

            //check if all mocked classes are correct and present
            if (filteredTypes.Count != testObjects.Count)
            {
                var diff = testObjects.Count - filteredTypes.Count;
                return new TestResultDTO(LogSeverity.FAIL, $"\n\tThere are {Math.Abs(diff)} {(diff > 0 ? "more" : "less")} test objects in the test objects list than there should be.");
            }

            var errorMessages = new List<string>();
            foreach (var (testObject, extraData) in testObjects)
            {
                if (filteredTypes.All(type => type != testObject.GetType()))
                {
                    errorMessages.Add($"The {testObject.GetType()} type object should not be in the test objects list.");
                }
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }


            Dictionary<string, string> currentVersion;
            try
            {
                currentVersion = (Dictionary<string, string>)PACSingletons.Instance.JsonDataCorrecter.GetType()
                    .GetField("saveVersionExceptions", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(PACSingletons.Instance.JsonDataCorrecter)!;
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"\n\tExeption because of (outdated?) test structure in {nameof(EntityUtils)}: " + ex);
            }

            //to/from json
            var errorMessages2 = new List<string>();
            foreach (var (testObject, extraData) in testObjects)
            {
                var genericConvertableTypes = testObject.GetType().GetInterfaces();
                var genericConvertableType = genericConvertableTypes.First(t => t.Name == jsonConvertableType.Name);

                var extraDataType = genericConvertableType.GenericTypeArguments[1];
                if (
                    extraData?.GetType() != extraDataType &&
                    extraData?.GetType() != (Nullable.GetUnderlyingType(extraDataType) ?? extraDataType)
                )
                {
                    errorMessages2.Add($"The type of the extra data for the {testObject.GetType()} type object should be {extraDataType} instead of {extraData?.GetType().ToString() ?? "[NULL]"}.");
                    continue;
                }

                var objJson = testObject.ToJson();
                var method = genericConvertableType?.GetMethod("FromJson", BindingFlags.Static | BindingFlags.Public);
                var saveVersion = currentVersion.GetValueOrDefault(testObject.GetType().FullName!, PAConstants.SAVE_VERSION);
                var parameters = new[] { objJson, extraData, saveVersion, null };

                try
                {
                    var succObj = method?.Invoke(null, parameters);

                    if (parameters[3] is null || succObj is not bool success || !success)
                    {
                        errorMessages2.Add($"Couldn't convert the json representation of {testObject.GetType()} back without {(parameters[3] is null ? "errors" : "warnings")}.");
                    }
                }
                catch (Exception ex)
                {
                    errorMessages2.Add($"FromJson method invokation threw an exception at the {testObject.GetType()} type object.");
                }
            }

            return errorMessages2.Count == 0
                ? new TestResultDTO(LogSeverity.PASS, "Not fully implemented!")
                : new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages2));
        }

        /// <summary>
        /// Checks if all objects that implement IJsonConvertable cab be converted to and from json.
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? ConfigUpdateTest()
        {
            var configFolderPath = Path.Join(PACConstants.ROOT_FOLDER, PAConstants.CONFIGS_FOLDER);
            if (Directory.Exists(configFolderPath))
            {
                Directory.Delete(configFolderPath, true);
            }
            PATools.ReloadConfigs();
            return null;
        }

        /// <summary>
        /// NOT COMPLETE!!!<br/>
        /// Checks if all reference save files can be loaded without errors or warnings.<br/>
        /// ONLY CHECKS FOR SUCCESFUL LOADING. NOT IF THE RESULTING OBJECT HAS THE SAME VALUES!
        /// </summary>
        [UsedImplicitly]
        public static TestResultDTO? BasicAllMainSaveFileVersionsLoadable()
        {
            // create current refrence save
            const string currentSaveName = "current";
            PATools.DeleteSave(currentSaveName);
            CreateTestSaveData(currentSaveName);
            SaveManager.MakeSave();
            var testBackupFilePath = Path.Join(Constants.TEST_REFERENCE_SAVES_FOLDER_PATH, $"{currentSaveName}.{PAConstants.BACKUP_EXT}");
            File.Delete(testBackupFilePath);
            ZipFile.CreateFromDirectory(PATools.GetSaveFolderPath(currentSaveName), testBackupFilePath);
            PATools.DeleteSave(currentSaveName);

            var leftowerImportedFiles = Directory.GetFiles(Constants.TEST_REFERENCE_SAVES_FOLDER_PATH)
                .Where(path => path.StartsWith("current_imported-(") && path.EndsWith(").zip"))
                .ToList();
            foreach (var leftowerImportedFile in leftowerImportedFiles)
            {
                File.Delete(leftowerImportedFile);
            }

            // list of importable saves
            var importedZipPaths = Directory.GetFiles(Constants.IMPORTED_REFERENCE_SAVES_FOLDER_PATH)
                .Where(path => Path.GetExtension(path) == ".zip")
                .ToList();
            importedZipPaths.Sort(new VersionStringZipPathComparer());
            PATools.RecreateSavesFolder();
            PACSingletons.Instance.ConsoleProxy.WriteLine();
            var importOverallSuccess = true;
            foreach (var importedZipPath in importedZipPaths)
            {
                var result = TestImportSaveFromZip(importedZipPath) ?? new TestResultDTO(LogSeverity.PASS);
                var saveName = Path.GetFileNameWithoutExtension(importedZipPath);

                var resultString = TestingUtils.GetResultString(result);
                PACSingletons.Instance.ConsoleProxy.WriteLine($"\r\tImporting ({saveName})..." + resultString);
                var messageText = result.resultMessage is null ? "" : ": " + result.resultMessage;
                PACSingletons.Instance.Logger.Log(
                    $"Save import {result.resultType}",
                    $"\"{Path.GetFileNameWithoutExtension(importedZipPath)}\"{(messageText is not null ? $", {messageText}" : "")}",
                    result.resultType == LogSeverity.PASS ? LogSeverity.INFO : LogSeverity.ERROR
                );
                importOverallSuccess &= result.resultType == LogSeverity.PASS;
            }
            var overallResult = importOverallSuccess ? new TestResultDTO() : new TestResultDTO(LogSeverity.FAIL, "See above for details");
            PACSingletons.Instance.ConsoleProxy.WriteLine($"Overall... {TestingUtils.GetResultString(overallResult)}");

            // list of reference saves
            var zipPaths = Directory.GetFiles(Constants.TEST_REFERENCE_SAVES_FOLDER_PATH)
                .Where(path => Path.GetExtension(path) == ".zip")
                .ToList();
            zipPaths.Sort(new VersionStringZipPathComparer());
            PATools.RecreateSavesFolder();
            var overallSuccess = true;
            foreach (var zipPath in zipPaths)
            {
                var result = TestLoadSaveFromZip(zipPath) ?? new TestResultDTO(LogSeverity.PASS);
                var saveName = Path.GetFileNameWithoutExtension(zipPath);

                var resultString = TestingUtils.GetResultString(result);
                PACSingletons.Instance.ConsoleProxy.WriteLine($"\r\tChecking ({saveName})..." + resultString);
                var messageText = result.resultMessage is null ? "" : ": " + result.resultMessage;
                PACSingletons.Instance.Logger.Log(Path.GetFileNameWithoutExtension(zipPath), result.resultType + messageText, LogSeverity.OTHER);
                overallSuccess &= result.resultType == LogSeverity.PASS;
            }

            var importedFiles = Directory.GetFiles(Constants.TEST_REFERENCE_SAVES_FOLDER_PATH)
                .Where(path => path.StartsWith("current_imported-(") && path.EndsWith(").zip"))
                .ToList();
            foreach (var importedFile in importedFiles)
            {
                File.Delete(importedFile);
            }

            PACSingletons.Instance.ConsoleProxy.Write("Overall...");
            return overallSuccess ? null : new TestResultDTO(LogSeverity.FAIL, "See above for details");
        }
        #endregion
        #endregion

        #region Private methods
        /// <summary>
        /// Tries to parse all chunks from a save file, and returns the position of first chunk that had an error while parsing.
        /// </summary>
        /// <param name="saveFolderName">The name of the save folder.</param>
        /// <param name="showProgressText">If not null, it writes out a progress percentage with this string while saving.</param>
        private static (long x, long y)? TryParseAllChunksFromFolder(string saveFolderName, string? showProgressText = null)
        {
            // get existing files
            var chunksFolderPath = Path.Join(PAConstants.SAVES_FOLDER_PATH, saveFolderName, PAConstants.SAVE_FOLDER_NAME_CHUNKS);
            var chunkFilePaths = Directory.GetFiles(chunksFolderPath);
            var existingChunks = new List<(long x, long y)>();
            foreach (var chunkFilePath in chunkFilePaths)
            {
                var chunkFileName = Path.GetFileName(chunkFilePath);
                var chunkExtension = chunkFileName is not null ? Path.GetExtension(chunkFileName).ToLower() : null;
                if (
                    chunkFileName is not null &&
                    chunkExtension is $".{PAConstants.SAVE_EXT}" or $".{PAConstants.OLD_SAVE_EXT}" &&
                    chunkFileName.StartsWith($"{PAConstants.CHUNK_FILE_NAME}{PAConstants.CHUNK_FILE_NAME_SEP}")
                )
                {
                    var chunkPositions = Path.GetFileNameWithoutExtension(chunkFileName).Replace($"{PAConstants.CHUNK_FILE_NAME}{PAConstants.CHUNK_FILE_NAME_SEP}", "").Split(PAConstants.CHUNK_FILE_NAME_SEP);
                    if (
                        chunkPositions.Length == 2 &&
                        long.TryParse(chunkPositions[0], out var posX) &&
                        long.TryParse(chunkPositions[1], out var posY)
                    )
                    {
                        existingChunks.Add((posX, posY));
                        continue;
                    }
                    PACSingletons.Instance.Logger.Log("Chunk file parse error", $"chunk positions couldn't be extracted from chunk file name: {chunkFileName}", LogSeverity.WARN);
                }
                PACSingletons.Instance.Logger.Log("Chunk file parse error", "file name is not chunk file name", LogSeverity.WARN);
            }

            var success = true;
            // load chunks
            if (showProgressText is not null)
            {
                var chunkNum = (double)existingChunks.Count;
                var loadingText = PACTools.GetStandardLoadingText(showProgressText);
                loadingText.Display();
                for (var x = 0; x < chunkNum; x++)
                {
                    success &= Chunk.FromFile(existingChunks[x], out _, out _, saveFolderName);
                    if (!success)
                    {
                        loadingText.StopLoading("DONE!");
                        return existingChunks[x];
                    }
                    loadingText.Value = (x + 1) / chunkNum;
                }
                loadingText.StopLoading("DONE!");
            }
            else
            {
                foreach (var chunkPos in existingChunks)
                {
                    success &= Chunk.FromFile(chunkPos, out _, out _, saveFolderName);
                    if (!success)
                    {
                        return chunkPos;
                    }
                }
            }

            PACSingletons.Instance.Logger.Log("Loaded all chunks from file", $"save folder name: {saveFolderName}");
            return null;
        }

        private static void CreateTestSaveData(string saveName)
        {
            SaveData.Initialize(
                saveName,
                $"test save ({saveName})",
                null, null,
                new Entity(EntityType.PLAYER,
                    $"test player ({saveName})",
                    extraData: new Dictionary<string, object?>
                    {
                        [PAConstants.JsonKeys.Entity.INVENTORY] = new Inventory(
                        [
                            new MaterialItem(Material.CLOTH, 15),
                            new MaterialItem(Material.GOLD, 5.27),
                            new MaterialItem(Material.HEALING_LIQUID, 0.3),
                            ItemUtils.CreateCompoundItem(ItemType.Weapon.SWORD, [Material.STEEL, Material.WOOD], 12),
                            ItemUtils.CreateCompoundItem(ItemType.Weapon.CLUB, [Material.WOOD], 3),
                            ItemUtils.CreateCompoundItem(ItemType.Weapon.ARROW, [Material.FLINT, Material.WOOD], 152),
                        ]),
                    }
                )
            );
            World.Initialize();
            SaveData.Instance.PlayerRef.AddPosition((15, -6));
            World.GenerateChunk((0, 0));
            World.GenerateChunk((-48, -458));
            World.GenerateChunk((126, -96));
            World.GenerateChunk((-9, 158));
            World.GenerateChunk((1235, 6));
        }

        private static TestResultDTO? TestImportSaveFromZip(string zipPath)
        {
            var saveName = Path.GetFileNameWithoutExtension(zipPath);
            var saveImportedName = $"current_imported-({saveName})";
            var saveToImportPath = Path.Join(PAExtras.Constants.EXPORTED_FOLDER_PATH, saveImportedName);

            if (Directory.Exists(saveToImportPath))
            {
                Directory.Delete(saveToImportPath, true);
                PACSingletons.Instance.Logger.Log("Deleted importable save", $"save name: {saveImportedName}");
            }
            ZipFile.ExtractToDirectory(zipPath, Path.Join(PAExtras.Constants.EXPORTED_FOLDER_PATH, saveImportedName));

            var saveImportedPath = Path.Join(PAConstants.SAVES_FOLDER_PATH, saveImportedName);
            if (Directory.Exists(saveImportedPath))
            {
                Directory.Delete(saveImportedPath, true);
                PACSingletons.Instance.Logger.Log("Deleted imported save", $"save name: {saveImportedName}");
            }

            try
            {
                SaveImporter.ImportSave(saveImportedName);
            }
            catch (Exception ex)
            {
                if (Directory.Exists(saveToImportPath))
                {
                    Directory.Delete(saveToImportPath, true);
                    PACSingletons.Instance.Logger.Log("Deleted importable save", $"save name: {saveImportedName}");
                }
                
                if (Directory.Exists(saveImportedPath))
                {
                    Directory.Delete(saveImportedPath, true);
                    PACSingletons.Instance.Logger.Log("Deleted imported save", $"save name: {saveImportedName}");
                }
                return new TestResultDTO(LogSeverity.FAIL, $"\"{saveImportedName}\" save importing failed.");
            }
            if (Directory.Exists(saveToImportPath))
            {
                Directory.Delete(saveToImportPath, true);
                PACSingletons.Instance.Logger.Log("Deleted importable save", $"save name: {saveImportedName}");
            }

            var saveImportedCompressedPath = Path.Join(Constants.TEST_REFERENCE_SAVES_FOLDER_PATH, saveImportedName) + ".zip";
            if (File.Exists(saveImportedCompressedPath))
            {
                File.Delete(saveImportedCompressedPath);
                PACSingletons.Instance.Logger.Log("Deleted compressed imported save", $"save name: {saveImportedName}");
            }
            
            ZipFile.CreateFromDirectory(saveImportedPath, saveImportedCompressedPath);
            if (Directory.Exists(saveImportedPath))
            {
                Directory.Delete(saveImportedPath, true);
                PACSingletons.Instance.Logger.Log("Deleted imported save", $"save name: {saveImportedName}");
            }
            return null;
        }

        private static TestResultDTO? TestLoadSaveFromZip(string zipPath)
        {
            var saveName = Path.GetFileNameWithoutExtension(zipPath);
            PATools.DeleteSave(saveName);
            ZipFile.ExtractToDirectory(zipPath, Path.Join(PAConstants.SAVES_FOLDER_PATH, saveName));
            var success = SaveManager.LoadSave(saveName, false);
            if (!success)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"\"{saveName}\" save file loading failed.");
            }
            
            var wrongChunk = TryParseAllChunksFromFolder(saveName, $"\tChecking ({saveName})...");
            if (wrongChunk is not null)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"chunk loading failed in \"{saveName}\" save at chunk (x: {wrongChunk.Value.x}, y: {wrongChunk.Value.y}).");
            }
            PATools.DeleteSave(saveName);
            return null;
        }

        private static TestResultDTO? WorldUtilsContentTypePropertyMapDictionaryCheckPrivate(
            List<Type> requiredKeys,
            IDictionary<Type, Dictionary<TileNoiseType, double>> checkedDictionary
        )
        {
            var paAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.GetName().Name == nameof(ProgressAdventure));
            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.TryGetValue(key, out var value))
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                    continue;
                }

                if (value is null)
                {
                    errorMessages.Add($"The value of the dictionary at \"{key}\" is null.");
                }
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }
        #endregion
    }
}
