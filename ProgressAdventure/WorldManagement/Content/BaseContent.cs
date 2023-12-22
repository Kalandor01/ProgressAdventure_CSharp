using NPrng.Generators;
using PACommon;
using PACommon.Extensions;
using PACommon.JsonUtils;
using static ProgressAdventure.Constants;
using PACTools = PACommon.Tools;

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
            Name = name ?? GenerateContentName();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns the name of the type of this content.
        /// </summary>
        public string GetTypeName()
        {
            return WorldUtils.contentTypeIDTextMap[type];
        }

        /// <summary>
        /// Returns the name of the subtype of this content.
        /// </summary>
        public string GetSubtypeName()
        {
            return WorldUtils.contentTypeIDSubtypeTextMap[type][subtype];
        }

        /// <summary>
        /// Should be called if a player is on the tile, that is the parrent of this content.
        /// </summary>
        /// <param name="tile">The parrent tile.</param>
        public virtual void Visit(Tile tile)
        {
            PACSingletons.Instance.Logger.Log($"Player visited \"{GetTypeName()}\": \"{GetSubtypeName()}\"{(Name is not null ? $" (\"{Name}\")" : "")}", $"x: {tile.relativePosition.x}, y: {tile.relativePosition.y}, visits: {tile.Visited}");
        }

        /// <summary>
        /// Tries to get a property from the object, based on the property name provided.
        /// </summary>
        /// <param name="propertyName">The name of the property to search for.</param>
        /// <param name="property">The value of the property if it was found.</param>
        public bool TryGetExtraProperty(string propertyName, out object? property)
        {
            return ToJson().TryGetValue(propertyName, out property);
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Generates a content name.
        /// </summary>
        protected virtual string GenerateContentName()
        {
            return GenerateContentName(chunkRandom);
        }
        #endregion

        #region Protected functions
        /// <summary>
        /// Generates a content name.
        /// </summary>
        /// <param name="randomGenrator">The genrator to use.</param>
        public static string GenerateContentName(SplittableRandom randomGenrator)
        {
            return SentenceGenerator.GenerateWordSequence((1, 3), randomGenerator: randomGenrator).Capitalize();
        }

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
        /// <typeparam name="T">The content type to parse from.</typeparam>
        /// <param name="chunkRandom">The parrent chunk's random generator.</param>
        /// <param name="jsonKey">The key to use.</param>
        /// <param name="data">The dictionary to search in.</param>
        /// <param name="defaultRange">The dafult range to use, if the value doesn't exist.</param>
        protected static long GetLongValueFromData<T>(SplittableRandom chunkRandom, string jsonKey, IDictionary<string, object?>? data, (long min, long max)? defaultRange = null)
        {
            if (!(
                data is not null &&
                PACTools.TryParseJsonValue<T, long>(data, jsonKey, out var value)
            ))
            {
                value = GetContentValueRange(chunkRandom, defaultRange);
            }
            return value;
        }
        #endregion

        #region JsonConvert
        #region Protected properties
        protected static List<(Action<IDictionary<string, object?>, SplittableRandom> objectJsonCorrecter, string newFileVersion)> VersionCorrecters { get; } = new()
        {
            // 2.2 -> 2.2.1
            ((oldJson, chunkRandom) =>
            {
                // subtype snake case rename, name not null
                if (
                    oldJson.TryGetValue("subtype", out var oldSubtype) &&
                    oldSubtype is string oldSubtypeString &&
                    oldSubtypeString == "banditCamp"
                )
                {
                    oldJson["subtype"] = "bandit_camp";
                }
                if (
                    oldJson.TryGetValue("name", out var oldName) &&
                    oldName is null &&
                    oldJson.TryGetValue("subtype", out var subtype) &&
                    subtype is string subtypeString
                )
                {
                    oldJson["name"] = GenerateContentName(chunkRandom);
                }
            }, "2.2.1"),
        };
        #endregion

        public virtual Dictionary<string, object?> ToJson()
        {
            return new Dictionary<string, object?>
            {
                [JsonKeys.BaseContent.TYPE] = GetTypeName(),
                [JsonKeys.BaseContent.SUBTYPE] = GetSubtypeName(),
                [JsonKeys.BaseContent.NAME] = Name
            };
        }

        /// <summary>
        /// Tries to load a content object from the content json, and returns, if it was successful without any warnings.
        /// </summary>
        /// <typeparam name="T">The content type to return.</typeparam>
        /// <param name="chunkRandom">The parrent chunk's random generator.</param>
        /// <param name="contentJson">The json representation of the content.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="contentObject">The content object that was loaded.</param>
        /// <returns>If the content was parsed without warnings.</returns>
        protected static bool LoadContent<T>(SplittableRandom chunkRandom, IDictionary<string, object?>? contentJson, string fileVersion, out T? contentObject)
            where T : BaseContent
        {
            contentObject = null;
            if (contentJson is null)
            {
                PACTools.LogJsonNullError<T>(typeof(T).ToString(), isError: true);
                return false;
            }

            PACSingletons.Instance.JsonDataCorrecter.CorrectJsonData<T, SplittableRandom>(ref contentJson, chunkRandom, VersionCorrecters, fileVersion);

            var contentSubtypeTypeMap = WorldUtils.contentTypeMap[typeof(T)];
            var contentTypeID = contentSubtypeTypeMap.First().Key.Super;

            if (!(
                PACTools.TryCastJsonAnyValue<T, string>(contentJson, JsonKeys.BaseContent.SUBTYPE, out var contentSubtypeString, true) &&
                WorldUtils.TryParseContentType(contentTypeID, contentSubtypeString, out ContentTypeID contentSubtype) &&
                contentSubtypeTypeMap.TryGetValue(contentSubtype, out var contentType)
            ))
            {
                if (contentSubtypeString is not null)
                {
                    PACTools.LogJsonError<T>($"unknown content subtype \"{contentSubtypeString}\" for content type \"{typeof(T)}\"", true);
                }
                return false;
            }

            var success = PACTools.TryParseJsonValue<T, string?>(contentJson, JsonKeys.BaseContent.NAME, out var contentName);

            // get content
            var content = Activator.CreateInstance(contentType, new object?[] { chunkRandom, contentName, contentJson });
            if (content is null)
            {
                PACTools.LogJsonError<T>($"couldn't create content object from type \"{contentType}\"!", true);
                return false;
            }

            contentObject = (T)content;
            return success;
        }
        #endregion
    }
}
