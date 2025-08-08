using NPrng.Generators;
using PACommon;
using PACommon.JsonUtils;

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
        /// <inheritdoc cref="StructureContent(SplittableRandom, Type, string?, JsonDictionary?)"/>
        public VillageStructure(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, typeof(VillageStructure), name, data)
        {
            population = GetLongValueFromData<VillageStructure>(base.chunkRandom, Constants.JsonKeys.VillageStructure.POPULATION, data, (50, 10000));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            PACSingletons.Instance.ConsoleProxy.WriteLine($"{SaveData.Instance.PlayerRef.FullName} entered the {Name} village.");
            PACSingletons.Instance.ConsoleProxy.WriteLine($"The village has a population of {population} people.");
        }
        #endregion

        #region JsonConvert
        public override JsonDictionary ToJson()
        {
            var structureJson = base.ToJson();
            structureJson.Add(Constants.JsonKeys.VillageStructure.POPULATION, population);
            return structureJson;
        }
        #endregion
    }
}
