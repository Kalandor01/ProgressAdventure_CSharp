namespace ProgressAdventure
{
    /// <summary>
    /// Interface for classes that can be converted to and from json.
    /// </summary>
    /// <typeparam name="T">The subclass.</typeparam>
    public interface IJsonConvertable<T> : IJsonReadable
        where T : IJsonConvertable<T>
    {
        /// <summary>
        /// Converts the json representation of the object to object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        static abstract T? FromJson(IDictionary<string, object?>? objectJson);
    }
}
