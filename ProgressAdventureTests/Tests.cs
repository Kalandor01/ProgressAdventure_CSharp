using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using PACommon.SettingsManagement;
using PACommon.TestUtils;
using ProgressAdventure;
using ProgressAdventure.Entity;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.SettingsManagement;
using ProgressAdventure.WorldManagement;
using ProgressAdventure.WorldManagement.Content;
using System.IO.Compression;
using System.Reflection;
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
                if (checkedDictionary.TryGetValue(key, out (int x, int y) value))
                {
                    if (
                        existingValues.Contains(value)
                    )
                    {
                        errorMessages.Add($"The dictionary already contains the value \"{value}\", associated with \"{key}\".");
                        continue;
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
                    continue;
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
                if (checkedDictionary.TryGetValue(key, out (double maxHp, double attack, double defence, double agility) value))
                {
                    if (existingValues.Contains(value))
                    {
                        errorMessages.Add($"The dictionary already contains the value \"{value}\", associated with \"{key}\".");
                        continue;
                    }
                    else
                    {
                        existingValues.Add(value);
                    }
                }
                else
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                    continue;
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
                    string.IsNullOrWhiteSpace(property.displayName) ||
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
                    continue;
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
        public static TestResultDTO? ItemUtilsMaterialItemAttributesDictionaryCheck()
        {
            var requiredKeys = Material.GetValues();
            var checkedDictionary = ItemUtils.MaterialItemAttributes;

            var existingValues = new List<EnumValue<Material>>();

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.TryGetValue(key, out MaterialItemAttributesDTO? _))
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
        public static TestResultDTO? ItemUtilsCompoundItemAttributesDictionaryCheck()
        {
            var requiredKeys = ItemType.GetAllValues().Where(v => ItemType.GetValues(v).Count == 0).ToList();
            var checkedDictionary = ItemUtils.CompoundItemAttributes;

            var existingValues = new List<string>();

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.TryGetValue(key, out CompoundItemAttributesDTO? _))
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
        /// Checks if the ItemUtils, compound item attributes dictionary contains all required keys and correct values.
        /// </summary>
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
                foreach (var itemRecipe in itemRecipes.Value)
                {
                    if (itemRecipe is null)
                    {
                        errorMessages.Add($"A recipe in the recipes list at item type \"{itemRecipes.Key}\" is null.");
                        continue;
                    }
                    if (
                        ItemUtils.CompoundItemAttributes[itemRecipes.Key].unit == ItemAmountUnit.AMOUNT &&
                        itemRecipe.resultAmount % 1 != 0
                    )
                    {
                        errorMessages.Add($"A recipe in the recipes list at item type \"{itemRecipes.Key}\" has a {nameof(itemRecipe.resultAmount)} that isn't an integer, but the item type that will be created can only have an integer amount.");
                        continue;
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
                    continue;
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
        public static TestResultDTO? SettingsUtilsSettingValueTypeMapDictionaryCheck()
        {
            var requiredKeys = Enum.GetValues<SettingsKey>();
            var checkedDictionary = SettingsUtils.SettingValueTypeMap;

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.ContainsKey(key))
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                    continue;
                }
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            return null;
        }

        /// <summary>
        /// Checks if the SettingsUtils, <c>GetDefaultSettings()</c> dictionary contains all required keys and correct values.
        /// </summary>
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
                    continue;
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
                if (!checkedDictionary.TryGetValue(key, out double value))
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                    continue;
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
        public static TestResultDTO? WorldUtilsContentTypeMapDictionaryCheck()
        {
            // get all classes that directly implement BaseContent directly
            var paAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == nameof(ProgressAdventure)).First();

            var requiredKeys = ContentType.GetValues(null);
            IDictionary<EnumTreeValue<ContentType>, Dictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO>> checkedDictionary;
            IDictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO> checkedDictionary2;

            try
            {
                checkedDictionary = Utils.GetInternalFieldFromStaticClass<IDictionary<EnumTreeValue<ContentType>, Dictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO>>>(typeof(WorldUtils), "contentTypeSubtypesMap");
                checkedDictionary2 = Utils.GetInternalPropertyFromStaticClass<IDictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO>>(typeof(WorldUtils), "BaseContentTypeMap");
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"\n\tExeption because of (outdated?) test structure in {nameof(WorldUtils)}: " + ex);
            }

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary2.TryGetValue(key, out var baseProps))
                {
                    errorMessages.Add($"The dictionary2 doesn't contain a value for \"{key}\".");
                    continue;
                }

                if (baseProps is null)
                {
                    errorMessages.Add($"The value of the dictionary2 at \"{key}\" is null.");
                    continue;
                }

                if (!checkedDictionary.TryGetValue(key, out var subTypeProps))
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                    continue;
                }

                if (subTypeProps is null)
                {
                    errorMessages.Add($"The value of the dictionary at \"{key}\" is null.");
                    continue;
                }

                var unfilteredSubTypes = paAssembly.GetTypes().Where(baseProps.matchingType.IsAssignableFrom);
                var filteredSubTypes = unfilteredSubTypes.Where(type => !type.IsAbstract && !type.IsInterface);

                var requiredValues = filteredSubTypes.ToList();
                var requiredSubkeys = ContentType.GetValues(key);
                var existingSubKeys = new List<EnumTreeValue<ContentType>>();
                
                foreach (var subValue in requiredValues)
                {
                    if (!subTypeProps.Any(subProp => subProp.Value.matchingType == subValue))
                    {
                        errorMessages.Add($"The sub-dictionary doesn't contain a key for \"{subValue}\".");
                        continue;
                    }

                    var subKey = subTypeProps.FirstOrDefault(x => x.Value.matchingType == subValue).Key;
                    if (!ContentType.TryGetValue(subKey.FullName, out _))
                    {
                        errorMessages.Add($"The sub-dictionary key at value \"{subValue}\" is not a valid ContentType.");
                        continue;
                    }

                    if (!requiredSubkeys.Any(rSubKey => rSubKey == subKey))
                    {
                        errorMessages.Add($"The sub-dictionary key \"{subKey}\" is not a subKey for the key \"{key}\".");
                        continue;
                    }

                    if (existingSubKeys.Contains(subKey))
                    {
                        errorMessages.Add($"The sub-dictionary already contains the key \"{subKey}\", associated with \"{subValue}\".");
                        continue;
                    }

                    existingSubKeys.Add(subKey);
                }
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
        public static TestResultDTO? WorldUtilsContentTypePropertyMapDictionaryCheck()
        {
            // get all classes that directly implement BaseContent directly
            var baseContentType = typeof(BaseContent);
            var paAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == nameof(ProgressAdventure)).First();
            var unfilteredTypes = paAssembly.GetTypes().Where(baseContentType.IsAssignableFrom);
            var filteredTypes = unfilteredTypes.Where(type => type.IsAbstract && !type.IsInterface && type.BaseType == baseContentType);

            var requiredKeys = filteredTypes.ToList();
            IDictionary<Type, Dictionary<Type, Dictionary<TileNoiseType, double>>> checkedDictionary;

            try
            {
                checkedDictionary = Utils.GetInternalFieldFromStaticClass<IDictionary<Type, Dictionary<Type, Dictionary<TileNoiseType, double>>>>(typeof(WorldUtils), "contentTypePropertyMap");
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"\n\tExeption because of (outdated?) test structure in {nameof(WorldUtils)}: " + ex);
            }

            var errorMessages = new List<string>();
            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.TryGetValue(key, out Dictionary<Type, Dictionary<TileNoiseType, double>>? value))
                {
                    errorMessages.Add($"The dictionary doesn't contain a value for \"{key}\".");
                    continue;
                }

                if (value is null)
                {
                    errorMessages.Add($"The value of the dictionary at \"{key}\" is null.");
                    continue;
                }

                var unfilteredSubTypes = paAssembly.GetTypes().Where(key.IsAssignableFrom);
                var filteredSubTypes = unfilteredSubTypes.Where(type => !type.IsAbstract && !type.IsInterface);

                var requiredSubKeys = filteredSubTypes.ToList();

                foreach (var subKey in requiredSubKeys)
                {
                    if (!value.TryGetValue(subKey, out Dictionary<TileNoiseType, double>? subValue))
                    {
                        errorMessages.Add($"The sub-dictionary doesn't contain a value for \"{subKey}\".");
                        continue;
                    }

                    if (subValue is null)
                    {
                        errorMessages.Add($"The value of the sub-dictionary at \"{subKey}\" is null.");
                        continue;
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
        #endregion

        #region Exists and loadable tests
        /// <summary>
        /// Checks if all material enums values can be turned into material items.
        /// </summary>
        public static TestResultDTO? AllMaterialItemTypesExistAndLoadable()
        {
            var itemAmount = 3;

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
                continue;
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
        public static TestResultDTO? AllCompoundItemTypesExistAndLoadable()
        {
            var itemAmount = 3;

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
            foreach (var entityType in EntityType.GetValues())
            {
                // get entity type properties
                if (!entitypropertiesMap.TryGetValue(entityType, out var properties))
                {
                    errorMessages.Add("Entity type has no properties in entity properties map");
                    continue;
                }

                var defEntityJson = new JsonDictionary()
                {
                    ["type"] = entityType,
                };

                Entity? entity;

                try
                {
                    PACSingletons.Instance.Logger.Log("Beggining mock entity creation", "ignore \"value is null\" warnings", LogSeverity.OTHER);
                    PACTools.TryFromJson(defEntityJson, PAConstants.SAVE_VERSION, out entity);
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
                    var success = PACTools.TryFromJson(entityJson, PAConstants.SAVE_VERSION, out loadedEntity);

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
                    loadedEntity.originalTeam == entity.originalTeam &&
                    loadedEntity.currentTeam == entity.currentTeam &&
                    loadedEntity.attributes.SequenceEqual(entity.attributes) &&
                    loadedEntity.drops.SequenceEqual(entity.drops) &&
                    (
                        (loadedEntity.TryGetInventory() is null && entity.TryGetInventory() is null) ||
                        (
                            loadedEntity.TryGetInventory() is Inventory loadedInv &&
                            entity.TryGetInventory() is Inventory entityInv &&
                            loadedInv.items.SequenceEqual(entityInv.items)
                        )
                    )
                )
                {
                    continue;
                }

                errorMessages.Add($"Original entity, and entity loaded from json are not the same for \"{entity.type}\"");
                continue;
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
        /// NOT COMPLETE!!!<br/>
        /// Checks if all reference save files can be loaded without errors or warnings.<br/>
        /// ONLY CHECKS FOR SUCCESFUL LOADING. NOT IF THE RESULTING OBJECT HAS THE SAME VALUES!
        /// </summary>
        public static TestResultDTO? BasicAllMainSaveFileVersionsLoadable()
        {
            // create current refrence save
            var currentSaveName = "current";
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
            Console.WriteLine();
            var importOverallSuccess = true;
            foreach (var importedZipPath in importedZipPaths)
            {
                var result = TestImportSaveFromZip(importedZipPath) ?? new TestResultDTO(LogSeverity.PASS);
                var saveName = Path.GetFileNameWithoutExtension(importedZipPath);

                var resultString = TestingUtils.GetResultString(result);
                Console.WriteLine($"\r\tImporting ({saveName})..." + resultString);
                var messageText = result.resultMessage is null ? "" : ": " + result.resultMessage;
                PACSingletons.Instance.Logger.Log(Path.GetFileNameWithoutExtension(importedZipPath), result.resultType + messageText, LogSeverity.OTHER);
                importOverallSuccess &= result.resultType == LogSeverity.PASS;
            }
            var overallResult = importOverallSuccess ? new TestResultDTO() : new TestResultDTO(LogSeverity.FAIL, "See above for details");
            Console.WriteLine($"Overall... {TestingUtils.GetResultString(overallResult)}");


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
                Console.WriteLine($"\r\tChecking ({saveName})..." + resultString);
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

            Console.Write("Overall...");
            return overallSuccess ? null : new TestResultDTO(LogSeverity.FAIL, "See above for details");
        }

        /// <summary>
        /// Checks if all objects that implement IJsonConvertable can be converted to and from json.<br/>
        /// ONLY CHECKS FOR SUCCESFUL CONVERSION. NOT IF THE RESULTING OBJECT HAS THE SAME VALUES FOR ATTRIBUTES OR NOT!
        /// </summary>
        public static TestResultDTO? BasicJsonConvertTest()
        {
            // list of classes that implement "IJsonConvertable<T>"!
            var testObjects = new List<IJsonReadable>
            {
                new Entity(EntityType.PLAYER),
                new CompoundItem(ItemType.Weapon.SWORD, [new MaterialItem(Material.CLOTH)]),
                new Inventory(),
                new MaterialItem(Material.FLINT),
                new ActionKey(ActionType.ESCAPE, [new()]),
                new Keybinds(),
                new Chunk((1, 1)),
                RandomStates.Initialize(),
                SaveData.Initialize("test", initialiseRandomGenerators: false),
                PACTools.FromJson<DisplaySaveData>(DisplaySaveData.ToJsonFromSaveData(SaveData.Instance), PAConstants.SAVE_VERSION) ?? throw new ArgumentNullException(nameof(DisplaySaveData), $"{nameof(DisplaySaveData)} from json should not be null."),
            };

            RandomStates.Initialize();

            // get all classes that implement IJsonConvertable<T>
            var jsonConvertableType = typeof(IJsonConvertable<>);
            var paAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == nameof(ProgressAdventure)).First();
            var unfilteredTypes = paAssembly.GetTypes().Where(jsonConvertableType.IsGenericAssignableFromType);
            var filteredTypes = unfilteredTypes.Where(type => !type.IsAbstract && !type.IsInterface);

            //check if all mocked classes are correct and present
            if (filteredTypes.Count() != testObjects.Count)
            {
                var diff = testObjects.Count - filteredTypes.Count();
                return new TestResultDTO(LogSeverity.FAIL, $"\n\tThere are {Math.Abs(diff)} {(diff > 0 ? "more" : "less")} test objects in the test objects list than there should be.");
            }

            var errorMessages = new List<string>();
            foreach (var testObject in testObjects)
            {
                if (!filteredTypes.Any(type => type == testObject.GetType()))
                {
                    errorMessages.Add($"The {testObject.GetType()} type object should not be in the test objects list.");
                    continue;
                }
            }
            if (errorMessages.Count != 0)
            {
                return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            }

            //to/from json
            foreach (var testObject in testObjects)
            {
                var objJson = testObject.ToJson();
                var genericConvertableTypes = testObject.GetType().GetInterfaces();
                var genericConvertableType = genericConvertableTypes.First(t => t.Name == jsonConvertableType.Name);
                var method = genericConvertableType?.GetMethod("FromJson", BindingFlags.Static | BindingFlags.Public);
                var parameters = new object?[] { objJson, PAConstants.SAVE_VERSION, null };
                var succObj = method?.Invoke(null, parameters);

                if (succObj is not bool success || !success)
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"Couldn't convert the json representation of {testObject.GetType()} back without errors/warnings.");
                }
            }

            return new TestResultDTO(LogSeverity.PASS, "Not fully implemented!");
        }

        /// <summary>
        /// Checks if all objects that implement IJsonConvertableExtra can be converted to and from json.<br/>
        /// ONLY CHECKS FOR SUCCESFUL CONVERSION. NOT IF THE RESULTING OBJECT HAS THE SAME VALUES FOR ATTRIBUTES OR NOT!
        /// </summary>
        public static TestResultDTO? BasicJsonConvertExtraTest()
        {
            // list of classes that implement "IJsonConvertableExra<T, TExtra>"!
            //var testObjects = new List<IJsonReadable>
            //{
            //    new Caveman(),
            //    new Ghoul(),
            //    new Troll(),
            //    new Dragon(),
            //    new Player(),
            //    new CompoundItem(ItemType.Weapon.SWORD, [new MaterialItem(Material.CLOTH)]),
            //    new Inventory(),
            //    new MaterialItem(Material.FLINT),
            //    new ActionKey(ActionType.ESCAPE, [new()]),
            //    new Keybinds(),
            //    new Chunk((1, 1)),
            //    RandomStates.Initialize(),
            //    SaveData.Initialize("test", initialiseRandomGenerators: false),
            //    PACTools.FromJson<DisplaySaveData>(DisplaySaveData.ToJsonFromSaveData(SaveData.Instance), PAConstants.SAVE_VERSION) ?? throw new ArgumentNullException(nameof(DisplaySaveData), $"{nameof(DisplaySaveData)} from json should not be null."),
            //};

            //RandomStates.Initialize();

            //// get all classes that implement IJsonConvertable<T>
            //var jsonConvertableType = typeof(IJsonConvertableExtra<>);
            //var paAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == nameof(ProgressAdventure)).First();
            //var unfilteredTypes = paAssembly.GetTypes().Where(jsonConvertableType.IsGenericAssignableFromType);
            //var filteredTypes = unfilteredTypes.Where(type => !type.IsAbstract && !type.IsInterface);

            ////check if all mocked classes are correct and present
            //if (filteredTypes.Count() != testObjects.Count)
            //{
            //    var diff = testObjects.Count - filteredTypes.Count();
            //    return new TestResultDTO(LogSeverity.FAIL, $"\n\tThere are {Math.Abs(diff)} {(diff > 0 ? "more" : "less")} test objects in the test objects list than there should be.");
            //}

            //var errorMessages = new List<string>();
            //foreach (var testObject in testObjects)
            //{
            //    if (!filteredTypes.Any(type => type == testObject.GetType()))
            //    {
            //        errorMessages.Add($"The {testObject.GetType()} type object should not be in the test objects list.");
            //        continue;
            //    }
            //}
            //if (errorMessages.Count != 0)
            //{
            //    return new TestResultDTO(LogSeverity.FAIL, "\n\t" + string.Join("\n\t", errorMessages));
            //}

            ////to/from json
            //foreach (var testObject in testObjects)
            //{
            //    var objJson = testObject.ToJson();
            //    var genericConvertableTypes = testObject.GetType().GetInterfaces();
            //    var genericConvertableType = genericConvertableTypes.First(t => t.Name == jsonConvertableType.Name);
            //    var method = genericConvertableType?.GetMethod("FromJson", BindingFlags.Static | BindingFlags.Public);
            //    var parameters = new object?[] { objJson, PAConstants.SAVE_VERSION, null };
            //    var succObj = method?.Invoke(null, parameters);

            //    if (succObj is not bool success || !success)
            //    {
            //        return new TestResultDTO(LogSeverity.FAIL, $"Couldn't convert the json representation of {testObject.GetType()} back without errors/warnings.");
            //    }
            //}

            return new TestResultDTO(LogSeverity.PASS, "Not implemented!");
        }

        /// <summary>
        /// Checks if all objects that implement IJsonConvertable cab be converted to and from json.
        /// </summary>
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
                var chunkExtension = chunkFileName is not null ? Path.GetExtension(chunkFileName) : null;
                if (
                    chunkFileName is not null &&
                    (chunkExtension == $".{PAConstants.SAVE_EXT}" || chunkExtension == $".{PAConstants.OLD_SAVE_EXT}") &&
                    chunkFileName.StartsWith($"{PAConstants.CHUNK_FILE_NAME}{PAConstants.CHUNK_FILE_NAME_SEP}")
                )
                {
                    var chunkPositions = Path.GetFileNameWithoutExtension(chunkFileName).Replace($"{PAConstants.CHUNK_FILE_NAME}{PAConstants.CHUNK_FILE_NAME_SEP}", "").Split(PAConstants.CHUNK_FILE_NAME_SEP);
                    if (
                        chunkPositions.Length == 2 &&
                        long.TryParse(chunkPositions[0], out long posX) &&
                        long.TryParse(chunkPositions[1], out long posY)
                    )
                    {
                        existingChunks.Add((posX, posY));
                        continue;
                    }
                    PACSingletons.Instance.Logger.Log("Chunk file parse error", $"chunk positions couldn't be extracted from chunk file name: {chunkFileName}", LogSeverity.WARN);
                }
                PACSingletons.Instance.Logger.Log("Chunk file parse error", $"file name is not chunk file name", LogSeverity.WARN);
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
                    success &= Chunk.FromFile(existingChunks[x], out var _, saveFolderName, true);
                    if (!success)
                    {
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
                    success &= Chunk.FromFile(chunkPos, out var _, saveFolderName, true);
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
                    (15, -6),
                    extraData: new Dictionary<string, object?>
                    {
                        [PAConstants.JsonKeys.Entity.Player.INVENTORY] = new Inventory(
                        [
                            new MaterialItem(Material.CLOTH, 15),
                            new MaterialItem(Material.GOLD, 5.27),
                            new MaterialItem(Material.HEALING_LIQUID, 0.3),
                            ItemUtils.CreateCompoundItem(ItemType.Weapon.SWORD, [Material.STEEL, Material.WOOD], 12),
                            ItemUtils.CreateCompoundItem(ItemType.Weapon.CLUB, [Material.WOOD], 3),
                            ItemUtils.CreateCompoundItem(ItemType.Weapon.ARROW, [Material.FLINT, Material.WOOD], 152),
                        ])
                    }
                )
            );
            World.Initialize();
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
                PAExtras.SaveImporter.ImportSave(saveImportedName);
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
            var success = SaveManager.LoadSave(saveName, false, false);
            if (!success)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"\"{saveName}\" save loading failed.");
            }
            var wrongChunk = TryParseAllChunksFromFolder(saveName, $"\tChecking ({saveName})...");
            if (wrongChunk != null)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"chunk loading failed in \"{saveName}\" save at chunk (x: {wrongChunk.Value.x}, y: {wrongChunk.Value.y}).");
            }
            PATools.DeleteSave(saveName);
            return null;
        }
        #endregion
    }
}
