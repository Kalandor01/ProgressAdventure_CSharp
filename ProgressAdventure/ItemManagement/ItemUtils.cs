using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using ProgressAdventure.Enums;
using System.Text;
using System.Text.RegularExpressions;
using PACTools = PACommon.Tools;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// Utils for items.
    /// </summary>
    public static class ItemUtils
    {
        #region Internal fields
        /// <summary>
        /// The item type for a material.
        /// </summary>
        internal static readonly ItemTypeID MATERIAL_ITEM_TYPE = ItemType.Misc.MATERIAL;
        /// <summary>
        /// The type name of a material item.
        /// </summary>
        internal static readonly string MATERIAL_TYPE_NAME = ItemIDToTypeName(MATERIAL_ITEM_TYPE);
        #endregion

        #region Public dicts
        /// <summary>
        /// The dictionary pairing up item types, to their attributes.
        /// </summary>
        public static readonly Dictionary<ItemTypeID, CompoundItemAttributesDTO> compoundItemAttributes = new()
        {
            //weapons
            [ItemType.Weapon.SWORD] = new CompoundItemAttributesDTO(ItemType.Weapon.SWORD),
            [ItemType.Weapon.BOW] = new CompoundItemAttributesDTO(ItemType.Weapon.BOW),
            [ItemType.Weapon.ARROW] = new CompoundItemAttributesDTO(ItemType.Weapon.ARROW),
            [ItemType.Weapon.CLUB] = new CompoundItemAttributesDTO(ItemType.Weapon.CLUB),
            [ItemType.Weapon.CLUB_WITH_TEETH] = new CompoundItemAttributesDTO(ItemType.Weapon.CLUB_WITH_TEETH, "*/0MC/* club with */1ML/*"),
            //defence
            [ItemType.Defence.SHIELD] = new CompoundItemAttributesDTO(ItemType.Defence.SHIELD),
            [ItemType.Defence.HELMET] = new CompoundItemAttributesDTO(ItemType.Defence.HELMET),
            [ItemType.Defence.CHESTPLATE] = new CompoundItemAttributesDTO(ItemType.Defence.CHESTPLATE),
            [ItemType.Defence.PANTS] = new CompoundItemAttributesDTO(ItemType.Defence.PANTS),
            [ItemType.Defence.BOOTS] = new CompoundItemAttributesDTO(ItemType.Defence.BOOTS),
            //misc
            [MATERIAL_ITEM_TYPE] = new CompoundItemAttributesDTO(MATERIAL_ITEM_TYPE, ItemAmountUnit.KG),
            [ItemType.Misc.BOTTLE] = new CompoundItemAttributesDTO(ItemType.Misc.BOTTLE),
            [ItemType.Misc.FILLED_BOTTLE] = new CompoundItemAttributesDTO(ItemType.Misc.FILLED_BOTTLE, "*/0ML/* bottle of */1MC/*"),
            [ItemType.Misc.COIN] = new CompoundItemAttributesDTO(ItemType.Misc.COIN),
            [ItemType.Misc.SWORD_BLADE] = new CompoundItemAttributesDTO(ItemType.Misc.SWORD_BLADE),
            [ItemType.Misc.SWORD_HILT] = new CompoundItemAttributesDTO(ItemType.Misc.SWORD_HILT),
            [ItemType.Misc.ARROW_TIP] = new CompoundItemAttributesDTO(ItemType.Misc.ARROW_TIP),
            [ItemType.Misc.ROD] = new CompoundItemAttributesDTO(ItemType.Misc.ROD),
        };

        /// <summary>
        /// The dictionary pairing up material types, to their item attributes.
        /// </summary>
        public static readonly Dictionary<Material, MaterialItemAttributesDTO> materialItemAttributes = new()
        {
            //weapons
            [Material.BRASS] = new MaterialItemAttributesDTO(Material.BRASS),
            [Material.CLOTH] = new MaterialItemAttributesDTO(Material.CLOTH),
            [Material.COPPER] = new MaterialItemAttributesDTO(Material.COPPER),
            [Material.GLASS] = new MaterialItemAttributesDTO(Material.GLASS),
            [Material.GOLD] = new MaterialItemAttributesDTO(Material.GOLD),
            [Material.IRON] = new MaterialItemAttributesDTO(Material.IRON),
            [Material.LEATHER] = new MaterialItemAttributesDTO(Material.LEATHER),
            [Material.ROTTEN_FLESH] = new MaterialItemAttributesDTO(Material.ROTTEN_FLESH),
            [Material.SILVER] = new MaterialItemAttributesDTO(Material.SILVER),
            [Material.STEEL] = new MaterialItemAttributesDTO(Material.STEEL),
            [Material.STONE] = new MaterialItemAttributesDTO(Material.STONE),
            [Material.TEETH] = new MaterialItemAttributesDTO(Material.TEETH),
            [Material.WOOD] = new MaterialItemAttributesDTO(Material.WOOD),
            [Material.WOOL] = new MaterialItemAttributesDTO(Material.WOOL),
            [Material.HEALING_LIQUID] = new MaterialItemAttributesDTO(Material.HEALING_LIQUID, ItemAmountUnit.L),
            [Material.FLINT] = new MaterialItemAttributesDTO(Material.FLINT),
            [Material.SILK] = new MaterialItemAttributesDTO(Material.SILK),
        };

        /// <summary>
        /// The dictionary pairing up material types, to their properties.
        /// </summary>
        public static readonly Dictionary<Material, MaterialPropertiesDTO> materialProperties = new()
        {
            //weapons
            [Material.BRASS] = new MaterialPropertiesDTO(8730),
            [Material.CLOTH] = new MaterialPropertiesDTO(1550),
            [Material.COPPER] = new MaterialPropertiesDTO(8960),
            [Material.GLASS] = new MaterialPropertiesDTO(2500),
            [Material.GOLD] = new MaterialPropertiesDTO(19300),
            [Material.IRON] = new MaterialPropertiesDTO(7874),
            [Material.LEATHER] = new MaterialPropertiesDTO(800),
            [Material.ROTTEN_FLESH] = new MaterialPropertiesDTO(1000),
            [Material.SILVER] = new MaterialPropertiesDTO(10490),
            [Material.STEEL] = new MaterialPropertiesDTO(7900),
            [Material.STONE] = new MaterialPropertiesDTO(2650),
            [Material.TEETH] = new MaterialPropertiesDTO(2900),
            [Material.WOOD] = new MaterialPropertiesDTO(600),
            [Material.WOOL] = new MaterialPropertiesDTO(1241),
            [Material.HEALING_LIQUID] = new MaterialPropertiesDTO(1015),
            [Material.SILK] = new MaterialPropertiesDTO(1400),
            [Material.FLINT] = new MaterialPropertiesDTO(2596),
        };

        /// <summary>
        /// The dictionary pairing up item types, to their recipes, if a recipe exists for that item type.
        /// </summary>
        public static readonly Dictionary<ItemTypeID, List<IngredientDTO>> itemRecipes = new()
        {
            // weapon
            [ItemType.Weapon.SWORD] = new List<IngredientDTO> { new IngredientDTO(ItemType.Misc.SWORD_BLADE, 1), new IngredientDTO(ItemType.Misc.SWORD_HILT, 1) },
            [ItemType.Weapon.BOW] = new List<IngredientDTO> { new IngredientDTO(ItemType.Misc.ROD, 1), new IngredientDTO(ItemType.Misc.ROD, 1) },
            [ItemType.Weapon.ARROW] = new List<IngredientDTO> { new IngredientDTO(ItemType.Misc.ARROW_TIP, 1), new IngredientDTO(ItemType.Misc.ROD, 1) },
            [ItemType.Weapon.CLUB] = new List<IngredientDTO> { new IngredientDTO(null, 0.5, ItemAmountUnit.M3) },
            [ItemType.Weapon.CLUB_WITH_TEETH] = new List<IngredientDTO> { new IngredientDTO(ItemType.Weapon.CLUB, 1), new IngredientDTO(Material.TEETH, 1, ItemAmountUnit.KG) },
            // defence
            [ItemType.Defence.SHIELD] = new List<IngredientDTO> { new IngredientDTO(null, 0.6, ItemAmountUnit.M3) },
            [ItemType.Defence.HELMET] = new List<IngredientDTO> { new IngredientDTO(null, 0.5, ItemAmountUnit.M3) },
            [ItemType.Defence.CHESTPLATE] = new List<IngredientDTO> { new IngredientDTO(null, 0.9, ItemAmountUnit.M3) },
            [ItemType.Defence.PANTS] = new List<IngredientDTO> { new IngredientDTO(null, 0.7, ItemAmountUnit.M3) },
            [ItemType.Defence.BOOTS] = new List<IngredientDTO> { new IngredientDTO(null, 0.4, ItemAmountUnit.M3) },
            // misc
            [ItemType.Misc.FILLED_BOTTLE] = new List<IngredientDTO> { new IngredientDTO(ItemType.Misc.BOTTLE, 1), new IngredientDTO(Material.HEALING_LIQUID , 0.5, ItemAmountUnit.L) },
            [ItemType.Misc.BOTTLE] = new List<IngredientDTO> { new IngredientDTO(null, 0.1, ItemAmountUnit.M3) },
            [ItemType.Misc.COIN] = new List<IngredientDTO> { new IngredientDTO(null, 0.02, ItemAmountUnit.M3) },
        };
        #endregion

        #region Internal dicts
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
        internal static readonly Dictionary<string, (string typeName, List<Dictionary<string, object?>> partsJson)> _legacyCompoundtemMap = new()
        {
            //weapons
            ["weapon/wooden_sword"] = ("weapon/sword", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?>
                {
                    ["type"] = "misc/sword_blade", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new List<Dictionary<string, object?>>
                    {
                        new Dictionary<string, object?> { ["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1 }
                    }
                },
                new Dictionary<string, object?>
                {
                    ["type"] = "misc/sword_hilt", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new List<Dictionary<string, object?>>
                    {
                        new Dictionary<string, object?> { ["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1 }
                    }
                },
            }),
            ["weapon/stone_sword"] = ("weapon/sword", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?>
                {
                    ["type"] = "misc/sword_blade", ["material"] = "STONE", ["amount"] = 1, ["parts"] = new List<Dictionary<string, object?>>
                    {
                        new Dictionary<string, object?> { ["type"] = "misc/material", ["material"] = "STONE", ["amount"] = 1 }
                    }
                },
                new Dictionary<string, object?>
                {
                    ["type"] = "misc/sword_hilt", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new List<Dictionary<string, object?>>
                    {
                        new Dictionary<string, object?> { ["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1 }
                    }
                },
            }),
            ["weapon/steel_sword"] = ("weapon/sword", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?>
                {
                    ["type"] = "misc/sword_blade", ["material"] = "STEEL", ["amount"] = 1, ["parts"] = new List<Dictionary<string, object?>>
                    {
                        new Dictionary<string, object?> { ["type"] = "misc/material", ["material"] = "STEEL", ["amount"] = 1 }
                    }
                },
                new Dictionary<string, object?>
                {
                    ["type"] = "misc/sword_hilt", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new List<Dictionary<string, object?>>
                    {
                        new Dictionary<string, object?> { ["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1 }
                    }
                },
            }),
            ["weapon/wooden_bow"] = ("weapon/bow", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?>
                {
                    ["type"] = "misc/rod", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new List<Dictionary<string, object?>>
                    {
                        new Dictionary<string, object?> { ["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1 }
                    }
                },
                new Dictionary<string, object?>
                {
                    ["type"] = "misc/rod", ["material"] = "SILK", ["amount"] = 1, ["parts"] = new List<Dictionary<string, object?>>
                    {
                        new Dictionary<string, object?> { ["type"] = "misc/material", ["material"] = "SILK", ["amount"] = 1 }
                    }
                },
            }),
            ["weapon/steel_arrow"] = ("weapon/arrow", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?>
                {
                    ["type"] = "misc/arrow_tip", ["material"] = "STEEL", ["amount"] = 1, ["parts"] = new List<Dictionary<string, object?>>
                    {
                        new Dictionary<string, object?> { ["type"] = "misc/material", ["material"] = "STEEL", ["amount"] = 1 }
                    }
                },
                new Dictionary<string, object?>
                {
                    ["type"] = "misc/rod", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new List<Dictionary<string, object?>>
                    {
                        new Dictionary<string, object?> { ["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1 }
                    }
                },
            }),
            ["weapon/wooden_club"] = ("weapon/club", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?> {["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1}
            }),
            ["weapon/club_with_teeth"] = ("weapon/club_with_teeth", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?>
                {
                    ["type"] = "weapon/club", ["material"] = "WOOD", ["amount"] = 1, ["parts"] = new List<Dictionary<string, object?>>
                    {
                        new Dictionary<string, object?> { ["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1 }
                    }
                },
                new Dictionary<string, object?> {["type"] = "misc/material", ["material"] = "TEETH", ["amount"] = 1},
            }),
            //defence
            ["defence/wooden_shield"] = ("defence/shield", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?> {["type"] = "misc/material", ["material"] = "WOOD", ["amount"] = 1}
            }),
            ["defence/leather_cap"] = ("defence/helmet", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?> {["type"] = "misc/material", ["material"] = "LEATHER", ["amount"] = 1}
            }),
            ["defence/leather_tunic"] = ("defence/chestplate", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?> {["type"] = "misc/material", ["material"] = "LEATHER", ["amount"] = 1}
            }),
            ["defence/leather_pants"] = ("defence/pants", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?> {["type"] = "misc/material", ["material"] = "LEATHER", ["amount"] = 1}
            }),
            ["defence/leather_boots"] = ("defence/boots", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?> {["type"] = "misc/material", ["material"] = "LEATHER", ["amount"] = 1}
            }),
            //materials
            ["material/bootle"] = ("misc/bottle", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?> {["type"] = "misc/material", ["material"] = "GLASS", ["amount"] = 1}
            }),
            //misc
            ["misc/health_potion"] = ("misc/filled_bottle", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?>
                {
                    ["type"] = "misc/bottle", ["material"] = "GLASS", ["amount"] = 1, ["parts"] = new List<Dictionary<string, object?>>
                    {
                        new Dictionary<string, object?> { ["type"] = "misc/material", ["material"] = "GLASS", ["amount"] = 1 }
                    }
                },
                new Dictionary<string, object?> {["type"] = "misc/material", ["material"] = "HEALING_LIQUID", ["amount"] = 1},
            }),
            ["misc/gold_coin"] = ("misc/coin", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?> {["type"] = "misc/material", ["material"] = "GOLD", ["amount"] = 1}
            }),
            ["misc/silver_coin"] = ("misc/coin", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?> {["type"] = "misc/material", ["material"] = "SILVER", ["amount"] = 1}
            }),
            ["misc/copper_coin"] = ("misc/coin", new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?> {["type"] = "misc/material", ["material"] = "COPPER", ["amount"] = 1}
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

        #region Public fuctions
        /// <summary>
        /// Return all item type IDs.
        /// </summary>
        public static List<ItemTypeID> GetAllItemTypes()
        {
            return PACTools.GetNestedStaticClassFields<ItemTypeID>(typeof(ItemType));
        }

        /// <summary>
        /// Returs the item type, if the item type ID is an id for an item type.
        /// </summary>
        /// <param name="itemTypeID">The int representation of the item's ID.</param>
        public static ItemTypeID? ToItemType(int itemTypeID)
        {
            var newItemType = (ItemTypeID)itemTypeID;
            var itemTypes = GetAllItemTypes();
            foreach (var itemType in itemTypes)
            {
                if (newItemType == itemType)
                {
                    return itemType;
                }
            }
            return null;
        }

        /// <summary>
        /// Tries to convert the int representation of the item ID to an item ID, and returns the success.
        /// </summary>
        /// <param name="itemTypeID">The int representation of the item's ID.</param>
        /// <param name="itemType">The resulting item, or a default item.</param>
        public static bool TryParseItemType(int itemTypeID, out ItemTypeID itemType)
        {
            var resultItem = ToItemType(itemTypeID);
            itemType = resultItem ?? ItemType.Misc.COIN;
            return resultItem is not null;
        }

        /// <summary>
        /// Tries to convert the string representation of the item's type to an item ID, and returns the success.
        /// </summary>
        /// <param name="itemTypeName">The int representation of the item's ID.</param>
        /// <param name="itemType">The resulting item, or a default item.</param>
        public static bool TryParseItemType(string? itemTypeName, out ItemTypeID itemType)
        {
            if (!string.IsNullOrWhiteSpace(itemTypeName))
            {
                foreach (var itemAttribute in compoundItemAttributes)
                {
                    if (itemAttribute.Value.typeName == itemTypeName)
                    {
                        itemType = itemAttribute.Key;
                        return true;
                    }
                }
            }

            itemType = ItemType.Misc.COIN;
            return false;
        }

        /// <summary>
        /// Converts the item type ID, to it's default type name.
        /// </summary>
        /// <param name="itemTypeID">The item type ID.</param>
        public static string ItemIDToTypeName(ItemTypeID itemTypeID)
        {
            var modifiedPath = new StringBuilder();
            var name = itemTypeID.ToString();
            if (name is null || !TryParseItemType(itemTypeID.GetHashCode(), out _))
            {
                Logger.Log("Unknown item type", $"ID: {itemTypeID.GetHashCode()}", LogSeverity.ERROR);
            }
            else
            {
                var actualNamePath = name.Split(nameof(ItemType) + ".").Last();
                var pathParts = actualNamePath.Split('.');
                for (var x = 0; x < pathParts.Length - 1; x++)
                {
                    var pathPart = pathParts[x];
                    var modifiedPathPart = new StringBuilder();
                    for (var y = 0; y < pathPart.Length; y++)
                    {
                        if (y != 0 && char.IsUpper(pathPart[y]))
                        {
                            modifiedPathPart.Append('_');
                        }
                        modifiedPathPart.Append(pathPart[y]);
                    }
                    modifiedPath.Append(modifiedPathPart + "/");
                }
                modifiedPath.Append(pathParts.Last());
            }
            var modifiedPathStr = modifiedPath.ToString().ToLower();
            return string.IsNullOrWhiteSpace(modifiedPathStr) ? "[UNKNOWN ITEM TYPE]" : modifiedPathStr;
        }

        /// <summary>
        /// Converts the item type ID, to it's default display name.
        /// </summary>
        /// <param name="itemTypeID">The item type ID.</param>
        public static string ItemIDToDisplayName(ItemTypeID itemTypeID)
        {
            var name = itemTypeID.ToString();
            if (name is null || !TryParseItemType(itemTypeID.GetHashCode(), out _))
            {
                Logger.Log("Unknown item type", $"ID: {itemTypeID.GetHashCode()}", LogSeverity.ERROR);
            }
            else
            {
                name = name.Split('.').Last().Replace("_", " ").Capitalize();
            }
            return name ?? "[UNKNOWN ITEM]";
        }

        /// <summary>
        /// Fills in the compound item's display name, using the parts used to create it.
        /// </summary>
        /// <param name="rawDisplayName">The raw display name of the compound item, where the name of a material in the parts list can be refrenced, by replacing it by "*/[index of part in the list][T: type, M: material, N: display name][U: upper, L: lower, C: capitalise]/*".</param>
        /// <param name="parts">The parts used to create the compound item.</param>
        public static string ParseCompoundItemDisplayName(string rawDisplayName, List<AItem> parts)
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
                        extraText = ItemIDToDisplayName(parts[materialIndex].Type).ToString()?.Replace("_", " ") ?? "";
                    }
                    else if (propertyType == "M")
                    {
                        extraText = parts[materialIndex].Material.ToString().Replace("_", " ");
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
                multiplier = (item.VolumeMultiplier / item.MassMultiplier) * 1000;
                return itemUnit == ItemAmountUnit.L ? multiplier : 1 / multiplier;
            }
            // M3 -- KG
            if (
                (itemUnit == ItemAmountUnit.M3 && targetUnit == ItemAmountUnit.KG) ||
                (targetUnit == ItemAmountUnit.M3 && itemUnit == ItemAmountUnit.KG)
            )
            {
                multiplier = item.VolumeMultiplier / item.MassMultiplier;
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
        /// Tries to create a compound item, from a list of ingredients.
        /// </summary>
        /// <param name="targetItem">The item type to try to create.</param>
        /// <param name="items">The list of items to use, as the input for the recipe.</param>
        /// <param name="amount">How much of the item to create.</param>
        public static CompoundItem? MakeItem(ItemTypeID targetItem, List<AItem> items, int amount = 1)
        {
            if (!itemRecipes.TryGetValue(targetItem, out List<IngredientDTO>? ingredients))
            {
                return null;
            }

            // get required items from the list
            var requiredItems = new List<AItem>();

            foreach (var ingredient in ingredients)
            {
                var itemFound = false;
                foreach (var item in items)
                {
                    if (
                        item.Type == ingredient.itemType &&
                        (ingredient.material is null || item.Material == ingredient.material) &&
                        (ingredient.unit is not null ? ConvertAmountToUnit(item, (ItemAmountUnit)ingredient.unit) : 1) >= ingredient.amount * amount &&
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

            // create the item
            var parts = new List<AItem>();
            for (int x = 0; x < ingredients.Count; x++)
            {
                var usedItem = requiredItems[x].DeepCopy();
                requiredItems[x].Amount -= ingredients[x].amount * amount;

                usedItem.Amount = ingredients[x].amount;
                parts.Add(usedItem);
            }

            return new CompoundItem(targetItem, parts, amount);
        }

        /// <summary>
        /// Creates a new compound item from the target type.
        /// </summary>
        /// <param name="targetItem">The item type to create.</param>
        /// <param name="materials">The materials to use, for the parts of the item, if posible.</param>
        /// <param name="amount">How much of the item to create.</param>
        public static CompoundItem CreateCompoumdItem(ItemTypeID targetItem, List<Material?> materials, double amount = 1)
        {
            // not craftable
            if (!itemRecipes.TryGetValue(targetItem, out List<IngredientDTO>? ingredients))
            {
                return new CompoundItem(targetItem, new List<AItem> { new MaterialItem(materials?.First() ?? Material.WOOD) }, amount);
            }

            // get parts
            var parts = new List<AItem>();

            for (var x = 0; x < ingredients.Count; x++)
            {
                var ingredient = ingredients[x];
                var material = ingredient.material ?? (materials is not null && materials.Count > x ? materials[x] : null);
                AItem part;
                if (ingredient.itemType == MATERIAL_ITEM_TYPE)
                {
                    part = new MaterialItem(material ?? Material.WOOD, ingredient.amount);
                }
                else
                {
                    part = CreateCompoumdItem(ingredient.itemType, material, ingredient.amount);
                }
                part.Amount = ingredient.unit is not null ? ConvertAmountToUnit(part, (ItemAmountUnit)ingredient.unit) : part.Amount;
                parts.Add(part);
            }

            // create item
            return new CompoundItem(targetItem, parts, amount);
        }

        /// <inheritdoc cref="CreateCompoumdItem(ItemTypeID, List{Material?}?, double)"/>
        /// <param name="material">The material to use, for the material of the item, if posible.</param>
        public static CompoundItem CreateCompoumdItem(ItemTypeID targetItem, Material? material = null, double amount = 1)
        {
            return CreateCompoumdItem(targetItem, new List<Material?> { material }, amount);
        }
        #endregion
    }
}
