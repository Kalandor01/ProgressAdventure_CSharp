using ProgressAdventure.Enums;
using System.Text;

namespace ProgressAdventure.ItemManagement
{
    public class Inventory : IJsonConvertable<Inventory>
    {
        #region Public fields
        public List<Item> items;
        #endregion

        #region Constructors
        public Inventory(List<Item>? items = null)
        {
            this.items = items ?? new List<Item>();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Adds an item to the inventory. If the item already exists, it adds to the amount.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>If the item already exists in the inventory.</returns>
        public bool Add(Item item)
        {
            foreach (var currItem in items)
            {
                if (currItem.Type == item.Type)
                {
                    currItem.amount += item.amount;
                    return true;
                }
            }
            items.Add(item);
            return false;
        }

        /// <summary>
        /// <inheritdoc cref="Add(Item)"/>
        /// </summary>
        /// <param name="itemType">The type of the item to add.</param>
        /// <param name="amount">The amount of the items to add.</param>
        /// <returns><inheritdoc cref="Add(Item)"/></returns>
        public bool Add(ItemTypeID itemType, int amount = 1)
        {
            foreach (var item in items)
            {
                if (item.Type == itemType)
                {
                    item.amount += amount;
                    return true;
                }
            }
            items.Add(new Item(itemType, amount));
            return false;
        }

        /// <summary>
        /// Removes some number of items, from the inventory, based on the type.
        /// </summary>
        /// <param name="itemType">The type of the item to remove.</param>
        /// <param name="amount">The amount of the items to remove. If its null, it removes all items of the type.</param>
        /// <returns>If the item existed in the inventory.</returns>
        public bool Remove(ItemTypeID itemType, uint? amount = null)
        {
            for (var x = 0; x < items.Count; x++)
            {
                if (items.ElementAt(x).Type == itemType)
                {
                    if (amount is null || items.ElementAt(x).amount - amount <= 0)
                    {
                        items.RemoveAt(x);
                    }
                    else
                    {
                        items.ElementAt(x).amount -= (int)amount;
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
        public void Loot(IEnumerable<Item> loot, string? looterName = null)
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


        public bool Use(ItemTypeID itemType)
        {
            for (int x = 0; x < items.Count; x++)
            {
                if (items.ElementAt(x).Type == itemType)
                {
                    items.ElementAt(x).Use();
                    if (items.ElementAt(x).amount <= 0)
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
        public Dictionary<string, object?> ToJson()
        {
            var itemsListJson = new List<Dictionary<string, object?>>();
            foreach (var item in items)
            {
                itemsListJson.Add(item.ToJson());
            }
            return new Dictionary<string, object?> { ["items"] = itemsListJson };
        }

        public static Inventory? FromJson(IDictionary<string, object?>? inventoryJson)
        {
            if (
                inventoryJson is not null &&
                inventoryJson.TryGetValue("items", out object? itemsJson) &&
                itemsJson is not null
            )
            {
                var items = new List<Item>();
                foreach (var itemJson in (IEnumerable<object?>)itemsJson)
                {
                    var item = Item.FromJson((IDictionary<string, object?>?)itemJson);
                    if (item is not null)
                    {
                        items.Add(item);
                    }
                }
                var inventory = new Inventory(items);
                return inventory;
            }
            else
            {
                Logger.Log("Inventory parse error", "items json is null", LogSeverity.ERROR);
                return null;
            }
        }
        #endregion
    }
}
