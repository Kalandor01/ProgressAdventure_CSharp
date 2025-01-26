using PACommon.Enums;

namespace ProgressAdventure.ItemManagement
{
    public class ItemType : AdvancedEnumTree<ItemType>
    {
        protected static readonly bool isClearable = UpdateIsClearable(true);

        /// <summary>
        /// Loads the default values of the enum.
        /// </summary>
        public static void InitializeDefaultValues()
        {
            var w = Weapon.CLUB;
            var d = Defence.HELMET;
            var m = Misc.ROD;
        }

        public static readonly EnumTreeValue<ItemType> _WEAPON = AddValue(null, $"{Constants.PA_CONFIGS_NAMESPACE}:{nameof(Weapon)}".ToLower());
        public static class Weapon
        {
            public static readonly EnumTreeValue<ItemType> SWORD = AddValue(_WEAPON, nameof(SWORD).ToLower());
            public static readonly EnumTreeValue<ItemType> BOW = AddValue(_WEAPON, nameof(BOW).ToLower());
            public static readonly EnumTreeValue<ItemType> ARROW = AddValue(_WEAPON, nameof(ARROW).ToLower());
            public static readonly EnumTreeValue<ItemType> CLUB = AddValue(_WEAPON, nameof(CLUB).ToLower());
            public static readonly EnumTreeValue<ItemType> CLUB_WITH_TEETH = AddValue(_WEAPON, nameof(CLUB_WITH_TEETH).ToLower());
        }

        public static readonly EnumTreeValue<ItemType> _DEFENCE = AddValue(null, $"{Constants.PA_CONFIGS_NAMESPACE}:{nameof(Defence)}".ToLower());
        public static class Defence
        {
            public static readonly EnumTreeValue<ItemType> SHIELD = AddValue(_DEFENCE, nameof(SHIELD).ToLower());
            public static readonly EnumTreeValue<ItemType> HELMET = AddValue(_DEFENCE, nameof(HELMET).ToLower());
            public static readonly EnumTreeValue<ItemType> CHESTPLATE = AddValue(_DEFENCE, nameof(CHESTPLATE).ToLower());
            public static readonly EnumTreeValue<ItemType> PANTS = AddValue(_DEFENCE, nameof(PANTS).ToLower());
            public static readonly EnumTreeValue<ItemType> BOOTS = AddValue(_DEFENCE, nameof(BOOTS).ToLower());
        }

        public static readonly EnumTreeValue<ItemType> _MISC = AddValue(null, $"{Constants.PA_CONFIGS_NAMESPACE}:{nameof(Misc)}".ToLower());
        public static class Misc
        {
            public static readonly EnumTreeValue<ItemType> MATERIAL = AddValue(_MISC, nameof(MATERIAL).ToLower());
            public static readonly EnumTreeValue<ItemType> BOTTLE = AddValue(_MISC, nameof(BOTTLE).ToLower());
            public static readonly EnumTreeValue<ItemType> FILLED_BOTTLE = AddValue(_MISC, nameof(FILLED_BOTTLE).ToLower());
            public static readonly EnumTreeValue<ItemType> COIN = AddValue(_MISC, nameof(COIN).ToLower());
            public static readonly EnumTreeValue<ItemType> SWORD_HILT = AddValue(_MISC, nameof(SWORD_HILT).ToLower());
            public static readonly EnumTreeValue<ItemType> SWORD_BLADE = AddValue(_MISC, nameof(SWORD_BLADE).ToLower());
            public static readonly EnumTreeValue<ItemType> ARROW_TIP = AddValue(_MISC, nameof(ARROW_TIP).ToLower());
            public static readonly EnumTreeValue<ItemType> ROD = AddValue(_MISC, nameof(ROD).ToLower());
        }
    }
}
