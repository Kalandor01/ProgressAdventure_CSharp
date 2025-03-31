using NPrng.Generators;
using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.ConfigManagement;
using System.Diagnostics.CodeAnalysis;
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
        public readonly EnumTreeValue<ContentType> type;
        /// <summary>
        /// The subtype of the content, specifying the actual type of the of the content.
        /// </summary>
        public readonly EnumTreeValue<ContentType> subtype;
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
        protected BaseContent(
            SplittableRandom chunkRandom,
            EnumTreeValue<ContentType> type,
            EnumTreeValue<ContentType> subtype,
            string? name = null,
            JsonDictionary? data = null
        )
        {
            if (
                type.Indexes.Count != 1 ||
                subtype.Indexes.Count == 0 ||
                type.Indexes.Count + 1 != subtype.Indexes.Count ||
                !ContentType.TryGetChildValue(type, subtype.Name, out var _) ||
                !ContentType.TryGetChildValue(null, type.Name, out var _) ||
                ContentType.TryGetChildValue(null, subtype.Name, out var _)
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
            return WorldUtils.BaseContentTypeMap[type].typeName;
        }

        /// <summary>
        /// Returns the name of the subtype of this content.
        /// </summary>
        public string GetSubtypeName()
        {
            return WorldUtils.contentTypeSubtypesMap[type][subtype].typeName;
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
        public bool TryGetExtraProperty(string propertyName, out JsonObject? property)
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
            return SentenceGenerator.GenerateNameSequence((1, 3), randomGenerator: randomGenrator);
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
        protected static long GetLongValueFromData<T>(SplittableRandom chunkRandom, string jsonKey, JsonDictionary? data, (long min, long max)? defaultRange = null)
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
        protected static List<(Action<JsonDictionary, SplittableRandom> objectJsonCorrecter, string newFileVersion)> VersionCorrecters { get; } =
        [
            // 2.2 -> 2.2.1
            ((oldJson, chunkRandom) =>
            {
                // subtype snake case rename, name not null
                JsonDataCorrecterUtils.TransformValue<BaseContent, string>(oldJson, "subtype", (subtype) =>
                {
                    return (subtype == "banditCamp", "bandit_camp");
                });
                if (
                    oldJson.TryGetValue("name", out var oldName) &&
                    oldName is null &&
                    oldJson.TryGetValue("subtype", out var subtype) &&
                    subtype?.Value.ToString() != "none"
                )
                {
                    oldJson["name"] = GenerateContentName(chunkRandom);
                }
            }, "2.2.1"),
            // 2.2.1 -> 2.2.2
            ((oldJson, chunkRandom) =>
            {
                // no more "content" content type, content type IDs are like item type IDs
                JsonDataCorrecterUtils.TransformValue<BaseContent, string>(oldJson, "type", (oldTypeValue) =>
                {
                    var isStructure = false;
                    if (oldTypeValue == "content")
                    {
                        isStructure = true;
                        oldTypeValue = "structure";
                    }

                    JsonDataCorrecterUtils.TransformValue<BaseContent, string>(oldJson, "subtype", (oldSubtypeValue) =>
                    {
                        return (
                            WorldUtils._legacyContentSubtypeNameMap.TryGetValue((oldTypeValue, oldSubtypeValue), out var newSubtype),
                            newSubtype
                        );
                    });

                    return (isStructure, oldTypeValue);
                });
            }, "2.2.2"),
            // 2.3 -> 2.4
            ((oldJson, chunkRandom) =>
            {
                // namespaced type/subtype
                if (
                    PACTools.TryParseJsonValue<BaseContent, string>(oldJson, "type", out var typeValue, false) &&
                    !string.IsNullOrWhiteSpace(typeValue) &&
                    PACTools.TryParseJsonValue<BaseContent, string>(oldJson, "subtype", out var subtypeValue, false) &&
                    !string.IsNullOrWhiteSpace(subtypeValue)
                )
                {
                    JsonDataCorrecterUtils.SetMultipleValues(oldJson, new Dictionary<string, JsonObject?>
                    {
                        ["type"] = ConfigUtils.GetSpecificNamespacedString(typeValue),
                        ["subtype"] = ConfigUtils.GetSpecificNamespacedString(subtypeValue),
                    });
                }
            }, "2.4"),
        ];
        #endregion

        public virtual JsonDictionary ToJson()
        {
            return new JsonDictionary
            {
                [JsonKeys.BaseContent.TYPE] = GetTypeName(),
                [JsonKeys.BaseContent.SUBTYPE] = GetSubtypeName(),
                [JsonKeys.BaseContent.NAME] = Name,
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
        protected static bool FromJson<T>(SplittableRandom chunkRandom, JsonDictionary? contentJson, string fileVersion, [NotNullWhen(true)] out T? contentObject)
            where T : BaseContent
        {
            contentObject = default;
            if (contentJson is null)
            {
                PACTools.LogJsonNullError<T>(typeof(T).ToString(), isError: true);
                return false;
            }

            PACSingletons.Instance.JsonDataCorrecter.CorrectJsonData<T, SplittableRandom>(contentJson, chunkRandom, VersionCorrecters, fileVersion);

            return FromJsonWithoutCorrection(chunkRandom, contentJson, fileVersion, ref contentObject);
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
        private static bool FromJsonWithoutCorrection<T>(SplittableRandom chunkRandom, JsonDictionary contentJson, string fileVersion, ref T? contentObject)
        {
            var contentType = WorldUtils.BaseContentTypeMap.First(props => props.Value.matchingType == typeof(T)).Key;

            if (!(
                PACTools.TryCastJsonAnyValue<T, string>(contentJson, JsonKeys.BaseContent.SUBTYPE, out var contentSubtypeString, true) &&
                WorldUtils.TryParseContentType(contentType, contentSubtypeString, out var contentProperties)
            ))
            {
                if (contentSubtypeString is not null)
                {
                    PACTools.LogJsonError<T>($"unknown content subtype \"{contentSubtypeString}\" for content type \"{typeof(T)}\"", true);
                }
                return false;
            }

            var success = PACTools.TryParseJsonValueNullable<T, string?>(contentJson, JsonKeys.BaseContent.NAME, out var contentName);

            // get content
            var content = Activator.CreateInstance(contentProperties.matchingType, [chunkRandom, contentName, contentJson]);
            if (content is null)
            {
                PACTools.LogJsonError<T>($"couldn't create content object from type \"{contentProperties}\"!", true);
                return false;
            }

            contentObject = (T)content;
            return success;
        }
        #endregion
    }
}
