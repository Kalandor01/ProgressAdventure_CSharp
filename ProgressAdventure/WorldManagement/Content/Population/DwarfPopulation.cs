using NPrng.Generators;

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
        /// <inheritdoc cref="PopulationContent(SplittableRandom, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public DwarfPopulation(SplittableRandom chunkRandom, string? name = null, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.Population.DWARF, name, data) { }
        #endregion
    }
}
