using System.Diagnostics;

namespace PACommon.JsonUtils
{
    [DebuggerDisplay("Length = {Value.Count}")]
    public class JsonArray : JsonObject<List<JsonObject?>>
    {
        public JsonArray(List<JsonObject?> value)
            : base(JsonObjectType.Array, value) { }
    }
}
