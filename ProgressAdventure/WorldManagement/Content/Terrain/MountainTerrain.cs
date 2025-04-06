using NPrng.Generators;
using PACommon.JsonUtils;

namespace ProgressAdventure.WorldManagement.Content.Terrain
{
    /// <summary>
    /// Class for mountain terrain content layer, for a tile.
    /// </summary>
    public class MountainTerrain : TerrainContent
    {
        #region Public fields
        /// <summary>
        /// The height of the mountain.
        /// </summary>
        public readonly long height;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="MountainTerrain"/>
        /// </summary>
        /// <inheritdoc cref="TerrainContent(SplittableRandom, ContentTypeID, string?, JsonDictionary?)"/>
        public MountainTerrain(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, ContentType.Terrain.MOUNTAIN, name, data)
        {
            height = GetLongValueFromData<MountainTerrain>(base.chunkRandom, Constants.JsonKeys.MountainTerrain.HEIGHT, data, (500, 10000));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.Instance.PlayerRef.FullName} climbed the {Name} mountain.");
            Console.WriteLine($"The mountain is {height}m tall.");
        }
        #endregion

        #region JsonConvert
        public override JsonDictionary ToJson()
        {
            var terrainJson = base.ToJson();
            terrainJson.Add(Constants.JsonKeys.MountainTerrain.HEIGHT, height);
            return terrainJson;
        }
        #endregion
    }
}
