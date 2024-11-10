using System.Diagnostics;

namespace PACommon.JsonUtils
{
    [DebuggerDisplay("Count = {Value.Count}")]
    public class JsonDictionary : JsonObject<Dictionary<string, JsonObject?>>
    {
        public JsonDictionary(Dictionary<string, JsonObject?> value)
            : base(JsonObjectType.Dictionary, value) { }
    }
}
