using NPrng.Generators;

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
        /// <inheritdoc cref="TerrainContent(SplittableRandom, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public ShoreTerrain(SplittableRandom chunkRandom, string? name = null, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.Terrain.SHORE, name, data)
        {
            depth = GetLongValueFromData<ShoreTerrain>(base.chunkRandom, Constants.JsonKeys.ShoreTerrain.DEPTH, data, (1, 100));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.Instance.player.FullName} entered the {Name} shore.");
            Console.WriteLine($"The shore is {depth}m deep.");
        }
        #endregion

        #region JsonConvert
        public override Dictionary<string, object?> ToJson()
        {
            var terrainJson = base.ToJson();
            terrainJson.Add(Constants.JsonKeys.ShoreTerrain.DEPTH, depth);
            return terrainJson;
        }
        #endregion
    }
}
