using NPrng.Generators;

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
        /// <inheritdoc cref="StructureContent(SplittableRandom, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        public BanditCampStructure(SplittableRandom chunkRandom, string? name = null, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.Structure.BANDIT_CAMP, name, data) { }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            Console.WriteLine($"{SaveData.Instance.player.FullName} entered a bandit camp.");
        }
        #endregion
    }
}
