using PACommon.Enums;

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
                        _instance ??= Initialize("1.0");
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
        /// <exception cref="ArgumentException"></exception>
        private JsonDataCorrecter(string saveVersion)
        {
            if (string.IsNullOrWhiteSpace(saveVersion))
            {
                throw new ArgumentException($"'{nameof(saveVersion)}' cannot be null or whitespace.", nameof(saveVersion));
            }
            this.saveVersion = saveVersion;
        }
        #endregion

        #region "Initializer"
        /// <summary>
        /// Initializes the object's values.
        /// </summary>
        /// <param name="saveVersion"><inheritdoc cref="saveVersion" path="//summary"/></param>
        /// <param name="logInitialization">Whether to log the fact that the singleton was initialized.</param>
        public static JsonDataCorrecter Initialize(string saveVersion, bool logInitialization = true)
        {
            _instance = new JsonDataCorrecter(saveVersion);
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
        public static void CorrectJsonDataVersion(string objectName, Action<IDictionary<string, object?>> objectJsonCorrecter, ref IDictionary<string, object?> objectJson, ref string fileVersion, string newFileVersion)
        {
            if (Utils.IsUpToDate(newFileVersion, fileVersion))
            {
                return;
            }

            objectJsonCorrecter(objectJson);
            PACSingletons.Instance.Logger.Log($"Corrected {objectName} json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
            fileVersion = newFileVersion;
        }

        /// <inheritdoc cref="CorrectJsonDataVersion(string, Action{IDictionary{string, object?}}, ref IDictionary{string, object?}, ref string, string)"/>
        /// <typeparam name="TE">The type of the extra data to input with the correcter.</typeparam>
        /// <param name="extraData">The extra data to input with the correcter.</param>
        public static void CorrectJsonDataVersion<TE>(string objectName, Action<IDictionary<string, object?>, TE> objectJsonCorrecter, ref IDictionary<string, object?> objectJson, TE extraData, ref string fileVersion, string newFileVersion)
        {
            if (Utils.IsUpToDate(newFileVersion, fileVersion))
            {
                return;
            }

            objectJsonCorrecter(objectJson, extraData);
            PACSingletons.Instance.Logger.Log($"Corrected {objectName} json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
            fileVersion = newFileVersion;
        }
        #endregion

        #region Public methods
        public void CorrectJsonData(
            string objectName,
            ref IDictionary<string, object?> objectJson,
            List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion
        )
        {
            if (!correcters.Any() || Utils.IsUpToDate(saveVersion, fileVersion) || Utils.IsUpToDate(correcters.Last().newFileVersion, fileVersion))
            {
                return;
            }

            PACSingletons.Instance.Logger.Log($"{objectName} json data is old", "correcting data");
            foreach (var (objectJsonCorrecter, newFileVersion) in correcters)
            {
                CorrectJsonDataVersion(objectName, objectJsonCorrecter, ref objectJson, ref fileVersion, newFileVersion);
            }
            PACSingletons.Instance.Logger.Log($"{objectName} json data corrected");
        }

        public void CorrectJsonData<T>(
            ref IDictionary<string, object?> objectJson,
            List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion
        )
        {
            CorrectJsonData(typeof(T).ToString(), ref objectJson, correcters, fileVersion);
        }

        public void CorrectJsonData<TE>(string objectName, ref IDictionary<string, object?> objectJson, TE extraData, List<(Action<IDictionary<string, object?>, TE> objectJsonCorrecter, string newFileVersion)> correcters, string fileVersion)
        {
            if (!correcters.Any() || Utils.IsUpToDate(saveVersion, fileVersion) || Utils.IsUpToDate(correcters.Last().newFileVersion, fileVersion))
            {
                return;
            }

            PACSingletons.Instance.Logger.Log($"{objectName} json data is old", "correcting data");
            foreach (var (objectJsonCorrecter, newFileVersion) in correcters)
            {
                CorrectJsonDataVersion(objectName, objectJsonCorrecter, ref objectJson, extraData, ref fileVersion, newFileVersion);
            }
            PACSingletons.Instance.Logger.Log($"{objectName} json data corrected");
        }

        public void CorrectJsonData<T, TE>(ref IDictionary<string, object?> objectJson, TE extraData, List<(Action<IDictionary<string, object?>, TE> objectJsonCorrecter, string newFileVersion)> correcters, string fileVersion)
        {
            CorrectJsonData(typeof(T).ToString(), ref objectJson, extraData, correcters, fileVersion);
        }
        #endregion
    }
}
