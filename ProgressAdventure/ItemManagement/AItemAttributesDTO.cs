using System.Text.Json.Serialization;
using PACommon.Enums;
using ProgressAdventure.Enums;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// Abstract class of a DTO used for storing the attributes of an <see cref="AItem"/>.
    /// </summary>
    public abstract class AItemAttributesDTO
    {
        #region Fields
        /// <summary>
        /// The display name of the item.
        /// </summary>
        [JsonPropertyName("display_name")]
        public readonly EnumValue<LocalizationKey> displayName;
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
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        [JsonConstructor]
        protected AItemAttributesDTO(EnumValue<LocalizationKey> displayName, ItemAmountUnit unit = ItemAmountUnit.AMOUNT)
        {
            this.displayName = displayName;
            this.unit = unit;
        }
        #endregion

        #region Overrides
        public override string? ToString()
        {
            return $"\"{PASingletons.Instance.Localizer.GetLocalizedString(displayName)}\", {unit}";
        }
        #endregion
    }
}
