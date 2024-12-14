using System.Text.Json.Serialization;

namespace ProgressAdventure.WorldManagement.Content
{
    public class ContentTypeIDPropertiesDTO
    {
        /// <summary>
        /// The unique name of the content, used in the json representation of the content.<br/>
        /// Usualy "content_category/content_type".
        /// </summary>
        [JsonPropertyName("type_name")]
        public readonly string typeName;
        /// <summary>
        /// The type of the content for this content type.
        /// </summary>
        [JsonPropertyName("matching_type")]
        public readonly Type matchingType;

        [JsonConstructor]
        public ContentTypeIDPropertiesDTO(string typeName, Type matchingType)
        {
            this.typeName = typeName;
            this.matchingType = matchingType;
        }

        public ContentTypeIDPropertiesDTO(ContentTypeID contentType, Type machingType)
            : this(WorldUtils.ContentIDToTypeName(contentType), machingType)
        { }
    }
}
