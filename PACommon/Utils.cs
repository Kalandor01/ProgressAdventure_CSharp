using FileManager;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace PACommon
{
    /// <summary>
    /// Contains general usefull functions.
    /// </summary>
    public static class Utils
    {
        #region Public functions
        /// <summary>
        /// ReadLine, but only accepts int.
        /// </summary>
        /// <param name="text">Text to write out when requesting the number.</param>
        /// <param name="errorText">Text to write out when the user inputs a wrong value.</param>
        /// <returns></returns>
        public static int ReadInt(string text = "Number: ", string errorText = "Not a whole number!")
        {
            return FileManager.Utils.ReadInt(text, errorText);
        }

        /// <summary>
        /// ReadLine, but only accepts double.
        /// </summary>
        /// <param name="text">Text to write out when requesting the number.</param>
        /// <param name="errorText">Text to write out when the user inputs a wrong value.</param>
        /// <returns></returns>
        public static double ReadDouble(string text = "Number: ", string errorText = "Not a number!")
        {
            while (true)
            {
                Console.Write(text);
                if (double.TryParse(Console.ReadLine(), out double result))
                {
                    return result;
                }
                Console.WriteLine(errorText);
            }
        }

        /// <summary>
        /// Reads a ulong value from a byte array.<br/>
        /// Will always read from the first (up to 8) bytes.
        /// </summary>
        /// <param name="buffer">The buffer to read from.</param>
        public static ulong ReadUlongFromByteArray(byte[] buffer)
        {
            ulong value = 0;

            var minLen = Math.Min(sizeof(ulong), buffer.Length);

            for (var x = minLen - 1; x >= 0; x--)
            {
                value += (ulong)buffer[x] << 8 * (minLen - x - 1);
            }
            return value;
        }

        /// <summary>
        /// Writes out text, and then returns what the user inputed.
        /// </summary>
        /// <param name="text">The text to write out.</param>
        public static string? Input(string text)
        {
            Console.Write(text);
            return Console.ReadLine();
        }

        /// <summary>
        /// Returns if the Nth bit in a number is 1.
        /// </summary>
        /// <param name="value">The number to get the bit from</param>
        /// <param name="place">The 0 based index of the bit.</param>
        public static bool GetBit(int value, int place)
        {
            return (value & (1 << place)) != 0;
        }

        /// <summary>
        /// Writes out text, and then waits for a key press.
        /// </summary>
        /// <param name="text">The text to write out.</param>
        public static void PressKey(string text = "")
        {
            FileManager.Utils.PressKey(text);
        }

        /// <summary>
        /// Converts numbers that are smaller than 10 to have a trailing 0.
        /// </summary>
        /// <param name="number">The number to pad.</param>
        public static string PadZero(int number)
        {
            return (number < 10 && number > 0 ? "0" : "") + number.ToString();
        }

        /// <summary>
        /// Turns a datetime object's date into a formated date string.
        /// </summary>
        /// <param name="dateTime">The DateTime object.</param>
        /// <param name="separation">The separation string.</param>
        public static string MakeDate(DateTime dateTime, string separation = "-")
        {
            return dateTime.ToString($"{dateTime.Year}{separation}MM{separation}dd");
        }

        /// <summary>
        /// Turns a datetime object's time part or a list into a formated time string.
        /// </summary>
        /// <param name="dateTime">The DateTime object.</param>
        /// <param name="separation">The separation string.</param>
        /// <param name="writeMs">Whether to write out the microsecond part of the time.</param>
        /// <param name="msSeparation">The microsecond separation string.</param>
        public static string MakeTime(DateTime dateTime, string separation = ":", bool writeMs = false, string msSeparation = ".")
        {
            return dateTime.ToString($"HH{separation}mm{separation}ss{(writeMs ? $"{msSeparation}ffffff" : "")}");
        }


        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        /// <summary>
        /// Tries to enable ANSI codes, so they work for the terminal outside of the debug console.
        /// </summary>
        public static bool TryEnableAnsiCodes()
        {
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            GetConsoleMode(handle, out var mode);
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            return SetConsoleMode(handle, mode);
        }

        /// <summary>
        /// Colors text fore/background.
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="foregroundColor">The RGB color of the foreground color.</param>
        /// <param name="backgroundColor">The RGB color of the background color.</param>
        /// <returns>A string that will color the text, when writen out in the console.</returns>
        public static string StylizedText(string text, (byte r, byte g, byte b)? foregroundColor = null, (byte r, byte g, byte b)? backgroundColor = null)
        {
            var txt = new StringBuilder();
            if (foregroundColor is not null)
            {
                txt.Append($"\u001b[38;2;{foregroundColor.Value.r};{foregroundColor.Value.g};{foregroundColor.Value.b}m");
            }
            if (backgroundColor is not null)
            {
                txt.Append($"\u001b[48;2;{backgroundColor.Value.r};{backgroundColor.Value.g};{backgroundColor.Value.b}m");
            }
            return txt.Append($"{text}\u001b[0m").ToString();
        }

        /// <inheritdoc cref="ConsoleUI.Utils.MoveCursor(ValueTuple{int, int})"/>
        public static void MoveCursor((int x, int y) offset)
        {
            ConsoleUI.Utils.MoveCursor(offset);
        }

        /// <inheritdoc cref="ConsoleUI.Utils.GetDisplayLen(string, int, bool)"/>
        public static int GetDisplayLen(string text, int startingXPos = 0)
        {
            return ConsoleUI.Utils.GetDisplayLen(text, startingXPos);
        }

        /// <summary>
        /// string.Replace(), but replaces all strings in the list.
        /// </summary>
        /// <param name="originalString">The string to replace from.</param>
        /// <param name="replacable">The list of strings to replace.</param>
        /// <param name="replace">The string to replace the string list with.</param>
        public static string ReplaceAll(string originalString, IEnumerable<string> replacable, string replace)
        {
            var bulider = new StringBuilder(originalString);
            foreach (var rep in replacable)
            {
                bulider.Replace(rep, replace);
            }
            return bulider.ToString();
        }

        /// <inheritdoc cref="ReplaceAll(string, IEnumerable{string}, string)"/>
        public static string ReplaceAll(string originalString, IEnumerable<char> replacable, string replace)
        {
            return ReplaceAll(originalString, replacable.Select(c => c.ToString()), replace);
        }

        /// <summary>
        /// Removes all characters from the string that can't be in file/folder names.
        /// </summary>
        /// <param name="fileName">The file/folder name string.</param>
        public static string RemoveInvalidFileNameCharacters(string fileName)
        {
            var invalidTrailingChars = new char[] { '.', ' ' };
            return ReplaceAll(fileName, Path.GetInvalidFileNameChars(), "").TrimEnd(invalidTrailingChars);
        }

        /// <summary>
        /// Tries to get an int from the beggining of the string, or the first char, and then chops it of from the passed in string.<br/>
        /// Treats the "." and the "-" in numbers as a character.
        /// </summary>
        /// <param name="str">The string to search.</param>
        /// <param name="result">The char, or int.</param>
        /// <returns>If the returned result is an int.</returns>
        public static bool GetFirstCharOrInt(ref string str, out string result)
        {
            result = "";
            if (str == "")
            {
                return false;
            }

            if (int.TryParse(str, out int resInt) && resInt >= 0)
            {
                str = "";
                result = resInt.ToString();
                return true;
            }

            for (int x = 0; x < str.Length; x++)
            {
                if (!int.TryParse(str[..(x + 1)], out _))
                {
                    if (x == 0)
                    {
                        result = str[0].ToString();
                        str = str[1..];
                        return false;
                    }

                    result = str[..x].ToString();
                    str = str[x..];
                    return true;
                }
            }

            str = "";
            return false;
        }

        /// <summary>
        /// Tries to get an int from the beggining of the string and then chops it of from the passed in string.<br/>
        /// Treats the "." and the "-" in numbers as a character.
        /// </summary>
        /// <param name="str">The string to search.</param>
        /// <param name="result">Ther int.</param>
        /// <returns>If it fount an int in the string.</returns>
        public static bool TryGetFirstInt(ref string str, out int result)
        {
            result = 0;
            if (str == "")
            {
                return false;
            }

            if (int.TryParse(str, out result) && result >= 0)
            {
                str = "";
                return true;
            }

            var x = -1;
            while ((x + 1) < str.Length)
            {
                x++;
                if (int.TryParse(str[..(x + 1)], out var intRes))
                {
                    result = intRes;
                    continue;
                }

                if (x == 0)
                {
                    x = -1;
                    result = 0;
                    str = str[1..];
                    continue;
                }

                str = str[x..];
                return true;
            }

            str = "";
            return false;
        }

        /// <summary>
        /// Returns if the current version string is equal or higher than the minimum version.
        /// </summary>
        /// <param name="minimumVersion">The minimum version number to qualify for being up to date.</param>
        /// <param name="currentVersion">The version number to check.</param>
        public static bool IsUpToDate(string minimumVersion, string currentVersion)
        {
            if (minimumVersion == currentVersion)
            {
                return true;
            }

            var version = currentVersion.Split(".");
            var minVersion = minimumVersion.Split(".");

            for (int x = 0; x < version.Length; x++)
            {
                // min v. shorter
                if (minVersion.Length < (x + 1))
                {
                    return true;
                }

                if (version[x] == minVersion[x])
                {
                    continue;
                }

                // both numbers
                if (
                    int.TryParse(version[x], out int versionInt) &&
                    int.TryParse(minVersion[x], out int minVersionInt
                )
                )
                {
                    return versionInt > minVersionInt;
                }

                // string version comparison
                var versionPart = version[x];
                var minVersionPart = minVersion[x];
                while (versionPart != "")
                {
                    // min v. shorter
                    if (minVersionPart == "")
                    {
                        return true;
                    }

                    var isVersionPartPieceInt = GetFirstCharOrInt(ref versionPart, out string versionPartResult);
                    var isMinVersionPartPieceInt = GetFirstCharOrInt(ref minVersionPart, out string minVersionPartResult);

                    if (versionPartResult == minVersionPartResult)
                    {
                        continue;
                    }

                    // numbers > letters
                    if (isMinVersionPartPieceInt != isVersionPartPieceInt)
                    {
                        return isMinVersionPartPieceInt;
                    }

                    return isVersionPartPieceInt ?
                        int.Parse(versionPartResult) > int.Parse(minVersionPartResult) :
                        new string[] { versionPartResult, minVersionPartResult }.Order().First() == minVersionPartResult;
                }
            }
            // v. <=
            return false;
        }

        /// <summary>
        /// Adds together two vectors.
        /// </summary>
        /// <param name="vector1">The 1. vector to add together.</param>
        /// <param name="vector2">The 2. vector to add together.</param>
        /// <param name="round">If true, it rounds the resulting values in the vector.</param>
        /// <returns></returns>
        public static (double x, double y) VectorAdd((double x, double y) vector1, (double x, double y) vector2, bool round = false)
        {
            var x = vector1.x + vector2.x;
            var y = vector1.y + vector2.y;
            if (round)
            {
                x = (int)x;
                y = (int)y;
            }
            return (x, y);
        }

        /// <summary>
        /// Multiplies the first vector's parts with the numbers in the second vector.
        /// </summary>
        /// <param name="vector">The vector to multiply.</param>
        /// <param name="multiplier">The vector to multiply the first vector by.</param>
        public static (double x, double y) VectorMultiply((double x, double y) vector, (double x, double y) multiplier)
        {
            var x = vector.x * multiplier.x;
            var y = vector.y * multiplier.y;
            return (x, y);
        }

        /// <summary>
        /// Rotates the vector. (clockwise)
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="angle">The angle to rotate by.</param>
        public static (double x, double y) VectorRotate((double x, double y) vector, double angle)
        {
            var radian = -angle * (Math.PI / 180);

            var cos = Math.Cos(radian);
            var sin = Math.Sin(radian);

            var x = vector.x * cos - vector.y * sin;
            var y = vector.x * sin + vector.y * cos;

            return (x, y);
        }

        /// <summary>
        /// Modulo. (not %)
        /// </summary>
        /// <param name="number">The number to modulo.</param>
        /// <param name="mod">The modulo number.</param>
        public static int Mod(int number, int mod)
        {
            var r = number % mod;
            return r < 0 ? r + mod : r;
        }

        /// <summary>
        /// Modulo. (not %)
        /// </summary>
        /// <param name="number">The number to modulo.</param>
        /// <param name="mod">The modulo number.</param>
        public static long Mod(long number, long mod)
        {
            var r = number % mod;
            return r < 0 ? r + mod : r;
        }

        /// <summary>
        /// Rounds down a number, until it is divisible by another number.
        /// </summary>
        /// <param name="num">The number to round down.</param>
        /// <param name="round">The number to round to.</param>
        public static long FloorRound(long num, long round)
        {
            return num - Mod(num, round);
        }

        /// <summary>
        /// Rounds up a number, until it is divisible by another number.
        /// </summary>
        /// <param name="num">The number to round up.</param>
        /// <param name="round">The number to round to.</param>
        public static long CeilRound(long num, long round)
        {
            return num + Mod(round - Mod(num, round), round);
        }

        /// <summary>
        /// Recursively writes out lists and dictionaries.
        /// </summary>
        /// <param name="writable">The object to write out</param>
        /// <param name="recursionNum">Recursion number. Should not be modified.</param>
        public static void RecursiveWrite(object? writable, int recursionNum = 0)
        {
            if (writable is null)
            {
                Console.WriteLine("[NULL]");
            }
            else if (writable is not string && writable is IDictionary writableDict)
            {
                foreach (var item in writableDict.Keys)
                {
                    Console.WriteLine(new string('\t', recursionNum) + item.ToString() + ":");
                    RecursiveWrite(writableDict[item], recursionNum + 1);
                }
            }
            else if (writable is not string && writable is IEnumerable writableList)
            {
                recursionNum++;
                foreach (var item in writableList)
                {
                    RecursiveWrite(item, recursionNum);
                }
            }
            else
            {
                Console.WriteLine(new string('\t', recursionNum) + writable);
            }
        }

        /// <summary>
        /// Splits a path to a path to the last folder, the name of the file and the extension of the file.<br/>
        /// Returns null, if the path is null or whitespace.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        public static (string? folderPath, string fileName, string? fileExtension)? SplitPathToParts(string? fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return null;
            }

            string? folderPath = null;
            string fileName;
            string? fileExtension = null;

            var splitPath = fullPath.Split(Path.DirectorySeparatorChar);
            if (splitPath.Length > 1)
            {
                folderPath = string.Join(Path.DirectorySeparatorChar, splitPath[..^1]);
            }
            var splitFilePath = splitPath.Last().Split('.');
            fileName = splitFilePath.Last();
            if (splitFilePath.Length > 1)
            {
                fileExtension = fileName;
                fileName = string.Join('.', splitFilePath[..^1]);
            }

            return (folderPath, fileName, fileExtension);
        }

        /// <summary>
        /// Gets an internal field from a non-static class.
        /// </summary>
        /// <typeparam name="T">The type of the internal field.</typeparam>
        /// <typeparam name="TClass">The type of the class to get the field from.</typeparam>
        /// <param name="fieldName">The name of the internal field.</param>
        /// <param name="instance">The instance to get the field from.</param>
        /// <exception cref="ArgumentNullException">Thrown if the field is null.</exception>
        public static T GetInternalFieldFromNonStaticClass<T, TClass>(TClass instance, string fieldName)
            where TClass : class
        {
            return GetInternalFieldFromClass<T>(instance.GetType(), fieldName, instance);
        }

        /// <summary>
        /// Gets an internal field from a static class.
        /// </summary>
        /// <typeparam name="T">The type of the internal field.</typeparam>
        /// <param name="classType">The type of the static class to get the field from.</param>
        /// <param name="fieldName">The name of the internal field.</param>
        /// <exception cref="ArgumentNullException">Thrown if the field is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the class is not a static class.</exception>
        public static T GetInternalFieldFromStaticClass<T>(Type classType, string fieldName)
        {
            if (
                !classType.IsAbstract ||
                !classType.IsSealed
            )
            {
                throw new ArgumentException("The class is not a static class.", nameof(classType));
            }
            return GetInternalFieldFromClass<T>(classType, fieldName);
        }

        /// <summary>
        /// Gets an internal property from a non-static class.
        /// </summary>
        /// <typeparam name="T">The type of the internal property.</typeparam>
        /// <typeparam name="TClass">The type of the class to get the property from.</typeparam>
        /// <param name="propertyName">The name of the internal property.</param>
        /// <param name="instance">The instance to get the property from.</param>
        /// <exception cref="ArgumentNullException">Thrown if the property is null.</exception>
        public static T GetInternalPropertyFromNonStaticClass<T, TClass>(TClass instance, string propertyName)
            where TClass : class
        {
            return GetInternalPropertyValueFromClass<T>(instance.GetType(), propertyName, instance);
        }

        /// <summary>
        /// Gets an internal property from a static class.
        /// </summary>
        /// <typeparam name="T">The type of the internal property.</typeparam>
        /// <param name="classType">The type of the static class to get the property from.</param>
        /// <param name="propertyName">The name of the internal property.</param>
        /// <exception cref="ArgumentNullException">Thrown if the property is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the class is not a static class.</exception>
        public static T GetInternalPropertyFromStaticClass<T>(Type classType, string propertyName)
        {
            if (
                !classType.IsAbstract ||
                !classType.IsSealed
            )
            {
                throw new ArgumentException("The class is not a static class.", nameof(classType));
            }
            return GetInternalPropertyValueFromClass<T>(classType, propertyName);
        }

        /// <summary>
        /// Searches for all public static fields in a (static) class, and all of its nested (public static) classes, and returns their values.<br/>
        /// (Normaly used for getting all types of a nested enum type. (old)(replaced with AdvancedEnumTree))
        /// </summary>
        /// <typeparam name="T">The type of values to search for.</typeparam>
        /// <param name="classType">The type of the static class to search in.</param>
        public static List<T> GetNestedStaticClassFields<T>(Type classType)
        {
            var subClassFieldValues = new List<T>();

            var subClasses = classType.GetNestedTypes();

            foreach (var subClass in subClasses)
            {
                subClassFieldValues.AddRange(GetNestedStaticClassFields<T>(subClass));
            }
            FieldInfo[] properties = classType.GetFields();

            var classFieldValues = new List<T>();
            foreach (FieldInfo property in properties)
            {
                if (property.IsStatic && property.FieldType == typeof(T))
                {
                    var value = property.GetValue(null);
                    if (value is not null)
                    {
                        classFieldValues.Add((T)value);
                    }
                }
            }
            classFieldValues.AddRange(subClassFieldValues);

            return classFieldValues;
        }

        /// <summary>
        /// Opens a file selection window, and returns the file, the user selected.<br/>
        /// By Michael <a href="https://stackoverflow.com/a/68712025">LINK</a>
        /// </summary>
        /// <param name="filters">A list of filters. A filter limits the type of files that can appear in the window.</param>
        /// <param name="windowTitle">The title of the window.</param>
        public static string? OpenFileDialog(IEnumerable<(string regex, string displayName)>? filters = null, string windowTitle = "Select file...")
        {
            var filter = filters is not null ? string.Join("", filters.Select(filter => $"{filter.displayName}\0{filter.regex}\0")) : "";
            var ofn = new OpenFileName();
            ofn.lStructSize = Marshal.SizeOf(ofn);
            ofn.lpstrFilter = filter;
            ofn.lpstrFile = new string(new char[256]);
            ofn.nMaxFile = ofn.lpstrFile.Length;
            ofn.lpstrFileTitle = new string(new char[64]);
            ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
            ofn.lpstrTitle = windowTitle;
            return GetOpenFileName(ref ofn) ? ofn.lpstrFile : null;
        }

        /// <summary>
        /// Returns the type from it's full name.
        /// </summary>
        /// <param name="typeName">The full name of the type.</param>
        public static Type? GetTypeFromName(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// Zips up a text.
        /// </summary>
        /// <param name="text">The text to zip.</param>
        /// <param name="encoding">The encoding of the original text.</param>
        public static byte[] Zip(string text, Encoding? encoding = null)
        {
            encoding ??= Constants.ENCODING;
            var bytes = encoding.GetBytes(text);
            var utfBytes = Encoding.Convert(encoding, Encoding.UTF8, bytes);

            return FileConversion.Zip(utfBytes);
        }

        /// <summary>
        /// Unzips bytes into text.
        /// </summary>
        /// <param name="zippedBytes">The zipped bytes.</param>
        /// <param name="encoding">The encoding of the original text.</param>
        public static string Unzip(byte[] zippedBytes, Encoding? encoding = null)
        {
            encoding ??= Constants.ENCODING;
            var utfBytes = FileConversion.Unzip(zippedBytes);
            var bytes = Encoding.Convert(Encoding.UTF8, encoding, utfBytes);
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// Converts a byte array to a ulong.
        /// </summary>
        /// <param name="arr">The byte array.</param>
        public static ulong ByteArrayToUlong(byte[] arr)
        {
            ulong val = 0;
            for (var x = 0; x < 8; x++)
            {
                val <<= 8;
                val += arr.Length > x ? arr[x] : 0UL;
            }
            return val;
        }

        /// <summary>
        /// Converts a ulong to a byte array.
        /// </summary>
        /// <param name="value">The ulong.</param>
        public static byte[] UlongToByteArray(ulong value)
        {
            var arr = new byte[8];
            for (var x = 0; x < 8; x++)
            {
                arr[7 - x] = (byte)value;
                value >>= 8;
            }
            return arr;
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Gets an internal field from a class.
        /// </summary>
        /// <typeparam name="T">The type of the internal field.</typeparam>
        /// <param name="fieldName">The name of the internal field.</param>
        /// <param name="instance">The instance to get the field from. If null, it assumes, that the class is a static class.</param>
        /// <exception cref="ArgumentNullException">Thrown if the field doesn't exist.</exception>
        private static T GetInternalFieldFromClass<T>(Type classType, string fieldName, object? instance = null)
        {
            var field = classType?.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (
                field is null ||
                field.GetValue(instance) is not T value
            )
            {
                throw new ArgumentNullException(nameof(fieldName), "The internal filed doesn't exist.");
            }
            return value;
        }

        /// <summary>
        /// Gets an internal property from a class.
        /// </summary>
        /// <typeparam name="T">The type of the internal property.</typeparam>
        /// <param name="propertyName">The name of the internal property.</param>
        /// <param name="instance">The instance to get the property from. If null, it assumes, that the class is a static class.</param>
        /// <exception cref="ArgumentNullException">Thrown if the property doesn't exist.</exception>
        private static T GetInternalPropertyValueFromClass<T>(Type classType, string propertyName, object? instance = null)
        {
            var property = classType?.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
            if (
                property is null ||
                property.GetValue(instance) is not T value
            )
            {
                throw new ArgumentNullException(nameof(propertyName), "The internal property doesn't exist.");
            }
            return value;
        }

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetOpenFileName(ref OpenFileName ofn);
        #endregion
    }
}
