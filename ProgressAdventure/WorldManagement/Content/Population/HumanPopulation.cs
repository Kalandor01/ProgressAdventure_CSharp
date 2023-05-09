namespace ProgressAdventure.WorldManagement.Content.Population
{
    /// <summary>
    /// Class for human population content layer, for a tile.
    /// </summary>
    public class HumanPopulation : PopulationContent
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="HumanPopulation"/>
        /// </summary>
        /// <inheritdoc cref="PopulationContent(ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public HumanPopulation(string? name, IDictionary<string, object?>? data = null)
            : base(ContentType.Population.HUMAN, name, data) { }
        #endregion
    }
}
