using PACommon.Enums;
using ProgressAdventure.ConfigManagement;

namespace ProgressAdventure.Enums
{
    /// <summary>
    /// Posible entity types.
    /// </summary>
    public class EntityType : AdvancedEnum<EntityType>
    {
        protected static readonly bool isClearable = UpdateIsClearable(true);
        protected static readonly bool isRemovable = UpdateIsRemovable(true);

        public static readonly EnumValue<EntityType> PLAYER = AddValue(ConfigUtils.MakeNamespacedString(nameof(PLAYER).ToLower()));
        public static readonly EnumValue<EntityType> CAVEMAN = AddValue(ConfigUtils.MakeNamespacedString(nameof(CAVEMAN).ToLower()));
        public static readonly EnumValue<EntityType> GHOUL = AddValue(ConfigUtils.MakeNamespacedString(nameof(GHOUL).ToLower()));
        public static readonly EnumValue<EntityType> TROLL = AddValue(ConfigUtils.MakeNamespacedString(nameof(TROLL).ToLower()));
        public static readonly EnumValue<EntityType> DRAGON = AddValue(ConfigUtils.MakeNamespacedString(nameof(DRAGON).ToLower()));
        public static readonly EnumValue<EntityType> DEMON = AddValue(ConfigUtils.MakeNamespacedString(nameof(DEMON).ToLower()));
        public static readonly EnumValue<EntityType> DWARF = AddValue(ConfigUtils.MakeNamespacedString(nameof(DWARF).ToLower()));
        public static readonly EnumValue<EntityType> ELF = AddValue(ConfigUtils.MakeNamespacedString(nameof(ELF).ToLower()));
        public static readonly EnumValue<EntityType> HUMAN = AddValue(ConfigUtils.MakeNamespacedString(nameof(HUMAN).ToLower()));
    }
}
