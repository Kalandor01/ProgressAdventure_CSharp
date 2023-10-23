using NPrng.Generators;

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
        /// <inheritdoc cref="TerrainContent(SplittableRandom, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public MountainTerrain(SplittableRandom chunkRandom, string? name = null, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.Terrain.MOUNTAIN, name, data)
        {
            height = GetLongValueFromData(base.chunkRandom, "height", data, (500, 10000));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.Instance.player.FullName} climbed a mountain.");
            Console.WriteLine($"The mountain is {height}m tall.");
        }
        #endregion

        #region JsonConvert
        public override Dictionary<string, object?> ToJson()
        {
            var terrainJson = base.ToJson();
            terrainJson.Add("height", height);
            return terrainJson;
        }
        #endregion
    }
}
