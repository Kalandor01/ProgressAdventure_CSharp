using NPrng.Generators;
using PACommon.JsonUtils;

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
        /// <inheritdoc cref="PopulationContent(SplittableRandom, ContentTypeID, string?, JsonDictionary?)"/>
        public DemonPopulation(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, ContentType.Population.DEMON, name, data) { }
        #endregion
    }
}
