using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.SettingsManagement;
using ProgressAdventure;
using ProgressAdventure.Entity;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.SettingsManagement;
using ProgressAdventure.WorldManagement;
using ProgressAdventure.WorldManagement.Content;
using SaveFileManager;
using System.Reflection;
using Attribute = ProgressAdventure.Enums.Attribute;
using PAConstants = ProgressAdventure.Constants;
using PACTools = PACommon.Tools;
using Utils = PACommon.Utils;

namespace ProgressAdventureTests
{
    public static class Tests
    {
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
                checkedDictionary = Utils.GetInternalFieldFromStaticClass<IDictionary<Facing, (int x, int y)>>(typeof(EntityUtils), "facingToMovementVectorMap");
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"Exeption because of (outdated?) test structure in {nameof(EntityUtils)}: " + ex);
            }

            var existingValues = new List<(int x, int y)>();

            foreach (var key in requiredKeys)
            {
                if (checkedDictionary.TryGetValue(key, out (int x, int y) value))
                {
                    if (
                        existingValues.Contains(value)
                    )
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The dictionary already contains the value \"{value}\", associated with \"{key}\".");
                    }
                    else
                    {
                        if (
                            value.x < -1 || value.x > 1 ||
                            value.y < -1 || value.y > 1
                        )
                        {
                            return new TestResultDTO(LogSeverity.FAIL, $"The value associated to \"{key}\" is wrong.");
                        }
                        existingValues.Add(value);
                    }
                }
                else
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a value for \"{key}\".");
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the EntityUtils, attributes stat change dictionary contains all required keys and correct values.
        /// </summary>
        public static TestResultDTO? EntityUtilsAttributeStatsChangeDictionaryCheck()
        {
            var requiredKeys = Enum.GetValues<Attribute>();
            IDictionary<Attribute, (double maxHp, double attack, double defence, double agility)> checkedDictionary;

            try
            {
                checkedDictionary = Utils.GetInternalFieldFromStaticClass<IDictionary<Attribute, (double maxHp, double attack, double defence, double agility)>>(typeof(EntityUtils), "attributeStatChangeMap");
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"Exeption because of (outdated?) test structure in {nameof(EntityUtils)}: " + ex);
            }

            var existingValues = new List<(double maxHp, double attack, double defence, double agility)>();

            foreach (var key in requiredKeys)
            {
                if (checkedDictionary.TryGetValue(key, out (double maxHp, double attack, double defence, double agility) value))
                {
                    if (existingValues.Contains(value))
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The dictionary already contains the value \"{value}\", associated with \"{key}\".");
                    }
                    else
                    {
                        existingValues.Add(value);
                    }
                }
                else
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a value for \"{key}\".");
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the EntityUtils, entity type map dictionary contains all required keys and correct values.
        /// </summary>
        public static TestResultDTO? EntityUtilsEntityTypeMapDictionaryCheck()
        {
            // get all classes that implement Entity
            var entityType = typeof(Entity);
            var paAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == nameof(ProgressAdventure)).First();
            var unfilteredTypes = paAssembly.GetTypes().Where(entityType.IsAssignableFrom);
            var filteredTypes = unfilteredTypes.Where(type => !type.IsAbstract && !type.IsInterface);

            var requiredValues = filteredTypes.ToList();
            IDictionary<string, Type> checkedDictionary;

            try
            {
                checkedDictionary = Utils.GetInternalFieldFromStaticClass<IDictionary<string, Type>>(typeof(EntityUtils), "entityTypeMap");
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"Exeption because of (outdated?) test structure in {nameof(EntityUtils)}: " + ex);
            }

            var existingKeys = new List<string>();

            foreach (var value in requiredValues)
            {
                if (checkedDictionary.Values.Contains(value))
                {
                    var key = checkedDictionary.FirstOrDefault(x => x.Value == value).Key;
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The dictionary key at value \"{value}\" is null or whitespace.");
                    }

                    if (existingKeys.Contains(key))
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The dictionary already contains the key \"{key}\", associated with \"{value}\".");
                    }
                    else
                    {
                        existingKeys.Add(key);
                    }
                }
                else
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a key for \"{value}\".");
                }
            }

            return null;
        }
        #endregion

        #region Items
        /// <summary>
        /// Checks if the ItemUtils, material properties dictionary contains all required keys and correct values.
        /// </summary>
        public static TestResultDTO? ItemUtilsMaterialPropertiesDictionaryCheck()
        {
            var requiredKeys = Enum.GetValues<Material>();
            var checkedDictionary = ItemUtils.materialProperties;

            foreach (var key in requiredKeys)
            {
                if (
                    !checkedDictionary.TryGetValue(key, out MaterialPropertiesDTO? value) ||
                    value is null
                )
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a value for \"{key}\".");
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the ItemUtils, material item attributes dictionary contains all required keys and correct values.
        /// </summary>
        public static TestResultDTO? ItemUtilsMaterialItemAttributesDictionaryCheck()
        {
            var requiredKeys = Enum.GetValues<Material>();
            var checkedDictionary = ItemUtils.materialItemAttributes;

            var existingValues = new List<string>();

            foreach (var key in requiredKeys)
            {
                if (checkedDictionary.TryGetValue(key, out MaterialItemAttributesDTO? value))
                {
                    if (value.typeName is null)
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The type name in the dictionary at \"{key}\" is null.");
                    }
                    existingValues.Add(value.typeName);
                }
                else
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a value for \"{key}\".");
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the ItemUtils, compound item attributes dictionary contains all required keys and correct values.
        /// </summary>
        public static TestResultDTO? ItemUtilsCompoundItemAttributesDictionaryCheck()
        {
            var requiredKeys = ItemUtils.GetAllItemTypes();
            var checkedDictionary = ItemUtils.compoundItemAttributes;

            var existingValues = new List<string>();

            foreach (var key in requiredKeys)
            {
                if (checkedDictionary.TryGetValue(key, out CompoundItemAttributesDTO? value))
                {
                    if (value.typeName is null)
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The type name in the dictionary at \"{key}\" is null.");
                    }
                    if (existingValues.Contains(value.typeName))
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The dictionary already contains the type name \"{value.typeName}\", associated with \"{key}\".");
                    }
                    else
                    {
                        existingValues.Add(value.typeName);
                    }
                }
                else
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a value for \"{key}\".");
                }
            }

            return null;
        }
        #endregion

        #region Settings
        /// <summary>
        /// Checks if the SettingsUtils, action type ignore mapping dictionary contains all required keys and correct values.
        /// </summary>
        public static TestResultDTO? SettingsUtilsActionTypeIgnoreMappingDictionaryCheck()
        {
            var requiredKeys = Enum.GetValues<ActionType>();
            var checkedDictionary = SettingsUtils.actionTypeIgnoreMapping;

            foreach (var key in requiredKeys)
            {
                if (checkedDictionary.TryGetValue(key, out List<GetKeyMode>? value))
                {
                    if (value is null)
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The ignore map in the dictionary at \"{key}\" is null.");
                    }
                }
                else
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a value for \"{key}\".");
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the SettingsUtils, action type response mapping dictionary contains all required keys and correct values.
        /// </summary>
        public static TestResultDTO? SettingsUtilsActionTypeResponseMappingDictionaryCheck()
        {
            var requiredKeys = Enum.GetValues<ActionType>();
            var checkedDictionary = SettingsUtils.actionTypeResponseMapping;

            var existingValues = new List<Key>();

            foreach (var key in requiredKeys)
            {
                if (checkedDictionary.TryGetValue(key, out Key value))
                {
                    if (existingValues.Contains(value))
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The dictionary already contains the value \"{value}\", associated with \"{key}\".");
                    }
                    else
                    {
                        existingValues.Add(value);
                    }
                }
                else
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a value for \"{key}\".");
                }
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

            foreach (var element in checkedDictionary)
            {
                if (existingKeys.Contains(element.Key))
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary already contains the key \"{element.Key}\".");
                }
                else
                {
                    existingKeys.Add(element.Key);
                }

                if (existingValues.Contains(element.Value))
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary already contains the value \"{element.Value}\", associated with \"{element.Key}\".");
                }
                else
                {
                    existingValues.Add(element.Value);
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the SettingsUtils, setting value type map dictionary contains all required keys and correct values.
        /// </summary>
        public static TestResultDTO? SettingsUtilsSettingValueTypeMapDictionaryCheck()
        {
            var requiredKeys = Enum.GetValues<SettingsKey>();
            var checkedDictionary = SettingsUtils.settingValueTypeMap;

            foreach (var key in requiredKeys)
            {
                if (checkedDictionary.TryGetValue(key, out Type? value))
                {
                    if (value is null)
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The ignore map in the dictionary at \"{key}\" is null.");
                    }
                }
                else
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a value for \"{key}\".");
                }
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

            foreach (var key in requiredKeys)
            {
                if (checkedDictionary.TryGetValue(key.ToString(), out object? value))
                {
                    if (value is null)
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The ignore map in the dictionary at \"{key}\" is null.");
                    }
                }
                else
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a value for \"{key}\".");
                }
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
                checkedDictionary = Utils.GetInternalFieldFromStaticClass<IDictionary<TileNoiseType, double>>(typeof(WorldUtils), "_tileNoiseOffsets");
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"Exeption because of (outdated?) test structure in {nameof(WorldUtils)}: " + ex);
            }

            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.TryGetValue(key, out double value))
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a value for \"{key}\".");
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the WorldUtils, content type map dictionary contains all required keys and correct values.
        /// </summary>
        public static TestResultDTO? WorldUtilsContentTypeMapDictionaryCheck()
        {
            // get all classes that directly implement BaseContent directly
            var baseContentType = typeof(BaseContent);
            var paAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == nameof(ProgressAdventure)).First();
            var unfilteredTypes = paAssembly.GetTypes().Where(baseContentType.IsAssignableFrom);
            var filteredTypes = unfilteredTypes.Where(type => type.IsAbstract && !type.IsInterface && type.BaseType == baseContentType);

            var requiredKeys = filteredTypes.ToList();
            IDictionary<Type, Dictionary<ContentTypeID, Type>> checkedDictionary;

            try
            {
                checkedDictionary = Utils.GetInternalFieldFromStaticClass<IDictionary<Type, Dictionary<ContentTypeID, Type>>>(typeof(WorldUtils), "contentTypeMap");
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"Exeption because of (outdated?) test structure in {nameof(WorldUtils)}: " + ex);
            }

            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.TryGetValue(key, out Dictionary<ContentTypeID, Type>? value))
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a value for \"{key}\".");
                }

                if (value is null)
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The value of the dictionary at \"{key}\" is null.");
                }

                var unfilteredSubTypes = paAssembly.GetTypes().Where(key.IsAssignableFrom);
                var filteredSubTypes = unfilteredSubTypes.Where(type => !type.IsAbstract && !type.IsInterface);

                var requiredValues = filteredSubTypes.ToList();
                var existingSubKeys = new List<ContentTypeID>();

                foreach (var subValue in requiredValues)
                {
                    if (value.ContainsValue(subValue))
                    {
                        var subKey = value.FirstOrDefault(x => x.Value == subValue).Key;
                        if (!WorldUtils.TryParseContentType(subKey.GetHashCode(), out _))
                        {
                            return new TestResultDTO(LogSeverity.FAIL, $"The sub-dictionary key at value \"{subValue}\" is not a valid ContentTypeID.");
                        }

                        if (existingSubKeys.Contains(subKey))
                        {
                            return new TestResultDTO(LogSeverity.FAIL, $"The sub-dictionary already contains the key \"{subKey}\", associated with \"{subValue}\".");
                        }
                        else
                        {
                            existingSubKeys.Add(subKey);
                        }
                    }
                    else
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The sub-dictionary doesn't contain a key for \"{subValue}\".");
                    }
                }
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
                checkedDictionary = Utils.GetInternalFieldFromStaticClass<IDictionary<Type, Dictionary<Type, Dictionary<TileNoiseType, double>>>>(typeof(WorldUtils), "_contentTypePropertyMap");
            }
            catch (Exception ex)
            {
                return new TestResultDTO(LogSeverity.FAIL, $"Exeption because of (outdated?) test structure in {nameof(WorldUtils)}: " + ex);
            }

            foreach (var key in requiredKeys)
            {
                if (!checkedDictionary.TryGetValue(key, out Dictionary<Type, Dictionary<TileNoiseType, double>>? value))
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a value for \"{key}\".");
                }

                if (value is null)
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The value of the dictionary at \"{key}\" is null.");
                }

                var unfilteredSubTypes = paAssembly.GetTypes().Where(key.IsAssignableFrom);
                var filteredSubTypes = unfilteredSubTypes.Where(type => !type.IsAbstract && !type.IsInterface);

                var requiredSubKeys = filteredSubTypes.ToList();

                foreach (var subKey in requiredSubKeys)
                {
                    if (!value.TryGetValue(subKey, out Dictionary<TileNoiseType, double>? subValue))
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The dictionary doesn't contain a value for \"{key}\".");
                    }

                    if (value is null)
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"The value of the dictionary at \"{key}\" is null.");
                    }
                }
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
            foreach (var material in Enum.GetValues<Material>())
            {
                var attributes = ItemUtils.materialItemAttributes[material];
                MaterialItem item;
                try
                {
                    item = new MaterialItem(material, amount: itemAmount);
                }
                catch (Exception ex)
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"Couldn't create item from type \"{material}\": " + ex);
                }

                allItems.Add(item);
            }

            // items loadable from json and are the same as before load
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
                    return new TestResultDTO(LogSeverity.FAIL, $"Loading item from json failed for \"{item.Type}\": " + ex);
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

                return new TestResultDTO(LogSeverity.FAIL, $"Original item, and item loaded from json are not the same for \"{item.Type}\"");
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
            foreach (var itemID in ItemUtils.GetAllItemTypes())
            {
                if (itemID != ItemType.Misc.MATERIAL)
                {
                    var attributes = ItemUtils.compoundItemAttributes[itemID];
                    CompoundItem item;
                    try
                    {
                        item = ItemUtils.CreateCompoumdItem(itemID, amount: itemAmount);
                    }
                    catch (Exception ex)
                    {
                        return new TestResultDTO(LogSeverity.FAIL, $"Couldn't create item from type \"{itemID}\": " + ex);
                    }

                    allItems.Add(item);
                }
            }

            // items loadable from json and are the same as before load
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
                    return new TestResultDTO(LogSeverity.FAIL, $"Loading item from json failed for \"{item.Type}\": " + ex);
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

                return new TestResultDTO(LogSeverity.FAIL, $"Original item, and item loaded from json are not the same for \"{item.Type}\"");
            }

            return null;
        }

        /// <summary>
        /// Checks if all entities have a type name, and can be loaded from json.
        /// </summary>
        public static TestResultDTO? AllEntitiesLoadable()
        {
            RandomStates.Initialise();

            var entityType = typeof(Entity);
            var paAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == nameof(ProgressAdventure)).First();
            var entityTypes = paAssembly.GetTypes().Where(entityType.IsAssignableFrom);
            var filteredEntityTypes = entityTypes.Where(type => type != typeof(Entity) && type != typeof(Entity<>));

            var entities = new List<Entity>();

            // check if entity exists and loadable from jsom
            foreach (var type in filteredEntityTypes)
            {
                // get entity type name
                var entityTypeMapName = "entityTypeMap";
                IDictionary<string, Type> entityTypeMap;

                try
                {
                    entityTypeMap = Utils.GetInternalFieldFromStaticClass<IDictionary<string, Type>>(typeof(EntityUtils), entityTypeMapName);
                }
                catch (Exception ex)
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"Exeption because of (outdated?) test structure in {nameof(EntityUtils)}: " + ex);
                }

                string? typeName = null;
                foreach (var eType in entityTypeMap)
                {
                    if (eType.Value == type)
                    {
                        typeName = eType.Key;
                        break;
                    }
                }

                if (typeName is null)
                {
                    return new TestResultDTO(LogSeverity.FAIL, "Entity type has no type name in entity type map");
                }


                var defEntityJson = new Dictionary<string, object?>()
                {
                    ["type"] = typeName,
                };

                Entity? entity;

                try
                {
                    Entity.AnyEntityFromJson(defEntityJson, PAConstants.SAVE_VERSION, out entity);

                    if (entity is null)
                    {
                        throw new ArgumentNullException(typeName);
                    }
                }
                catch (Exception ex)
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"Entity creation from default json failed for \"{entityType}\": " + ex);
                }

                entities.Add(entity);
            }

            // entities loadable from json and are the same as before load
            foreach (var entity in entities)
            {
                Entity? loadedEntity;

                try
                {
                    var entityJson = entity.ToJson();
                    var success = Entity.AnyEntityFromJson(entityJson, PAConstants.SAVE_VERSION, out loadedEntity);

                    if (loadedEntity is null || !success)
                    {
                        throw new ArgumentNullException(entity.GetType().ToString());
                    }
                }
                catch (Exception ex)
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"Loading entity from json failed for \"{entity.GetType()}\": " + ex);
                }

                if (
                    loadedEntity.GetType() == entity.GetType() &&
                    loadedEntity.FullName == entity.FullName &&
                    loadedEntity.MaxHp == entity.MaxHp &&
                    loadedEntity.CurrentHp == entity.CurrentHp &&
                    loadedEntity.Attack == entity.Attack &&
                    loadedEntity.Defence == entity.Defence &&
                    loadedEntity.Agility == entity.Agility
                )
                {
                    continue;
                }

                return new TestResultDTO(LogSeverity.FAIL, $"Original entity, and entity loaded from json are not the same for \"{entity.GetType()}\"");
            }

            return null;
        }
        #endregion

        #region Other tests
        /// <summary>
        /// NOT WORKING!!!<br/>
        /// Checks if all objects that implement IJsonConvertable cab be converted to and from json.<br/>
        /// ONLY CHECKS FOR SUCCESFUL CONVERSION. NOT IF THE RESULTING OBJECT HAS THE SAME VALUES FOR ATTRIBUTES OR NOT!
        /// </summary>
        public static TestResultDTO? BasicJsonConvertTest()
        {
            // list of classes that implement "IJsonConvertable<T>"!
            var testObjects = new List<IJsonReadable>
            {
                new Caveman(),
                new Ghoul(),
                new Troll(),
                new Dragon(),
                new Player(),
                new CompoundItem(ItemType.Weapon.SWORD, new List<AItem> { new MaterialItem(Material.CLOTH) }),
                new Inventory(),
                new MaterialItem(Material.FLINT),
                new ActionKey(ActionType.ESCAPE, new List<ConsoleKeyInfo> { new ConsoleKeyInfo() }),
                new Keybinds(),
                new Chunk((1, 1)),
            };

            RandomStates.Initialise();

            // get all classes that implement IJsonConvertable<T>
            var jsonConvertableType = typeof(IJsonConvertable<>);
            var paAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == nameof(ProgressAdventure)).First();
            var unfilteredTypes = paAssembly.GetTypes().Where(jsonConvertableType.IsGenericAssignableFromType);
            var filteredTypes = unfilteredTypes.Where(type => !type.IsAbstract && !type.IsInterface);

            //check if all mocked classes are correct and present
            if (filteredTypes.Count() != testObjects.Count)
            {
                var diff = testObjects.Count - filteredTypes.Count();
                return new TestResultDTO(LogSeverity.FAIL, $"There are {Math.Abs(diff)} {(diff > 0 ? "more" : "less")} test objects in the test objects list than there should be.");
            }
            foreach (var testObject in testObjects)
            {
                if (!filteredTypes.Any(type => type == testObject.GetType()))
                {
                    return new TestResultDTO(LogSeverity.FAIL, $"The {testObject.GetType()} type object should not be in the test objects list.");
                }
            }

            //to/from json
            foreach (var testObject in testObjects)
            {
                var objJson = testObject.ToJson();
                //PACTools.FromJson<T>(objJson, PAConstants.SAVE_VERSION);
            }

            return new TestResultDTO(LogSeverity.PASS, "Not realy implemented!");
        }
        #endregion
    }
}
