using PACommon.Enums;
using System.Text.Json.Serialization;
using Attribute = ProgressAdventure.Enums.Attribute;

namespace ProgressAdventure.EntityManagement
{
    public class EntityAttributeChance
    {
        [JsonPropertyName("negative_attribute")]
        public readonly EnumValue<Attribute> negativeAttribute;

        [JsonPropertyName("positive_attribute")]
        public readonly EnumValue<Attribute> positiveAttribute;

        [JsonPropertyName("negative_attribute_chance")]
        public readonly double negativeAttributeChance;

        [JsonPropertyName("positive_attribute_chance")]
        public readonly double positiveAttributeChance;

        [JsonConstructor]
        public EntityAttributeChance(
            EnumValue<Attribute> negativeAttribute,
            EnumValue<Attribute> positiveAttribute,
            double negativeAttributeChance,
            double positiveAttributeChance
        )
        {
            this.negativeAttribute = negativeAttribute;
            this.positiveAttribute = positiveAttribute;
            this.negativeAttributeChance = negativeAttributeChance;
            this.positiveAttributeChance = positiveAttributeChance;
        }

        public EntityAttributeChance(
            EnumValue<Attribute> attribute,
            double attributeChance
        )
        {
            negativeAttribute = attribute;
            positiveAttribute = attribute;
            negativeAttributeChance = attributeChance;
            positiveAttributeChance = 0;
        }

        public override string? ToString()
        {
            var negChStr = $"{negativeAttributeChance * 100}%";
            if (negativeAttribute != positiveAttribute)
            {
                return $"{negChStr} {negativeAttribute} <-> {positiveAttributeChance * 100}% {positiveAttribute}";
            }
            return $"{negChStr} {negativeAttribute}";
        }
    }
}
