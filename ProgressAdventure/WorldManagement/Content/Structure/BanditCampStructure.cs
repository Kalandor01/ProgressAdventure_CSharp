namespace ProgressAdventure.WorldManagement.Content.Structure
{
    /// <summary>
    /// Class for bandit camp structure content layer, for a tile.
    /// </summary>
    public class BanditCampStructure : StructureContent
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="BanditCampStructure"/>
        /// </summary>
        /// <inheritdoc cref="StructureContent(ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public BanditCampStructure(string? name, IDictionary<string, object?>? data = null)
            : base(ContentType.Structure.BANDIT_CAMP, name, data) { }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.player.fullName} entered a bandit camp.");
        }
        #endregion
    }
}
