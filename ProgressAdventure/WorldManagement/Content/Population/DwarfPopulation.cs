using NPrng.Generators;
using PACommon.JsonUtils;

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
        /// <inheritdoc cref="PopulationContent(SplittableRandom, ContentTypeID, string?, JsonDictionary?)"/>
        public DwarfPopulation(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, ContentType.Population.DWARF, name, data) { }
        #endregion
    }
}
