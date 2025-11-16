using PACommon.Enums;
using ProgressAdventure.ConfigManagement;

namespace ProgressAdventure.Enums
{
    /// <summary>
    /// All existing languages.
    /// </summary>
    public class Language : AdvancedEnum<Language>
    {
        protected static readonly bool isClearable = UpdateIsClearable(true);
        protected static readonly bool isRemovable = UpdateIsRemovable(true);

        public static readonly EnumValue<Language> ENGLISH = AddValue(ConfigUtils.MakeNamespacedString(nameof(ENGLISH).ToLower()));
    }
}
