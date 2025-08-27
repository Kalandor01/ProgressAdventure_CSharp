namespace PAVisualizer.Extensions
{
    public static class ObjectExtensions
    {
        public static T SplitInline<T>(this T obj, out T inlineCopy)
        {
            inlineCopy = obj;
            return obj;
        }
    }
}
