using System.Reflection;

namespace PACommon.Extensions
{
    /// <summary>
    /// Object for storing extensions for <c>object</c>.
    /// </summary>
    public static class ObjectExtensions
    {
        // By aloisdg <a href="https://stackoverflow.com/a/11308879">LINK</a>
        #region DeepCopy
        private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(string)) return true;
            return type.IsValueType & type.IsPrimitive;
        }

        public static object DeepCopy(this object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<object, object>(new ReferenceEqualityComparer()));
        }

        public static T DeepCopy<T>(this T original)
        {
            return (T)DeepCopy((object)original);
        }

        private static object InternalCopy(object originalObject, IDictionary<object, object> visited)
        {
            if (originalObject == null)
            {
                return null;
            }
            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect))
            {
                return originalObject;
            }
            if (visited.ContainsKey(originalObject))
            {
                return visited[originalObject];
            }
            if (typeof(Delegate).IsAssignableFrom(typeToReflect))
            {
                return null;
            }
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType) == false)
                {
                    Array clonedArray = (Array)cloneObject;
                    clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }

            }
            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && filter(fieldInfo) == false)
                {
                    continue;
                }
                if (IsPrimitive(fieldInfo.FieldType))
                {
                    continue;
                }
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }
        #endregion

        #region Pubctic functions
        /// <summary>
        /// Recursively goes through a tree-like structure of objects, and applies a selector for all elements of the tree.
        /// </summary>
        /// <typeparam name="TE">The type of the elements of the tree.</typeparam>
        /// <typeparam name="TResult">The type that the selector returns</typeparam>
        /// <param name="rootElement">The root element of the tree.</param>
        /// <param name="childrenSelector">The function to select the list, containing the children of a node.</param>
        /// <param name="selector">A transform function to apply to each element.<br/>
        /// If the first element of the tuple is false, the result won't be added to the list.</param>
        /// <param name="propagationPredicate">A function to test an element for a condition.<br/>
        /// If the condition fails, the children of this element won't be checked.</param>
        /// <returns>A list of values, that the selector returned for each element.</returns>
        public static IEnumerable<TResult> RecursiveSelect<TE, TResult>(
            this TE rootElement,
            Func<TE, IEnumerable<TE>> childrenSelector,
            Func<TE, (bool select, TResult result)> selector,
            Func<TE, bool>? propagationPredicate = null
        )
        {
            var results = new List<TResult>();
            RecursiveSelectInternal(rootElement, childrenSelector, selector, results, propagationPredicate);
            return results;
        }
        #endregion

        #region Private functions
        /// <returns></returns>
        /// <param name="topElement">The top element.</param>
        /// <param name="resultsBuffer">The list for the ressult selector's return values.</param>
        /// <inheritdoc cref="RecursiveSelect{TE, TResult}(TE, Func{TE, IEnumerable{TE}}, Func{TE, ValueTuple{bool, TResult}}, Func{TE, bool}?)"/>
        private static void RecursiveSelectInternal<TE, TResult>(
            this TE topElement,
            Func<TE, IEnumerable<TE>> childrenSelector,
            Func<TE, (bool select, TResult result)> selector,
            IList<TResult> resultsBuffer,
            Func<TE, bool>? propagationPredicate = null
        )
        {
            var (select, result) = selector(topElement);
            if (select)
            {
                resultsBuffer.Add(result);
            }

            if (propagationPredicate is not null && !propagationPredicate(topElement))
            {
                return;
            }

            foreach (var child in childrenSelector(topElement))
            {
                RecursiveSelectInternal(child, childrenSelector, selector, resultsBuffer, propagationPredicate);
            }
        }
        #endregion
    }

    #region DeepCopy utility classes
    public class ReferenceEqualityComparer : EqualityComparer<object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        public override int GetHashCode(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.GetHashCode();
        }
    }

    internal static class ArrayExtensions
    {
        public static void ForEach(this Array array, Action<Array, int[]> action)
        {
            if (array.LongLength == 0)
            {
                return;
            }
            ArrayTraverse walker = new(array);
            do
            {
                action(array, walker.Position);
            }
            while (walker.Step());
        }
    }

    internal class ArrayTraverse
    {
        public int[] Position;
        private readonly int[] maxLengths;

        public ArrayTraverse(Array array)
        {
            maxLengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; ++i)
            {
                maxLengths[i] = array.GetLength(i) - 1;
            }
            Position = new int[array.Rank];
        }

        public bool Step()
        {
            for (int i = 0; i < Position.Length; ++i)
            {
                if (Position[i] < maxLengths[i])
                {
                    Position[i]++;
                    for (int j = 0; j < i; j++)
                    {
                        Position[j] = 0;
                    }
                    return true;
                }
            }
            return false;
        }
    }
    #endregion
}