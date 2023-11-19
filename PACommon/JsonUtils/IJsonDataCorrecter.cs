using PACommon.Enums;
using PACommon.Logging;

namespace PACommon.JsonUtils
{
    /// <summary>
    /// Interface containing utils to convert json data for <c>IJsonConvertable</c> objects to different versions.
    /// </summary>
    public interface IJsonDataCorrecter
    {
        #region Public methods
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
        );

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
        );
        #endregion
    }
}
