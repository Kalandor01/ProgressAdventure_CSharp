using NPrng;
using NPrng.Generators;
using NPrng.Serializers;
using PACommon.Enums;
using PACommon.JsonUtils;
using SaveFileManager;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PACommon
{
    /// <summary>
    /// Contains project specific useful functions.
    /// </summary>
    public static class Tools
    {
        #region Public functions
        #region Encode/decode short
        /// <param name="data">The list of data to write to the file, where each element of the list is a line.</param>
        /// <inheritdoc cref="EncodeSaveShort(IEnumerable{IDictionary}, string, long, string)"/>
        public static void EncodeSaveShort(IDictionary data, string filePath, long seed, string extension)
        {
            EncodeSaveShort(new List<IDictionary> { data }, filePath, seed, extension);
        }

        /// <summary>
        /// Shorthand for <c>EncodeFile</c> + convert from json to string.
        /// </summary>
        /// <param name="dataList">The data to write to the file.</param>
        /// <param name="filePath">The path and the name of the file without the extension, that will be created.<br/>
        /// If the path contains a *, it will be replaced with the seed.</param>
        /// <param name="seed">The seed for encoding the file.</param>
        /// <param name="extension">The extension of the file that will be created.</param>
        public static void EncodeSaveShort(IEnumerable<IDictionary> dataList, string filePath, long seed, string extension)
        {
            var JsonDataList = dataList.Select(JsonSerializer.SerializeJson);
            FileConversion.EncodeFile(JsonDataList, seed, filePath, extension, Constants.FILE_ENCODING_VERSION, Constants.ENCODING);
        }

        /// <summary>
        /// Shorthand for <c>DecodeFile</c> + convert from string to json.
        /// </summary>
        /// <param name="filePath">The path and the name of the file without the extension, that will be decoded.<br/>
        /// If the path contains a *, it will be replaced with the seed.</param>
        /// <param name="lineNum">The line, that you want go get back (starting from 0).</param>
        /// <param name="seed">The seed for decoding the file.</param>
        /// <param name="extension">The extension of the file that will be decoded.</param>
        /// <param name="expected">If the file is expected to exist.<br/>
        /// ONLY ALTERS THE LOGS DISPLAYED, IF THE FILE/FOLDER DOESN'T EXIST.</param>
        /// <exception cref="FormatException">Exeption thrown, if the file couldn't be decode.</exception>
        /// <exception cref="FileNotFoundException">Exeption thrown, if the file couldn't be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Exeption thrown, if the directory containing the file couldn't be found.</exception>
        public static Dictionary<string, object?>? DecodeSaveShort(string filePath, long seed, string extension, int lineNum = 0, bool expected = true)
        {
            var safeFilePath = Path.GetRelativePath(Constants.ROOT_FOLDER, filePath);

            string decodedLine;
            try
            {
                decodedLine = FileConversion.DecodeFile((long)seed, filePath, extension, lineNum + 1, Constants.ENCODING).Last();
            }
            catch (FormatException)
            {
                PACSingletons.Instance.Logger.Log("Decode error", $"file name: {safeFilePath}.{extension}", LogSeverity.ERROR);
                throw;
            }
            catch (FileNotFoundException)
            {
                PACSingletons.Instance.Logger.Log("File not found", $"{(expected ? "" : "(but it was expected) ")}file name: {safeFilePath}.{extension}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
                throw;
            }
            catch (DirectoryNotFoundException)
            {
                PACSingletons.Instance.Logger.Log("Folder containing file not found", $"{(expected ? "" : "(but it was expected) ")}file name: {safeFilePath}.{extension}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
                throw;
            }

            try
            {
                return JsonSerializer.DeserializeJson(decodedLine);
            }
            catch (Exception)
            {
                PACSingletons.Instance.Logger.Log("Json decode error", $"file name: {safeFilePath}.{extension}", LogSeverity.ERROR);
                throw;
            }
        }
        #endregion

        #region Recreate folder
        /// <summary>
        /// Recreates the folder, if it doesn't exist.
        /// </summary>
        /// <param name="folderName">The name of the folder to check.</param>
        /// <param name="parentFolderPath">The path to the parrent folder, where the folder should be located.</param>
        /// <param name="displayName">The display name of the folder, for the logger, if it needs to be recreated.</param>
        /// <returns>If the folder needed to be recreated.</returns>
        public static bool RecreateFolder(string folderName, string? parentFolderPath = null, string? displayName = null)
        {
            parentFolderPath ??= Constants.ROOT_FOLDER;
            displayName ??= folderName.ToLower();

            var folderPath = Path.Join(parentFolderPath, folderName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                PACSingletons.Instance.Logger.Log($"Recreating {displayName} folder");
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Random
        /// <summary>
        /// Turns a Splittable random into its string representation.
        /// </summary>
        /// <param name="randomGenerator">The random generator.</param>
        public static string SerializeRandom(SplittableRandom randomGenerator)
        {
            return new SplittableRandomSerializer().WriteToString(randomGenerator);
        }

        /// <summary>
        /// Turns the string representation of a Splittable random into an object.
        /// </summary>
        /// <param name="randomString">The random generator's string representation.</param>
        public static SplittableRandom? DeserializeRandom(string? randomString)
        {
            if (randomString is null)
            {
                PACSingletons.Instance.Logger.Log("Random parse error", "random seed is null", LogSeverity.WARN);
                return null;
            }
            try
            {
                return (SplittableRandom)new SplittableRandomSerializer().ReadFromString(randomString);
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is FormatException)
                {
                    PACSingletons.Instance.Logger.Log("Random parse error", "cannot parse random generator from seed string", LogSeverity.WARN);
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Tries to turn the string representation of a Splittable random into an object, and returns the success.
        /// </summary>
        /// <param name="randomString">The random generator's string representation.</param>
        /// <param name="random">The random generator, that got deserialised.</param>
        public static bool TryDeserializeRandom(string? randomString, out SplittableRandom? random)
        {
            random = DeserializeRandom(randomString);
            return random is not null;
        }

        /// <summary>
        /// Returns a new <c>SplittableRandom</c> from another <c>SplittableRandom</c>.
        /// </summary>
        /// <param name="parrentRandom">The random generator to use, to generate the other generator.</param>
        public static SplittableRandom MakeRandomGenerator(SplittableRandom parrentRandom)
        {
            return parrentRandom.Split();
        }
        #endregion

        #region FromJson
        /// <summary>
        /// Tries to convert the json representation of the object to an object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        public static bool TryFromJson<T>(IDictionary<string, object?>? objectJson, string fileVersion, [NotNullWhen(true)] out T? convertedObject)
            where T : IJsonConvertable<T>
        {
            return T.FromJson(objectJson, fileVersion, out convertedObject);
        }

        /// <summary>
        /// Converts the json representation of the object to an object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        public static T? FromJson<T>(IDictionary<string, object?>? objectJson, string fileVersion)
            where T : IJsonConvertable<T>
        {
            T.FromJson(objectJson, fileVersion, out T? convertedObject);
            return convertedObject;
        }

        /// <summary>
        /// FromJson(), but without correcting the json data first.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        public static T? FromJsonWithoutCorrection<T>(IDictionary<string, object?> objectJson, string fileVersion, [NotNullWhen(true)] ref T? convertedObject)
            where T : IJsonConvertable<T>
        {
            T.FromJsonWithoutCorrection(objectJson, fileVersion, ref convertedObject);
            return convertedObject;
        }
        #endregion

        #region Logger short
        /// <summary>
        /// Logs a json parsing error for a custom error.
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <param name="message">The error message.</param>
        /// <param name="isError">If the error will make it, so the parsing function is halted.</param>
        public static void LogJsonError<T>(string message, bool isError = false)
        {
            var stackTrace = GetFromJsonCallStackString();
            PACSingletons.Instance.Logger.Log(
                $"{typeof(T)} parse {(isError ? "error" : "warning")}",
                message + (stackTrace is null ? "" : $"\n{stackTrace}"),
                isError ? LogSeverity.ERROR : LogSeverity.WARN
            );
        }

        /// <summary>
        /// Logs a json parsing error for a parameter parsing error.
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <param name="parameterName">The name of the parameter (or class) that caused the error.</param>
        /// <param name="extraInfo">Some extra information about the error, like a key associated to the value.</param>
        /// <param name="isError">If the error will make it, so the parsing function is halted.</param>
        public static void LogJsonParseError<T>(string parameterName, string? extraInfo = null, bool isError = false)
        {
            LogJsonError<T>($"couldn't parse {parameterName}{(extraInfo is not null ? $", {extraInfo}" : "")}", isError);
        }

        /// <summary>
        /// Logs a json parsing error for a null parameter error.
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <param name="parameterName">The name of the parameter (or class) that caused the error.</param>
        /// <param name="extraInfo">Some extra information about the error, like a key associated to the value.</param>
        /// <param name="isError">If the error will make it, so the parsing function is halted.</param>
        public static void LogJsonNullError<T>(string parameterName, string? extraInfo = null, bool isError = false)
        {
            LogJsonError<T>($"{parameterName} json is null{(extraInfo is not null ? $", {extraInfo}" : "")}", isError);
        }

        /// <summary>
        /// Returns the call stack of "FromJson()" methods.
        /// </summary>
        public static List<StackFrame> GetFromJsonCallStack()
        {
            var frames = new StackTrace(true).GetFrames();
            var fromJsonMethodName = "FromJsonWithoutCorrection";
            return frames.Where(frame => frame.GetMethod()?.Name.Contains(fromJsonMethodName) ?? false).ToList();
        }

        /// <summary>
        /// Returns the string representation of the call stack of "FromJson()" methods.
        /// </summary>
        public static string? GetFromJsonCallStackString()
        {
            var stackFrames = GetFromJsonCallStack();
            return !stackFrames.Any() ? null : string.Join(
                "\n",
                stackFrames.Select(frame =>
                $"\tat {frame.GetMethod()?.DeclaringType?.FullName} in {frame.GetFileName()}:line {frame.GetFileLineNumber()}"
            ));
        }
        #endregion

        #region Json parse short
        /// <summary>
        /// Tries to get a value from a json dictionary, and logs a warning, if it doesn't exist or null.
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <param name="objectJson">The json dictionary to parse the value from.</param>
        /// <param name="jsonKey">The key, to the value.</param>
        /// <param name="value">The returned value.</param>
        /// <param name="logParseWarnings">Whether to log any parse warnings.</param>
        /// <param name="isCritical">If the value is critical for the parsing of the object.<br/>
        /// If true, it will return immidietly if it can't be parsed and logs errors.</param>
        /// <returns>If the value was sucessfuly parsed.</returns>
        public static bool TryGetJsonObjectValue<T>(
            IDictionary<string, object?> objectJson,
            string jsonKey,
            [NotNullWhen(true)] out object? value,
            bool logParseWarnings = true,
            bool isCritical = false
        )
        {
            if (
                objectJson.TryGetValue(jsonKey, out value) &&
                value is not null
            )
            {
                return true;
            }
            if (logParseWarnings)
            {
                LogJsonNullError<T>(jsonKey, null, isCritical);
            }
            return false;
        }

        /// <summary>
        /// Tries to cast a value of a specific type from a value, and logs a warning, if it isn't the expected type.
        /// </summary>
        /// <param name="objectValue">The value to try to cast.</param>
        /// <typeparam name="TRes">The expected type of the result.</typeparam>
        /// <param name="parameterName">The name of the parameter to try to cast.</param>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(IDictionary{string, object?}, string, out object?, bool, bool)"/>
        public static bool TryCastAnyValueForJsonParsing<T, TRes>(object? objectValue, [NotNullWhen(true)] out TRes? value, string? parameterName = null, bool isCritical = false)
        {
            value = default;

            if (objectValue is TRes resultValue)
            {
                value = resultValue;
                return true;
            }

            LogJsonError<T>($"{parameterName ?? "parameter"} is not the expected type ({typeof(TRes)})", isCritical);
            return false;
        }

        /// <summary>
        /// Tries to get a value of a specific type from a json dictionary, and logs a warning, if it doesn't exist, or it can't be cast to the expected type.
        /// </summary>
        /// <typeparam name="TRes">The expected type of the result.</typeparam>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(IDictionary{string, object?}, string, out object?, bool, bool)"/>
        public static bool TryCastJsonAnyValue<T, TRes>(
            IDictionary<string, object?> objectJson,
            string jsonKey,
            [NotNullWhen(true)] out TRes? value,
            bool isCritical = false
        )
        {
            value = default;
            if (!TryGetJsonObjectValue<T>(objectJson, jsonKey, out var result, isCritical: isCritical))
            {
                return false;
            }

            return TryCastAnyValueForJsonParsing<T, TRes>(result, out value, jsonKey, isCritical);
        }

        /// <summary>
        /// Tries to parse a json value to a type. If the value can't be parsed, it logs a json parse warning.<br/>
        /// Usable types:<br/>
        /// - bool<br/>
        /// - string<br/>
        /// - numbers<br/>
        /// - enums<br/>
        /// - SplittableRandom<br/>
        /// - any type that has a converter? and can convert from string representation to object<br/>
        /// - nullables (will only make the default value null instead of the default value for the type)
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <typeparam name="TRes">The type to parse to.</typeparam>
        /// <param name="value">The value to parse.</param>
        /// <param name="parsedValue">The parsed value, or default if it wasn't successful.</param>
        /// <param name="logParseWarnings">Whether to log any parse warnings.</param>
        /// <param name="parameterName">The parameter name to use for logging unsuccesful parsing.</param>
        /// <param name="parameterExtraInfo">Some extra information about the parameter, like a key associated to the value.</param>
        /// <param name="isCritical">If the value is critical for the parsing of the object.<br/>
        /// If true, it will return immidietly if it can't be parsed and logs errors.</param>
        /// <returns>If the value was successfuly parsed.</returns>
        public static bool TryParseValueForJsonParsing<T, TRes>(
            object? value,
            [NotNullWhen(true)] out TRes? parsedValue,
            string? parameterName = null,
            bool logParseWarnings = true,
            string? parameterExtraInfo = null,
            bool isCritical = false
        )
        {
            parsedValue = default;
            var valueText = value?.ToString();

            if (valueText is null)
            {
                return false;
            }

            var parseType = typeof(TRes);
            var actualType = Nullable.GetUnderlyingType(parseType) ?? parseType;

            if (actualType == typeof(string))
            {
                parsedValue = (TRes)(object)valueText;
                return true;
            }

            var parseSuccess = true;
            if (actualType == typeof(SplittableRandom))
            {
                parseSuccess = TryDeserializeRandom(valueText, out var parsedRandom);
                if (parsedRandom is not null)
                {
                    parsedValue = (TRes)(object)parsedRandom;
                }
                else if (logParseWarnings)
                {
                    LogJsonParseError<T>(parameterName ?? $"{parseType} type parameter", parameterExtraInfo, isCritical);
                }
                return parseSuccess;
            }

            var parsedResult = default(TRes?);
            try
            {
                var converter = TypeDescriptor.GetConverter(parseType);
                parsedResult = (TRes?)converter.ConvertFromString(valueText);
            }
            catch
            {
                parseSuccess = false;
            }

            if (parseSuccess && parsedResult is not null)
            {
                if (!actualType.IsEnum || Enum.IsDefined(actualType, parsedResult))
                {
                    parsedValue = parsedResult;
                    return true;
                }
            }

            if (logParseWarnings)
            {
                LogJsonParseError<T>(parameterName ?? $"{parseType} type parameter", parameterExtraInfo, isCritical);
            }
            return false;
        }

        /// <summary>
        /// Tries to parse a value from a json dictionary, and logs a warning, if it can't pe parsed.<br/>
        /// Usable types:<br/>
        /// - bool<br/>
        /// - string<br/>
        /// - numbers<br/>
        /// - enums<br/>
        /// - SplittableRandom<br/>
        /// - any type that has a converter? and can convert from string representation to object (has [type].TryParse()?)<br/>
        /// - nullables (will only make the default value null instead of the default value for the type)
        /// </summary>
        /// <typeparam name="TRes">The type to parse to.</typeparam>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(IDictionary{string, object?}, string, out object?, bool, bool)"/>
        public static bool TryParseJsonValue<T, TRes>(
            IDictionary<string, object?> objectJson,
            string jsonKey,
            [NotNullWhen(true)] out TRes? value,
            bool logParseWarnings = true,
            bool isCritical = false
        )
        {
            value = default;
            if (!TryGetJsonObjectValue<T>(objectJson, jsonKey, out var result, logParseWarnings, isCritical))
            {
                return false;
            }

            return TryParseValueForJsonParsing<T, TRes>(result, out value, jsonKey, logParseWarnings, isCritical: isCritical);
        }

        /// <summary>
        /// Tries to parse an IJsonConvertable value from a json dictionary, and logs a warning, if it can't pe parsed.
        /// </summary>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <typeparam name="TJc">The IJsonConvertable class to convert to.</typeparam>
        /// <returns>If the object was parsed without warnings.</returns>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(IDictionary{string, object?}, string, out object?, bool, bool)"/>
        public static bool TryParseJsonConvertableValue<T, TJc>(
            IDictionary<string, object?> objectJson,
            string fileVersion,
            string jsonKey,
            [NotNullWhen(true)] out TJc? value,
            bool isCritical = false
        )
            where TJc : IJsonConvertable<TJc>
        {
            value = default;
            if (!TryCastJsonAnyValue<T, IDictionary<string, object?>>(objectJson, jsonKey, out var result, isCritical))
            {
                return false;
            }

            var success = TryFromJson(result, fileVersion, out value);
            if (value is null)
            {
                LogJsonParseError<T>(jsonKey, null, isCritical);
            }
            return success;
        }

        /// <summary>
        /// Tries to parse a list value from another list, and logs a warning, if an element can't be parsed.
        /// </summary>
        /// <typeparam name="TIn">The type of the values in the input list.</typeparam>
        /// <typeparam name="TRes">The type of the values in the result list.</typeparam>
        /// <param name="listValue">The list to parse the values from.</param>
        /// <param name="listName">The name of the list.</param>
        /// <param name="parseFunction">The function to use, to parse the elemets of the list to the correct type.<br/>
        /// If the success is false or result is null, it will not be added to the list.</param>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(IDictionary{string, object?}, string, out object?, bool, bool)"/>
        public static bool TryParseListValueForJsonParsing<T, TIn, TRes>(
            IEnumerable<TIn> listValue,
            string listName,
            Func<TIn, (bool success, TRes? result)> parseFunction,
            out List<TRes> value
        )
        {
            value = new List<TRes>();
            foreach (var element in listValue)
            {
                var (success, parsedResult) = parseFunction(element);
                if (success && parsedResult is not null)
                {
                    value.Add(parsedResult);
                }
                else
                {
                    LogJsonParseError<T>($"an element of the {listName} list");
                }
            }
            return true;
        }

        /// <summary>
        /// Tries to parse a list value from another list, and logs a warning, if an element can't be parsed.
        /// </summary>
        /// <typeparam name="TRes">The type of the values in the result list.</typeparam>
        /// <param name="listValue">The list to parse the values from.</param>
        /// <param name="listName">The name of the list.</param>
        /// <param name="parseFunction">The function to use, to parse the elemets of the list to the correct type.<br/>
        /// If the success is false or result is null, it will not be added to the list.</param>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(IDictionary{string, object?}, string, out object?, bool, bool)"/>
        public static bool TryParseListValueForJsonParsing<T, TRes>(
            IEnumerable<object?> listValue,
            string listName,
            Func<object?, (bool success, TRes? result)> parseFunction,
            out List<TRes> value
        )
        {
            return TryParseListValueForJsonParsing<T, object?, TRes>(listValue, listName, parseFunction, out value);
        }

        /// <summary>
        /// Tries to parse a list value from a json dictionary, and logs a warning, if it can't be parsed.
        /// </summary>
        /// <typeparam name="TRes">The type of the values in the result list.</typeparam>
        /// <param name="parseFunction">The function to use, to parse the elemets of the list to the correct type.<br/>
        /// If the success is false or result is null, it will not be added to the list.</param>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(IDictionary{string, object?}, string, out object?, bool, bool)"/>
        public static bool TryParseJsonListValue<T, TRes>(
            IDictionary<string, object?> objectJson,
            string jsonKey,
            Func<object?, (bool success, TRes? result)> parseFunction,
            [NotNullWhen(true)] out List<TRes>? value,
            bool isCritical = false
        )
        {
            value = null;
            if (!(
                TryCastJsonAnyValue<T, IEnumerable<object?>>(objectJson, jsonKey, out var resultList, isCritical) &&
                resultList is not null
            ))
            {
                return false;
            }

            return TryParseListValueForJsonParsing<T, TRes>(resultList, jsonKey, parseFunction, out value);
        }
        #endregion

        #region Other
        /// <summary>
        /// Gets what the value will be in a KeyValidatorDelegate in a TextField.
        /// </summary>
        /// <inheritdoc cref="TextField.KeyValidatorDelegate"/>
        public static string GetNewValueForKeyValidatorDelegate(StringBuilder currentValue, ConsoleKeyInfo? inputKey, int cursorPosition)
        {
            if (inputKey is null)
            {
                var sbText = currentValue.ToString();
                return sbText[0..cursorPosition] + sbText[(cursorPosition + 1)..];
            }
            return currentValue.ToString().Insert(cursorPosition, inputKey?.KeyChar.ToString() ?? "");
        }
        #endregion
        #endregion
    }
}