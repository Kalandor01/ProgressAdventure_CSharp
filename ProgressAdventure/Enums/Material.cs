using PACommon.Enums;
using ProgressAdventure.ConfigManagement;

namespace ProgressAdventure.Enums
{
    /// <summary>
    /// All existing materials.
    /// </summary>
    public class Material : AdvancedEnum<Material>
    {
        protected static readonly bool isClearable = UpdateIsClearable(true);
        protected static readonly bool isRemovable = UpdateIsRemovable(true);

        public static readonly EnumValue<Material> WOOD = AddValue(ConfigUtils.MakeNamespacedString(nameof(WOOD).ToLower()));
        public static readonly EnumValue<Material> STONE = AddValue(ConfigUtils.MakeNamespacedString(nameof(STONE).ToLower()));
        public static readonly EnumValue<Material> COPPER = AddValue(ConfigUtils.MakeNamespacedString(nameof(COPPER).ToLower()));
        public static readonly EnumValue<Material> BRASS = AddValue(ConfigUtils.MakeNamespacedString(nameof(BRASS).ToLower()));
        public static readonly EnumValue<Material> IRON = AddValue(ConfigUtils.MakeNamespacedString(nameof(IRON).ToLower()));
        public static readonly EnumValue<Material> STEEL = AddValue(ConfigUtils.MakeNamespacedString(nameof(STEEL).ToLower()));
        public static readonly EnumValue<Material> GLASS = AddValue(ConfigUtils.MakeNamespacedString(nameof(GLASS).ToLower()));
        public static readonly EnumValue<Material> LEATHER = AddValue(ConfigUtils.MakeNamespacedString(nameof(LEATHER).ToLower()));
        public static readonly EnumValue<Material> TEETH = AddValue(ConfigUtils.MakeNamespacedString(nameof(TEETH).ToLower()));
        public static readonly EnumValue<Material> WOOL = AddValue(ConfigUtils.MakeNamespacedString(nameof(WOOL).ToLower()));
        public static readonly EnumValue<Material> CLOTH = AddValue(ConfigUtils.MakeNamespacedString(nameof(CLOTH).ToLower()));
        public static readonly EnumValue<Material> SILVER = AddValue(ConfigUtils.MakeNamespacedString(nameof(SILVER).ToLower()));
        public static readonly EnumValue<Material> GOLD = AddValue(ConfigUtils.MakeNamespacedString(nameof(GOLD).ToLower()));
        public static readonly EnumValue<Material> ROTTEN_FLESH = AddValue(ConfigUtils.MakeNamespacedString(nameof(ROTTEN_FLESH).ToLower()));
        public static readonly EnumValue<Material> HEALING_LIQUID = AddValue(ConfigUtils.MakeNamespacedString(nameof(HEALING_LIQUID).ToLower()));
        public static readonly EnumValue<Material> FLINT = AddValue(ConfigUtils.MakeNamespacedString(nameof(FLINT).ToLower()));
        public static readonly EnumValue<Material> SILK = AddValue(ConfigUtils.MakeNamespacedString(nameof(SILK).ToLower()));
    }
}
