namespace PACommon.Extensions
{
    public static class ICollectionExtensions
    {
        /// <summary>
        /// Returns if all elements in the first list, exist in the second list, exactly once. The order of the elements don't mater.
        /// </summary>
        /// <param name="list1">The first list.</param>
        /// <param name="list2">The second list.</param>
        /// <typeparam name="T">The type of the objects in the collections.</typeparam>
        /// <param name="comparer">The function to use, to check the equality of the elements in the lists.</param>
        public static bool UnorderedSequenceEqual<T>(this ICollection<T> list1, ICollection<T> list2, Func<T, T, bool> comparer)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            var foundElements = new List<int>
            {
                Capacity = list1.Count
            };

            int currentIndex;

            foreach (var element1 in list1)
            {
                currentIndex = 0;
                foreach (var element2 in list2)
                {
                    if (comparer(element1, element2) && !foundElements.Contains(currentIndex))
                    {
                        foundElements.Add(currentIndex);
                        break;
                    }
                    currentIndex++;
                }
                if (currentIndex >= list1.Count)
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc cref="UnorderedSequenceEqual{T}(ICollection{T}, ICollection{T}, Func{T, T, bool})"/>
        public static bool UnorderedSequenceEqual<T>(this ICollection<T> list1, ICollection<T> list2)
        {
            return UnorderedSequenceEqual(list1, list2, (element1, element2) => (element1 is null && element2 is null) || (element1 is not null && element2 is not null && element1.Equals(element2)));
        }
    }
}
