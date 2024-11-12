namespace PACommon.JsonUtils
{
    /// <summary>
    /// Interface for classes that can be converted to json.
    /// </summary>
    public interface IJsonReadable
    {
        /// <summary>
        /// Returns the json representation of the object.
        /// </summary>
        JsonDictionary ToJson();
    }
}
