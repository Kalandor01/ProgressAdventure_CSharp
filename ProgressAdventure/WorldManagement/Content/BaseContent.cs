using NPrng.Generators;
using ProgressAdventure.Enums;

namespace ProgressAdventure.WorldManagement.Content
{
    /// <summary>
    /// Abstract class for a layer of content, for a tile.
    /// </summary>
    public abstract class BaseContent : IJsonReadable
    {
        #region Public fields
        /// <summary>
        /// The type of the content, specifying the layer in the tile.
        /// </summary>
        public readonly ContentTypeID type;
        /// <summary>
        /// The subtype of the content, specifying the actual type of the of the content.
        /// </summary>
        public readonly ContentTypeID subtype;
        /// <summary>
        /// The name of the content layer.
        /// </summary>
        public string? Name { get; private set; }
        #endregion

        #region Protected fields
        /// <summary>
        /// A referance to the chunk's random generator, that this content is in.
        /// </summary>
        protected readonly SplittableRandom chunkRandom;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="BaseContent"/>
        /// </summary>
        /// <param name="chunkRandom">The parrent chunk's random generator.</param>
        /// <param name="type"><inheritdoc cref="type" path="//summary"/></param>
        /// <param name="subtype"><inheritdoc cref="subtype" path="//summary"/></param>
        /// <param name="name"><inheritdoc cref="Name" path="//summary"/></param>
        /// <param name="data">The extra data for this content. Specific to each content subtype.</param>
        /// <exception cref="ArgumentException">Thrown, if the type is not a base type, and the subtype is not the child of that type.</exception>
        protected BaseContent(SplittableRandom chunkRandom, ContentTypeID type, ContentTypeID subtype, string? name = null, IDictionary<string, object?>? data = null)
        {
            if (
                subtype.Super != type ||
                type.Super != ContentType.BaseContentType ||
                subtype.Super == ContentType.BaseContentType
            )
            {
                throw new ArgumentException("Content types are missmached.");
            }

            this.chunkRandom = chunkRandom;
            this.type = type;
            this.subtype = subtype;
            Name = name;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Should be called if a player is on the tile, that is the parrent of this content.
        /// </summary>
        /// <param name="tile">The parrent tile.</param>
        public virtual void Visit(Tile tile)
        {
            Logger.Log($"Player visited \"{WorldUtils.contentTypeIDTextMap[type]}\": \"{WorldUtils.contentTypeIDSubtypeTextMap[type][subtype]}\"{(Name is not null ? $" ({Name})" : "")}", $"x: {tile.relativePosition.x}, y: {tile.relativePosition.y}, visits: {tile.Visited}");
        }
        #endregion

        #region Protected methods
        protected virtual void GenContentName()
        {
            Name ??= subtype.ToString() + " " + RandomStates.MiscRandom.GenerateInRange(0, 100000).ToString();
        }
        #endregion

        #region Protected functions
        /// <summary>
        /// Returns a value, from a range, using the world random.
        /// </summary>
        /// <param name="chunkRandom">The parrent chunk's random generator.</param>
        /// <param name="valueRange">The range, the value can have.</param>
        protected static long GetContentValueRange(SplittableRandom chunkRandom, (long min, long max)? valueRange = null)
        {
            if (valueRange is not null)
            {
                if (valueRange.Value.min != valueRange.Value.max)
                {
                    return chunkRandom.GenerateInRange(valueRange.Value.min, valueRange.Value.max);
                }
                else
                {
                    return valueRange.Value.min;
                }
            }
            else
            {
                return chunkRandom.GenerateInRange(1, 1000);
            }
        }

        /// <summary>
        /// Rturns the value of the long, at the given key, or makes a new value.
        /// </summary>
        /// <param name="chunkRandom">The parrent chunk's random generator.</param>
        /// <param name="key">The key to use.</param>
        /// <param name="data">The dictionary to search in.</param>
        /// <param name="defaultRange">The dafult range to use, if the value doesn't exist.</param>
        protected static long GetLongValueFromData(SplittableRandom chunkRandom, string key, IDictionary<string, object?>? data, (long min, long max)? defaultRange = null)
        {
            if (
                data is not null &&
                data.TryGetValue(key, out object? valueRaw) &&
                long.TryParse(valueRaw?.ToString(), out long value)
            ) { }
            else
            {
                value = GetContentValueRange(chunkRandom, defaultRange);
            }
            return value;
        }
        #endregion

        #region JsonConvert
        public virtual Dictionary<string, object?> ToJson()
        {
            return new Dictionary<string, object?>
            {
                ["type"] = WorldUtils.contentTypeIDTextMap[type],
                ["subtype"] = WorldUtils.contentTypeIDSubtypeTextMap[type][subtype],
                ["name"] = Name
            };
        }

        /// <summary>
        /// Load a content object from the content json.
        /// </summary>
        /// <typeparam name="T">The content type to return.</typeparam>
        /// <param name="chunkRandom">The parrent chunk's random generator.</param>
        /// <param name="contentJson">The json representation of the content.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <exception cref="ArgumentException">Thrown if the content type cannot be created.</exception>
        protected static T LoadContent<T>(SplittableRandom chunkRandom, IDictionary<string, object?>? contentJson, string fileVersion)
            where T : BaseContent
        {
            var contentTypeMap = WorldUtils.contentTypeMap[typeof(T)];
            var contentTypeID = contentTypeMap.First().Key.Super;
            Type contentType;
            string? contentName = null;
            if (contentJson is not null)
            {
                // get content subtype
                if (
                    contentJson.TryGetValue("subtype", out object? contentSubtypeString) &&
                    WorldUtils.TryParseContentType(contentTypeID, contentSubtypeString?.ToString(), out ContentTypeID contentSubtype) &&
                    contentTypeMap.ContainsKey(contentSubtype)
                )
                {
                    contentType = contentTypeMap[contentSubtype];
                }
                else
                {
                    Logger.Log("Content parse error", "couldn't get content type from json", LogSeverity.ERROR);
                    contentType = contentTypeMap.First().Value;
                }
                // get content name
                if (
                    contentJson.TryGetValue("name", out object? contentNameValue)
                )
                {
                    contentName = contentNameValue?.ToString();
                }
                else
                {
                    Logger.Log("Content parse error", "couldn't get content name from json", LogSeverity.WARN);
                }
            }
            else
            {
                Logger.Log("Content parse error", "content json was null", LogSeverity.ERROR);
                contentType = contentTypeMap.First().Value;
            }
            // get content
            var content = Activator.CreateInstance(contentType, new object?[] { chunkRandom, contentName, contentJson }) ?? throw new ArgumentNullException(message: "Couldn't create content object from type!", null);
            return (T)content;
        }
        #endregion
    }
}
