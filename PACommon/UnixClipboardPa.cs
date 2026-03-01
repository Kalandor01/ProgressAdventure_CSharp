using System.Diagnostics;
using Clipboard;

namespace PACommon
{
    /// <summary>
    /// The Unix implementation of the <see cref="IClipboard"/>.<br/>
    /// Modified from the original from avalible in <see cref="Clipboard.UnixClipboard"/>.
    /// </summary>
    public class UnixClipboardPa : IClipboard
    {
        private const string MissingCommands =
            "No clipboard utilities available. Please install xsel, xclip, wl-clipboard or Termux:API add-on for termux-clipboard-get/set.";
        
        private const string WAYLAND_COPY = "wl-copy";
        private const string XORG1_COPY = "xsel";
        private const string XORG2_COPY = "xclip";
        private const string SOME_COPY = "clip.exe";
        private const string TERMUX_COPY = "termux-clipboard-set";

        private const string WAYLAND_PASTE = "wl-paste";
        private const string XORG1_PASTE = "xsel";
        private const string XORG2_PASTE = "xclip";
        private const string POWERSHELL_PASTE = "powershell.exe";
        private const string TERMUX_PASTE = "termux-clipboard-get";
        
        private static readonly List<(string name, string args)> s_copy =
        [
            (WAYLAND_COPY, string.Empty),
            (XORG1_COPY, "--input --clipboard"),
            (XORG2_COPY, "-int -selection clipboard"),
            (SOME_COPY, string.Empty),
            (TERMUX_COPY, string.Empty),
        ];
        
        private static readonly List<(string name, string args)> s_paste =
        [
            (WAYLAND_PASTE, "--no-newline"),
            (XORG1_PASTE, "--output --clipboard"),
            (XORG2_PASTE, "-out -selection clipboard"),
            (POWERSHELL_PASTE, "Get-Clipboard"),
            (TERMUX_PASTE, string.Empty),
        ];

        private readonly (string command, string args) _paste;
        private readonly (string command, string args) _copy;

        private readonly bool _trim;
        private readonly bool _unsupported;

        /// <summary>
        /// Initialize the <see cref="UnixClipboardPa"/> class.
        /// </summary>
        public UnixClipboardPa()
        {
            static bool ExistOnPath(string fileName)
            {
                if (File.Exists(fileName))
                {
                    return true;
                }
                
                var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                foreach (var path in pathEnv.Split(Path.PathSeparator))
                {
                    if (File.Exists(Path.Combine(path, fileName)))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            _paste = s_paste.FirstOrDefault(x => ExistOnPath(x.name));
            _copy = s_copy.FirstOrDefault(x => ExistOnPath(x.name));

            _unsupported = _paste.command == null || _copy.command == null;
            _trim = _paste.command != _copy.command &&
                _copy.command != WAYLAND_COPY;
        }

        /// <summary>
        /// Initialize the <see cref="UnixClipboardPa"/> class.
        /// </summary>
        /// <param name="paste">The paste command.</param>
        /// <param name="pasteArgs">The paste args.</param>
        /// <param name="copy">The copy command.</param>
        /// <param name="copyArgs">The copy args.</param>
        /// <param name="trim">If it should trim.</param>
        public UnixClipboardPa(string paste, string pasteArgs, 
            string copy, string copyArgs,
            bool trim = false)
        {
            _paste = (paste, pasteArgs);
            _copy = (copy, copyArgs);
            _trim = trim;
            _unsupported = false;
        }

        /// <inheritdoc cref="IClipboard.Read"/>
        public string Read()
        {
            if (_unsupported)
            {
                throw new MissingCommandClipboardException(MissingCommands);
            }

            using var process = new Process();
            process.StartInfo.FileName = _paste.command;
            process.StartInfo.Arguments = _paste.args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();
            var text = process.StandardOutput.ReadToEnd();
            if (_trim && text.Length > 0)
            {
                return text[..^2];
            }

            return text;
        }

        /// <inheritdoc cref="IClipboard.ReadAsync"/>
        public async Task<string> ReadAsync(CancellationToken cancellationToken = default)
        {
            if (_unsupported)
            {
                throw new MissingCommandClipboardException(MissingCommands);
            }

            using var process = new Process();
            process.StartInfo.FileName = _paste.command;
            process.StartInfo.Arguments = _paste.args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var text = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            if (_trim && text.Length > 0)
            {
                return text[..^2];
            }

            return text;
        }

        /// <inheritdoc cref="IClipboard.Write"/>
        public void Write(string text)
        {
            if (_unsupported)
            {
                throw new MissingCommandClipboardException(MissingCommands);
            }

            using var process = new Process();
            process.StartInfo.FileName = _copy.command;
            process.StartInfo.Arguments = _copy.args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;

            process.Start();
            process.StandardInput.Write(text);
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();
        }

        /// <inheritdoc cref="IClipboard.WriteAsync"/>
        public async Task WriteAsync(string text, CancellationToken cancellationToken = default)
        {
            if (_unsupported)
            {
                throw new MissingCommandClipboardException(MissingCommands);
            }

            using var process = new Process();
            process.StartInfo.FileName = _copy.command;
            process.StartInfo.Arguments = _copy.args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;

            process.Start();
            await process.StandardInput.WriteAsync(text.AsMemory(), cancellationToken).ConfigureAwait(false);
            await process.StandardInput.FlushAsync().ConfigureAwait(false);
            process.StandardInput.Close();
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
