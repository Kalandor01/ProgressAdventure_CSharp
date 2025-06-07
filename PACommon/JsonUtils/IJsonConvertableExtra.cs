using System.Diagnostics.CodeAnalysis;

namespace PACommon.JsonUtils
{
    /// <summary>
    /// Interface for classes that can be converted to and from json, with some extra data.
    /// </summary>
    /// <typeparam name="TSelf">The type that implements the interface.</typeparam>
    /// <typeparam name="TExtra">The extra data.</typeparam>
    public interface IJsonConvertableExtra<TSelf, TExtra> : IJsonReadable
        where TSelf : IJsonConvertableExtra<TSelf, TExtra>
    {
        #region Protected properties
        protected static virtual List<(Action<JsonDictionary, TExtra> objectJsonCorrecter, string newFileVersion)> VersionCorrecters { get; } = [];
        #endregion

        #region Public functions
        /// <summary>
        /// Converts the json representation of the object to an object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="extraData">The extra data to help with the conversion.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        public static virtual bool FromJson(
            JsonDictionary? objectJson,
            TExtra extraData,
            string fileVersion,
            [NotNullWhen(true)] out TSelf? convertedObject
        )
        {
            convertedObject = default;
            if (objectJson is null)
            {
                Tools.LogJsonNullError<TSelf>(typeof(TSelf).ToString(), isError: true);
                return false;
            }

            PACSingletons.Instance.JsonDataCorrecter.CorrectJsonData<TSelf, TExtra>(objectJson, extraData, TSelf.VersionCorrecters, fileVersion);

            return TSelf.FromJsonWithoutCorrection(objectJson, extraData, fileVersion, ref convertedObject);
        }
        #endregion

        #region Protected abstract functions
        /// <summary>
        /// <see cref="FromJson(JsonDictionary?, TExtra, string, out TSelf?)"/>, but without correcting the json data first.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="extraData">The extra data.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        public abstract static bool FromJsonWithoutCorrection(
            JsonDictionary objectJson,
            TExtra extraData,
            string fileVersion,
            [NotNullWhen(true)] ref TSelf? convertedObject
        );
        #endregion
    }
}
