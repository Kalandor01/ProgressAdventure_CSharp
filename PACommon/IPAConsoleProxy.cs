using ConsoleUI;
using System.Text;

namespace PACommon
{
    public interface IPAConsoleProxy : IConsoleProxy, IDisposable
    {
        #region Properties
        /// <summary>
        /// The encoding of the output.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// The tile of the console.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// If a key is avalible to read.
        /// </summary>
        public bool KeyAvailable { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Writes out text, and then returns what the user inputed.
        /// </summary>
        /// <param name="text">The text to write out.</param>
        public string? ReadLine(string text);

        /// <summary>
        /// Tries to enable ANSI codes, so they work for the output.
        /// </summary>
        public bool TryEnableAnsiCodes();
        #endregion
    }
}
