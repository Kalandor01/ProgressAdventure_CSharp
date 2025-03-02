using NPrng.Generators;
using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.EntityManagement;

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
        protected PopulationContent(SplittableRandom chunkRandom, EnumTreeValue<ContentType> subtype, string? name, JsonDictionary? data = null)
            : base(chunkRandom, ContentType._POPULATION, subtype, name, data)
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

        /// <inheritdoc cref="BaseContent.FromJson{T}(SplittableRandom, JsonDictionary?, string, out T)"/>
        public static bool FromJson(SplittableRandom chunkRandom, JsonDictionary? contentJson, string fileVersion, out PopulationContent? contentObject)
        {
            return BaseContent.FromJson(chunkRandom, contentJson, fileVersion, out contentObject);
        }
        #endregion
    }
}
