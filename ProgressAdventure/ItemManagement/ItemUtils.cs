using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// Utils for items.
    /// </summary>
    public static class ItemUtils
    {
        #region Json correction dicts
        /// <summary>
        /// The dictionary pairing up old item type IDs, to their name.
        /// </summary>
        internal static readonly Dictionary<int, string> _legacyItemTypeNameMap = new()
        {
            //weapons
            [65536] = "weapon/wooden_sword",
            [65537] = "weapon/stone_sword",
            [65538] = "weapon/steel_sword",
            [65539] = "weapon/wooden_bow",
            [65540] = "weapon/steel_arrow",
            [65541] = "weapon/wooden_club",
            [65542] = "weapon/club_with_teeth",
            //defence
            [65792] = "defence/wooden_shield",
            [65793] = "defence/leather_cap",
            [65794] = "defence/leather_tunic",
            [65795] = "defence/leather_pants",
            [65796] = "defence/leather_boots",
            //materials
            [66048] = "material/bootle",
            [66049] = "material/wool",
            [66050] = "material/cloth",
            [66051] = "material/wood",
            [66052] = "material/stone",
            [66053] = "material/steel",
            [66054] = "material/gold",
            [66055] = "material/teeth",
            //misc
            [66304] = "misc/health_potion",
            [66305] = "misc/gold_coin",
            [66306] = "misc/silver_coin",
            [66307] = "misc/copper_coin",
            [66308] = "misc/rotten_flesh",
        };

        /// <summary>
        /// The dictionary pairing up old item type names, to the ones, including the material of the item.
        /// </summary>
        internal static readonly Dictionary<string, (string typeName, JsonArray partsJson)> _legacyCompoundtemMap = new()
        {
            //weapons
            ["weapon/wooden_sword"] = ("weapon/sword", new JsonArray
            {
                new JsonDictionary()
                {
                    ["type"] = "misc/sword_blade", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new JsonArray
                    {
                        new JsonDictionary() { ["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1 }
                    }
                },
                new JsonDictionary()
                {
                    ["type"] = "misc/sword_hilt", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new JsonArray
                    {
                        new JsonDictionary() {["type"] = "misc/material",["material"] = "WOOD",["amount"] = 1 }
                    }
                },
            }),
            ["weapon/stone_sword"] = ("weapon/sword", new JsonArray
            {
                new JsonDictionary()
                {
                    ["type"] = "misc/sword_blade", ["material"] = "STONE", ["amount"] = 1, ["parts"] = new JsonArray
                    {
                        new JsonDictionary() { ["type"] = "misc/material", ["material"] = "STONE", ["amount"] = 1 }
                    }
                },
                new JsonDictionary()
                {
                    ["type"] = "misc/sword_hilt", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new JsonArray
                    {
                        new JsonDictionary() { ["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1 }
                    }
                },
            }),
            ["weapon/steel_sword"] = ("weapon/sword", new JsonArray
            {
                new JsonDictionary()
                {
                    ["type"] = "misc/sword_blade", ["material"] = "STEEL", ["amount"] = 1, ["parts"] = new JsonArray
                    {
                        new JsonDictionary() { ["type"] = "misc/material", ["material"] = "STEEL", ["amount"] = 1 }
                    }
                },
                new JsonDictionary()
                {
                    ["type"] = "misc/sword_hilt", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new JsonArray
                    {
                        new JsonDictionary() { ["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1 }
                    }
                },
            }),
            ["weapon/wooden_bow"] = ("weapon/bow", new JsonArray
            {
                new JsonDictionary()
                {
                    ["type"] = "misc/rod", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new JsonArray
                    {
                        new JsonDictionary() { ["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1 }
                    }
                },
                new JsonDictionary()
                {
                    ["type"] = "misc/rod", ["material"] = "SILK", ["amount"] = 1, ["parts"] = new JsonArray
                    {
                        new JsonDictionary() { ["type"] = "misc/material", ["material"] = "SILK", ["amount"] = 1 }
                    }
                },
            }),
            ["weapon/steel_arrow"] = ("weapon/arrow", new JsonArray
            {
                new JsonDictionary()
                {
                    ["type"] = "misc/arrow_tip", ["material"] = "STEEL", ["amount"] = 1, ["parts"] = new JsonArray
                    {
                        new JsonDictionary() { ["type"] = "misc/material", ["material"] = "STEEL", ["amount"] = 1 }
                    }
                },
                new JsonDictionary()
                {
                    ["type"] = "misc/rod", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new JsonArray
                    {
                        new JsonDictionary() { ["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1 }
                    }
                },
            }),
            ["weapon/wooden_club"] = ("weapon/club", new JsonArray
            {
                new JsonDictionary() {["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1}
            }),
            ["weapon/club_with_teeth"] = ("weapon/club_with_teeth", new JsonArray
            {
                new JsonDictionary()
                {
                    ["type"] = "weapon/club", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new JsonArray
                    {
                        new JsonDictionary() { ["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1 }
                    }
                },
                new JsonDictionary() {["type"] = "misc/material", ["material"] = "TEETH", ["amount"] = 1},
            }),
            //defence
            ["defence/wooden_shield"] = ("defence/shield", new JsonArray
            {
                new JsonDictionary() {["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1}
            }),
            ["defence/leather_cap"] = ("defence/helmet", new JsonArray
            {
                new JsonDictionary() {["type"] = "misc/material", ["material"] = "LEATHER", ["amount"] = 1}
            }),
            ["defence/leather_tunic"] = ("defence/chestplate", new JsonArray
            {
                new JsonDictionary() {["type"] = "misc/material", ["material"] = "LEATHER", ["amount"] = 1}
            }),
            ["defence/leather_pants"] = ("defence/pants", new JsonArray
            {
                new JsonDictionary() {["type"] = "misc/material", ["material"] = "LEATHER", ["amount"] = 1}
            }),
            ["defence/leather_boots"] = ("defence/boots", new JsonArray
            {
                new JsonDictionary() {["type"] = "misc/material", ["material"] = "LEATHER", ["amount"] = 1}
            }),
            //materials
            ["material/bootle"] = ("misc/bottle", new JsonArray
            {
                new JsonDictionary() {["type"] = "misc/material", ["material"] = "GLASS", ["amount"] = 1}
            }),
            //misc
            ["misc/health_potion"] = ("misc/filled_bottle", new JsonArray
            {
                new JsonDictionary()
                {
                    ["type"] = "misc/bottle", ["material"] = "GLASS", ["amount"] = 1, ["parts"] = new JsonArray
                    {
                        new JsonDictionary() { ["type"] = "misc/material", ["material"] = "GLASS", ["amount"] = 1 }
                    }
                },
                new JsonDictionary() {["type"] = "misc/material", ["material"] = "HEALING_LIQUID", ["amount"] = 1},
            }),
            ["misc/gold_coin"] = ("misc/coin", new JsonArray
            {
                new JsonDictionary() {["type"] = "misc/material", ["material"] = "GOLD", ["amount"] = 1}
            }),
            ["misc/silver_coin"] = ("misc/coin", new JsonArray
            {
                new JsonDictionary() {["type"] = "misc/material", ["material"] = "SILVER", ["amount"] = 1}
            }),
            ["misc/copper_coin"] = ("misc/coin", new JsonArray
            {
                new JsonDictionary() {["type"] = "misc/material", ["material"] = "COPPER", ["amount"] = 1}
            }),
        };

        /// <summary>
        /// The dictionary pairing up old item type names, to the ones, including the material of the item.
        /// </summary>
        internal static readonly Dictionary<string, string> _legacyMaterialItemMap = new()
        {
            //materials
            ["material/wool"] = "wool",
            ["material/cloth"] = "cloth",
            ["material/wood"] = "wood",
            ["material/stone"] = "stone",
            ["material/steel"] = "steel",
            ["material/gold"] = "gold",
            ["material/teeth"] = "teeth",
            //misc
            ["misc/rotten_flesh"] = "rotten_flesh",
        };
        #endregion

        #region Internal constants
        /// <summary>
        /// The item type for a material.
        /// </summary>
        internal static readonly EnumTreeValue<ItemType> MATERIAL_ITEM_TYPE = ItemType.Misc.MATERIAL;
        /// <summary>
        /// The type name of a material item.
        /// </summary>
        internal static readonly string MATERIAL_TYPE_NAME = MATERIAL_ITEM_TYPE.FullName;
        #endregion

        #region Default config values
        /// <summary>
        /// The default value for the config used for the values of <see cref="Material"/>.
        /// </summary>
        private static readonly List<EnumValue<Material>> _defaultMaterials =
        [
            Material.WOOD,
            Material.STONE,
            Material.COPPER,
            Material.BRASS,
            Material.IRON,
            Material.STEEL,
            Material.GLASS,
            Material.LEATHER,
            Material.TEETH,
            Material.WOOL,
            Material.CLOTH,
            Material.SILVER,
            Material.GOLD,
            Material.ROTTEN_FLESH,
            Material.HEALING_LIQUID,
            Material.FLINT,
            Material.SILK,
        ];

        /// <summary>
        /// The default value for the config used for the values of <see cref="ItemType"/>.
        /// </summary>
        private static readonly List<EnumTreeValue<ItemType>> _defaultItemTypes =
        [
            //weapons
            ItemType.Weapon.SWORD,
            ItemType.Weapon.BOW,
            ItemType.Weapon.ARROW,
            ItemType.Weapon.CLUB,
            ItemType.Weapon.CLUB_WITH_TEETH,
            //defence
            ItemType.Defence.SHIELD,
            ItemType.Defence.HELMET,
            ItemType.Defence.CHESTPLATE,
            ItemType.Defence.PANTS,
            ItemType.Defence.BOOTS,
            //misc
            MATERIAL_ITEM_TYPE,
            ItemType.Misc.BOTTLE,
            ItemType.Misc.FILLED_BOTTLE,
            ItemType.Misc.COIN,
            ItemType.Misc.SWORD_BLADE,
            ItemType.Misc.SWORD_HILT,
            ItemType.Misc.ARROW_TIP,
            ItemType.Misc.ROD,
        ];

        /// <summary>
        /// The default value for the config used for the value of <see cref="CompoundItemAttributes"/>.
        /// </summary>
        private static readonly Dictionary<EnumTreeValue<ItemType>, CompoundItemAttributesDTO> _defaultCompoundItemAttributes = new()
        {
            //weapons
            [ItemType.Weapon.SWORD] = new CompoundItemAttributesDTO(ItemType.Weapon.SWORD),
            [ItemType.Weapon.BOW] = new CompoundItemAttributesDTO(ItemType.Weapon.BOW),
            [ItemType.Weapon.ARROW] = new CompoundItemAttributesDTO(ItemType.Weapon.ARROW),
            [ItemType.Weapon.CLUB] = new CompoundItemAttributesDTO(ItemType.Weapon.CLUB),
            [ItemType.Weapon.CLUB_WITH_TEETH] = new CompoundItemAttributesDTO("*/0MC/* club with */1ML/*"),
            //defence
            [ItemType.Defence.SHIELD] = new CompoundItemAttributesDTO(ItemType.Defence.SHIELD),
            [ItemType.Defence.HELMET] = new CompoundItemAttributesDTO(ItemType.Defence.HELMET),
            [ItemType.Defence.CHESTPLATE] = new CompoundItemAttributesDTO(ItemType.Defence.CHESTPLATE),
            [ItemType.Defence.PANTS] = new CompoundItemAttributesDTO(ItemType.Defence.PANTS),
            [ItemType.Defence.BOOTS] = new CompoundItemAttributesDTO(ItemType.Defence.BOOTS),
            //misc
            [MATERIAL_ITEM_TYPE] = new CompoundItemAttributesDTO(MATERIAL_ITEM_TYPE, ItemAmountUnit.KG),
            [ItemType.Misc.BOTTLE] = new CompoundItemAttributesDTO(ItemType.Misc.BOTTLE),
            [ItemType.Misc.FILLED_BOTTLE] = new CompoundItemAttributesDTO("*/0MC/* bottle of */1MC/*"),
            [ItemType.Misc.COIN] = new CompoundItemAttributesDTO(ItemType.Misc.COIN),
            [ItemType.Misc.SWORD_BLADE] = new CompoundItemAttributesDTO(ItemType.Misc.SWORD_BLADE),
            [ItemType.Misc.SWORD_HILT] = new CompoundItemAttributesDTO(ItemType.Misc.SWORD_HILT),
            [ItemType.Misc.ARROW_TIP] = new CompoundItemAttributesDTO(ItemType.Misc.ARROW_TIP),
            [ItemType.Misc.ROD] = new CompoundItemAttributesDTO(ItemType.Misc.ROD),
        };

        /// <summary>
        /// The default value for the config used for the value of <see cref="MaterialItemAttributes"/>.
        /// </summary>
        private static readonly Dictionary<EnumValue<Material>, MaterialItemAttributesDTO> _defaultMaterialItemAttributes = new()
        {
            [Material.BRASS] = new MaterialItemAttributesDTO(Material.BRASS, new MaterialPropertiesDTO(8730)),
            [Material.CLOTH] = new MaterialItemAttributesDTO(Material.CLOTH, new MaterialPropertiesDTO(1550)),
            [Material.COPPER] = new MaterialItemAttributesDTO(Material.COPPER, new MaterialPropertiesDTO(8960)),
            [Material.GLASS] = new MaterialItemAttributesDTO(Material.GLASS, new MaterialPropertiesDTO(2500)),
            [Material.GOLD] = new MaterialItemAttributesDTO(Material.GOLD, new MaterialPropertiesDTO(19300)),
            [Material.IRON] = new MaterialItemAttributesDTO(Material.IRON, new MaterialPropertiesDTO(7875)),
            [Material.LEATHER] = new MaterialItemAttributesDTO(Material.LEATHER, new MaterialPropertiesDTO(800)),
            [Material.ROTTEN_FLESH] = new MaterialItemAttributesDTO(Material.ROTTEN_FLESH, new MaterialPropertiesDTO(1000)),
            [Material.SILVER] = new MaterialItemAttributesDTO(Material.SILVER, new MaterialPropertiesDTO(10490)),
            [Material.STEEL] = new MaterialItemAttributesDTO(Material.STEEL, new MaterialPropertiesDTO(7900)),
            [Material.STONE] = new MaterialItemAttributesDTO(Material.STONE, new MaterialPropertiesDTO(2650)),
            [Material.TEETH] = new MaterialItemAttributesDTO(Material.TEETH, new MaterialPropertiesDTO(2900)),
            [Material.WOOD] = new MaterialItemAttributesDTO(Material.WOOD, new MaterialPropertiesDTO(600)),
            [Material.WOOL] = new MaterialItemAttributesDTO(Material.WOOL, new MaterialPropertiesDTO(1241)),
            [Material.HEALING_LIQUID] = new MaterialItemAttributesDTO(Material.HEALING_LIQUID, new MaterialPropertiesDTO(1015), ItemAmountUnit.L),
            [Material.FLINT] = new MaterialItemAttributesDTO(Material.FLINT, new MaterialPropertiesDTO(2596)),
            [Material.SILK] = new MaterialItemAttributesDTO(Material.SILK, new MaterialPropertiesDTO(1400)),
            // TODO: diferent density per state
            // M    : SOLID, LIQUID, GAS
            // WATER: 920, 1000, 3 kg/m^3
        };

        /// <summary>
        /// The default value for the config used for the value of <see cref="ItemRecipes"/>.
        /// </summary>
        private static Dictionary<EnumTreeValue<ItemType>, List<RecipeDTO>> _defaultItemRecipes;
        private static void LoadDefaultItemRecipes()
        {
            _defaultItemRecipes ??= new()
            {
                // weapon
                [ItemType.Weapon.SWORD] =
                [
                    new([new(ItemType.Misc.SWORD_BLADE, 1), new(ItemType.Misc.SWORD_HILT, 1)]),
                ],
                [ItemType.Weapon.BOW] =
                [
                    new([new(ItemType.Misc.ROD, 1), new(ItemType.Misc.ROD, 1)]),
                ],
                [ItemType.Weapon.ARROW] =
                [
                    new([new(ItemType.Misc.ARROW_TIP, 1), new(ItemType.Misc.ROD, 1)]),
                ],
                [ItemType.Weapon.CLUB] =
                [
                    new([new(0.5, ItemAmountUnit.M3)]),
                ],
                [ItemType.Weapon.CLUB_WITH_TEETH] =
                [
                    new([new(ItemType.Weapon.CLUB, 1), new(Material.TEETH, 1, ItemAmountUnit.KG)]),
                ],
                // defence
                [ItemType.Defence.SHIELD] =
                [
                    new([new(0.6, ItemAmountUnit.M3)]),
                ],
                [ItemType.Defence.HELMET] =
                [
                    new([new(0.5, ItemAmountUnit.M3)]),
                ],
                [ItemType.Defence.CHESTPLATE] =
                [
                    new([new(0.9, ItemAmountUnit.M3)]),
                ],
                [ItemType.Defence.PANTS] =
                [
                    new([new(0.7, ItemAmountUnit.M3)]),
                ],
                [ItemType.Defence.BOOTS] =
                [
                    new([new(0.4, ItemAmountUnit.M3)]),
                ],
                // misc
                [ItemType.Misc.FILLED_BOTTLE] =
                [
                    new([new(ItemType.Misc.BOTTLE, 1), new(Material.HEALING_LIQUID, 0.5, ItemAmountUnit.L)]),
                ],
                [ItemType.Misc.BOTTLE] =
                [
                    new([new(6e-4, ItemAmountUnit.M3)]),
                ],
                [ItemType.Misc.COIN] =
                [
                    new([new(7e-6, ItemAmountUnit.M3)]),
                ],
            };
        }
        #endregion

        #region Config values
        /// <summary>
        /// The dictionary pairing up item types, to their attributes.
        /// </summary>
        public static Dictionary<EnumTreeValue<ItemType>, CompoundItemAttributesDTO> CompoundItemAttributes { get; private set; }

        /// <summary>
        /// The dictionary pairing up material types, to their item attributes.
        /// </summary>
        public static Dictionary<EnumValue<Material>, MaterialItemAttributesDTO> MaterialItemAttributes { get; private set; }

        /// <summary>
        /// The dictionary pairing up item types, to their recipes, if a recipe exists for that item type.
        /// </summary>
        public static Dictionary<EnumTreeValue<ItemType>, List<RecipeDTO>> ItemRecipes { get; private set; }
        #endregion

        #region Constructors
        static ItemUtils()
        {
            _ = _legacyItemTypeNameMap;
            _ = _legacyCompoundtemMap;
            _ = _legacyMaterialItemMap;

            LoadDefaultConfigs1();

            LoadDefaultItemRecipes();

            LoadDefaultConfigs2();
        }
        #endregion

        #region Public fuctions
        #region Configs
        #region Write default config or get reload common data
        private static (string configName, bool paddingData) WriteDefaultConfigOrGetReloadDataMaterials(bool isWriteConfig)
        {
            var basePath = Path.Join(Constants.CONFIGS_ITEM_SUBFOLDER_NAME, "materials");
            if (isWriteConfig)
            {
                PACSingletons.Instance.ConfigManager.SetConfig(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultMaterials
                );
                return default;
            }
            return (basePath, false);
        }

        private static (string configName, bool paddingData) WriteDefaultConfigOrGetReloadDataItemTypes(bool isWriteConfig)
        {
            var basePath = Path.Join(Constants.CONFIGS_ITEM_SUBFOLDER_NAME, "item_types");
            if (isWriteConfig)
            {
                PACSingletons.Instance.ConfigManager.SetConfig(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultItemTypes
                );
                return default;
            }
            return (basePath, false);
        }

        private static (
            string configName,
            Func<EnumTreeValue<ItemType>, string> serializeKeys
        ) WriteDefaultConfigOrGetReloadDataCompoundItemAttributes(bool isWriteConfig)
        {
            var basePath = Path.Join(Constants.CONFIGS_ITEM_SUBFOLDER_NAME, "compound_item_attributes");
            static string KeySerializer(EnumTreeValue<ItemType> key) => key.FullName!;
            if (!isWriteConfig)
            {
                return (basePath, KeySerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultCompoundItemAttributes,
                    KeySerializer
                );
            return default;
        }

        private static (
            string configName,
            Func<EnumValue<Material>, string> serializeKeys
        ) WriteDefaultConfigOrGetReloadDataMaterialItemAttributes(bool isWriteConfig)
        {
            var basePath = Path.Join(Constants.CONFIGS_ITEM_SUBFOLDER_NAME, "material_item_attributes");
            static string KeySerializer(EnumValue<Material> key) => key.Name!;
            if (!isWriteConfig)
            {
                return (basePath, KeySerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultMaterialItemAttributes,
                    KeySerializer
                );
            return default;
        }

        private static (
            string configName,
            Func<EnumTreeValue<ItemType>, string> serializeKeys
        ) WriteDefaultConfigOrGetReloadDataItemRecipes(bool isWriteConfig)
        {
            var basePath = Path.Join(Constants.CONFIGS_ITEM_SUBFOLDER_NAME, "item_recipes");
            static string KeySerializer(EnumTreeValue<ItemType> key) => key.FullName!;
            if (!isWriteConfig)
            {
                return (basePath, KeySerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultItemRecipes,
                    KeySerializer
                );
            return default;
        }
        #endregion

        public static void LoadDefaultConfigs1()
        {
            Tools.LoadDefultAdvancedEnum(_defaultMaterials);
            Tools.LoadDefultAdvancedEnumTree(_defaultItemTypes);
            CompoundItemAttributes = _defaultCompoundItemAttributes;
            MaterialItemAttributes = _defaultMaterialItemAttributes;
        }

        public static void LoadDefaultConfigs2()
        {
            ItemRecipes = _defaultItemRecipes;
        }

        /// <summary>
        /// Resets all variables that come from configs.
        /// </summary>
        public static void LoadDefaultConfigs()
        {
            LoadDefaultConfigs1();
            LoadDefaultConfigs2();
        }

        /// <summary>
        /// Resets all config files to their default states.
        /// </summary>
        public static void WriteDefaultConfigs()
        {
            WriteDefaultConfigOrGetReloadDataMaterials(true);
            WriteDefaultConfigOrGetReloadDataItemTypes(true);
            WriteDefaultConfigOrGetReloadDataCompoundItemAttributes(true);
            WriteDefaultConfigOrGetReloadDataMaterialItemAttributes(true);
            WriteDefaultConfigOrGetReloadDataItemRecipes(true);
        }

        private static void ReloadConfigs1(
            List<(string folderName, string namespaceName)> namespaceFolders,
            bool isVanillaInvalid,
            int? showProgressIndentation
        )
        {
            ConfigUtils.ReloadConfigsAggregateAdvancedEnum(
                WriteDefaultConfigOrGetReloadDataMaterials(false).configName,
                namespaceFolders,
                _defaultMaterials,
                isVanillaInvalid,
                showProgressIndentation,
                true
            );

            ConfigUtils.ReloadConfigsAggregateAdvancedEnumTree(
                WriteDefaultConfigOrGetReloadDataItemTypes(false).configName,
                namespaceFolders,
                _defaultItemTypes,
                isVanillaInvalid,
                showProgressIndentation,
                true
            );

            var compoundAttributesData = WriteDefaultConfigOrGetReloadDataCompoundItemAttributes(false);
            CompoundItemAttributes = ConfigUtils.ReloadConfigsAggregateDict(
                compoundAttributesData.configName,
                namespaceFolders,
                _defaultCompoundItemAttributes,
                compoundAttributesData.serializeKeys,
                key => (ItemType.TryGetValue(ConfigUtils.GetNamepsacedString(key), out var value) ? value : null)
                    ?? throw new ArgumentNullException($"Unknown item type name in \"{compoundAttributesData.configName}\" config: \"{key}\"", "item type"),
                isVanillaInvalid,
                showProgressIndentation
            );

            var materialAttributesData = WriteDefaultConfigOrGetReloadDataMaterialItemAttributes(false);
            MaterialItemAttributes = ConfigUtils.ReloadConfigsAggregateDict(
                materialAttributesData.configName,
                namespaceFolders,
                _defaultMaterialItemAttributes,
                materialAttributesData.serializeKeys,
                key => Material.GetValue(ConfigUtils.GetNamepsacedString(key)),
                isVanillaInvalid,
                showProgressIndentation
            );
        }

        private static void ReloadConfigs2(
            List<(string folderName, string namespaceName)> namespaceFolders,
            bool isVanillaInvalid,
            int? showProgressIndentation
        )
        {
            var itemRecipesData = WriteDefaultConfigOrGetReloadDataItemRecipes(false);
            ItemRecipes = ConfigUtils.ReloadConfigsAggregateDict(
                itemRecipesData.configName,
                namespaceFolders,
                _defaultItemRecipes,
                itemRecipesData.serializeKeys,
                key => ParseItemType(ConfigUtils.GetNamepsacedString(key))
                    ?? throw new ArgumentNullException($"Unknown item type name in \"{itemRecipesData.configName}\" config: \"{key}\"", "item type"),
                isVanillaInvalid,
                showProgressIndentation
            );
        }

        /// <summary>
        /// Reloads all values that come from configs.
        /// </summary>
        /// <param name="namespaceFolders">The name of the currently active config folders.</param>
        /// <param name="isVanillaInvalid">If the vanilla config is valid.</param>
        /// <param name="showProgressIndentation">If not null, shows the progress of loading the configs on the console.</param>
        public static void ReloadConfigs(
            List<(string folderName, string namespaceName)> namespaceFolders,
            bool isVanillaInvalid,
            int? showProgressIndentation = null
        )
        {
            Tools.ReloadConfigsFolderDisplayProgress(Constants.CONFIGS_ITEM_SUBFOLDER_NAME, showProgressIndentation);
            showProgressIndentation = showProgressIndentation + 1 ?? null;

            ReloadConfigs1(namespaceFolders, isVanillaInvalid, showProgressIndentation);
            ReloadConfigs2(namespaceFolders, isVanillaInvalid, showProgressIndentation);
        }
        #endregion

        /// <summary>
        /// Converts the string representation of the item's type to an item type.
        /// </summary>
        /// <param name="itemTypeName">The string representation of the item's type.</param>
        public static EnumTreeValue<ItemType>? ParseItemType(string? itemTypeName)
        {
            if (string.IsNullOrWhiteSpace(itemTypeName))
            {
                return null;
            }
            return CompoundItemAttributes.FirstOrDefault(itemAttribute => itemAttribute.Key.FullName == itemTypeName).Key; ;
        }

        /// <summary>
        /// Tries to convert the string representation of the item's type to an item type, and returns the success.
        /// </summary>
        /// <param name="itemTypeName">The string representation of the item's type.</param>
        /// <param name="itemType">The resulting item, or a default item.</param>
        public static bool TryParseItemType(string? itemTypeName, [NotNullWhen(true)] out EnumTreeValue<ItemType>? itemType)
        {
            itemType = ParseItemType(itemTypeName);
            return itemType is not null;
        }

        /// <summary>
        /// Converts the item type, to it's default display name.
        /// </summary>
        /// <param name="itemType">The item type.</param>
        public static string ItemTypeToDisplayName(EnumTreeValue<ItemType> itemType)
        {
            var displayName = ConfigUtils.RemoveNamespace(itemType.FullName)
                .Split(ItemType.LayerNameSeparator)
                .Last()
                .Replace("_", " ")
                .Capitalize();
            return string.IsNullOrWhiteSpace(displayName) ? "[INVALID ITEM NAME]" : displayName;
        }

        /// <summary>
        /// Fills in the compound item's display name, using the parts used to create it.
        /// </summary>
        /// <param name="rawDisplayName">The raw display name of the compound item, where the name of a material in the parts list can be refrenced, by replacing it by "*/[index of part in the list][T: type, M: material, N: display name][U: upper, L: lower, C: capitalise]/*".</param>
        /// <param name="parts">The parts used to create the compound item.</param>
        public static string ParseCompoundItemDisplayName(string rawDisplayName, IList<AItem> parts)
        {
            var pattern = "\\*/(\\d+)([TMN])([ULC])/\\*";
            var finalName = new StringBuilder();
            var nameParts = Regex.Split(rawDisplayName, pattern);
            for (var x = 0; x < nameParts.Length; x++)
            {
                // material index
                if (
                    x % 4 == 1 &&
                    int.TryParse(nameParts[x], out int materialIndex) &&
                    materialIndex < parts.Count
                )
                {
                    string extraText;

                    var propertyType = nameParts[x + 1];
                    var caseLetter = nameParts[x + 2];

                    if (propertyType == "T")
                    {
                        extraText = ItemTypeToDisplayName(parts[materialIndex].Type) ?? "";
                    }
                    else if (propertyType == "M")
                    {
                        extraText = ConfigUtils.RemoveNamespace(parts[materialIndex].Material.Name).Replace("_", " ");
                    }
                    else
                    {
                        extraText = parts[materialIndex].DisplayName;
                    }

                    if (caseLetter == "L")
                    {
                        extraText = extraText.ToLower();
                    }
                    else if (caseLetter == "C")
                    {
                        extraText = extraText.Capitalize();
                    }

                    finalName.Append(extraText);
                }
                // plain text
                else if (x % 4 == 0)
                {
                    finalName.Append(nameParts[x]);
                }
            }
            return finalName.ToString();
        }

        /// <summary>
        /// Gets the unit conversion multiplier, between the current item's unit, and the target unit, so that: 1 [item's unit] = [multiplier] [target unit].
        /// </summary>
        /// <param name="item">The item to get the properties from.</param>
        /// <param name="targetUnit">The unit to convert to.</param>
        public static double ConvertAmountToUnitMultiplier(AItem item, ItemAmountUnit targetUnit)
        {
            if (
                item.Unit == targetUnit ||
                item.Unit == ItemAmountUnit.AMOUNT ||
                targetUnit == ItemAmountUnit.AMOUNT
            )
            {
                return 1;
            }

            var itemUnit = item.Unit;

            double multiplier;

            // L -- KG
            if (
                (itemUnit == ItemAmountUnit.L && targetUnit == ItemAmountUnit.KG) ||
                (targetUnit == ItemAmountUnit.L && itemUnit == ItemAmountUnit.KG)
            )
            {
                multiplier = item.Density * 1000;
                return itemUnit == ItemAmountUnit.L ? multiplier : 1 / multiplier;
            }
            // M3 -- KG
            if (
                (itemUnit == ItemAmountUnit.M3 && targetUnit == ItemAmountUnit.KG) ||
                (targetUnit == ItemAmountUnit.M3 && itemUnit == ItemAmountUnit.KG)
            )
            {
                multiplier = item.Density;
                return itemUnit == ItemAmountUnit.M3 ? multiplier : 1 / multiplier;
            }
            // M3 -- L
            if (
                (itemUnit == ItemAmountUnit.M3 && targetUnit == ItemAmountUnit.L) ||
                (targetUnit == ItemAmountUnit.M3 && itemUnit == ItemAmountUnit.L)
            )
            {
                multiplier = 1000;
                return itemUnit == ItemAmountUnit.M3 ? multiplier : 1 / multiplier;
            }

            return 1;
        }

        /// <summary>
        /// Returns the amount of items that the inputed item would have, if it had the target unit.
        /// </summary>
        /// <param name="item">The item, who's amount to convert.</param>
        /// <param name="targetUnit">The unit to convert to.</param>
        public static double ConvertAmountToUnit(AItem item, ItemAmountUnit targetUnit)
        {
            return item.Amount * ConvertAmountToUnitMultiplier(item, targetUnit);
        }

        /// <summary>
        /// Creates a CompoundItem from a pre-prepared list of items.
        /// </summary>
        /// <param name="targetItem">The item to create.</param>
        /// <param name="inputItems">The pre-prepared list of items</param>
        /// <param name="recipe">The recipe to use.</param>
        /// <param name="amount">The amount of times to complete the recipe.</param>
        public static CompoundItem CreateItemFromOrderedList(
            EnumTreeValue<ItemType> targetItem,
            List<AItem> inputItems,
            RecipeDTO recipe,
            int amount = 1
        )
        {
            var parts = new List<AItem>();
            for (int x = 0; x < recipe.ingredients.Count; x++)
            {
                var ingredient = recipe.ingredients[x];
                var usedAmountMultiplier = ingredient.unit is not null
                    ? 1 / ConvertAmountToUnitMultiplier(inputItems[x], (ItemAmountUnit)ingredient.unit)
                    : 1;
                var usedAmount = usedAmountMultiplier * ingredient.amount;
                var usedItem = inputItems[x].DeepCopy();
                inputItems[x].Amount -= usedAmount * amount;

                usedItem.Amount = usedAmount;
                parts.Add(usedItem);
            }

            return new CompoundItem(targetItem, parts, recipe.resultAmount * amount);
        }

        /// <summary>
        /// Creates a CompoundItem from a list of items, like "CompleteRecipe()", but doesn't check if the given list of items can actualy create that item, and creates new items and takes more item than there are if nececary.
        /// </summary>
        /// <param name="targetItem">The item to create.</param>
        /// <param name="recipe">The recipe to use.</param>
        /// <param name="inputItems">The list of items to use.</param>
        /// <param name="amount">The amount of times to complete the recipe.</param>
        public static CompoundItem CompleteRecipeWithoutChecking(
            EnumTreeValue<ItemType> targetItem,
            RecipeDTO recipe,
            List<AItem> inputItems,
            int amount = 1
        )
        {
            var requiredItems = new List<AItem>();
            foreach (var ingredient in recipe.ingredients)
            {
                var itemFound = false;
                foreach (var item in inputItems)
                {
                    if (
                        ingredient.itemType == item.Type &&
                        (ingredient.material is null || item.Material == ingredient.material) &&
                        (ingredient.unit is not null ? ConvertAmountToUnit(item, (ItemAmountUnit)ingredient.unit) : 1) >= ingredient.amount * amount
                    )
                    {
                        requiredItems.Add(item);
                        itemFound = true;
                        break;
                    }
                }
                if (!itemFound)
                {
                    AItem createdItem;
                    if (ingredient.itemType == MATERIAL_ITEM_TYPE)
                    {
                        createdItem = new MaterialItem(ingredient.material ?? Material.WOOD, ingredient.amount);
                    }
                    else
                    {
                        createdItem = CreateCompoundItem(ingredient.itemType, ingredient.material, ingredient.amount);
                    }
                    requiredItems.Add(createdItem);
                }
            }
            return CreateItemFromOrderedList(targetItem, inputItems, recipe, amount);
        }

        /// <summary>
        /// Tries to create a compound item, from a list of ingredients.
        /// </summary>
        /// <param name="targetItem">The item type to try to create.</param>
        /// <param name="inputItems">The list of items to use, as the input for the recipe.</param>
        /// <param name="targetRecipe">The target recipe to use for that item.<br/>
        /// If null it tries to use all of them in order, until it succedes.</param>
        /// <param name="amount">How many times to complete the recipe.</param>
        public static CompoundItem? CompleteRecipe(
            EnumTreeValue<ItemType> targetItem,
            List<AItem> inputItems,
            int amount = 1,
            RecipeDTO? targetRecipe = null
        )
        {
            if (!ItemRecipes.TryGetValue(targetItem, out List<RecipeDTO>? recipes))
            {
                return null;
            }

            List<AItem>? requiredItems = null;
            RecipeDTO? usedRecipe = null;

            if (targetRecipe is not null)
            {
                usedRecipe = targetRecipe;
                requiredItems = GetRequiredItemsForRecipe(usedRecipe, inputItems, amount);
            }
            else
            {
                foreach (var recipe in recipes)
                {
                    requiredItems = GetRequiredItemsForRecipe(recipe, inputItems, amount);
                    if (requiredItems is not null)
                    {
                        usedRecipe = recipe;
                        break;
                    }
                }
                if (usedRecipe is null)
                {
                    return null;
                }
            }

            if (requiredItems is null)
            {
                return null;
            }

            return CreateItemFromOrderedList(targetItem, requiredItems, usedRecipe, amount);
        }

        /// <param name="targetRecipeIndex">The index of the target recipe to use for that item.</param>
        /// <inheritdoc cref="CompleteRecipe(EnumTreeValue{ItemType}, List{AItem}, int, RecipeDTO?)"/>
        public static CompoundItem? CompleteRecipe(
            EnumTreeValue<ItemType> targetItem,
            List<AItem> inputItems,
            int amount,
            int targetRecipeIndex
        )
        {
            if (!ItemRecipes.TryGetValue(targetItem, out List<RecipeDTO>? recipes))
            {
                return null;
            }

            if (targetRecipeIndex < 0 || targetRecipeIndex > recipes.Count - 1)
            {
                PACSingletons.Instance.Logger.Log("Item making error", "invalid item recipe index", LogSeverity.WARN);
                return null;
            }

            var usedRecipe = recipes[targetRecipeIndex];

            return CompleteRecipe(targetItem, inputItems, amount, usedRecipe);
        }

        /// <summary>
        /// Creates a new compound item from the target type.
        /// </summary>
        /// <param name="targetItem">The item type to create.</param>
        /// <param name="materials">The materials to use, for the parts of the item, if posible.</param>
        /// <param name="amount">How much of the item to create.</param>
        /// <param name="targetRecipeTree">The recipe tree for target recipe to use for that item.<br/>
        /// If null it tries to use the first recipe that creates the amount of items that were requested.</param>
        public static CompoundItem CreateCompoundItem(
            EnumTreeValue<ItemType> targetItem,
            List<EnumValue<Material>?> materials,
            double amount = 1,
            RecipeTreeDTO? targetRecipeTree = null
        )
        {
            // not craftable
            if (!ItemRecipes.TryGetValue(targetItem, out List<RecipeDTO>? recipes))
            {
                return new CompoundItem(targetItem, [new MaterialItem(materials?.First() ?? Material.WOOD)], amount);
            }

            // get recipe
            RecipeDTO recipe;
            if (targetRecipeTree is not null)
            {
                recipe = recipes[Math.Clamp((int)targetRecipeTree.recipeIndex, 0, recipes.Count - 1)];
            }
            else
            {
                var selectedRecipe = recipes.First();
                foreach (var candidateRecipe in recipes)
                {
                    if (candidateRecipe.resultAmount == amount)
                    {
                        selectedRecipe = candidateRecipe;
                        break;
                    }
                }
                recipe = selectedRecipe;
            }

            // get parts
            var parts = new List<AItem>();

            for (var x = 0; x < recipe.ingredients.Count; x++)
            {
                var ingredient = recipe.ingredients[x];
                var material = ingredient.material ?? (materials is not null && materials.Count > x ? materials[x] : null);
                AItem part;
                if (ingredient.itemType == MATERIAL_ITEM_TYPE)
                {
                    part = new MaterialItem(material ?? Material.WOOD, ingredient.amount);
                }
                else
                {
                    var partRecipeTree = (
                            targetRecipeTree is not null &&
                            targetRecipeTree.partRecipeTrees is not null &&
                            targetRecipeTree.partRecipeTrees.Count > x
                        ) ? targetRecipeTree.partRecipeTrees[x] : null;
                    part = CreateCompoundItem(ingredient.itemType, material, ingredient.amount, partRecipeTree);
                }
                part.Amount = ingredient.unit is not null ? ConvertAmountToUnit(part, (ItemAmountUnit)ingredient.unit) : part.Amount;
                parts.Add(part);
            }

            return new CompoundItem(targetItem, parts, amount);
        }

        /// <inheritdoc cref="CreateCompoundItem(ItemTypeID, List{Material?}?, int?, double)"/>
        /// <param name="material">The material to use, for the material of the item, if posible.</param>
        public static CompoundItem CreateCompoundItem(
            EnumTreeValue<ItemType> targetItem,
            EnumValue<Material>? material = null,
            double amount = 1,
            RecipeTreeDTO? targetRecipe = null
        )
        {
            return CreateCompoundItem(targetItem, [material], amount, targetRecipe);
        }

        /// <summary>
        /// Checks if a recipe can be completed, using the given items.
        /// </summary>
        /// <param name="recipe">The recipe to check.</param>
        /// <param name="inputItems">The items to use, to check, if the recipe can be completed with.</param>
        /// <param name="amount">The amount of times to try and complete the recipe.</param>
        /// <returns>A list of items that would be required to complete the recipe the required amount of times, or null, if it can't be completed.</returns>
        public static List<AItem>? GetRequiredItemsForRecipe(RecipeDTO recipe, List<AItem> inputItems, int amount = 1)
        {
            var requiredItems = new List<AItem>();

            foreach (var ingredient in recipe.ingredients)
            {
                var itemFound = false;
                foreach (var item in inputItems)
                {
                    if (
                        item.Type == ingredient.itemType &&
                        (ingredient.material is null || item.Material == ingredient.material) &&
                        (ingredient.unit is not null ? ConvertAmountToUnit(item, (ItemAmountUnit)ingredient.unit) : item.Amount) >= ingredient.amount * amount &&
                        !requiredItems.Contains(item)
                    )
                    {
                        requiredItems.Add(item);
                        itemFound = true;
                        break;
                    }
                }

                if (!itemFound)
                {
                    return null;
                }
            }
            return requiredItems;
        }
        #endregion
    }
}
