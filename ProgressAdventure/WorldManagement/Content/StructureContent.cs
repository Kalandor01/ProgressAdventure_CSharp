using NPrng.Generators;
using PACommon.Enums;
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
        protected StructureContent(SplittableRandom chunkRandom, EnumTreeValue<ContentType> subtype, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, ContentType._STRUCTURE, subtype, name, data) { }
        #endregion

        #region Public functions
        /// <inheritdoc cref="BaseContent.FromJson{T}(SplittableRandom, JsonDictionary?, string, out T)"/>
        public static bool FromJson(SplittableRandom chunkRandom, JsonDictionary? contentJson, string fileVersion, out StructureContent? contentObject)
        {
            return BaseContent.FromJson(chunkRandom, contentJson, fileVersion, out contentObject);
        }
        #endregion
    }
}
