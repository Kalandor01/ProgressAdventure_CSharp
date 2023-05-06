namespace ProgressAdventure.WorldManagement.Content
{
    /// <summary>
    /// Abstract class for the terrain content layer, for a tile.
    /// </summary>
    public abstract class TerrainContent : BaseContent
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="TerrainContent"/>
        /// </summary>
        /// <inheritdoc cref="BaseContent(ContentTypeID, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        protected TerrainContent(ContentTypeID subtype, string? name, IDictionary<string, object?>? data = null)
            : base(ContentType.TerrainContentType, subtype, name, data) { }
        #endregion
    }
}
