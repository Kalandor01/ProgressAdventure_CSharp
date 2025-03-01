using PACommon;
using PACommon.Enums;
using ProgressAdventure.Enums;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ItemManagement
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
        [JsonPropertyName("item_type")]
        public EnumTreeValue<ItemType> itemType;
        /// <summary>
        /// The material of the item.
        /// </summary>
        [JsonPropertyName("material")]
        public EnumValue<Material> material;
        /// <summary>
        /// The chance for the entity to drop this item per roll.
        /// </summary>
        [JsonPropertyName("chance")]
        public double chance;
        /// <summary>
        /// The minimum amount of items to add per successfull roll.
        /// </summary>
        [JsonPropertyName("amount_min")]
        public int amountMin;
        /// <summary>
        /// The maximum amount of items to add per successfull roll.
        /// </summary>
        [JsonPropertyName("amount_max")]
        public int amountMax;
        /// <summary>
        /// The number of rolls to do.
        /// </summary>
        [JsonPropertyName("rolls")]
        public int rolls;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="LootFactory"/>
        /// </summary>
        /// <param name="itemType"><inheritdoc cref="itemType" path="//summary"/></param>
        /// <param name="material"><inheritdoc cref="material" path="//summary"/></param>
        /// <param name="chance"><inheritdoc cref="chance" path="//summary"/></param>
        /// <param name="amountMin"><inheritdoc cref="amountMin" path="//summary"/></param>
        /// <param name="amountMax"><inheritdoc cref="amountMax" path="//summary"/></param>
        /// <param name="rolls"><inheritdoc cref="rolls" path="//summary"/></param>
        /// <exception cref="ArgumentException">Thrown if the item type is an unknown item type, or the material was required, but wasn't provided.</exception>

        [JsonConstructor]
        public LootFactory(
            EnumTreeValue<ItemType> itemType,
            EnumValue<Material> material,
            double chance,
            int rolls,
            int amountMin,
            int amountMax
        )
        {
            if (!ItemUtils.TryParseItemType(itemType.FullName, out _))
            {
                PACSingletons.Instance.Logger.Log("Unknown item type", itemType.ToString(), LogSeverity.ERROR);
                throw new ArgumentException("Unknown item type", nameof(itemType));
            }
            this.itemType = itemType;
            this.material = material;
            this.chance = chance;
            this.amountMin = amountMin;
            this.amountMax = amountMax;
            this.rolls = rolls;
        }

        public LootFactory(
            EnumTreeValue<ItemType> itemType,
            EnumValue<Material> material,
            double chance = 1,
            int rolls = 1,
            int amount = 1
        )
            : this(
                 itemType,
                 material,
                 chance,
                 rolls,
                 amount,
                 amount
            )
        {

        }
        #endregion

        #region Public functions
        /// <summary>
        /// Converts a list of <c>LootFactory</c>s into a list of <c>Item</c>s.
        /// </summary>
        /// <param name="drops">A list of <c>LootFactory</c>s.</param>
        /// <returns></returns>
        public static List<AItem> LootManager(IEnumerable<LootFactory>? drops = null)
        {
            if (drops is null)
            {
                return [];
            }

            var loot = new List<AItem>();
            foreach (var drop in drops)
            {
                var num = 0L;
                for (var x = 0; x < drop.rolls; x++)
                {
                    num += RandomStates.Instance.MainRandom.GenerateDouble() <= drop.chance ? RandomStates.Instance.MainRandom.GenerateInRange(drop.amountMin, drop.amountMax) : 0;
                }
                if (num > 0)
                {
                    loot.Add(
                        drop.itemType == ItemUtils.MATERIAL_ITEM_TYPE ?
                            new MaterialItem(drop.material, num) :
                            ItemUtils.CreateCompoundItem(drop.itemType, drop.material, num)
                    );
                }
            }
            return loot;
        }

        public override string? ToString()
        {
            var rollsStr = rolls != 1 ? $"{rolls} x " : "";
            var chanceStr = chance != 1 ? $"{chance * 100}% chance of " : "";
            var amountStr = amountMin != 1 || amountMax != amountMin
                ? (amountMin != amountMax ? $"({amountMin}-{amountMax})" : amountMin) + " "
                : "";
            return $"{rollsStr}{chanceStr}{amountStr}{itemType}({material})";
        }
        #endregion
    }
}
