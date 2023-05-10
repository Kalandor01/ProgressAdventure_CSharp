namespace ProgressAdventure.WorldManagement.Content
{
    /// <summary>
    /// Abstract class for the structure content layer, for a tile.
    /// </summary>
    public abstract class StructureContent : BaseContent
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="StructureContent"/>
        /// </summary>
        /// <inheritdoc cref="BaseContent(ContentTypeID, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        protected StructureContent(ContentTypeID subtype, string? name = null, IDictionary<string, object?>? data = null)
            : base(ContentType.StructureContentType, subtype, name, data) { }
        #endregion

        #region Public functions
        /// <inheritdoc cref="BaseContent.LoadContent{T}(IDictionary{string, object?}?)"/>
        public static StructureContent FromJson(IDictionary<string, object?>? contentJson)
        {
            return LoadContent<StructureContent>(contentJson);
        }
        #endregion
    }
}
