using NPrng.Generators;

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
        /// <inheritdoc cref="BaseContent(SplittableRandom, ContentTypeID, ContentTypeID, string?, IDictionary{string, object?}?)"/>
        protected TerrainContent(SplittableRandom chunkRandom, ContentTypeID subtype, string? name = null, IDictionary<string, object?>? data = null)
            : base(chunkRandom, ContentType.TerrainContentType, subtype, name, data) { }
        #endregion

        #region Public functions
        /// <inheritdoc cref="BaseContent.LoadContent{T}(SplittableRandom, IDictionary{string, object?}?, string)"/>
        public static TerrainContent FromJson(SplittableRandom chunkRandom, IDictionary<string, object?>? contentJson, string fileVersion)
        {
            return LoadContent<TerrainContent>(chunkRandom, contentJson, fileVersion);
        }
        #endregion
    }
}
