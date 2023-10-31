using PACommon.Enums;

namespace PACommon.JsonUtils
{
    /// <summary>
    /// Object containing utils to convert json data for <c>IJsonConvertable</c> objects to different versions.
    /// </summary>
    public class JsonDataCorrecter
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
        /// <param name="logsFolderParrentPath"><inheritdoc cref="logsFolderParrentPath" path="//summary"/></param>
        public static JsonDataCorrecter Initialize(string saveVersion)
        {
            _instance = new JsonDataCorrecter(saveVersion);
            Logger.Instance.Log($"{nameof(JsonDataCorrecter)} initialized");
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
            if (!Utils.IsUpToDate(newFileVersion, fileVersion))
            {
                objectJsonCorrecter(objectJson);

                Logger.Instance.Log($"Corrected {objectName} json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                fileVersion = newFileVersion;
            }
        }

        /// <summary>
        /// Tries to correct the json data of the object, if it's out of date.
        /// </summary>
        /// <param name="objectName">The name of the object to convert.</param>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="correcters">The list of function to use, to correct the old json data.</param>
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

            Logger.Instance.Log($"{objectName} json data is old", "correcting data");
            foreach (var (objectJsonCorrecter, newFileVersion) in correcters)
            {
                CorrectJsonDataVersion(objectName, objectJsonCorrecter, ref objectJson, ref fileVersion, newFileVersion);
            }
            Logger.Instance.Log($"{objectName} json data corrected");
        }

        /// <summary>
        /// Tries to correct the json data of the object, if it's out of date.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="correcters">The list of function to use, to correct the old json data.</param>
        public void CorrectJsonData<T>(
            ref IDictionary<string, object?> objectJson,
            List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion
        )
        {
            CorrectJsonData(typeof(T).ToString(), ref objectJson, correcters, fileVersion);
        }
        #endregion
    }
}
