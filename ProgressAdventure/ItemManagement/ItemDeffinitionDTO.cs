using PACommon.Enums;
using ProgressAdventure.Enums;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// DTO used for storing the attributes of an item deffinition.
    /// </summary>
    public class ItemDeffinitionDTO
    {
        #region Fields
        /// <summary>
        /// The type of the item.
        /// </summary>
        [JsonPropertyName("item_type")]
        public readonly EnumTreeValue<ItemType> itemType;
        /// <summary>
        /// The material of the item, that is required for the deffinition.
        /// </summary>
        [JsonPropertyName("material")]
        public readonly EnumValue<Material>? material;
        /// <summary>
        /// The amount of material to use in m3 to create the item.
        /// </summary>
        [JsonPropertyName("volume")]
        public readonly double volume;
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="ItemDeffinitionDTO"/><br/>
        /// Used for a compound item based deffinition.
        /// </summary>
        /// <param name="itemType"><inheritdoc cref="itemType" path="//summary"/></param>
        /// <param name="volume"><inheritdoc cref="volume" path="//summary"/></param>
        public ItemDeffinitionDTO(EnumTreeValue<ItemType> itemType, double volume = 1)
            : this(
                  itemType,
                  null,
                  volume
                )
        { }

        /// <summary>
        /// <inheritdoc cref="ItemDeffinitionDTO"/><br/>
        /// Used for a material item based deffinition.
        /// </summary>
        /// <param name="material"><inheritdoc cref="material" path="//summary"/></param>
        /// <param name="volume"><inheritdoc cref="volume" path="//summary"/></param>
        public ItemDeffinitionDTO(EnumValue<Material> material, double volume = 1)
            : this(ItemUtils.MATERIAL_ITEM_TYPE, material, volume)
        { }

        /// <summary>
        /// <inheritdoc cref="ItemDeffinitionDTO"/><br/>
        /// Used for a material item based deffinition that accepts any material.
        /// </summary>
        /// <param name="volume"><inheritdoc cref="volume" path="//summary"/></param>
        public ItemDeffinitionDTO(double volume = 1)
            : this(ItemUtils.MATERIAL_ITEM_TYPE, null, volume)
        { }

        /// <summary>
        /// <inheritdoc cref="ItemDeffinitionDTO"/>
        /// </summary>
        /// <param name="itemType"><inheritdoc cref="itemType" path="//summary"/></param>
        /// <param name="material"><inheritdoc cref="material" path="//summary"/></param>
        /// <param name="volume"><inheritdoc cref="volume" path="//summary"/></param>
        [JsonConstructor]
        public ItemDeffinitionDTO(EnumTreeValue<ItemType> itemType, EnumValue<Material>? material = null, double volume = 1)
        {
            this.itemType = itemType;
            this.material = material;
            this.volume = Math.Max(volume, 0);

            if (itemType != ItemUtils.MATERIAL_ITEM_TYPE)
            {
                var itemUnit = ItemUtils.CompoundItemAttributes[this.itemType].unit;
                if (itemUnit == ItemAmountUnit.AMOUNT)
                {
                    throw new ArgumentException("A deffinition item unit cannot be amount", nameof(itemType));
                }
            }
        }
        #endregion

        public override string? ToString()
        {
            return $"{(
                    material is null && itemType == ItemType.Misc.MATERIAL
                        ? "ANY MATERIAL"
                        : $"{(material is null ? "" : $"{material} ")}{(itemType == ItemType.Misc.MATERIAL ? "" : itemType.FullName)}"
                )} x{volume}";
        }
    }
}
