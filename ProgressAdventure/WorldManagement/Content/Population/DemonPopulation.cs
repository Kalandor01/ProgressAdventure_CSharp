namespace ProgressAdventure.WorldManagement.Content.Population
{
    /// <summary>
    /// Class for demon population content layer, for a tile.
    /// </summary>
    public class DemonPopulation : PopulationContent
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="DemonPopulation"/>
        /// </summary>
        /// <inheritdoc cref="PopulationContent(ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public DemonPopulation(string? name = null, IDictionary<string, object?>? data = null)
            : base(ContentType.Population.DEMON, name, data) { }
        #endregion
    }
}
