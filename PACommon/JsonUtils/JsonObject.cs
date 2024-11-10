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
