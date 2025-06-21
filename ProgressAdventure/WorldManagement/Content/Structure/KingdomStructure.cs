using NPrng.Generators;
using PACommon;
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
        /// <inheritdoc cref="StructureContent(SplittableRandom, Type, string?, JsonDictionary?)"/>
        public KingdomStructure(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, typeof(KingdomStructure), name, data)
        {
            population = GetLongValueFromData<KingdomStructure>(base.chunkRandom, Constants.JsonKeys.KingdomStructure.POPULATION, data, (10000, 10000000));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            PACSingletons.Instance.ConsoleProxy.WriteLine($"{SaveData.Instance.PlayerRef.FullName} entered the {Name} kingdom.");
            PACSingletons.Instance.ConsoleProxy.WriteLine($"The kingdom has a population of {population} people.");
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
