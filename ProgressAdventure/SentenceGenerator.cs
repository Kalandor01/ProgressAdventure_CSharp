using NPrng.Generators;
using System.Text;

namespace ProgressAdventure
{
    /// <summary>
    /// Class for generating sentences and words/names.
    /// </summary>
    public static class SentenceGenerator
    {
        #region Public constants
        /// <summary>
        /// A list of letters, where the number of letters coresponds to the distribution of letters in a word, in the english alphabet.
        /// </summary>
        public static readonly char[] STANDARD_ENGLISH_LETTER_DISTRIBUTION =
            new Dictionary<char, double>()
            {
                ['a'] = 8.2,
                ['b'] = 1.5,
                ['c'] = 2.8,
                ['d'] = 4.3,
                ['e'] = 12.7,
                ['f'] = 2.2,
                ['g'] = 2.0,
                ['h'] = 6.1,
                ['i'] = 7.0,
                ['j'] = 0.15,
                ['k'] = 0.77,
                ['l'] = 4.0,
                ['m'] = 2.4,
                ['n'] = 6.7,
                ['o'] = 7.5,
                ['p'] = 1.9,
                ['q'] = 0.095,
                ['r'] = 6.0,
                ['s'] = 6.3,
                ['t'] = 9.1,
                ['u'] = 2.8,
                ['v'] = 0.98,
                ['w'] = 2.4,
                ['x'] = 0.15,
                ['y'] = 2.0,
                ['z'] = 0.074,
            }
            .SelectMany(letter => Enumerable.Repeat(letter.Key, (int)(letter.Value * LETTER_DISTRIBUTION_VALUE_MULTIPLIER)))
            .ToArray();
        #endregion

        #region Private constants
        /// <summary>
        /// The value to multiply the letter distribution value with, to increase the precision of the distribution, but put more letters into the letter distribution array.
        /// </summary>
        private const int LETTER_DISTRIBUTION_VALUE_MULTIPLIER = 1000;
        #endregion

        #region Public functions
        /// <summary>
        /// Generates a word made of semi-random letters.
        /// </summary>
        /// <param name="minLetters">The minimum number of letters in the word.</param>
        /// <param name="maxLetters">The maximum number of letters in the word.</param>
        /// <param name="randomGenerator">The generator to use to generate the word.</param>
        public static string GenerateRandomWord(int minLetters = 2, int maxLetters = 8, SplittableRandom? randomGenerator = null)
        {
            randomGenerator ??= RandomStates.Instance.MiscRandom;
            return RandomSentence.SentenceGenerator.UnstructuredRandom(minLetters, maxLetters, STANDARD_ENGLISH_LETTER_DISTRIBUTION, randomGenerator);
        }

        /// <summary>
        /// Generates a sequence of words, seperated by a space.
        /// </summary>
        /// <param name="wordCount">The number of word in the sequence.</param>
        /// <param name="letterCount">The number of letters in a word.</param>
        /// <param name="randomGenerator">The generator to use to generate the word.</param>
        public static string GenerateWordSequence((int min, int max) wordCount, (int min, int max)? letterCount = null, SplittableRandom? randomGenerator = null)
        {
            randomGenerator ??= RandomStates.Instance.MiscRandom;
            var wordCountNum = randomGenerator.GenerateInRange(wordCount.min, wordCount.max);
            letterCount ??= (2, 8);
            var sentence = new StringBuilder();
            for (int x = 0; x < wordCountNum; x++)
            {
                sentence.Append(GenerateRandomWord(letterCount.Value.min, letterCount.Value.max, randomGenerator));
                if (x < wordCountNum - 1)
                {
                    sentence.Append(' ');
                }
            }
            return sentence.ToString();
        }
        #endregion
    }
}
