using NPrng.Generators;
using PACommon.JsonUtils;

namespace ProgressAdventure.WorldManagement.Content.Population
{
    /// <summary>
    /// Class for elf population content layer, for a tile.
    /// </summary>
    public class ElfPopulation : PopulationContent
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="ElfPopulation"/>
        /// </summary>
        /// <inheritdoc cref="PopulationContent(SplittableRandom, ContentTypeID, string?, JsonDictionary?)"/>
        public ElfPopulation(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, ContentType.Population.ELF, name, data) { }
        #endregion
    }
}
