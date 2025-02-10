using PACommon.Enums;
using ProgressAdventure.ConfigManagement;

namespace ProgressAdventure.Enums
{
    /// <summary>
    /// Posible attributes that modify an entity's stats.
    /// </summary>
    public class Attribute : AdvancedEnum<Attribute>
    {
        protected static readonly bool isClearable = UpdateIsClearable(true);
        
        /// <summary>
        /// Doubles all of the entity's stats.
        /// </summary>
        public static readonly EnumValue<Attribute> RARE = AddValue(ConfigUtils.MakeNamespacedString(nameof(RARE).ToLower()));
        /// <summary>
        /// Halfs all of the entity's stats.
        /// </summary>
        public static readonly EnumValue<Attribute> CRIPPLED = AddValue(ConfigUtils.MakeNamespacedString(nameof(CRIPPLED).ToLower()));
        /// <summary>
        /// Doubles the health of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> HEALTHY = AddValue(ConfigUtils.MakeNamespacedString(nameof(HEALTHY).ToLower()));
        /// <summary>
        /// Halfs the health of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> SICK = AddValue(ConfigUtils.MakeNamespacedString(nameof(SICK).ToLower()));
        /// <summary>
        /// Doubles the attack of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> STRONG = AddValue(ConfigUtils.MakeNamespacedString(nameof(STRONG).ToLower()));
        /// <summary>
        /// Halfs the attack of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> WEAK = AddValue(ConfigUtils.MakeNamespacedString(nameof(WEAK).ToLower()));
        /// <summary>
        /// Doubles the defence of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> TOUGH = AddValue(ConfigUtils.MakeNamespacedString(nameof(TOUGH).ToLower()));
        /// <summary>
        /// Halfs the defence of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> FRAIL = AddValue(ConfigUtils.MakeNamespacedString(nameof(FRAIL).ToLower()));
        /// <summary>
        /// Doubles the agility of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> AGILE = AddValue(ConfigUtils.MakeNamespacedString(nameof(AGILE).ToLower()));
        /// <summary>
        /// Halfs the agility of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> SLOW = AddValue(ConfigUtils.MakeNamespacedString(nameof(SLOW).ToLower()));
    }
}
