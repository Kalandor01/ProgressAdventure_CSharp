using PACommon.Enums;
using ProgressAdventure.ConfigManagement;

namespace ProgressAdventure.Localization;

public class Text : AdvancedEnum<Text>
{
    protected static readonly bool isClearable = UpdateIsClearable(true);
    protected static readonly bool isRemovable = UpdateIsRemovable(true);

    /// <summary>
    /// args: folder being loaded from
    /// </summary>
    public static readonly EnumValue<Text> LOADING_FROM_FOLDER_1 = AddValue(ConfigUtils.MakeNamespacedString(nameof(LOADING_FROM_FOLDER_1).ToLower()));
    /// <summary>
    /// args: file being loaded from
    /// </summary>
    public static readonly EnumValue<Text> LOADING_FILE_FROM_CONFIG_1 = AddValue(ConfigUtils.MakeNamespacedString(nameof(LOADING_FILE_FROM_CONFIG_1).ToLower()));
}
