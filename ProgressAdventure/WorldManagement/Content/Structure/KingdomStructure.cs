using NPrng.Generators;
using PACommon.JsonUtils;

namespace ProgressAdventure.WorldManagement.Content.Structure
{
    /// <summary>
    /// Class for kingdom structure content layer, for a tile.
    /// </summary>
    public class KingdomStructure : StructureContent
    {
        #region Public fields
        /// <summary>
        /// The population of the village.
        /// </summary>
        public readonly long population;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="KingdomStructure"/>
        /// </summary>
        /// <inheritdoc cref="StructureContent(SplittableRandom, ContentTypeID, string?, JsonDictionary?)"/>
        public KingdomStructure(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, ContentType.Structure.KINGDOM, name, data)
        {
            population = GetLongValueFromData<KingdomStructure>(base.chunkRandom, Constants.JsonKeys.KingdomStructure.POPULATION, data, (10000, 10000000));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.Instance.PlayerRef.FullName} entered the {Name} kingdom.");
            Console.WriteLine($"The kingdom has a population of {population} people.");
        }
        #endregion

        #region JsonConvert
        public override JsonDictionary ToJson()
        {
            var structureJson = base.ToJson();
            structureJson.Add(Constants.JsonKeys.KingdomStructure.POPULATION, population);
            return structureJson;
        }
        #endregion
    }
}
