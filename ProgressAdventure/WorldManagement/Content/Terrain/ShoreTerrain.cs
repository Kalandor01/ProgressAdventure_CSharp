using NPrng.Generators;
using PACommon.JsonUtils;

namespace ProgressAdventure.WorldManagement.Content.Terrain
{
    /// <summary>
    /// Class for shore terrain content layer, for a tile.
    /// </summary>
    public class ShoreTerrain : TerrainContent
    {
        #region Public fields
        /// <summary>
        /// The depth of the shore.
        /// </summary>
        public readonly long depth;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="ShoreTerrain"/>
        /// </summary>
        /// <inheritdoc cref="TerrainContent(SplittableRandom, ContentTypeID, string?, JsonDictionary?)"/>
        public ShoreTerrain(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, ContentType.Terrain.SHORE, name, data)
        {
            depth = GetLongValueFromData<ShoreTerrain>(base.chunkRandom, Constants.JsonKeys.ShoreTerrain.DEPTH, data, (1, 100));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.Instance.PlayerRef.FullName} entered the {Name} shore.");
            Console.WriteLine($"The shore is {depth}m deep.");
        }
        #endregion

        #region JsonConvert
        public override JsonDictionary ToJson()
        {
            var terrainJson = base.ToJson();
            terrainJson.Add(Constants.JsonKeys.ShoreTerrain.DEPTH, depth);
            return terrainJson;
        }
        #endregion
    }
}
