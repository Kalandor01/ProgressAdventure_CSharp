using NPrng.Generators;

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
        /// <inheritdoc cref="PopulationContent(SplittableRandom, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public HumanPopulation(SplittableRandom chunkRandom, string? name = null, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.Population.HUMAN, name, data) { }
        #endregion
    }
}
