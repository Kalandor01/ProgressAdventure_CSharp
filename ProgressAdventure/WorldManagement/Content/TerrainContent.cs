using NPrng.Generators;
using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.Enums;

namespace ProgressAdventure.WorldManagement.Content
{
    /// <summary>
    /// Abstract class for the terrain layer, for a tile.
    /// </summary>
    public abstract class TerrainContent : BaseContent<TerrainType>
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="TerrainContent"/>
        /// </summary>
        /// <inheritdoc cref="BaseContent{TType}(SplittableRandom, EnumValue{TType}, string?, JsonDictionary?)"/>
        protected TerrainContent(
            SplittableRandom chunkRandom,
            EnumValue<TerrainType> type,
            string? name = null,
            JsonDictionary? data = null
        )
            :base(chunkRandom, type, name, data) { }
        #endregion

        #region Public methods
        public override string GetTypeName()
        {
            return WorldUtils.TerrainTypeMap[type].displayName;
        }
        #endregion

        #region Public functions
        /// <inheritdoc cref="BaseContent{TType}.FromJson{T}(SplittableRandom, JsonDictionary?, string, out T)"/>
        public static bool FromJson(
            SplittableRandom chunkRandom,
            JsonDictionary? contentJson,
            string fileVersion,
            out TerrainContent? contentObject
        )
        {
            return BaseContent<TerrainType>.FromJson(chunkRandom, contentJson, fileVersion, out contentObject);
        }
        #endregion
    }
}
