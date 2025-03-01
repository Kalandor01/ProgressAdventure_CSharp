using System.Numerics;
using System.Text.Json.Serialization;

namespace ProgressAdventure.Entity
{
    public class EntityAttributeValue<T>
        where T : INumber<T>
    {
        [JsonPropertyName("base_value")]
        public T baseValue;

        [JsonPropertyName("negative_fluctuation")]
        public T negativeFluctuation;

        [JsonPropertyName("positive_fluctuation")]
        public T positiveFluctuation;

        [JsonConstructor]
        public EntityAttributeValue(T baseValue, T negativeFluctuation, T positiveFluctuation)
        {
            this.baseValue = baseValue;
            this.negativeFluctuation = negativeFluctuation;
            this.positiveFluctuation = positiveFluctuation;
        }

        public override string? ToString()
        {
            return $"(-{negativeFluctuation} {baseValue} +{positiveFluctuation})";
        }
    }
}
