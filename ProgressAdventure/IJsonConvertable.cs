using ProgressAdventure.Enums;

namespace ProgressAdventure
{
    /// <summary>
    /// Interface for classes that can be converted to and from json.
    /// </summary>
    /// <typeparam name="T">The subclass.</typeparam>
    public interface IJsonConvertable<T> : IJsonReadable
        where T : IJsonConvertable<T>
    {
        #region Protected properties
        protected static virtual List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> VersionCorrecters { get; } = new() { };
        #endregion

        #region Public functions
        /// <summary>
        /// Tries to convert the json representation of the object to an object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        public static virtual bool FromJson(IDictionary<string, object?>? objectJson, string fileVersion, out T? convertedObject)
        {
            convertedObject = default;
            if (objectJson is null)
            {
                Logger.Log($"{typeof(T)} parse error", $"{typeof(T).ToString().ToLower()} json is null", LogSeverity.ERROR);
                return false;
            }

            CorrectJsonData(ref objectJson, fileVersion);

            return T.FromJsonWithoutCorrection(objectJson, fileVersion, ref convertedObject);
        }
        #endregion

        #region Protected virtual functions
        /// <summary>
        /// Tries to correct the json data of the object, if it's out of date.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        protected static void CorrectJsonData(ref IDictionary<string, object?> objectJson, string fileVersion)
        {
            //correct data
            if (Tools.IsUpToDate(Constants.SAVE_VERSION, fileVersion))
            {
                return;
            }

            Logger.Log($"{typeof(T)} json data is old", "correcting data");
            foreach (var (objectJsonCorrecter, newFileVersion) in T.VersionCorrecters)
            {
                Tools.CorrectJsonDataVersion<T>(objectJsonCorrecter, ref objectJson, ref fileVersion, newFileVersion);
            }
            Logger.Log($"{typeof(T)} json data corrected");
        }
        #endregion

        #region Protected abstract functions
        /// <summary>
        /// FromJson(), but without correcting the json data first.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        protected abstract static bool FromJsonWithoutCorrection(IDictionary<string, object?> objectJson, string fileVersion, ref T? convertedObject);
        #endregion
    }
}
