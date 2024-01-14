using NPrng;
using NPrng.Generators;
using NPrng.Serializers;

namespace PACommon.Extensions
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
        public static double Triangular(
            this AbstractPseudoRandomGenerator generator,
            double lower,
            double middle,
            double upper
        )
        {
            var num = generator.GenerateDouble();
            if (!(num < (middle - lower) / (upper - lower)))
            {
                return upper - Math.Sqrt((1.0 - num) * (upper - lower) * (upper - middle));
            }
            return lower + Math.Sqrt(num * (upper - lower) * (middle - lower));
        }

        /// <summary>
        /// Returns a random bool.
        /// </summary>
        /// <param name="trueChance">The chance of the result being true.</param>
        public static bool GenerateBool(this AbstractPseudoRandomGenerator generator)
        {
            return generator.Generate() >= 0;
        }

        /// <summary>
        /// <inheritdoc cref="GenerateBool(AbstractPseudoRandomGenerator)"/>
        /// </summary>
        /// <param name="trueChance">The chance of the result being true. (0 = 0%, 1 = 100%)</param>
        public static bool GenerateBool(this AbstractPseudoRandomGenerator generator, double trueChance = 0.5)
        {
            return generator.GenerateDouble() < trueChance;
        }

        /// <summary>
        /// Runs a negative delegate, a positive delegate or nothing, dependin or the resulting random value, with weighted chance.
        /// </summary>
        /// <param name="negativeChance">The chance of the negative outcome being run. (0 = 0%, 1 = 100%)</param>
        /// <param name="positiveChance">The chance of the positive outcome being run. (0 = 0%, 1 = 100%)</param>
        public static void GenerateTriChance(
            this AbstractPseudoRandomGenerator generator,
            Action negativeAction,
            Action positiveAction,
            double negativeChance = 1.0/3,
            double positiveChance = 1.0/3
        )
        {
            var value = generator.GenerateDouble();
            if (value < negativeChance)
            {
                negativeAction();
            }
            else if (value > 1 - positiveChance)
            {
                positiveAction();
            }
        }

        /// <summary>
        /// Gets the seed from a generator, using the serializer.
        /// </summary>
        /// <param name="serializer">The serializer to use, to get the seed.</param>
        /// <param name="generator">The generator to get the seed from.</param>
        public static ulong GetSeed(this SplittableRandomSerializer serializer, SplittableRandom generator)
        {
            var serializedString = serializer.WriteToString(generator);
            var byteArray = Convert.FromBase64String(serializedString);
            return Utils.ReadUlongFromByteArray(byteArray);
        }

        /// <summary>
        /// Gets the gamma from a generator, using the serializer.
        /// </summary>
        /// <param name="serializer">The serializer to use, to get the gamma.</param>
        /// <param name="generator">The generator to get the gamma from.</param>
        public static ulong GetGamma(this SplittableRandomSerializer serializer, SplittableRandom generator)
        {
            var serializedString = serializer.WriteToString(generator);
            var byteArray = Convert.FromBase64String(serializedString);
            return Utils.ReadUlongFromByteArray(byteArray[sizeof(ulong)..]);
        }

        /// <summary>
        /// Gets the seed from the generator.
        /// </summary>
        /// <param name="generator">The generator to get the seed from.</param>
        public static ulong GetSeed(this SplittableRandom generator)
        {
            return new SplittableRandomSerializer().GetSeed(generator);
        }

        /// <summary>
        /// Gets the gamma from the generator.
        /// </summary>
        /// <param name="generator">The generator to get the gamma from.</param>
        public static ulong GetGamma(this SplittableRandom generator)
        {
            return new SplittableRandomSerializer().GetGamma(generator);
        }
    }
}
