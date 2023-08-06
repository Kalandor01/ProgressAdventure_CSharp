using ProgressAdventure.Enums;
using ProgressAdventure.Extensions;
using System.Text;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// Utils for items.
    /// </summary>
    public static class ItemUtils
    {
        #region Public dicts
        /// <summary>
        /// The dictionary pairing up item types, to their attributes.
        /// </summary>
        public static readonly Dictionary<ItemTypeID, ItemAttributesDTO> itemAttributes = new()
        {
            //weapons
            [ItemType.Weapon.SWORD] = new ItemAttributesDTO(ItemType.Weapon.SWORD),
            [ItemType.Weapon.BOW] = new ItemAttributesDTO(ItemType.Weapon.BOW),
            [ItemType.Weapon.ARROW] = new ItemAttributesDTO(ItemType.Weapon.ARROW),
            [ItemType.Weapon.CLUB] = new ItemAttributesDTO(ItemType.Weapon.CLUB),
            [ItemType.Weapon.CLUB_WITH_TEETH] = new ItemAttributesDTO(ItemType.Weapon.CLUB_WITH_TEETH, "Teeth club", false),
            //defence
            [ItemType.Defence.SHIELD] = new ItemAttributesDTO(ItemType.Defence.SHIELD),
            [ItemType.Defence.HELMET] = new ItemAttributesDTO(ItemType.Defence.HELMET),
            [ItemType.Defence.CHESTPLATE] = new ItemAttributesDTO(ItemType.Defence.CHESTPLATE),
            [ItemType.Defence.PANTS] = new ItemAttributesDTO(ItemType.Defence.PANTS),
            [ItemType.Defence.BOOTS] = new ItemAttributesDTO(ItemType.Defence.BOOTS),
            //misc
            [ItemType.Misc.BOTTLE] = new ItemAttributesDTO(ItemType.Misc.BOTTLE),
            [ItemType.Misc.HEALTH_POTION] = new ItemAttributesDTO(ItemType.Misc.HEALTH_POTION, false, true),
            [ItemType.Misc.COIN] = new ItemAttributesDTO(ItemType.Misc.COIN),
            [ItemType.Misc.MATERIAL] = new ItemAttributesDTO(ItemType.Misc.MATERIAL),
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
        internal static readonly Dictionary<string, (string typeName, string? material)> _legacyItemNameMaterialMap = new()
        {
            //weapons
            ["weapon/wooden_sword"] = ("weapon/sword", "wood"),
            ["weapon/stone_sword"] = ("weapon/sword", "stone"),
            ["weapon/steel_sword"] = ("weapon/sword", "steel"),
            ["weapon/wooden_bow"] = ("weapon/bow", "wood"),
            ["weapon/steel_arrow"] = ("weapon/arrow", "steel"),
            ["weapon/wooden_club"] = ("weapon/club", "wood"),
            ["weapon/club_with_teeth"] = ("weapon/club_with_teeth", null),
            //defence
            ["defence/wooden_shield"] = ("defence/shield", "wood"),
            ["defence/leather_cap"] = ("defence/helmet", "leather"),
            ["defence/leather_tunic"] = ("defence/chestplate", "leather"),
            ["defence/leather_pants"] = ("defence/pants", "leather"),
            ["defence/leather_boots"] = ("defence/boots", "leather"),
            //materials
            ["material/bootle"] = ("misc/bottle", "glass"),
            ["material/wool"] = ("misc/material", "wool"),
            ["material/cloth"] = ("misc/material", "cloth"),
            ["material/wood"] = ("misc/material", "wood"),
            ["material/stone"] = ("misc/material", "stone"),
            ["material/steel"] = ("misc/material", "steel"),
            ["material/gold"] = ("misc/material", "gold"),
            ["material/teeth"] = ("misc/material", "teeth"),
            //misc
            ["misc/health_potion"] = ("misc/health_potion", null),
            ["misc/gold_coin"] = ("misc/coin", "gold"),
            ["misc/silver_coin"] = ("misc/coin", "silver"),
            ["misc/copper_coin"] = ("misc/coin", "copper"),
            ["misc/rotten_flesh"] = ("misc/material", "rotten_flesh"),
        };
        #endregion

        #region Public fuctions
        /// <summary>
        /// Return all item type IDs.
        /// </summary>
        public static List<ItemTypeID> GetAllItemTypes()
        {
            return Tools.GetNestedStaticClassFields<ItemTypeID>(typeof(ItemType));
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
                foreach (var itemAttribute in itemAttributes)
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
                Logger.Log("Unknown item type", $"ID: {itemTypeID.GetHashCode()}", Enums.LogSeverity.ERROR);
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
                Logger.Log("Unknown item type", $"ID: {itemTypeID.GetHashCode()}", Enums.LogSeverity.ERROR);
            }
            else
            {
                name = name.Split('.').Last().Replace("_", " ").Capitalize();
            }
            return name ?? "[UNKNOWN ITEM]";
        }
        #endregion
    }
}
