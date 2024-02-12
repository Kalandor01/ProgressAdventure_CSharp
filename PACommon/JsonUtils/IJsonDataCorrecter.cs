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
        /// <param name="orderCorrecters">Whether to order the json correcters by version number, before correcting, or the default if null.</param>
        public void CorrectJsonData(
            string objectName,
            ref IDictionary<string, object?> objectJson,
            List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        );

        /// <summary>
        /// Tries to correct the json data of the object, if it's out of date.
        /// </summary>
        /// <typeparam name="T">The type of the object that the json object represents.</typeparam>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="correcters">The list of function to use, to correct the old json data.</param>
        /// <param name="orderCorrecters">Whether to order the json correcters by version number, before correcting, or the default if null.</param>
        public void CorrectJsonData<T>(
            ref IDictionary<string, object?> objectJson,
            List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        );

        /// <inheritdoc cref="CorrectJsonData(string, ref IDictionary{string, object?}, List{ValueTuple{Action{IDictionary{string, object?}}, string}}, string, bool?)"/>
        /// <typeparam name="TE">The type of the extra data to input with all correcters.</typeparam>
        /// <param name="extraData">The extra data to input with the correcter.</param>
        public void CorrectJsonData<TE>(
            string objectName,
            ref IDictionary<string, object?> objectJson,
            TE extraData,
            List<(Action<IDictionary<string, object?>, TE> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        );

        /// <inheritdoc cref="CorrectJsonData{T}(ref IDictionary{string, object?}, List{ValueTuple{Action{IDictionary{string, object?}}, string}}, string, bool?)"/>
        /// <typeparam name="TE">The type of the extra data to input with all correcters.</typeparam>
        /// <param name="extraData">The extra data to input with the correcter.</param>
        public void CorrectJsonData<T, TE>(
            ref IDictionary<string, object?> objectJson,
            TE extraData,
            List<(Action<IDictionary<string, object?>, TE> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion,
            bool? orderCorrecters = null
        );
        #endregion
    }
}
