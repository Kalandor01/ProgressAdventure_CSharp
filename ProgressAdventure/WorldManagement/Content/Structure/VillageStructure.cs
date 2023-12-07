using NPrng.Generators;

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
        /// <inheritdoc cref="StructureContent(SplittableRandom, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public VillageStructure(SplittableRandom chunkRandom, string? name = null, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.Structure.VILLAGE, name, data)
        {
            population = GetLongValueFromData<VillageStructure>(base.chunkRandom, Constants.JsonKeys.VillageStructure.POPULATION, data, (50, 10000));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.Instance.player.FullName} entered a village.");
            Console.WriteLine($"The village has a population of {population} people.");
        }
        #endregion

        #region JsonConvert
        public override Dictionary<string, object?> ToJson()
        {
            var structureJson = base.ToJson();
            structureJson.Add(Constants.JsonKeys.VillageStructure.POPULATION, population);
            return structureJson;
        }
        #endregion
    }
}
