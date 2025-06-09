using PACommon.Enums;
using ProgressAdventure.ConfigManagement;

namespace ProgressAdventure.Enums
{
    /// <summary>
    /// All existing terrain types.
    /// </summary>
    public class TerrainType : AdvancedEnum<TerrainType>
    {
        protected static readonly bool isClearable = UpdateIsClearable(true);
        protected static readonly bool isRemovable = UpdateIsRemovable(true);

        public static readonly EnumValue<TerrainType> FIELD = AddValue(ConfigUtils.MakeNamespacedString(nameof(FIELD).ToLower()));
        public static readonly EnumValue<TerrainType> MOUNTAIN = AddValue(ConfigUtils.MakeNamespacedString(nameof(MOUNTAIN).ToLower()));
        public static readonly EnumValue<TerrainType> OCEAN = AddValue(ConfigUtils.MakeNamespacedString(nameof(OCEAN).ToLower()));
        public static readonly EnumValue<TerrainType> SHORE = AddValue(ConfigUtils.MakeNamespacedString(nameof(SHORE).ToLower()));
    }
}
