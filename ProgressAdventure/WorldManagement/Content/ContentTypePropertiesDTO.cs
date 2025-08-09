using System.Text.Json.Serialization;
using PACommon.Enums;
using PACommon.Extensions;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;

namespace ProgressAdventure.WorldManagement.Content
{
    /// <summary>
    /// NEEDS TO BE REWORKED SOON!!!<br/>
    /// Class containing content properties.
    /// </summary>
    public abstract class ContentTypePropertiesDTO
    {
        /// <summary>
        /// The display name of the content.
        /// </summary>
        [JsonPropertyName("display_name")]
        public readonly string displayName;
        /// <summary>
        /// The type of the content for this content type.
        /// </summary>
        [JsonPropertyName("matching_type")]
        public readonly Type matchingType;
        /// <summary>
        /// The additive modifier for the value, where if the next layer's content type difference is bigger than it, then an empty content type will be generated.
        /// </summary>
        [JsonPropertyName("no_next_layer_content_modifier")]
        public readonly double noNextLayerContentModifier;

        public ContentTypePropertiesDTO(string displayName, Type matchingType, double noNextLayerContentModifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
            this.displayName = displayName;
            this.matchingType = matchingType ?? throw new ArgumentNullException(nameof(matchingType));
            this.noNextLayerContentModifier = noNextLayerContentModifier;
        }

        protected ContentTypePropertiesDTO(EnumValueBase contentType, Type matchingType, double noNextLayerContentModifier)
            :this(
                 displayName: ConfigUtils.RemoveNamespace(contentType.Name).Replace('_', ' ').Capitalize(),
                 matchingType,
                 noNextLayerContentModifier
            )
        { }
    }

    public class TerrainTypePropertiesDTO : ContentTypePropertiesDTO
    {
        [JsonConstructor]
        public TerrainTypePropertiesDTO(string displayName, Type matchingType, double noNextLayerContentModifier)
            :base(displayName, matchingType, noNextLayerContentModifier)
        { }

        public TerrainTypePropertiesDTO(EnumValue<TerrainType> terrainType, Type matchingType, double noNextLayerContentModifier = 0)
            : base(contentType: terrainType, matchingType, noNextLayerContentModifier)
        { }
    }

    public class StructureTypePropertiesDTO : ContentTypePropertiesDTO
    {
        /// <summary>
        /// The percent chance a fight starting when the player visits this structure.
        /// </summary>
        [JsonPropertyName("fight_chance")]
        public readonly double fightChance;

        [JsonConstructor]
        public StructureTypePropertiesDTO(
            string displayName,
            Type matchingType,
            double noNextLayerContentModifier,
            double fightChance
        )
            : base(displayName, matchingType, noNextLayerContentModifier)
        {
            this.fightChance = fightChance;
        }

        public StructureTypePropertiesDTO(
            EnumValue<StructureType> structureType,
            Type matchingType,
            double noNextLayerContentModifier = 0,
            double fightChance = 0
        )
            : base(contentType: structureType, matchingType, noNextLayerContentModifier)
        {
            this.fightChance = fightChance;
        }
    }
}
