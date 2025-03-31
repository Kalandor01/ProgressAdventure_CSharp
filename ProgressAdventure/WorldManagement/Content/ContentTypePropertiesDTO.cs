using PACommon.Enums;
using System.Text.Json.Serialization;

namespace ProgressAdventure.WorldManagement.Content
{
    public class ContentTypePropertiesDTO
    {
        /// <summary>
        /// The unique name of the content, used in the json representation of the content.<br/>
        /// Usualy "namespace:content_category/content_type".
        /// </summary>
        [JsonPropertyName("type_name")]
        public readonly string typeName;
        /// <summary>
        /// The type of the content for this content type.
        /// </summary>
        [JsonPropertyName("matching_type")]
        public readonly Type? matchingType;

        [JsonConstructor]
        public ContentTypePropertiesDTO(string typeName, Type? matchingType)
        {
            this.typeName = typeName;
            this.matchingType = matchingType;
        }

        public ContentTypePropertiesDTO(EnumTreeValue<ContentType> contentType, Type? machingType)
            : this(contentType.FullName, machingType)
        { }
    }
}
