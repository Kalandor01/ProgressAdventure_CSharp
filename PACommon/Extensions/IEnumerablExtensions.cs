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
    }
}
