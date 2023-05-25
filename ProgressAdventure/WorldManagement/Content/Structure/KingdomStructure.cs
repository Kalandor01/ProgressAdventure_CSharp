using NPrng.Generators;

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
        /// <inheritdoc cref="StructureContent(SplittableRandom, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public KingdomStructure(SplittableRandom chunkRandom, string? name = null, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.Structure.KINGDOM, name, data)
        {
            population = GetLongValueFromData(base.chunkRandom, "population", data, (10000, 10000000));
        }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.player.FullName} entered a kingdom.");
            Console.WriteLine($"The kingdom has a population of {population} people.");
        }
        #endregion

        #region JsonConvert
        public override Dictionary<string, object?> ToJson()
        {
            var structureJson = base.ToJson();
            structureJson.Add("population", population);
            return structureJson;
        }
        #endregion
    }
}
