namespace PACommon.Extensions
{
    public static class IEnumerablExtensions
    {
        private class LambdaComparer<T> : IComparer<T>
        {
            private readonly Func<T?, T?, int> _comparerFunction;

            public LambdaComparer(Func<T?, T?, int> comparerFunction)
            {
                _comparerFunction = comparerFunction;
            }

            public int Compare(T? x, T? y)
            {
                return _comparerFunction(x, y);
            }
        }

        /// <summary>
        /// Sorts the list into another list, in a stable way.
        /// </summary>
        /// <typeparam name="TE">The type of the elements of the list.</typeparam>
        /// <param name="list">The list to sort.</param>
        /// <param name="comparer">The function to sort the eleemnts of the list by.<br/>
        /// 0: equals, positive: first is bigger, negative: second is bigger</param>
        public static IEnumerable<TE> StableSort<TE>(this IEnumerable<TE> list, Func<TE?, TE?, int> comparer)
        {
            return list.OrderBy(e => e, new LambdaComparer<TE>(comparer));
        }

        /// <summary>
        /// Forces the list to reverse using <see cref="Enumerable.Reverse{TSource}(IEnumerable{TSource})"/> instead of <see cref="List{T}.Reverse()"/>.
        /// </summary>
        /// <typeparam name="TE">The type of the elements of the list.</typeparam>
        /// <param name="list">The list to reverse.</param>
        public static IEnumerable<TE> ReverseEnum<TE>(this IEnumerable<TE> list)
        {
            return list.Reverse();
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey,List{TElement}}"/> from an <see cref="IEnumerable{TSource}"/> according to specified key and value selector.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey">The type of the keys from elements of <paramref name="source"/></typeparam>
        /// <typeparam name="TElement">The type of the values from elements of <paramref name="source"/></typeparam>
        /// <param name="source">The <see cref="IEnumerable{TSource}"/> to create a <see cref="Dictionary{TKey,List{TElement}}"/> from.</param>
        /// <param name="keySelector"></param>
        /// <param name="valueSelector"></param>
        /// <returns>A <see cref="Dictionary{TKey,List{TElement}}"/> that contains keys and values from <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is a null reference.</exception>
        public static Dictionary<TKey, List<TElement>> ToListDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> valueSelector
        )
            where TKey : notnull
        {
            var dict = new Dictionary<TKey, List<TElement>>();
            foreach (var element in source)
            {
                var key = keySelector(element);
                var value = valueSelector(element);
                if (dict.TryGetValue(key, out var list))
                {
                    list.Add(value);
                }
                else
                {
                    dict[key] = [value];
                }
            }
            return dict;
        }
    }
}
