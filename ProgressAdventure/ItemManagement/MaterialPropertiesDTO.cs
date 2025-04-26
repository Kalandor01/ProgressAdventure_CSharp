using System.Text.Json.Serialization;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// DTO used for storing the properties of a material.
    /// </summary>
    public class MaterialPropertiesDTO
    {
        #region Fields
        /// <summary>
        /// The density of the material in kg/m3.
        /// </summary>
        [JsonPropertyName("density")]
        public readonly double density;
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="MaterialPropertiesDTO"/>
        /// </summary>
        /// <param name="density"><inheritdoc cref="density" path="//summary"/></param>
        [JsonConstructor]
        public MaterialPropertiesDTO(double density)
        {
            if (density < 0)
            {
                throw new ArgumentException("Density must be bigger than 0.", nameof(density));
            }
            this.density = density;
        }
        #endregion
    }
}
