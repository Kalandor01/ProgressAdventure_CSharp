namespace ProgressAdventure.WorldManagement.Content.Structure
{
    /// <summary>
    /// Class for village structure content layer, for a tile.
    /// </summary>
    public class VillageStructure : StructureContent
    {
        #region Public fields
        /// <summary>
        /// The population of the village.
        /// </summary>
        public readonly long population;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="VillageStructure"/>
        /// </summary>
        /// <inheritdoc cref="StructureContent(ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public VillageStructure(string? name, IDictionary<string, object?>? data = null)
            : base(ContentType.Structure.VILLAGE, name, data)
        {
            population = GetLongValueFromData("population", data, (50, 10000));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.player.fullName} entered a village.");
            Console.WriteLine($"The village has a population of {population} people.");
        }

        public override Dictionary<string, object?> ToJson()
        {
            var structureJson = base.ToJson();
            structureJson.Add("population", population);
            return structureJson;
        }
        #endregion
    }
}
