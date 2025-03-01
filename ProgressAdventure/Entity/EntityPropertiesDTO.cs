using ProgressAdventure.ItemManagement;
using System.Text.Json.Serialization;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// Properties for creating an entity type.
    /// </summary>
    public class EntityPropertiesDTO
    {
        [JsonPropertyName("display_name")]
        public readonly string displayName;

        [JsonPropertyName("max_hp")]
        public readonly EntityAttributeValue<int> maxHp;

        [JsonPropertyName("attack")]
        public readonly EntityAttributeValue<int> attack;

        [JsonPropertyName("defence")]
        public readonly EntityAttributeValue<int> defence;

        [JsonPropertyName("agility")]
        public readonly EntityAttributeValue<int> agility;

        [JsonPropertyName("attribute_chances")]
        public readonly List<EntityAttributeChance> attributeChances;

        [JsonPropertyName("original_team")]
        public readonly int originalTeam;

        [JsonPropertyName("team_change_change")]
        public readonly double teamChangeChange;

        [JsonPropertyName("loot")]
        public readonly List<LootFactory> loot;

        [JsonPropertyName("updates_world_when_moving")]
        public readonly bool updatesWorldWhenMoving;

        /// <summary>
        /// <inheritdoc cref="EntityPropertiesDTO" path="//summary"/>
        /// </summary>
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="maxHp"><inheritdoc cref="maxHp" path="//summary"/></param>
        /// <param name="attack"><inheritdoc cref="attack" path="//summary"/></param>
        /// <param name="defence"><inheritdoc cref="defence" path="//summary"/></param>
        /// <param name="agility"><inheritdoc cref="agility" path="//summary"/></param>
        /// <param name="attributeChances"><inheritdoc cref="attributeChances" path="//summary"/></param>
        /// <param name="originalTeam"><inheritdoc cref="originalTeam" path="//summary"/></param>
        /// <param name="teamChangeChange"><inheritdoc cref="teamChangeChange" path="//summary"/></param>
        /// <param name="loot"><inheritdoc cref="loot" path="//summary"/></param>
        /// <param name="updatesWorldWhenMoving"><inheritdoc cref="updatesWorldWhenMoving" path="//summary"/></param>
        [JsonConstructor]
        public EntityPropertiesDTO(
            string displayName,
            EntityAttributeValue<int> maxHp,
            EntityAttributeValue<int> attack,
            EntityAttributeValue<int> defence,
            EntityAttributeValue<int> agility,
            List<EntityAttributeChance>? attributeChances = null,
            int originalTeam = 1,
            double teamChangeChange = 0.005,
            List<LootFactory>? loot = null,
            bool updatesWorldWhenMoving = false
        )
        {
            this.displayName = displayName;
            this.maxHp = maxHp;
            this.attack = attack;
            this.defence = defence;
            this.agility = agility;
            this.attributeChances = attributeChances ?? EntityUtils.GetDefaultAttributeChances();
            this.originalTeam = originalTeam;
            this.teamChangeChange = teamChangeChange;
            this.loot = loot ?? [];
            this.updatesWorldWhenMoving = updatesWorldWhenMoving;
        }

        /// <summary>
        /// <inheritdoc cref="EntityPropertiesDTO" path="//summary"/><br/>
        /// </summary>
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="baseMaxHp"><inheritdoc cref="maxHp" path="//summary"/></param>
        /// <param name="baseAttack"><inheritdoc cref="attack" path="//summary"/></param>
        /// <param name="baseDefence"><inheritdoc cref="defence" path="//summary"/></param>
        /// <param name="baseAgility"><inheritdoc cref="agility" path="//summary"/></param>
        /// <param name="negativeFluctuation"></param>
        /// <param name="positiveFluctuation"></param>
        /// <param name="attributeChances"><inheritdoc cref="attributeChances" path="//summary"/></param>
        /// <param name="originalTeam"><inheritdoc cref="originalTeam" path="//summary"/></param>
        /// <param name="teamChangeChange"><inheritdoc cref="teamChangeChange" path="//summary"/></param>
        /// <param name="loot"><inheritdoc cref="loot" path="//summary"/></param>
        /// <param name="updatesWorldWhenMoving"><inheritdoc cref="updatesWorldWhenMoving" path="//summary"/></param>
        public EntityPropertiesDTO(
            string displayName,
            int baseMaxHp,
            int baseAttack,
            int baseDefence,
            int baseAgility,
            int negativeFluctuation = 2,
            int positiveFluctuation = 3,
            List<EntityAttributeChance>? attributeChances = null,
            int originalTeam = 1,
            double teamChangeChange = 0.005,
            List<LootFactory>? loot = null,
            bool updatesWorldWhenMoving = false
        )
            : this(
                displayName,
                new(baseMaxHp, negativeFluctuation, positiveFluctuation),
                new(baseAttack, negativeFluctuation, positiveFluctuation),
                new(baseDefence, negativeFluctuation, positiveFluctuation),
                new(baseAgility, negativeFluctuation, positiveFluctuation),
                attributeChances,
                originalTeam,
                teamChangeChange,
                loot,
                updatesWorldWhenMoving
            )
        {

        }

        public override string? ToString()
        {
            return displayName;
        }
    }
}
