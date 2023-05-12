using NPrng.Generators;

namespace ProgressAdventure.WorldManagement.Content.Population
{
    /// <summary>
    /// Class for no population content layer, for a tile.
    /// </summary>
    public class NoPopulation : PopulationContent
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="NoPopulation"/>
        /// </summary>
        /// <inheritdoc cref="PopulationContent(SplittableRandom, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public NoPopulation(SplittableRandom chunkRandom, string? name = null, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.Population.NONE, name, data) { }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile) { }
        #endregion
    }
}
