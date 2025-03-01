using NPrng.Generators;
using PACommon.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

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

        #region Implicit casts
        public static implicit operator JsonObject?(string? v) => v is null ? null : new JsonValue(v);
        public static implicit operator JsonObject(char v) => new JsonValue(v.ToString());
        public static implicit operator JsonObject(bool v) => new JsonValue(v);
        public static implicit operator JsonObject(long v) => new JsonValue(v);
        public static implicit operator JsonObject(ulong v) => new JsonValue(v);
        public static implicit operator JsonObject(float v) => new JsonValue(v);
        public static implicit operator JsonObject(double v) => new JsonValue(v);
        public static implicit operator JsonObject(int v) => new JsonValue(v);
        public static implicit operator JsonObject(uint v) => new JsonValue(v);
        public static implicit operator JsonObject(Guid v) => new JsonValue(v);
        public static implicit operator JsonObject(DateTime v) => new JsonValue(v);
        public static implicit operator JsonObject(TimeSpan v) => new JsonValue(v);
        public static implicit operator JsonObject?(Enum? v) => v is null ? null : new JsonValue(v.ToString());
        public static implicit operator JsonObject?(EnumValueBase? v) => v is null ? null : new JsonValue(v.Name);
        public static implicit operator JsonObject?(EnumTreeValueBase? v) => v is null ? null : new JsonValue(v.FullName);
        public static implicit operator JsonObject?(SplittableRandom? v) => v is null ? null : new JsonValue(Tools.SerializeRandom(v));
        public static implicit operator JsonObject?(List<JsonObject?>? v) => v is null ? null : new JsonArray(v);
        public static implicit operator JsonObject?(Dictionary<string, JsonObject?>? v) => v is null ? null : new JsonDictionary(v);
        #endregion

        public static bool operator ==(JsonObject obj1, JsonObject obj2)
        {
            return obj1.Type == obj2.Type && obj1.Value.Equals(obj2.Value);
        }

        public static bool operator !=(JsonObject obj1, JsonObject obj2)
        {
            return !(obj1 == obj2);
        }

        public bool TryAsType([NotNullWhen(true)] out object? value)
        {
            bool success;
            switch (Type)
            {
                case JsonObjectType.Dictionary:
                    value = Value as Dictionary<string, JsonObject?>;
                    success = value is not null;
                    break;
                case JsonObjectType.Array:
                    value = Value as List<JsonObject?>;
                    success = value is not null;
                    break;
                case JsonObjectType.String:
                    value = Value.ToString();
                    success = value is not null;
                    break;
                case JsonObjectType.Double:
                    success = double.TryParse(Value?.ToString(), out var doubleValue);
                    value = doubleValue;
                    break;
                case JsonObjectType.WholeNumber:
                    success = BigInteger.TryParse(Value?.ToString(), out var number);
                    if (
                        !success ||
                        number > ulong.MaxValue ||
                        number < long.MinValue
                    )
                    {
                        value = null;
                        break;
                    }

                    success = true;
                    if (number > long.MaxValue)
                    {
                        value = (ulong)number;
                        break;
                    }
                    value = (long)number;
                    break;
                case JsonObjectType.Bool:
                    success = bool.TryParse(Value?.ToString(), out var boolValue);
                    value = boolValue;
                    break;
                case JsonObjectType.DateTime:
                    success = DateTime.TryParse(Value?.ToString(), out var dateTimeValue);
                    value = dateTimeValue;
                    break;
                case JsonObjectType.TimeSpan:
                    success = TimeSpan.TryParse(Value?.ToString(), out var timeSpanValue);
                    value = timeSpanValue;
                    break;
                case JsonObjectType.Guid:
                    success = Guid.TryParse(Value?.ToString(), out var guidValue);
                    value = guidValue;
                    break;
                default:
                    value = null;
                    success = false;
                    break;
            }
            return success;
        }

        public override string? ToString()
        {
            return Value.ToString();
        }
    }
}
