﻿using PInvoke;
using System.Collections;
using System.Text;

namespace ProgressAdventure
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
        /// Writes out text, and then returns what the user inputed.
        /// </summary>
        /// <param name="text">The text to write out.</param>
        public static string? Input(string text)
        {
            return SaveFileManager.Utils.Input(text);
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
        /// <param name="writeMs">Whether to write out the microsecond part of the time or not.</param>
        /// <param name="msSeparation">The microsecond separation string.</param>
        public static string MakeTime(DateTime dateTime, string separation = ":", bool writeMs = false, string msSeparation = ".")
        {
            return dateTime.ToString($"HH{separation}mm{separation}ss{(writeMs ? $"{msSeparation}ffffff" : "")}");
        }

        /// <summary>
        /// Tries to enable ANSI codes, so they work for the terminal outside of the debug console.
        /// </summary>
        public static bool TryEnableAnsiCodes()
        {
            var stdHandle = Kernel32.StdHandle.STD_OUTPUT_HANDLE;

            var consoleHandle = Kernel32.GetStdHandle(stdHandle);
            if (Kernel32.GetConsoleMode(consoleHandle, out var consoleBufferModes) &&
                consoleBufferModes.HasFlag(Kernel32.ConsoleBufferModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING))
                return true;

            consoleBufferModes |= Kernel32.ConsoleBufferModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            return Kernel32.SetConsoleMode(consoleHandle, consoleBufferModes);
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
        /// Offsets the coordinates of the cursor.
        /// </summary>
        /// <param name="offset">The offset coordinates.</param>
        public static void MoveCursor((int x, int y) offset)
        {
            (int x, int y) = (Math.Clamp(Console.CursorLeft + offset.x, 0, Console.BufferWidth - 1), Math.Clamp(Console.CursorTop - offset.y, 0, Console.BufferHeight - 1));
            Console.SetCursorPosition(x, y);
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
        #endregion
    }
}
