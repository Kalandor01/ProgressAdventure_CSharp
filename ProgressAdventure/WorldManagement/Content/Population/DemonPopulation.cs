using NPrng.Generators;

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
        /// <inheritdoc cref="PopulationContent(SplittableRandom, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public DemonPopulation(SplittableRandom chunkRandom, string? name = null, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.Population.DEMON, name, data) { }
        #endregion
    }
}
