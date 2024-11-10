namespace PACommon.JsonUtils
{
    public abstract class JsonObject
    {
        public JsonObjectType Type { get; }

        public object Value { get; }

        public JsonObject(JsonObjectType type, object value)
        {
            Type = type;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static bool operator ==(JsonObject obj1, JsonObject obj2)
        {
            return obj1.Type == obj2.Type && obj1.Value.Equals(obj2.Value);
        }

        public static bool operator !=(JsonObject obj1, JsonObject obj2)
        {
            return !(obj1 == obj2);
        }

        public override string? ToString()
        {
            return Value.ToString();
        }
    }
}
