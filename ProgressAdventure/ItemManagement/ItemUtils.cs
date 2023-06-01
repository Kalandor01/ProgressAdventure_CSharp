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
        public static readonly Dictionary<ItemTypeID, ItemAttributes> itemAttributes = new()
        {
            //weapons
            [ItemType.Weapon.WOODEN_SWORD] = new ItemAttributes("weapon/wooden_sword", null, null),
            [ItemType.Weapon.STONE_SWORD] = new ItemAttributes("weapon/stone_sword", null, null),
            [ItemType.Weapon.STEEL_SWORD] = new ItemAttributes("weapon/steel_sword", null, null),
            [ItemType.Weapon.WOODEN_BOW] = new ItemAttributes("weapon/wooden_bow", null, null),
            [ItemType.Weapon.STEEL_ARROW] = new ItemAttributes("weapon/steel_arrow", null, null),
            [ItemType.Weapon.WOODEN_CLUB] = new ItemAttributes("weapon/wooden_club", null, null),
            [ItemType.Weapon.CLUB_WITH_TEETH] = new ItemAttributes("weapon/club_with_teeth", "Teeth club", false),
            //defence
            [ItemType.Defence.WOODEN_SHIELD] = new ItemAttributes("defence/wooden_shield", null, null),
            [ItemType.Defence.LEATHER_CAP] = new ItemAttributes("defence/leather_cap", null, null),
            [ItemType.Defence.LEATHER_TUNIC] = new ItemAttributes("defence/leather_tunic", null, null),
            [ItemType.Defence.LEATHER_PANTS] = new ItemAttributes("defence/leather_pants", null, null),
            [ItemType.Defence.LEATHER_BOOTS] = new ItemAttributes("defence/leather_boots", null, null),
            //materials
            [ItemType.Material.BOOTLE] = new ItemAttributes("material/bootle", null, null),
            [ItemType.Material.WOOL] = new ItemAttributes("material/wool", null, null),
            [ItemType.Material.CLOTH] = new ItemAttributes("material/cloth", null, null),
            [ItemType.Material.WOOD] = new ItemAttributes("material/wood", null, null),
            [ItemType.Material.STONE] = new ItemAttributes("material/stone", null, null),
            [ItemType.Material.STEEL] = new ItemAttributes("material/steel", null, null),
            [ItemType.Material.GOLD] = new ItemAttributes("material/gold", null, null),
            [ItemType.Material.TEETH] = new ItemAttributes("material/teeth", null, null),
            //misc
            [ItemType.Misc.HEALTH_POTION] = new ItemAttributes("misc/health_potion", "Health potion", true),
            [ItemType.Misc.GOLD_COIN] = new ItemAttributes("misc/gold_coin", null, null),
            [ItemType.Misc.SILVER_COIN] = new ItemAttributes("misc/silver_coin", null, null),
            [ItemType.Misc.COPPER_COIN] = new ItemAttributes("misc/copper_coin", null, null),
            [ItemType.Misc.ROTTEN_FLESH] = new ItemAttributes("misc/rotten_flesh", null, null),
        };
        #endregion

        #region Public fuctions
        /// <summary>
        /// Returs the item type, if the item type ID is an id for an item type.
        /// </summary>
        /// <param name="itemTypeID">The int representation of the item's ID.</param>
        public static ItemTypeID? ToItemType(int itemTypeID)
        {
            var newItemType = (ItemTypeID)itemTypeID;
            var itemTypes = Tools.GetNestedStaticClassFields<ItemTypeID>(typeof(ItemType));
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
        /// Converts the ite mtype ID, to it's default display name.
        /// </summary>
        /// <param name="itemTypeID">The item type ID.</param>
        public static string ItemIDToDisplayName(ItemTypeID itemTypeID)
        {
            var name = itemTypeID.ToString();
            if (name is null)
            {
                Logger.Log("Unknown item type", $"ID: {itemTypeID.GetHashCode()}");
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
