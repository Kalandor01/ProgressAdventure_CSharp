using PACommon.Enums;
using ProgressAdventure.ConfigManagement;

namespace ProgressAdventure.Enums
{
    /// <summary>
    /// All existing structure types.
    /// </summary>
    public class StructureType : AdvancedEnum<StructureType>
    {
        protected static readonly bool isClearable = UpdateIsClearable(true);
        protected static readonly bool isRemovable = UpdateIsRemovable(true);
        
        public static readonly EnumValue<StructureType> NONE = AddValue(ConfigUtils.MakeNamespacedString(nameof(NONE).ToLower()));
        public static readonly EnumValue<StructureType> VILLAGE = AddValue(ConfigUtils.MakeNamespacedString(nameof(VILLAGE).ToLower()));
        public static readonly EnumValue<StructureType> KINGDOM = AddValue(ConfigUtils.MakeNamespacedString(nameof(KINGDOM).ToLower()));
        public static readonly EnumValue<StructureType> BANDIT_CAMP = AddValue(ConfigUtils.MakeNamespacedString(nameof(BANDIT_CAMP).ToLower()));
    }
}
