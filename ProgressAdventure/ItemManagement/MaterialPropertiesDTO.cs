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

        /// <summary>
        /// The temperature where the material melts in °C.
        /// </summary>
        [JsonPropertyName("melting_point")]
        public readonly double meltingPoint;

        /// <summary>
        /// The temperature where the material evaporates in °C.
        /// </summary>
        [JsonPropertyName("boiling_point")]
        public readonly double boilingPoint;

        /// <summary>
        /// If the material burnas instead of melting.<br/>
        /// If true, the <see cref="meltingPoint"/> is the temperature at witch decomposition begins, and the <see cref="boilingPoint"/> is the self ignition point.
        /// </summary>
        [JsonPropertyName("burns")]
        public readonly bool burns;
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="MaterialPropertiesDTO"/>
        /// </summary>
        /// <param name="density"><inheritdoc cref="density" path="//summary"/></param>
        /// <param name="meltingPoint"><inheritdoc cref="meltingPoint" path="//summary"/></param>
        /// <param name="boilingPoint"><inheritdoc cref="boilingPoint" path="//summary"/></param>
        /// <param name="burns"><inheritdoc cref="burns" path="//summary"/></param>
        [JsonConstructor]
        public MaterialPropertiesDTO(double density, double meltingPoint, double boilingPoint, bool burns = false)
        {
            if (density < 0)
            {
                throw new ArgumentException("Density must be bigger than 0.", nameof(density));
            }
            this.density = density;


            if (meltingPoint < Constants.ZERO_KELVIN_IN_C)
            {
                throw new ArgumentException("Melting point must be bigger than 0K.", nameof(meltingPoint));
            }
            this.meltingPoint = meltingPoint;

            if (boilingPoint < meltingPoint)
            {
                throw new ArgumentException("Boiling point must not be smaller tham the melting point.", nameof(boilingPoint));
            }
            this.boilingPoint = boilingPoint;
            this.burns = burns;
        }
        #endregion
    }
}
