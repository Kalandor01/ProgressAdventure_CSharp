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
        /// <inheritdoc cref="TerrainContent(ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public ShoreTerrain(string? name, IDictionary<string, object?>? data = null)
            : base(ContentType.Terrain.SHORE, name, data)
        {
            depth = GetLongValueFromData("depth", data, (1, 100));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.player.fullName} entered a shore.");
            Console.WriteLine($"The shore is {depth}m deep.");
        }

        public override Dictionary<string, object?> ToJson()
        {
            var terrainJson = base.ToJson();
            terrainJson.Add("depth", depth);
            return terrainJson;
        }
        #endregion
    }
}
