using System.Diagnostics;

namespace PACommon.JsonUtils
{
    public abstract class JsonObject<T> : JsonObject
        where T : notnull
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public new T Value { get => (T)base.Value; }

        public JsonObject(JsonObjectType type, T value)
            : base(type, value) { }
    }
}
