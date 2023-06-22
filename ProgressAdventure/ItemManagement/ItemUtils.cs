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
            [ItemType.Weapon.WOODEN_SWORD] = new ItemAttributesDTO(ItemType.Weapon.WOODEN_SWORD),
            [ItemType.Weapon.STONE_SWORD] = new ItemAttributesDTO(ItemType.Weapon.STONE_SWORD),
            [ItemType.Weapon.STEEL_SWORD] = new ItemAttributesDTO(ItemType.Weapon.STEEL_SWORD),
            [ItemType.Weapon.WOODEN_BOW] = new ItemAttributesDTO(ItemType.Weapon.WOODEN_BOW),
            [ItemType.Weapon.STEEL_ARROW] = new ItemAttributesDTO(ItemType.Weapon.STEEL_ARROW),
            [ItemType.Weapon.WOODEN_CLUB] = new ItemAttributesDTO(ItemType.Weapon.WOODEN_CLUB),
            [ItemType.Weapon.CLUB_WITH_TEETH] = new ItemAttributesDTO(ItemType.Weapon.CLUB_WITH_TEETH, "Teeth club"),
            //defence
            [ItemType.Defence.WOODEN_SHIELD] = new ItemAttributesDTO(ItemType.Defence.WOODEN_SHIELD),
            [ItemType.Defence.LEATHER_CAP] = new ItemAttributesDTO(ItemType.Defence.LEATHER_CAP),
            [ItemType.Defence.LEATHER_TUNIC] = new ItemAttributesDTO(ItemType.Defence.LEATHER_TUNIC),
            [ItemType.Defence.LEATHER_PANTS] = new ItemAttributesDTO(ItemType.Defence.LEATHER_PANTS),
            [ItemType.Defence.LEATHER_BOOTS] = new ItemAttributesDTO(ItemType.Defence.LEATHER_BOOTS),
            //materials
            [ItemType.Material.BOOTLE] = new ItemAttributesDTO(ItemType.Material.BOOTLE),
            [ItemType.Material.WOOL] = new ItemAttributesDTO(ItemType.Material.WOOL),
            [ItemType.Material.CLOTH] = new ItemAttributesDTO(ItemType.Material.CLOTH),
            [ItemType.Material.WOOD] = new ItemAttributesDTO(ItemType.Material.WOOD),
            [ItemType.Material.STONE] = new ItemAttributesDTO(ItemType.Material.STONE),
            [ItemType.Material.STEEL] = new ItemAttributesDTO(ItemType.Material.STEEL),
            [ItemType.Material.GOLD] = new ItemAttributesDTO(ItemType.Material.GOLD),
            [ItemType.Material.TEETH] = new ItemAttributesDTO(ItemType.Material.TEETH),
            //misc
            [ItemType.Misc.HEALTH_POTION] = new ItemAttributesDTO(ItemType.Misc.HEALTH_POTION, true),
            [ItemType.Misc.GOLD_COIN] = new ItemAttributesDTO(ItemType.Misc.GOLD_COIN),
            [ItemType.Misc.SILVER_COIN] = new ItemAttributesDTO(ItemType.Misc.SILVER_COIN),
            [ItemType.Misc.COPPER_COIN] = new ItemAttributesDTO(ItemType.Misc.COPPER_COIN),
            [ItemType.Misc.ROTTEN_FLESH] = new ItemAttributesDTO(ItemType.Misc.ROTTEN_FLESH),
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
            itemType = resultItem ?? ItemType.Misc.COPPER_COIN;
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

            itemType = ItemType.Misc.COPPER_COIN;
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
                name = name.Split('.').Last();
                name = name[0].ToString().ToUpper() + name[1..].ToLower();
                name = name.Replace("_", " ");
            }
            return name ?? "[UNKNOWN ITEM]";
        }
        #endregion
    }
}
