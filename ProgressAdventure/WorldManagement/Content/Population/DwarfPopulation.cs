namespace ProgressAdventure.WorldManagement.Content.Population
{
    /// <summary>
    /// Class for dwarf population content layer, for a tile.
    /// </summary>
    public class DwarfPopulation : PopulationContent
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="DwarfPopulation"/>
        /// </summary>
        /// <inheritdoc cref="PopulationContent(ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public DwarfPopulation(string? name, IDictionary<string, object?>? data = null)
            : base(ContentType.Population.DWARF, name, data) { }
        #endregion
    }
}
