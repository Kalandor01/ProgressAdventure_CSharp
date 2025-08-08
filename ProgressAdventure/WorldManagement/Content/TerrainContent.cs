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
        /// <param name="childType">The type of the class that implements this class.</param>
        protected TerrainContent(
            SplittableRandom chunkRandom,
            Type childType,
            string? name = null,
            JsonDictionary? data = null
        )
            :base(chunkRandom, WorldUtils.GetContentTypeFromClassType(WorldUtils.TerrainTypeMap, childType), name, data) { }
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
