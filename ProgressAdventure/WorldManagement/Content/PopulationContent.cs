using NPrng.Generators;

namespace ProgressAdventure.WorldManagement.Content
{
    /// <summary>
    /// Abstract class for the population content layer, for a tile.
    /// </summary>
    public abstract class PopulationContent : BaseContent
    {
        #region Public fields
        public long amount;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="PopulationContent"/>
        /// </summary>
        /// <inheritdoc cref="BaseContent(SplittableRandom, ContentTypeID, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        protected PopulationContent(SplittableRandom chunkRandom, ContentTypeID subtype, string? name, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.PopulationContentType, subtype, name, data)
        {
            amount = GetLongValueFromData(this.chunkRandom, "amount", data, (1, 1000));
        }
        #endregion

        #region Public methods
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            if (subtype != ContentType.Population.NONE && amount > 0)
            {
                Console.WriteLine($"There {(amount == 1 ? "is" : "are")} {amount} {WorldUtils.contentTypeIDSubtypeTextMap[type][subtype]}{(amount == 1 ? "" : "s")} here.");
            }
            if (subtype != ContentType.Population.NONE && tile.structure.subtype != ContentType.Structure.NONE)
            {
                if (tile.structure.subtype == ContentType.Structure.BANDIT_CAMP)
                {
                    if (chunkRandom.GenerateDouble() < 0.75)
                    {
                        Console.WriteLine("fight");
                    }
                }
                else if (
                    tile.structure.subtype == ContentType.Structure.VILLAGE ||
                    tile.structure.subtype == ContentType.Structure.KINGDOM
                )
                {
                    if (chunkRandom.GenerateDouble() < 0.01)
                    {
                        Console.WriteLine("fight");
                    }
                }
            }
        }

        public override Dictionary<string, object?> ToJson()
        {
            var populationJson = base.ToJson();
            populationJson.Add("amount", amount);
            return populationJson;
        }
        #endregion

        #region Public functions
        /// <inheritdoc cref="BaseContent.LoadContent{T}(SplittableRandom, IDictionary{string, object?}?)"/>
        public static PopulationContent FromJson(SplittableRandom chunkRandom, IDictionary<string, object?>? contentJson)
        {
            return LoadContent<PopulationContent>(chunkRandom, contentJson);
        }
        #endregion
    }
}
