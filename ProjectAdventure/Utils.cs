using ProjectAdventure.Enums;
using System.Collections.Generic;
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


        //def remove_bad_characters(save_name:str):
        //    """
        //    Removes all characters that can't be in file/folder names.\n
        //    (\\\\/:*"?:<>|)
        //    """
        //    bad_chars = ["\\", "/", ":", "*", "\"", "?", ":", "<", ">", "|"]
        //    for char in bad_chars:
        //        save_name = save_name.replace(char, "")
        //    return save_name


        //def enum_item_finder(name:str, enum: EnumType) -> Enum|None:
        //    """
        //    Gives back the enum item, from the enum item name.\n
        //    Returns `None` if it doesn't exist.
        //    """
        //    try: return enum._member_map_[name]
        //        except KeyError: return None


        //def vector_add(vector1:tuple[float, float], vector2:tuple[float, float], round= False) :
        //    """
        //    Adds together the the two vectors.\n
        //    If `round` is True, it rounds the resoulting values in the vector.
        //    """
        //    x = vector1[0] + vector2[0]
        //    if round:
        //        x = int (x)
        //        y = vector1[1] + vector2[1]
        //    if round:
        //        y = int (y)
        //    return (x, y)


        //def vector_multiply(vector:tuple[float, float], multiplier:tuple[float, float], round= False) :
        //    """
        //    Multiplies the first vector's parts with the numbers in the second "vector".\n
        //    If `round` is True, it rounds the resoulting values in the vector.
        //    """
        //    x = vector[0] * multiplier[0]
        //    if round:
        //        x = int (x)
        //    y = vector[1] * multiplier[1]
        //    if round:
        //        y = int (y)
        //    return (x, y)


        //def press_key(text= "", allow_buffered_inputs= False) :
        //    """
        //    Writes out text, and then stalls until the user presses any key.\n
        //    If `allow_buffered_inputs` is `False`, if the user pressed some buttons before this function was called the function will not register those button presses.
        //    """
        //    if not allow_buffered_inputs:
        //        while kbhit() :
        //            getwch()
        //    print(text, end= "", flush= True)
        //    if getwch() in DOUBLE_KEYS:
        //        getwch()
        //    print()
        #endregion
    }
}
