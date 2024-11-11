using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PACommon.JsonUtils
{
    [DebuggerDisplay("Count = {Value.Count}")]
    public class JsonDictionary : JsonObject<Dictionary<string, JsonObject?>>, IDictionary<string, JsonObject?>
    {
        #region Constructors
        public JsonDictionary(Dictionary<string, JsonObject?> value)
            : base(JsonObjectType.Dictionary, value)
        {

        }

        public JsonDictionary()
            : this([])
        {

        }
        #endregion

        #region Interface methods
        public JsonObject? this[string key] { get => Value[key]; set => Value[key] = value; }

        public ICollection<string> Keys => Value.Keys;

        public ICollection<JsonObject?> Values => Value.Values;

        public int Count => Value.Count;

        public bool IsReadOnly => false;

        public void Add(string key, JsonObject? value)
        {
            Value.Add(key, value);
        }

        public void Add(KeyValuePair<string, JsonObject?> item)
        {
            Value.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Value.Clear();
        }

        public bool Contains(KeyValuePair<string, JsonObject?> item)
        {
            return Value.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return Value.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, JsonObject?>[] array, int arrayIndex)
        {
            Value.ToArray().CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, JsonObject?>> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return Value.Remove(key);
        }

        public bool Remove(KeyValuePair<string, JsonObject?> item)
        {
            return Value.Remove(item.Key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out JsonObject? value)
        {
            return Value.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Value.GetEnumerator();
        }
        #endregion
    }
}
