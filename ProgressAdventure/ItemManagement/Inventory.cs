using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using PACTools = PACommon.Tools;

namespace ProgressAdventure.ItemManagement
{
    public class Inventory : IJsonConvertable<Inventory>
    {
        #region Public fields
        public List<AItem> items;
        #endregion

        #region Constructors
        public Inventory(List<AItem>? items = null)
        {
            this.items = items ?? [];
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Tries to find an item in the inventory, based on the input item type and material.
        /// </summary>
        /// <param name="itemType">The item type to search for.</param>
        /// <param name="material">The material to search for.</param>
        public AItem? FindByType(EnumTreeValue<ItemType> itemType, Material? material)
        {
            foreach (var currItem in items)
            {
                if (currItem.Type == itemType && currItem.Material == material)
                {
                    return currItem;
                }
            }
            return null;
        }

        /// <summary>
        /// Tries to find an item in the inventory, based on the input item's type and material.
        /// </summary>
        /// <param name="item">The item whose type nad material to search for.</param>
        public AItem? FindByType(AItem item)
        {
            return FindByType(item.Type, item.Material);
        }

        /// <summary>
        /// Adds an item to the inventory. If the item already exists, it adds to the amount.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>If the item already exists in the inventory.</returns>
        public bool Add(AItem item)
        {
            var foundItem = FindByType(item);
            if (foundItem is not null)
            {
                foundItem.Amount += item.Amount;
                return true;
            }

            items.Add(item);
            return false;
        }

        /// <summary>
        /// <inheritdoc cref="Add(AItem)"/><br/>
        /// For adding a new material item to the inventory.
        /// </summary>
        /// <param name="material">The material of the item.</param>
        /// <param name="amount">The amount of the items to add.</param>
        /// <returns><inheritdoc cref="Add(AItem)"/></returns>
        public bool Add(Material material, int amount = 1)
        {
            return Add(new MaterialItem(material, amount));
        }

        /// <summary>
        /// <inheritdoc cref="Add(AItem)"/><br/>
        /// For adding a new compound item to the inventory.
        /// </summary>
        /// <param name="itemType">The type of the item to add.</param>
        /// <param name="materials">The list of materials, for parts of the item.</param>
        /// <param name="amount">The amount of the items to add.</param>
        /// <exception cref="ArgumentException">Thrown if the item type is an unknown item type id.</exception>
        /// <returns><inheritdoc cref="Add(AItem)"/></returns>
        public bool Add(EnumTreeValue<ItemType> itemType, List<Material?> materials, int amount = 1)
        {
            if (itemType == ItemUtils.MATERIAL_ITEM_TYPE)
            {
                return Add(materials.First() ?? Material.WOOD, amount);
            }
            return Add(ItemUtils.CreateCompoundItem(itemType, materials, amount, null));
        }

        /// <summary>
        /// <inheritdoc cref="Add(AItem)"/><br/>
        /// For adding a new compound item to the inventory.
        /// </summary>
        /// <inheritdoc cref="Add(ItemTypeID, List{Material?}, int)"/>
        /// <param name="material">The material of the item.</param>
        public bool Add(EnumTreeValue<ItemType> itemType, Material material, int amount = 1)
        {
            return Add(itemType, [material], amount);
        }

        /// <summary>
        /// Clears all items that have 0 amount.
        /// </summary>
        public void ClearFalseItems()
        {
            items.RemoveAll(item => item.Amount <= 0);
        }

        /// <summary>
        /// Removes some number of items, from the inventory, based on the type.
        /// </summary>
        /// <param name="itemType">The type of the item to remove.</param>
        /// <param name="material">The material of the item.</param>
        /// <param name="amount">The amount of the items to remove. If its null, it removes all items of the type.</param>
        /// <returns>If the item existed in the inventory.</returns>
        public bool Remove(EnumTreeValue<ItemType> itemType, Material material, uint? amount = null)
        {
            for (var x = 0; x < items.Count; x++)
            {
                if (items.ElementAt(x).Type == itemType && items.ElementAt(x).Material == material)
                {
                    if (amount is null || items.ElementAt(x).Amount - amount <= 0)
                    {
                        items.RemoveAt(x);
                    }
                    else
                    {
                        items.ElementAt(x).Amount -= (int)amount;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds a list of items to the inventory.
        /// </summary>
        /// <param name="loot">The list of items to add.</param>
        /// <param name="looterName">Wether to write out what was picked up by who.</param>
        public void Loot(IEnumerable<AItem> loot, string? looterName = null)
        {
            foreach (var item in loot)
            {
                Add(item);
                if (looterName is not null)
                {
                    Console.WriteLine($"{looterName} picked up {item}");
                }
            }
        }

        /// <summary>
        /// Adds a list of items to the inventory, from the looted entity's drops, and removes them from theirs.
        /// </summary>
        /// <param name="lootedEntity">The entity to loot.</param>
        /// <param name="looterName">Wether to write out what was picked up by who.</param>
        public void Loot(Entity.Entity lootedEntity, string? looterName = null)
        {
            if (looterName is not null)
            {
                Console.WriteLine($"{looterName} looted the body of {lootedEntity.FullName}");
            }
            Loot(lootedEntity.drops, looterName);
            lootedEntity.drops.Clear();
        }

        /// <summary>
        /// Uses the item, if it's usable.
        /// </summary>
        /// <param name="itemType">The type of the item to use.</param>
        /// <param name="material">The material of the item to use.</param>
        public bool Use(EnumTreeValue<ItemType> itemType, Material material)
        {
            for (int x = 0; x < items.Count; x++)
            {
                if (items.ElementAt(x).Type == itemType && items.ElementAt(x).Material == material)
                {
                    // TODO: add back use???
                    // here
                    if (items.ElementAt(x).Amount == 0)
                    {
                        items.RemoveAt(x);
                    }
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Public overrides
        public override string ToString()
        {
            var txt = new StringBuilder("Inventory:");
            foreach (var item in items)
            {
                txt.Append($"\n\t{item}");
            }
            return txt.ToString();
        }
        #endregion

        #region JsonConvertable
        public JsonDictionary ToJson()
        {
            return new JsonDictionary
            {
                [Constants.JsonKeys.Inventory.ITEMS] = items.Select(item => (JsonObject?)item.ToJson()).ToList(),
            };
        }

        static bool IJsonConvertable<Inventory>.FromJsonWithoutCorrection(JsonDictionary inventoryJson, string fileVersion, [NotNullWhen(true)] ref Inventory? inventory)
        {
            var success = true;
            success = PACTools.TryParseJsonListValue<Inventory, AItem>(inventoryJson, Constants.JsonKeys.Inventory.ITEMS,
                itemJson =>
                {
                    var parseSuccess = PACTools.TryFromJson(itemJson as JsonDictionary, fileVersion, out AItem? itemObject);
                    success &= parseSuccess;
                    return (parseSuccess, itemObject);
                },
                out var items, true);

            if (items is null)
            {
                return false;
            }

            inventory = new Inventory(items);
            return success;
        }
        #endregion
    }
}
