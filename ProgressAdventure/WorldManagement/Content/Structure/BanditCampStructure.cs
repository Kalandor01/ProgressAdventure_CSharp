using NPrng.Generators;
using PACommon;
using PACommon.JsonUtils;

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
        /// <inheritdoc cref="StructureContent(SplittableRandom, Type, string?, JsonDictionary?)"/>
        public BanditCampStructure(SplittableRandom chunkRandom, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, typeof(BanditCampStructure), name, data) { }
        #endregion

        #region Public overrides
        public override void Visit(Tile tile)
        {
            base.Visit(tile);
            PACSingletons.Instance.ConsoleProxy.WriteLine($"{SaveData.Instance.PlayerRef.FullName} entered the {Name} bandit camp.");
        }
        #endregion
    }
}
