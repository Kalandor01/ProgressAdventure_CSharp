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
        public readonly double density;
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="MaterialPropertiesDTO"/>
        /// </summary>
        /// <param name="density"><inheritdoc cref="density" path="//summary"/></param>
        public MaterialPropertiesDTO(double density)
        {
            this.density = density;
            if (this.density < 0)
            {
                this.density = 0;
            }
        }
        #endregion
    }
}
