using System.Diagnostics.CodeAnalysis;

namespace PACommon.JsonUtils
{
    public class JsonValue : JsonObject<object>
    {
        #region Constructors
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
        #endregion

        #region Public methods
        public bool TryAsULong(out ulong value)
        {
            value = default;
            if (TryAsValueType(JsonObjectType.WholeNumber, out object? objValue))
            {
                if (
                    objValue is long lValue &&
                    lValue < 0
                )
                {
                    return false;
                }
                value = objValue is ulong ulValue ? ulValue : Convert.ToUInt64(objValue);
                return true;
            }
            return false;
        }

        public bool TryAsLong(out long value)
        {
            value = default;
            if (TryAsValueType(JsonObjectType.WholeNumber, out object? objValue))
            {
                if (
                    objValue is ulong ulValue &&
                    ulValue > long.MaxValue
                )
                {
                    return false;
                }
                value = objValue is long lValue ? lValue : Convert.ToInt64(objValue);
                return true;
            }
            return false;
        }

        public bool TryAsString([NotNullWhen(true)] out string? value, bool strict = true)
        {
            if (!strict)
            {
                value = Value.ToString();
                return value is not null;
            }

            return TryAsValueType(JsonObjectType.String, out value);
        }

        public bool TryAsDouble(out double value)
        {
            return TryAsValueType(JsonObjectType.Double, out value);
        }

        public bool TryAsBool(out bool value)
        {
            return TryAsValueType(JsonObjectType.Bool, out value);
        }

        public bool TryAsDateTime(out DateTime value)
        {
            return TryAsValueType(JsonObjectType.DateTime, out value);
        }

        public bool TryAsTimeSpan(out TimeSpan value)
        {
            return TryAsValueType(JsonObjectType.TimeSpan, out value);
        }

        public bool TryAsGuid(out Guid value)
        {
            return TryAsValueType(JsonObjectType.Guid, out value);
        }
        #endregion

        #region Private methods
        private bool TryAsValueType<T>(JsonObjectType type, [NotNullWhen(true)] out T? value)
        {
            value = default;
            if (
                Type == type &&
                TryAsType(out var objValue)
            )
            {
                value = (T)objValue;
                return true;
            }
            return false;
        }
        #endregion
    }
}
