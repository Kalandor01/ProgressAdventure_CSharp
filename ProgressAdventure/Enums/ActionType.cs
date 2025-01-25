using PACommon.Enums;

namespace ProgressAdventure.Enums
{
    public class ActionType : AdvancedEnum<ActionType>
    {
        protected static readonly bool isClearable = UpdateIsClearable(true);

        public static readonly EnumValue<ActionType> ESCAPE = AddValue(nameof(ESCAPE));
        public static readonly EnumValue<ActionType> UP = AddValue(nameof(UP));
        public static readonly EnumValue<ActionType> DOWN = AddValue(nameof(DOWN));
        public static readonly EnumValue<ActionType> LEFT = AddValue(nameof(LEFT));
        public static readonly EnumValue<ActionType> RIGHT = AddValue(nameof(RIGHT));
        public static readonly EnumValue<ActionType> ENTER = AddValue(nameof(ENTER));
        public static readonly EnumValue<ActionType> STATS = AddValue(nameof(STATS));
        public static readonly EnumValue<ActionType> SAVE = AddValue(nameof(SAVE));
    }
}
