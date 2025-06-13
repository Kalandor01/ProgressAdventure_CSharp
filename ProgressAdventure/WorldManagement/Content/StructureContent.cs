using NPrng.Generators;
using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.Enums;

namespace ProgressAdventure.WorldManagement.Content
{
    /// <summary>
    /// Abstract class for the structure layer, for a tile.
    /// </summary>
    public abstract class StructureContent : BaseContent<StructureType>
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="StructureContent"/>
        /// </summary>
        /// <inheritdoc cref="BaseContent{TType}(SplittableRandom, EnumValue{TType}, string?, JsonDictionary?)"/>
        /// <param name="childType">The type of the class that implements this class.</param>
        protected StructureContent(
            SplittableRandom chunkRandom,
            Type childType,
            string? name = null,
            JsonDictionary? data = null
        )
            :base(chunkRandom, WorldUtils.GetContentTypeFromClassType(WorldUtils.StructureTypeMap, childType), name, data) { }
        #endregion

        #region Public methods
        public override string GetTypeName()
        {
            return WorldUtils.StructureTypeMap[type].displayName;
        }
        #endregion

        #region Public functions
        /// <inheritdoc cref="BaseContent{TType}.FromJson{T}(SplittableRandom, JsonDictionary?, string, out T)"/>
        public static bool FromJson(
            SplittableRandom chunkRandom,
            JsonDictionary? contentJson,
            string fileVersion,
            out StructureContent? contentObject
        )
        {
            return BaseContent<StructureType>.FromJson(chunkRandom, contentJson, fileVersion, out contentObject);
        }
        #endregion
    }
}
