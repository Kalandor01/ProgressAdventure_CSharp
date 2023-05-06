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
        /// <inheritdoc cref="BaseContent(ContentTypeID, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        protected PopulationContent(ContentTypeID subtype, string? name, IDictionary<string, object?>? data = null)
            : base(ContentType.PopulationContentType, subtype, name, data)
        {
            amount = GetLongValueFromData("amount", data, (1, 1000));
        }
        #endregion

        #region Protected methods
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            if (subtype != ContentType.Population.NONE && amount > 0)
            {
                Console.WriteLine($"There {(amount == 1 ? "is" : "are")} {amount} {subtype}{(amount == 1 ? "" : "s")} here.");
            }
            if (subtype != ContentType.Population.NONE && tile.structure.subtype != ContentType.Structure.NONE)
            {
                if (tile.structure.subtype == ContentType.Structure.BANDIT_CAMP)
                {
                    if (SaveData.WorldRandom.GenerateDouble() < 0.75)
                    {
                        Console.WriteLine("fight");
                    }
                }
                else if (
                    tile.structure.subtype == ContentType.Structure.VILLAGE ||
                    tile.structure.subtype == ContentType.Structure.KINGDOM
                )
                {
                    if (SaveData.WorldRandom.GenerateDouble() < 0.01)
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
    }
}
