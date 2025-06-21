using NPrng.Generators;
using PACommon;
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
        /// <inheritdoc cref="TerrainContent(SplittableRandom, Type, string?, JsonDictionary?)"/>
        public ShoreTerrain(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, typeof(ShoreTerrain), name, data)
        {
            depth = GetLongValueFromData<ShoreTerrain>(base.chunkRandom, Constants.JsonKeys.ShoreTerrain.DEPTH, data, (1, 100));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            PACSingletons.Instance.ConsoleProxy.WriteLine($"{SaveData.Instance.PlayerRef.FullName} entered the {Name} shore.");
            PACSingletons.Instance.ConsoleProxy.WriteLine($"The shore is {depth}m deep.");
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
