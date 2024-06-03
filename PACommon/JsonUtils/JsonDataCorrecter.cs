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
        /// The current save version.
        /// </summary>
        private readonly string saveVersion;
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
        /// <param name="saveVersion"><inheritdoc cref="saveVersion" path="//summary"/></param>
        /// <param name="orderCorrecters"><inheritdoc cref="orderCorrecters" path="//summary"/></param>
        /// <exception cref="ArgumentException"></exception>
        private JsonDataCorrecter(string saveVersion, bool orderCorrecters)
        {
            if (string.IsNullOrWhiteSpace(saveVersion))
            {
                throw new ArgumentException($"'{nameof(saveVersion)}' cannot be null or whitespace.", nameof(saveVersion));
            }
            this.saveVersion = saveVersion;
            this.orderCorrecters = orderCorrecters;
        }
        #endregion

        #region "Initializer"
        /// <summary>
        /// Initializes the object's values.
        /// </summary>
        /// <param name="saveVersion"><inheritdoc cref="saveVersion" path="//summary"/></param>
        /// <param name="orderCorrecters"><inheritdoc cref="orderCorrecters" path="//summary"/></param>
        /// <param name="logInitialization">Whether to log the fact that the singleton was initialized.</param>
        public static JsonDataCorrecter Initialize(string saveVersion, bool orderCorrecters = false, bool logInitialization = true)
        {
            _instance = new JsonDataCorrecter(saveVersion, orderCorrecters);
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
            Action<IDictionary<string, object?>> objectJsonCorrecter,
            IDictionary<string, object?> objectJson,
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
            Action<IDictionary<string, object?>, TE> objectJsonCorrecter,
            IDictionary<string, object?> objectJson,
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
            IDictionary<string, object?> objectJson,
            List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        )
        {
            CorrectJsonDataPrivate(objectName, objectJson, false, correcters, fileVersion, orderCorrecters);
        }

        public void CorrectJsonData<T>(
            IDictionary<string, object?> objectJson,
            List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        )
        {
            CorrectJsonData(typeof(T).ToString(), objectJson, correcters, fileVersion, orderCorrecters);
        }

        public void CorrectJsonData<TE>(
            string objectName,
            IDictionary<string, object?> objectJson,
            TE extraData,
            List<(Action<IDictionary<string, object?>, TE> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        )
        {
            CorrectJsonDataPrivate(objectName, objectJson, extraData, correcters, fileVersion, orderCorrecters);
        }

        public void CorrectJsonData<T, TE>(
            IDictionary<string, object?> objectJson,
            TE extraData,
            List<(Action<IDictionary<string, object?>, TE> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        )
        {
            CorrectJsonData(typeof(T).ToString(), objectJson, extraData, correcters, fileVersion, orderCorrecters);
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
            IDictionary<string, object?> objectJson,
            TE? extraData,
            List<(TA objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        )
        {
            if (correcters.Count == 0 || Utils.IsUpToDate(saveVersion, fileVersion))
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
            var isPassInExtraData = lastCorrecter.objectJsonCorrecter is Action<IDictionary<string, object?>, TE>;
            foreach (var (objectJsonCorrecter, newFileVersion) in orderedCorrecters)
            {
                if (isPassInExtraData)
                {
                    CorrectJsonDataVersionPrivate(objectName, () => (objectJsonCorrecter as Action<IDictionary<string, object?>, TE>)(objectJson, extraData), ref fileVersion, newFileVersion);
                }
                else
                {
                    CorrectJsonDataVersionPrivate(objectName, () => (objectJsonCorrecter as Action<IDictionary<string, object?>>)(objectJson), ref fileVersion, newFileVersion);
                }
            }
            PACSingletons.Instance.Logger.Log($"{objectName} json data corrected");
        }
        #endregion
    }
}
