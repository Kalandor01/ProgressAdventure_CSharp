using NPrng.Generators;
using PACommon.JsonUtils;

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
        /// <inheritdoc cref="PopulationContent(SplittableRandom, ContentTypeID, string?, JsonDictionary?)"/>
        public HumanPopulation(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, ContentType.Population.HUMAN, name, data) { }
        #endregion
    }
}
