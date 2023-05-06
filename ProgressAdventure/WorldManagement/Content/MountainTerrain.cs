namespace ProgressAdventure.WorldManagement.Content
{
    public class MountainTerrain : TerrainContent
    {
        #region Public fields
        public readonly long height;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="FieldTerrain"/>
        /// </summary>
        /// <inheritdoc cref="TerrainContent(ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public MountainTerrain(string? name, IDictionary<string, object?>? data = null)
            : base(ContentType.Terrain.MOUNTAIN, name, data)
        {
            height = GetLongValueFromData("height", data, (500, 10000));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.player.fullName} climbed a mountain.");
            Console.WriteLine($"The mountain is {height}m tall.");
        }

        public override Dictionary<string, object?> ToJson()
        {
            var terrainJson = base.ToJson();
            terrainJson.Add("height", height);
            return terrainJson;
        }
        #endregion
    }
}
