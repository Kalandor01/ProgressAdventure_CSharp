using NPrng;

namespace ProgressAdventure
{
    /// <summary>
    /// Object for storing extensions for <c>NPrng</c>.
    /// </summary>
    public static class NPrngExtensions
    {
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
