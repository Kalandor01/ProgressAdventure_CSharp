using System.Collections;
using System.Diagnostics;

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
        
        #region Public methods
        /// <summary>
        /// Merges the second dictionary's keys (and list values) into the first dictionary.
        /// </summary>
        /// <param name="secondDict">The second dict to merge into the first.</param>
        /// <param name="mergeDepth">How deep the merging should go.<br/>
        /// -1 = infinite</param>
        /// <param name="mergeLists">If values in lists should be merged too. (without deduplication)<br/>
        /// Needs <paramref name="mergeDepth"/> &gt;= 2.</param>
        /// <returns>The first (merged) discionary.</returns>
        public JsonDictionary Merge(JsonDictionary secondDict, int mergeDepth = 1, bool mergeLists = false)
        {
            if (mergeDepth != -1 && mergeDepth <= 0)
            {
                return this;
            }
            
            if (mergeDepth != -1)
            {
                mergeDepth--;
            }
            
            foreach (var entry in secondDict)
            {
                if (!ContainsKey(entry.Key))
                {
                    this[entry.Key] = entry.Value;
                    continue;
                }
                
                if (mergeDepth != -1 && mergeDepth <= 0)
                {
                    continue;
                }
                
                var existingItem = this[entry.Key];
                if (
                    existingItem is JsonDictionary existingDict &&
                    entry.Value is JsonDictionary newDict
                )
                {
                    existingDict.Merge(newDict, mergeDepth, mergeLists);
                }
                else if (
                    mergeLists &&
                    existingItem is JsonArray existingArray &&
                    entry.Value is JsonArray newArray
                )
                {
                    foreach (var item in newArray)
                    {
                        existingArray.Add(item);
                    }
                }
            }
            
            return this;
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
        
        public bool TryGetValue(string key, out JsonObject? value)
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
