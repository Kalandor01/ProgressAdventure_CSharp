using NPrng.Generators;
using PACommon.JsonUtils;

namespace ProgressAdventure.WorldManagement.Content.Terrain
{
    /// <summary>
    /// Class for ocean terrain content layer, for a tile.
    /// </summary>
    public class OceanTerrain : TerrainContent
    {
        #region Public fields
        /// <summary>
        /// The depth of the shore.
        /// </summary>
        public readonly long depth;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="OceanTerrain"/>
        /// </summary>
        /// <inheritdoc cref="TerrainContent(SplittableRandom, ContentTypeID, string?, JsonDictionary?)"/>
        public OceanTerrain(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, ContentType.Terrain.OCEAN, name, data)
        {
            depth = GetLongValueFromData<OceanTerrain>(base.chunkRandom, Constants.JsonKeys.OceanTerrain.DEPTH, data, (100, 20000));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.Instance.PlayerRef.FullName} entered the {Name} ocean.");
            Console.WriteLine($"The ocean is {depth}m deep.");
        }
        #endregion

        #region JsonConvert
        public override JsonDictionary ToJson()
        {
            var terrainJson = base.ToJson();
            terrainJson.Add(Constants.JsonKeys.OceanTerrain.DEPTH, depth);
            return terrainJson;
        }
        #endregion
    }
}
