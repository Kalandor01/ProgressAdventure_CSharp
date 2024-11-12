using NPrng.Generators;
using PACommon.JsonUtils;

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
        /// <inheritdoc cref="PopulationContent(SplittableRandom, ContentTypeID, string?, JsonDictionary?)"/>
        public NoPopulation(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, ContentType.Population.NONE, name, data)
        {
            amount = 0;
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile) { }
        #endregion
    }
}
