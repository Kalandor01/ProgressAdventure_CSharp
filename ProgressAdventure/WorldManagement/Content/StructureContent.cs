using NPrng.Generators;
using PACommon.JsonUtils;

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
        /// <inheritdoc cref="BaseContent(SplittableRandom, ContentTypeID, ContentTypeID, string?, JsonDictionary?)"/>
        protected StructureContent(SplittableRandom chunkRandom, ContentTypeID subtype, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, ContentType.StructureContentType, subtype, name, data) { }
        #endregion

        #region Public functions
        /// <inheritdoc cref="BaseContent.LoadContent{T}(SplittableRandom, JsonDictionary?, string, out T)"/>
        public static bool FromJson(SplittableRandom chunkRandom, JsonDictionary? contentJson, string fileVersion, out StructureContent? contentObject)
        {
            return LoadContent(chunkRandom, contentJson, fileVersion, out contentObject);
        }
        #endregion
    }
}
