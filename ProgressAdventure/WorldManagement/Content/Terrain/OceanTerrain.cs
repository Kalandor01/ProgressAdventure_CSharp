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
        /// <inheritdoc cref="TerrainContent(ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public OceanTerrain(string? name = null, IDictionary<string, object?>? data = null)
            : base(ContentType.Terrain.OCEAN, name, data)
        {
            depth = GetLongValueFromData("depth", data, (100, 20000));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.player.fullName} entered an ocean.");
            Console.WriteLine($"The ocean is {depth}m deep.");
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
