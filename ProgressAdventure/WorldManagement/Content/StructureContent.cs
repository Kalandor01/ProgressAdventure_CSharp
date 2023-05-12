using NPrng.Generators;

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
        /// <inheritdoc cref="BaseContent(SplittableRandom, ContentTypeID, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        protected StructureContent(SplittableRandom chunkRandom, ContentTypeID subtype, string? name = null, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.StructureContentType, subtype, name, data) { }
        #endregion

        #region Public functions
        /// <inheritdoc cref="BaseContent.LoadContent{T}(SplittableRandom, IDictionary{string, object?}?)"/>
        public static StructureContent FromJson(SplittableRandom chunkRandom, IDictionary<string, object?>? contentJson)
        {
            return LoadContent<StructureContent>(chunkRandom, contentJson);
        }
        #endregion
    }
}
