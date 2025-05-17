using System.Text.Json.Serialization;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// DTO used for storing the properties of a <see cref="CompoundItem"/>.
    /// </summary>
    public class CompoundItemPropertiesDTO
    {
        #region Fields
        /// <summary>
        /// The X dimension of the item in meters.
        /// </summary>
        [JsonPropertyName("dimension_x")]
        public readonly double? dimensionX;

        /// <summary>
        /// The Y dimension of the item in meters.
        /// </summary>
        [JsonPropertyName("dimension_y")]
        public readonly double? dimensionY;

        /// <summary>
        /// The Z dimension of the item in meters.
        /// </summary>
        [JsonPropertyName("dimension_z")]
        public readonly double? dimensionZ;
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="CompoundItemPropertiesDTO"/>
        /// </summary>
        /// <param name="dimensionX"><inheritdoc cref="dimensionX" path="//summary"/></param>
        /// <param name="dimensionY"><inheritdoc cref="dimensionY" path="//summary"/></param>
        /// <param name="dimensionZ"><inheritdoc cref="dimensionZ" path="//summary"/></param>
        public CompoundItemPropertiesDTO(
            double? dimensionX,
            double? dimensionY,
            double? dimensionZ
        )
        {
            if (dimensionX is not null && dimensionX <= 0)
            {
                throw new ArgumentException("X dimension must be bigger than 0.", nameof(dimensionX));
            }
            if (dimensionY is not null && dimensionY <= 0)
            {
                throw new ArgumentException("Y dimension must be bigger than 0.", nameof(dimensionY));
            }
            if (dimensionZ is not null && dimensionZ <= 0)
            {
                throw new ArgumentException("Z dimension must be bigger than 0.", nameof(dimensionZ));
            }

            this.dimensionX = dimensionX;
            this.dimensionY = dimensionY;
            this.dimensionZ = dimensionZ;
        }
        #endregion
    }
}
