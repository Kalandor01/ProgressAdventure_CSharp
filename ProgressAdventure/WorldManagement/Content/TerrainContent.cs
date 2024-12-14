using NPrng.Generators;
using PACommon.JsonUtils;

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
        /// <inheritdoc cref="BaseContent(SplittableRandom, ContentTypeID, ContentTypeID, string?, JsonDictionary?)"/>
        protected TerrainContent(SplittableRandom chunkRandom, ContentTypeID subtype, string? name = null, JsonDictionary? data = null)
            : base(chunkRandom, ContentType.TERRAIN, subtype, name, data) { }
        #endregion

        #region Public functions
        /// <inheritdoc cref="BaseContent.FromJson{T}(SplittableRandom, JsonDictionary?, string, out T)"/>
        public static bool FromJson(SplittableRandom chunkRandom, JsonDictionary? contentJson, string fileVersion, out TerrainContent? contentObject)
        {
            return BaseContent.FromJson(chunkRandom, contentJson, fileVersion, out contentObject);
        }
        #endregion
    }
}
