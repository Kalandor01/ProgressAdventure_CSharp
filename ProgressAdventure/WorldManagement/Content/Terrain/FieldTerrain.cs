using NPrng.Generators;
using PACommon.JsonUtils;

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
        /// <inheritdoc cref="TerrainContent(SplittableRandom, ContentTypeID, string?, JsonDictionary?)"/>
        public FieldTerrain(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, ContentType.Terrain.FIELD, name, data) { }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.Instance.PlayerRef.FullName} entered the {Name} field.");
        }
        #endregion
    }
}
