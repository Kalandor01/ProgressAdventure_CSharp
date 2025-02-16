using PACommon.Enums;
using ProgressAdventure.ConfigManagement;

namespace ProgressAdventure.Enums
{
    public class ActionType : AdvancedEnum<ActionType>
    {
        protected static readonly bool isClearable = UpdateIsClearable(true);
        protected static readonly bool isRemovable = UpdateIsRemovable(true);

        public static readonly EnumValue<ActionType> ESCAPE = AddValue(ConfigUtils.MakeNamespacedString(nameof(ESCAPE).ToLower()));
        public static readonly EnumValue<ActionType> UP = AddValue(ConfigUtils.MakeNamespacedString(nameof(UP).ToLower()));
        public static readonly EnumValue<ActionType> DOWN = AddValue(ConfigUtils.MakeNamespacedString(nameof(DOWN).ToLower()));
        public static readonly EnumValue<ActionType> LEFT = AddValue(ConfigUtils.MakeNamespacedString(nameof(LEFT).ToLower()));
        public static readonly EnumValue<ActionType> RIGHT = AddValue(ConfigUtils.MakeNamespacedString(nameof(RIGHT).ToLower()));
        public static readonly EnumValue<ActionType> ENTER = AddValue(ConfigUtils.MakeNamespacedString(nameof(ENTER).ToLower()));
        public static readonly EnumValue<ActionType> STATS = AddValue(ConfigUtils.MakeNamespacedString(nameof(STATS).ToLower()));
        public static readonly EnumValue<ActionType> SAVE = AddValue(ConfigUtils.MakeNamespacedString(nameof(SAVE).ToLower()));
    }
}
