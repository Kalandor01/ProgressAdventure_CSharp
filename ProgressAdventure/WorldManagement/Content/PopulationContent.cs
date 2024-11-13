using NPrng.Generators;
using PACommon.JsonUtils;
using ProgressAdventure.Entity;

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
        /// <inheritdoc cref="BaseContent(SplittableRandom, ContentTypeID, ContentTypeID, string?, JsonDictionary?)"/>
        protected PopulationContent(SplittableRandom chunkRandom, ContentTypeID subtype, string? name, JsonDictionary? data = null)
            : base(chunkRandom, ContentType.PopulationContentType, subtype, name, data)
        {
            amount = GetLongValueFromData<PopulationContent>(this.chunkRandom, Constants.JsonKeys.PopulationContent.AMOUNT, data, (1, 1000));
        }
        #endregion

        #region Public methods
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            if (subtype != ContentType.Population.NONE && amount > 0)
            {
                Console.WriteLine($"There {(amount == 1 ? "is" : "are")} {amount} {GetSubtypeName()}{(amount == 1 ? "" : "s")} here.");
            }
            if (subtype != ContentType.Population.NONE && tile.structure.subtype != ContentType.Structure.NONE)
            {
                if (tile.structure.subtype == ContentType.Structure.BANDIT_CAMP)
                {
                    if (chunkRandom.GenerateDouble() < 0.75)
                    {
                        EntityUtils.RandomFight();
                    }
                }
                else if (
                    tile.structure.subtype == ContentType.Structure.VILLAGE ||
                    tile.structure.subtype == ContentType.Structure.KINGDOM
                )
                {
                    if (chunkRandom.GenerateDouble() < 0.01)
                    {
                        EntityUtils.RandomFight();
                    }
                }
            }
        }
        #endregion

        #region JsonConvert
        public override JsonDictionary ToJson()
        {
            var populationJson = base.ToJson();
            populationJson.Add(Constants.JsonKeys.PopulationContent.AMOUNT, amount);
            return populationJson;
        }

        /// <inheritdoc cref="BaseContent.LoadContent{T}(SplittableRandom, JsonDictionary?, string, out T)"/>
        public static bool FromJson(SplittableRandom chunkRandom, JsonDictionary? contentJson, string fileVersion, out PopulationContent? contentObject)
        {
            return LoadContent(chunkRandom, contentJson, fileVersion, out contentObject);
        }
        #endregion
    }
}
