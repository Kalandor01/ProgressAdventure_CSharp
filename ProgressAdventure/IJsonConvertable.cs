namespace ProgressAdventure
{
    /// <summary>
    /// Interface for classes that can be converted to and from json.
    /// </summary>
    /// <typeparam name="T">The subclass.</typeparam>
    public interface IJsonConvertable<T> : IJsonReadable
        where T : IJsonConvertable<T>
    {
        ///// <summary>
        ///// Tries to convert the json representation of the object to an object format, and returns, if it was succesful without any warnings.
        ///// </summary>
        ///// <param name="objectJson">The json representation of the object.</param>
        ///// <param name="fileVersion">The version number of the loaded file.</param>
        //static abstract bool FromJson(IDictionary<string, object?>? objectJson, string fileVersion, out T? convertedObject);


        /// <summary>
        /// Converts the json representation of the object to object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        static abstract T? FromJson(IDictionary<string, object?>? objectJson, string fileVersion);
    }
}
