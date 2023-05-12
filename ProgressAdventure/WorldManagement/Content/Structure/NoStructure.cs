using NPrng.Generators;

namespace ProgressAdventure.WorldManagement.Content.Structure
{
    /// <summary>
    /// Class for no structure content layer, for a tile.
    /// </summary>
    public class NoStructure : StructureContent
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="NoStructure"/>
        /// </summary>
        /// <inheritdoc cref="StructureContent(SplittableRandom, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public NoStructure(SplittableRandom chunkRandom, string? name = null, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.Structure.NONE, name, data) { }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile) { }
        #endregion
    }
}
