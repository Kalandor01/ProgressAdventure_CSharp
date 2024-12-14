using PACommon.Enums;
using System.Diagnostics.CodeAnalysis;

namespace PACommon.JsonUtils
{
    /// <summary>
    /// Interface for classes that can be converted to and from json.
    /// </summary>
    /// <typeparam name="T">The subclass.</typeparam>
    public interface IJsonConvertable<T> : IJsonReadable
        where T : IJsonConvertable<T>
    {
        #region Protected properties
        protected static virtual List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> VersionCorrecters { get; } = [];
        #endregion

        #region Public functions
        /// <summary>
        /// Converts the json representation of the object to an object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        public static virtual bool FromJson(JsonDictionary? objectJson, string fileVersion, [NotNullWhen(true)] out T? convertedObject)
        {
            convertedObject = default;
            if (objectJson is null)
            {
                Tools.LogJsonNullError<T>(typeof(T).ToString(), isError: true);
                return false;
            }

            PACSingletons.Instance.JsonDataCorrecter.CorrectJsonData<T>(objectJson, T.VersionCorrecters, fileVersion);

            return T.FromJsonWithoutCorrection(objectJson, fileVersion, ref convertedObject);
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
        public abstract static bool FromJsonWithoutCorrection(JsonDictionary objectJson, string fileVersion, [NotNullWhen(true)] ref T? convertedObject);
        #endregion
    }
}
