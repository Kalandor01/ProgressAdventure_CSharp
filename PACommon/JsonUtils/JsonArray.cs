using System.Collections;
using System.Diagnostics;

namespace PACommon.JsonUtils
{
    [DebuggerDisplay("Length = {Value.Count}")]
    public class JsonArray : JsonObject<List<JsonObject?>>, IList<JsonObject?>
    {
        public JsonArray(List<JsonObject?> value)
            : base(JsonObjectType.Array, value) { }

        #region Interface methods
        public JsonObject? this[int index] { get => Value[index]; set => Value[index] = value; }

        public int Count => Value.Count;

        public bool IsReadOnly => false;

        public void Add(JsonObject? item)
        {
            Value.Add(item);
        }

        public void Clear()
        {
            Value.Clear();
        }

        public bool Contains(JsonObject? item)
        {
            return Value.Contains(item);
        }

        public void CopyTo(JsonObject?[] array, int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }

        public IEnumerator<JsonObject?> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        public int IndexOf(JsonObject? item)
        {
            return Value.IndexOf(item);
        }

        public void Insert(int index, JsonObject? item)
        {
            Value.Insert(index, item);
        }

        public bool Remove(JsonObject? item)
        {
            return Value.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Value.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Value.GetEnumerator();
        }
        #endregion
    }
}
