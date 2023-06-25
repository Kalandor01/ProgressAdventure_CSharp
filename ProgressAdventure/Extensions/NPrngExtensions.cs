using NPrng;

namespace ProgressAdventure.Extensions
{
    /// <summary>
    /// Object for storing extensions for <c>NPrng</c>.
    /// </summary>
    public static class NPrngExtensions
    {
        /// <summary>
        /// Returns a number in a range, between the min and max, in a triangular distribution.
        /// </summary>
        /// <param name="lower">The lower bound of the range.</param>
        /// <param name="middle">The median bound of the range.</param>
        /// <param name="upper">The upper bound of the range.</param>
        public static double Triangular(this AbstractPseudoRandomGenerator generator,
            double lower, double middle, double upper)
        {
            var num = generator.GenerateDouble();
            if (!(num < (middle - lower) / (upper - lower)))
            {
                return upper - Math.Sqrt((1.0 - num) * (upper - lower) * (upper - middle));
            }
            return lower + Math.Sqrt(num * (upper - lower) * (middle - lower));
        }
    }
}
