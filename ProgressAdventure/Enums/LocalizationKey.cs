using PACommon.Enums;
using ProgressAdventure.ConfigManagement;

namespace ProgressAdventure.Enums
{
    /// <summary>
    /// All keys used to get localized strings.
    /// </summary>
    public class LocalizationKey : AdvancedEnum<LocalizationKey>
    {
        protected static readonly bool isClearable = UpdateIsClearable(true);
        protected static readonly bool isRemovable = UpdateIsRemovable(true);

        #region Main/Initialization
        public static readonly EnumValue<LocalizationKey> LANGUAGE_NAME_ENGLISH_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(LANGUAGE_NAME_ENGLISH_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> APPLICATION_TITLE_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(APPLICATION_TITLE_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> LOADING_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(LOADING_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> LOADING_COMMON_SINGLETONS_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(LOADING_COMMON_SINGLETONS_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> ACTIVATING_ANSI_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(ACTIVATING_ANSI_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> LOADING_PA_SINGLETONS_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(LOADING_PA_SINGLETONS_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> RELOADING_CONFIGS_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(RELOADING_CONFIGS_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> DONE_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(DONE_0).ToLower()));
        /// <summary>
        /// args: folder being loaded from
        /// </summary>
        public static readonly EnumValue<LocalizationKey> LOADING_FROM_FOLDER_1 = AddValue(ConfigUtils.MakeNamespacedString(nameof(LOADING_FROM_FOLDER_1).ToLower()));
        /// <summary>
        /// args: file being loaded from
        /// </summary>
        public static readonly EnumValue<LocalizationKey> LOADING_FILE_FROM_CONFIG_1 = AddValue(ConfigUtils.MakeNamespacedString(nameof(LOADING_FILE_FROM_CONFIG_1).ToLower()));
        public static readonly EnumValue<LocalizationKey> FAILED_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(FAILED_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> RESTART_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(RESTART_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> RESTART_IN_SAFE_MODE_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(RESTART_IN_SAFE_MODE_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> EXIT_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(EXIT_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> ERROR_COLON_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(ERROR_COLON_0).ToLower()));
        #endregion

        #region Keybinds
        public static readonly EnumValue<LocalizationKey> ESCAPE_0  = AddValue(ConfigUtils.MakeNamespacedString(nameof(ESCAPE_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> UP_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(UP_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> DOWN_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(DOWN_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> LEFT_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(LEFT_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> RIGHT_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(RIGHT_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> ENTER_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(ENTER_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> STATS_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(STATS_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> SAVE_0 = AddValue(ConfigUtils.MakeNamespacedString(nameof(SAVE_0).ToLower()));
        #endregion

        #region Items
        public static readonly EnumValue<LocalizationKey> COMPOUND_ITEM_DISPLAY_NAME_CLUB_WITH_TEETH_0  = AddValue(ConfigUtils.MakeNamespacedString(nameof(COMPOUND_ITEM_DISPLAY_NAME_CLUB_WITH_TEETH_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> COMPOUND_ITEM_DISPLAY_NAME_PIECE_0  = AddValue(ConfigUtils.MakeNamespacedString(nameof(COMPOUND_ITEM_DISPLAY_NAME_PIECE_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> COMPOUND_ITEM_DISPLAY_NAME_BOTTLE_0  = AddValue(ConfigUtils.MakeNamespacedString(nameof(COMPOUND_ITEM_DISPLAY_NAME_BOTTLE_0).ToLower()));
        #endregion

        #region Entities
        public static readonly EnumValue<LocalizationKey> ENTITY_DISPLAY_NAME_PLAYER_0  = AddValue(ConfigUtils.MakeNamespacedString(nameof(ENTITY_DISPLAY_NAME_PLAYER_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> ENTITY_DISPLAY_NAME_DEMON_0  = AddValue(ConfigUtils.MakeNamespacedString(nameof(ENTITY_DISPLAY_NAME_DEMON_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> ENTITY_DISPLAY_NAME_DWARF_0  = AddValue(ConfigUtils.MakeNamespacedString(nameof(ENTITY_DISPLAY_NAME_DWARF_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> ENTITY_DISPLAY_NAME_ELF_0  = AddValue(ConfigUtils.MakeNamespacedString(nameof(ENTITY_DISPLAY_NAME_ELF_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> ENTITY_DISPLAY_NAME_HUMAN_0  = AddValue(ConfigUtils.MakeNamespacedString(nameof(ENTITY_DISPLAY_NAME_HUMAN_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> ENTITY_DISPLAY_NAME_CAVEMAN_0  = AddValue(ConfigUtils.MakeNamespacedString(nameof(ENTITY_DISPLAY_NAME_CAVEMAN_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> ENTITY_DISPLAY_NAME_GHOUL_0  = AddValue(ConfigUtils.MakeNamespacedString(nameof(ENTITY_DISPLAY_NAME_GHOUL_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> ENTITY_DISPLAY_NAME_TROLL_0  = AddValue(ConfigUtils.MakeNamespacedString(nameof(ENTITY_DISPLAY_NAME_TROLL_0).ToLower()));
        public static readonly EnumValue<LocalizationKey> ENTITY_DISPLAY_NAME_DRAGON_0  = AddValue(ConfigUtils.MakeNamespacedString(nameof(ENTITY_DISPLAY_NAME_DRAGON_0).ToLower()));
        #endregion
    }
}
