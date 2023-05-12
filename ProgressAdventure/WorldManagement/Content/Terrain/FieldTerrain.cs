using NPrng.Generators;

namespace ProgressAdventure.WorldManagement.Content.Terrain
{
    /// <summary>
    /// Class for filed terrain content layer, for a tile.
    /// </summary>
    public class FieldTerrain : TerrainContent
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="FieldTerrain"/>
        /// </summary>
        /// <inheritdoc cref="TerrainContent(SplittableRandom, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public FieldTerrain(SplittableRandom chunkRandom, string? name = null, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.Terrain.FIELD, name, data) { }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.player.fullName} entered a field.");
        }
        #endregion
    }
}
