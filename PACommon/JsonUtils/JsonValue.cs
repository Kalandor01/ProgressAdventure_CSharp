using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace PACommon.JsonUtils
{
    public class JsonValue : JsonObject<object>
    {
        public JsonValue(JsonObjectType type, string value)
            : base(type, value) { }

        public JsonValue(string value)
            : base(JsonObjectType.String, value) { }

        public JsonValue(double value)
            : base(JsonObjectType.Double, value) { }

        public JsonValue(string value, bool isWholeNumberAsString)
            : base(isWholeNumberAsString ? JsonObjectType.WholeNumber : JsonObjectType.String, value) { }
        
        public JsonValue(long value)
            : base(JsonObjectType.WholeNumber, value) { }

        public JsonValue(ulong value)
            : base(JsonObjectType.WholeNumber, value) { }

        public JsonValue(bool value)
            : base(JsonObjectType.Bool, value) { }

        public JsonValue(DateTime value)
            : base(JsonObjectType.DateTime, value) { }

        public JsonValue(TimeSpan value)
            : base(JsonObjectType.TimeSpan, value) { }

        public JsonValue(Guid value)
            : base(JsonObjectType.Guid, value) { }

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
    }
}
