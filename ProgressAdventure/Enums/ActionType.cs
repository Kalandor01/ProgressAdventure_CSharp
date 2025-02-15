using PACommon.Enums;
using ProgressAdventure.ConfigManagement;

namespace ProgressAdventure.Enums
{
    public class ActionType : AdvancedEnum<ActionType>
    {
        protected static readonly bool isClearable = UpdateIsClearable(true);
        protected static readonly bool isRemovable = UpdateIsRemovable(true);

        public static readonly EnumValue<ActionType> ESCAPE = AddValue(ConfigUtils.MakeNamespacedString(nameof(ESCAPE)));
        public static readonly EnumValue<ActionType> UP = AddValue(ConfigUtils.MakeNamespacedString(nameof(UP)));
        public static readonly EnumValue<ActionType> DOWN = AddValue(ConfigUtils.MakeNamespacedString(nameof(DOWN)));
        public static readonly EnumValue<ActionType> LEFT = AddValue(ConfigUtils.MakeNamespacedString(nameof(LEFT)));
        public static readonly EnumValue<ActionType> RIGHT = AddValue(ConfigUtils.MakeNamespacedString(nameof(RIGHT)));
        public static readonly EnumValue<ActionType> ENTER = AddValue(ConfigUtils.MakeNamespacedString(nameof(ENTER)));
        public static readonly EnumValue<ActionType> STATS = AddValue(ConfigUtils.MakeNamespacedString(nameof(STATS)));
        public static readonly EnumValue<ActionType> SAVE = AddValue(ConfigUtils.MakeNamespacedString(nameof(SAVE)));
    }
}
