using PACommon.Enums;

namespace ProgressAdventure.Enums
{
    /// <summary>
    /// Posible attributes that modify an entity's stats.
    /// </summary>
    public class Attribute : AdvancedEnum<Attribute>
    {
        /// <summary>
        /// Doubles all of the entity's stats.
        /// </summary>
        public static readonly EnumValue<Attribute> RARE = AddValue(nameof(RARE));
        /// <summary>
        /// Halfs all of the entity's stats.
        /// </summary>
        public static readonly EnumValue<Attribute> CRIPPLED = AddValue(nameof(CRIPPLED));
        /// <summary>
        /// Doubles the health of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> HEALTHY = AddValue(nameof(HEALTHY));
        /// <summary>
        /// Halfs the health of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> SICK = AddValue(nameof(SICK));
        /// <summary>
        /// Doubles the attack of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> STRONG = AddValue(nameof(STRONG));
        /// <summary>
        /// Halfs the attack of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> WEAK = AddValue(nameof(WEAK));
        /// <summary>
        /// Doubles the defence of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> TOUGH = AddValue(nameof(TOUGH));
        /// <summary>
        /// Halfs the defence of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> FRAIL = AddValue(nameof(FRAIL));
        /// <summary>
        /// Doubles the agility of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> AGILE = AddValue(nameof(AGILE));
        /// <summary>
        /// Halfs the agility of the entity.
        /// </summary>
        public static readonly EnumValue<Attribute> SLOW = AddValue(nameof(SLOW));
    }
}
