using PACommon.Enums;
using PACommon.Extensions;

namespace PACommon.JsonUtils
{
    /// <summary>
    /// Object containing utils to convert json data for <c>IJsonConvertable</c> objects to different versions.
    /// </summary>
    public class JsonDataCorrecter : IJsonDataCorrecter
    {
        #region Private fields
        /// <summary>
        /// Object used for locking the thread while the singleton gets created.
        /// </summary>
        private static readonly object _threadLock = new();
        /// <summary>
        /// The singleton istance.
        /// </summary>
        private static JsonDataCorrecter? _instance = null;

        /// <summary>
        /// The default current save version.
        /// </summary>
        private readonly string defaultSaveVersion;
        /// <summary>
        /// The dictionary containig the object type names that don't use the default current save version paired with their current save version.
        /// </summary>
        private readonly Dictionary<string, string> saveVersionExceptions;
        /// <summary>
        /// Whether to order the json correcters by version number, before correcting, if the value passed into a correction method is null.
        /// </summary>
        private readonly bool orderCorrecters;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_instance" path="//summary"/>
        /// </summary>
        public static JsonDataCorrecter Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_threadLock)
                    {
                        _instance ??= Initialize("1.0", false);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Private constructors
        /// <summary>
        /// <inheritdoc cref="JsonDataCorrecter" path="//summary"/>
        /// </summary>
        /// <param name="defaultSaveVersion"><inheritdoc cref="defaultSaveVersion" path="//summary"/></param>
        /// <param name="orderCorrecters"><inheritdoc cref="orderCorrecters" path="//summary"/></param>
        /// <param name="specialObjectsBySaveVersion">The dictionary pairing special save versions with a list of object type names that use that save version as their current save version.</param>
        /// <exception cref="ArgumentException"></exception>
        private JsonDataCorrecter(
            string defaultSaveVersion,
            bool orderCorrecters,
            IDictionary<string, IList<Type>>? specialObjectsBySaveVersion
        )
        {
            if (string.IsNullOrWhiteSpace(defaultSaveVersion))
            {
                throw new ArgumentException($"'{nameof(defaultSaveVersion)}' cannot be null or whitespace.", nameof(defaultSaveVersion));
            }
            this.defaultSaveVersion = defaultSaveVersion;
            this.orderCorrecters = orderCorrecters;
            saveVersionExceptions = specialObjectsBySaveVersion is null
                ? []
                : specialObjectsBySaveVersion.SelectMany(
                    versionEx => versionEx.Value.Select(type => (type, versionEx.Key))
                )
                .ToDictionary(item => item.type.FullName!, item => item.Key);
        }
        #endregion

        #region "Initializer"
        /// <summary>
        /// Initializes the object's values.
        /// </summary>
        /// <param name="defaultSaveVersion"><inheritdoc cref="defaultSaveVersion" path="//summary"/></param>
        /// <param name="orderCorrecters"><inheritdoc cref="orderCorrecters" path="//summary"/></param>
        /// <param name="specialObjectsBySaveVersion">The dictionary pairing special save versions with a list of object type names that use that save version as their current save version.</param>
        public static JsonDataCorrecter Initialize(
            string defaultSaveVersion,
            bool orderCorrecters = false,
            IDictionary<string, IList<Type>>? specialObjectsBySaveVersion = null,
            bool logInitialization = true
        )
        {
            _instance = new JsonDataCorrecter(defaultSaveVersion, orderCorrecters, specialObjectsBySaveVersion);
            if (logInitialization)
            {
                PACSingletons.Instance.Logger.Log($"{nameof(JsonDataCorrecter)} initialized");
            }
            return _instance;
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Corrects the json data to a specific version.
        /// </summary>
        /// <param name="objectName">The name of the object to convert.</param>
        /// <param name="objectJsonCorrecter">The correcter function.</param>
        /// <param name="objectJson">The json data to correct.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="newFileVersion">The version number, this function will correct the json data to.</param>
        public static void CorrectJsonDataVersion(
            string objectName,
            Action<JsonDictionary> objectJsonCorrecter,
            JsonDictionary objectJson,
            ref string fileVersion,
            string newFileVersion
        )
        {
            CorrectJsonDataVersionPrivate(objectName, () => objectJsonCorrecter(objectJson), ref fileVersion, newFileVersion);
        }

        /// <inheritdoc cref="CorrectJsonDataVersion(string, Action{IDictionary{string, object?}}, ref IDictionary{string, object?}, ref string, string)"/>
        /// <typeparam name="TE">The type of the extra data to input with the correcter.</typeparam>
        /// <param name="extraData">The extra data to input with the correcter.</param>
        public static void CorrectJsonDataVersion<TE>(
            string objectName,
            Action<JsonDictionary, TE> objectJsonCorrecter,
            JsonDictionary objectJson,
            TE extraData,
            ref string fileVersion,
            string newFileVersion
        )
        {
            CorrectJsonDataVersionPrivate(objectName, () => objectJsonCorrecter(objectJson, extraData), ref fileVersion, newFileVersion);
        }
        #endregion

        #region Public methods
        public void CorrectJsonData(
            string objectName,
            JsonDictionary objectJson,
            List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        )
        {
            CorrectJsonDataPrivate(objectName, objectJson, false, correcters, fileVersion, orderCorrecters);
        }

        public void CorrectJsonData<T>(
            JsonDictionary objectJson,
            List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        )
        {
            CorrectJsonData(typeof(T).FullName ?? typeof(T).Name, objectJson, correcters, fileVersion, orderCorrecters);
        }

        public void CorrectJsonData<TE>(
            string objectName,
            JsonDictionary objectJson,
            TE extraData,
            List<(Action<JsonDictionary, TE> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        )
        {
            CorrectJsonDataPrivate(objectName, objectJson, extraData, correcters, fileVersion, orderCorrecters);
        }

        public void CorrectJsonData<T, TE>(
            JsonDictionary objectJson,
            TE extraData,
            List<(Action<JsonDictionary, TE> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        )
        {
            CorrectJsonData(typeof(T).FullName ?? typeof(T).Name, objectJson, extraData, correcters, fileVersion, orderCorrecters);
        }
        #endregion

        #region Private Functions
        private static void CorrectJsonDataVersionPrivate(
            string objectName,
            Action objectJsonCorrecterWrapped,
            ref string fileVersion,
            string newFileVersion
        )
        {
            if (Utils.IsUpToDate(newFileVersion, fileVersion))
            {
                return;
            }

            objectJsonCorrecterWrapped();
            PACSingletons.Instance.Logger.Log($"Corrected {objectName} json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
            fileVersion = newFileVersion;
        }

        private void CorrectJsonDataPrivate<TA, TE>(
            string objectName,
            JsonDictionary objectJson,
            TE? extraData,
            List<(TA objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        )
        {
            var currentSaveVersion = saveVersionExceptions.TryGetValue(objectName, out var alternateSaveVersion)
                ? alternateSaveVersion
                : defaultSaveVersion;
            if (correcters.Count == 0 || Utils.IsUpToDate(currentSaveVersion, fileVersion))
            {
                return;
            }

            var orderedCorrecters = orderCorrecters ?? this.orderCorrecters
                ? correcters.StableSort(
                    (correcter1, correcter2) =>
                        correcter1.newFileVersion == correcter2.newFileVersion ? 0 :
                            (Utils.IsUpToDate(correcter1.newFileVersion, correcter2.newFileVersion) ? -1 : 1)
                    )
                : correcters;

            var lastCorrecter = orderedCorrecters.Last();
            if (Utils.IsUpToDate(orderedCorrecters.Last().newFileVersion, fileVersion))
            {
                return;
            }

            PACSingletons.Instance.Logger.Log($"{objectName} json data is old", "correcting data");
            var isPassInExtraData = lastCorrecter.objectJsonCorrecter is Action<JsonDictionary, TE>;
            foreach (var (objectJsonCorrecter, newFileVersion) in orderedCorrecters)
            {
                if (isPassInExtraData)
                {
                    CorrectJsonDataVersionPrivate(objectName, () => (objectJsonCorrecter as Action<JsonDictionary, TE>)(objectJson, extraData), ref fileVersion, newFileVersion);
                }
                else
                {
                    CorrectJsonDataVersionPrivate(objectName, () => (objectJsonCorrecter as Action<JsonDictionary>)(objectJson), ref fileVersion, newFileVersion);
                }
            }
            PACSingletons.Instance.Logger.Log($"{objectName} json data corrected");
        }
        #endregion
    }
}
