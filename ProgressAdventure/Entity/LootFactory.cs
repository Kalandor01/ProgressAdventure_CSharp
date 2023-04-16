using ProgressAdventure.Enums;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// Object for assembling the loot, an entity will drop.
    /// </summary>
    public class LootFactory
    {
        #region Public fields
        /// <summary>
        /// The type of the item.
        /// </summary>
        ItemType itemType;
        /// <summary>
        /// The chance for the entity to drop this item per roll.
        /// </summary>
        double chance;
        /// <summary>
        /// The minimum amount of items to add per successfull roll.
        /// </summary>
        int amountMin;
        /// <summary>
        /// The maximum amount of items to add per successfull roll.
        /// </summary>
        int amountMax;
        /// <summary>
        /// The number of rolls to do.
        /// </summary>
        int rolls;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="LootFactory"/>
        /// </summary>
        /// <param name="itemType"><inheritdoc cref="itemType" path="//summary"/></param>
        /// <param name="chance"><inheritdoc cref="chance" path="//summary"/></param>
        /// <param name="amountMin"><inheritdoc cref="amountMin" path="//summary"/></param>
        /// <param name="amountMax"><inheritdoc cref="amountMax" path="//summary"/></param>
        /// <param name="rolls"><inheritdoc cref="rolls" path="//summary"/></param>
        public LootFactory(ItemType itemType, double chance = 1, int amountMin = 1, int? amountMax = null, int rolls = 1)
        {
            this.itemType = itemType;
            this.chance = chance;
            this.amountMin = amountMin;
            this.amountMax = (int)(amountMax is not null ? amountMax : this.amountMin);
            this.rolls = rolls;
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Converts a list of <c>LootFactory</c>s into a list of <c>Item</c>s.
        /// </summary>
        /// <param name="drops">A list of <c>LootFactory</c>s.</param>
        /// <returns></returns>
        public static IEnumerable<Item> LootManager(IEnumerable<LootFactory>? drops = null)
        {
            var loot = Enumerable.Empty<Item>();
            if (drops is not null)
            {
                foreach (var drop in drops)
                {
                    var num = 0L;
                    for (var x = 0; x < drop.rolls; x++)
                    {
                        num += SaveData.MainRandom.GenerateDouble() <= drop.chance ? SaveData.MainRandom.GenerateInRange(drop.amountMin, drop.amountMax) : 0;
                    }
                    if (num > 0)
                    {
                        loot.Append(new Item(drop.itemType, num));
                    }
                }
            }
            return loot;
        }
        #endregion
    }
}
