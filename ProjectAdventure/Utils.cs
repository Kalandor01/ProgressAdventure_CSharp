using System.Text;

namespace ProjectAdventure
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
            return SaveFileManager.Utils.ReadInt(text, errorText);
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
        /// Writes out text, and then waits for a key press.
        /// </summary>
        /// <param name="text">The text to write out.</param>
        public static void PressKey(string text = "")
        {
            SaveFileManager.Utils.PressKey(text);
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
        /// Turns a list into a formated date string.
        /// </summary>
        /// <param name="dateList">The list of ints.</param>
        /// <param name="separation">The separation string.</param>
        public static string MakeDate(IEnumerable<int> dateList, string separation = "-")
        {
            if (dateList.Count() > 2)
            {
                return MakeDate(new DateTime(dateList.ElementAt(0), dateList.ElementAt(1), dateList.ElementAt(2)), separation);
            }
            else
            {
                return "[date list error]";
            }
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
        /// Turns a list into a formated time string.
        /// </summary>
        /// <param name="dateTimeList">The list of ints.</param>
        /// <param name="separation">The separation string.</param>
        /// <param name="writeMs">Whether to write out the microsecond part of the time or not.</param>
        /// <param name="msSeparation">The microsecond separation string.</param>
        public static string MakeTime(IEnumerable<int> dateTimeList, string separation = ":", bool writeMs = false, string msSeparation = ".")
        {
            if (dateTimeList.Count() > 5)
            {
                return MakeTime(new DateTime(dateTimeList.ElementAt(0), dateTimeList.ElementAt(1), dateTimeList.ElementAt(2), dateTimeList.ElementAt(3), dateTimeList.ElementAt(4), dateTimeList.ElementAt(5), dateTimeList.ElementAt(6)), separation, writeMs, msSeparation);
            }
            else
            {
                return "[time list error]";
            }
        }

        /// <summary>
        /// Turns a datetime object's time part or a list into a formated time string.
        /// </summary>
        /// <param name="dateTime">The DateTime object.</param>
        /// <param name="separation">The separation string.</param>
        /// <param name="writeMs">Whether to write out the microsecond part of the time or not.</param>
        /// <param name="msSeparation">The microsecond separation string.</param>
        public static string MakeTime(DateTime dateTime, string separation = ":", bool writeMs = false, string msSeparation = ".")
        {
            return dateTime.ToString($"HH{separation}mm{separation}ss{(writeMs ? $"{msSeparation}ffffff" : "")}");
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

        /// <summary>
        /// string.Replace(), but replaces all strings in the list.
        /// </summary>
        /// <param name="originalString">The string to replace from.</param>
        /// <param name="replacable">The list of strings to replace.</param>
        /// <param name="replace">The string to replace the string list with.</param>
        /// <returns></returns>
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
            return ReplaceAll(fileName, Path.GetInvalidFileNameChars(), "");
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
        /// <param name="round">If true, it rounds the resulting values in the vector.</param>
        /// <returns></returns>
        public static (double x, double y) VectorMultiply((double x, double y) vector, (double x, double y) multiplier, bool round = false)
        {
            var x = vector.x * multiplier.x;
            var y = vector.y * multiplier.y;
            if (round)
            {
                x = (int)x;
                y = (int)y;
            }
            return (x, y);
        }
        #endregion
    }
}
