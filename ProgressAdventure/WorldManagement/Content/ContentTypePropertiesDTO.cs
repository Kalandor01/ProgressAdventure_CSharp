using PACommon.Enums;
using PACommon.Extensions;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using System.Text.Json.Serialization;

namespace ProgressAdventure.WorldManagement.Content
{
    public class ContentTypePropertiesDTO
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

        [JsonConstructor]
        public ContentTypePropertiesDTO(string displayName, Type matchingType)
        {
            this.displayName = displayName;
            this.matchingType = matchingType;
        }

        private ContentTypePropertiesDTO(EnumValueBase contentType, Type matchingType)
            :this(displayName: ConfigUtils.RemoveNamespace(contentType.Name).Replace('_', ' ').Capitalize(), matchingType)
        { }

        public ContentTypePropertiesDTO(EnumValue<TerrainType> terrainType, Type machingType)
            :this(contentType: terrainType, machingType)
        { }

        public ContentTypePropertiesDTO(EnumValue<StructureType> structureType, Type machingType)
            :this(contentType: structureType, machingType)
        { }
    }
}
