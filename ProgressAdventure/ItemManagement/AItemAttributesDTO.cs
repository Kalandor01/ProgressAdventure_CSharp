using ProgressAdventure.Enums;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// Abstract class of a DTO used for storing the attributes of an <c>AItem</c>.
    /// </summary>
    public abstract class AItemAttributesDTO
    {
        #region Fields
        /// <summary>
        /// The unique name of the item, used in the json representation of the item.<br/>
        /// Usualy "item_category/item_type".
        /// </summary>
        [JsonPropertyName("type_name")]
        public readonly string typeName;
        /// <summary>
        /// The display name of the item.
        /// </summary>
        [JsonPropertyName("display_name")]
        public readonly string displayName;
        /// <summary>
        /// <inheritdoc cref="ItemAmountUnit"/>
        /// </summary>
        [JsonPropertyName("unit")]
        public readonly ItemAmountUnit unit;
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="AItemAttributesDTO"/>
        /// </summary>
        /// <param name="typeName"><inheritdoc cref="typeName" path="//summary"/></param>
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        [JsonConstructor]
        protected AItemAttributesDTO(string typeName, string displayName, ItemAmountUnit unit = ItemAmountUnit.AMOUNT)
        {
            this.typeName = typeName;
            this.displayName = displayName;
            this.unit = unit;
        }
        #endregion

        #region Overrides
        public override string? ToString()
        {
            return $"{typeName} ({displayName}), {unit}";
        }
        #endregion
    }
}
