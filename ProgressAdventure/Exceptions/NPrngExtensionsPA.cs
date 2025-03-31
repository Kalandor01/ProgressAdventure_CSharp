using NPrng.Generators;
using PACommon;
using PACommon.Extensions;

namespace ProgressAdventure.Extensions
{
    /// <summary>
    /// Object for storing extensions for <see cref="NPrng"/>.
    /// </summary>
    public static class NPrngExtensionsPA
    {
        /// <summary>
        /// Returns the seed string representation of the random generator.
        /// </summary>
        public static string ToSeedString(this SplittableRandom random)
        {
            var gamma = random.GetGamma();
            var seedArray = Utils.UlongToByteArray(random.GetSeed()).ReverseEnum().ToArray();

            return Convert.ToBase64String(gamma == NPrngExtensions.GOLDEN_GAMMA
                ? seedArray
                : [.. seedArray, .. Utils.UlongToByteArray(gamma).ReverseEnum().ToArray()]
            );
        }
        
        /// <summary>
        /// Returns a random generator from any string.
        /// </summary>
        /// <param name="text">The string to generate the random generator from.</param>
        /// <param name="seedString">A unified equivalent of the inputed string.</param>
        public static SplittableRandom GetRandomFromString(string text, out string seedString)
        {
            // string to seed + gamma
            byte[] arr;
            try
            {
                arr = Convert.FromBase64String(text);
            }
            catch (Exception e)
            {
                arr = text.ToCharArray().Select(ch => (byte)ch).ToArray();
            }
            var limitedArray = new byte[16];
            for (var x = 0; x < arr.Length; x++)
            {
                limitedArray[x % 16] = (byte)(limitedArray[x % 16] + arr[x]);
            }

            var seedArrReverse = limitedArray[..8];
            var gammaArrReverse = limitedArray[8..];
            var seedArr = seedArrReverse.ReverseEnum().ToArray();
            var gammaArr = gammaArrReverse.ReverseEnum().ToArray();

            // seed + gamma to seed string
            var gamma = Utils.ByteArrayToUlong(gammaArr);
            var isGammaUseles = gamma == 0 || gamma == NPrngExtensions.GOLDEN_GAMMA;
            if (gamma == 0)
            {
                gammaArr = Utils.UlongToByteArray(NPrngExtensions.GOLDEN_GAMMA);
            }

            seedString = Convert.ToBase64String(!isGammaUseles ? [.. seedArrReverse, .. gammaArrReverse] : seedArrReverse);

            // seed + gamma to random
            var randomStateText = Convert.ToBase64String(seedArr.Concat(gammaArr).ToArray());
            return PACommon.Tools.DeserializeRandom(randomStateText)
                ?? throw new InvalidOperationException("RNG deserialization from RNG seed string failed!");
        }
    }
}
