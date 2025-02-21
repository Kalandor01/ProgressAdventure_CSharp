using FileManager;
using NPrng;
using NPrng.Generators;
using NPrng.Serializers;
using PACommon.Enums;
using PACommon.JsonUtils;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        /// <summary>
        /// Same as <see cref="EncodeFileShort(IEnumerable{JsonDictionary}, string, long, string)"/>, but for plain json files.
        /// </summary>
        /// <param name="dataList">The list of data to write to the file, where each element of the list is a line.</param>
        /// <param name="filePath">The path and the name of the file without the extension, that will be created.</param>
        /// <param name="extension">The extension of the file that will be created.</param>
        public static void SaveJsonFile(IEnumerable<JsonDictionary> dataList, string filePath, string extension = "json")
        {
            var jsonDataList = dataList.Select(d => JsonSerializer.SerializeJson(d));
            File.WriteAllLines($"{filePath}.{extension}", jsonDataList, Constants.ENCODING);
        }

        /// <summary>
        /// Same as <see cref="EncodeFileShort(JsonDictionary, string, long, string)"/>, but for plain json files.
        /// </summary>
        /// <param name="data">The data to write to the file.</param>
        /// <param name="format">Whether to format the json string. This will make the text span multiple lines.</param>
        /// <inheritdoc cref="SaveJsonFile(IEnumerable{JsonDictionary}, string, string)"/>
        public static void SaveJsonFile(JsonDictionary data, string filePath, string extension = "json", bool format = false)
        {
            if (format)
            {
                var jsonData = JsonSerializer.SerializeJson(data, true);
                File.WriteAllText($"{filePath}.{extension}", jsonData, Constants.ENCODING);
            }
            else
            {
                SaveJsonFile([data], filePath, extension);
            }
        }

        /// <summary>
        /// Same as <see cref="SaveJsonFile(IEnumerable{JsonDictionary}, string, string)"/>, but zips + base64 encodes the file.
        /// </summary>
        /// <param name="dataList">The list of data to write to the file, where each element of the list is a line.</param>
        /// <param name="filePath">The path and the name of the file without the extension, that will be created.</param>
        /// <param name="extension">The extension of the file that will be created.</param>
        public static void SaveCompressedFile(IEnumerable<JsonDictionary> dataList, string filePath, string extension)
        {
            var jsonDataList = dataList.Select(d => JsonSerializer.SerializeJson(d));

            var encodedLines = new List<string>();
            foreach (var line in jsonDataList)
            {
                encodedLines.Add(Convert.ToBase64String(Utils.Zip(line)));
            }
            File.WriteAllLines($"{filePath}.{extension}", encodedLines, Constants.ENCODING);
        }

        /// <summary>
        /// Same as <see cref="SaveJsonFile(JsonDictionary, string, string)"/>, but zips + base64 encodes the file.
        /// </summary>
        /// <param name="data">The data to write to the file.</param>
        /// <inheritdoc cref="SaveCompressedFile(IEnumerable{JsonDictionary}, string, string)"/>
        public static void SaveCompressedFile(JsonDictionary data, string filePath, string extension)
        {
            SaveJsonFile([data], filePath, extension);
        }

        /// <summary>
        /// Shorthand for <see cref="FileConversion.EncodeFile(IEnumerable{string}, long, string, string, int, Encoding?)"/> + convert from json to string.
        /// </summary>
        /// <param name="dataList">The list of data to write to the file, where each element of the list is a line.</param>
        /// <param name="filePath">The path and the name of the file without the extension, that will be created.<br/>
        /// If the path contains a *, it will be replaced with the seed.</param>
        /// <param name="seed">The seed for encoding the file.</param>
        /// <param name="extension">The extension of the file that will be created.</param>
        public static void EncodeFileShort(IEnumerable<JsonDictionary> dataList, string filePath, long seed, string extension)
        {
            var jsonDataList = dataList.Select(d => JsonSerializer.SerializeJson(d));
            FileConversion.EncodeFile(jsonDataList, seed, filePath, extension, Constants.FILE_ENCODING_VERSION, Constants.ENCODING);
        }

        /// <param name="data">The data to write to the file.</param>
        /// <inheritdoc cref="EncodeFileShort(IEnumerable{JsonDictionary}, string, long, string)"/>
        public static void EncodeFileShort(JsonDictionary data, string filePath, long seed, string extension)
        {
            EncodeFileShort([data], filePath, seed, extension);
        }

        /// <summary>
        /// Same as <see cref="DecodeFileShort(string, long, string, int, bool)"/>, but for plain json files.
        /// </summary>
        /// <param name="filePath">The path and the name of the file without the extension, that will be loaded.</param>
        /// <param name="lineNum">The line, that you want go get back (starting from 0).<br/>
        /// If it's null, it loads the entire file.</param>
        /// <param name="extension">The extension of the file that will be loaded.</param>
        /// <param name="expected">If the file is expected to exist.</param>
        public static JsonDictionary? LoadJsonFile(string filePath, int? lineNum = 0, string extension = "json", bool expected = true)
        {
            return DecodeSaveAny(0, filePath, null, extension, lineNum, expected);
        }

        /// <summary>
        /// Same as <see cref="LoadJsonFile(string, int, string, bool)"/>, but base64 decodes + unzips the file.
        /// </summary>
        /// <param name="filePath">The path and the name of the file without the extension, that will be loaded.</param>
        /// <param name="extension">The extension of the file that will be loaded.</param>
        /// <param name="lineNum">The line, that you want go get back (starting from 0).</param>
        /// <param name="expected">If the file is expected to exist.</param>
        public static JsonDictionary? LoadCompressedFile(string filePath, string extension, int lineNum = 0, bool expected = true)
        {
            return DecodeSaveAny(1, filePath, null, extension, lineNum, expected);
        }

        /// <summary>
        /// Shorthand for <see cref="FileConversion.DecodeFile(long, string, string, int, Encoding?)"/> + convert from string to json.
        /// </summary>
        /// <param name="filePath">The path and the name of the file without the extension, that will be decoded.<br/>
        /// If the path contains a *, it will be replaced with the seed.</param>
        /// <param name="lineNum">The line, that you want go get back (starting from 0).</param>
        /// <param name="seed">The seed for decoding the file.</param>
        /// <param name="extension">The extension of the file that will be decoded.</param>
        /// <param name="expected">If the file is expected to exist.</param>
        /// <exception cref="FormatException">Exeption thrown, if the file couldn't be decode.</exception>
        public static JsonDictionary? DecodeFileShort(string filePath, long seed, string extension, int lineNum = 0, bool expected = true)
        {
            return DecodeSaveAny(2, filePath, seed, extension, lineNum, expected);
        }
        #endregion

        #region Recreate folder
        /// <summary>
        /// Recreates the folder, if it doesn't exist.
        /// </summary>
        /// <param name="folderPath">The path of the folder to check.</param>
        /// <param name="displayName">The display name of the folder, for the logger, if it needs to be recreated.</param>
        /// <returns>If the folder needed to be recreated.</returns>
        public static bool RecreateFolder(string folderPath, string? displayName = null)
        {
            folderPath = folderPath.Contains(Path.DirectorySeparatorChar) ? folderPath : Path.Join(Constants.ROOT_FOLDER, folderPath);
            displayName ??= new DirectoryInfo(folderPath).Name.ToLower();

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
                if (e is ArgumentNullException or FormatException)
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
        public static bool TryDeserializeRandom(string? randomString, [NotNullWhen(true)] out SplittableRandom? random)
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

        #region Json
        #region FromJson
        /// <summary>
        /// Tries to convert the json representation of the object to an object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        public static bool TryFromJson<T>(JsonDictionary? objectJson, string fileVersion, [NotNullWhen(true)] out T? convertedObject)
            where T : IJsonConvertable<T>
        {
            return T.FromJson(objectJson, fileVersion, out convertedObject);
        }

        /// <summary>
        /// Converts the json representation of the object to an object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        public static T? FromJson<T>(JsonDictionary? objectJson, string fileVersion)
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
        public static T? FromJsonWithoutCorrection<T>(JsonDictionary objectJson, string fileVersion, [NotNullWhen(true)] ref T? convertedObject)
            where T : IJsonConvertable<T>
        {
            T.FromJsonWithoutCorrection(objectJson, fileVersion, ref convertedObject);
            return convertedObject;
        }

        /// <summary>
        /// Tries to do FromJson(), but without correcting the json data first.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        public static bool TryFromJsonWithoutCorrection<T>(
            JsonDictionary objectJson,
            string fileVersion,
            [NotNullWhen(true)] out T? convertedObject
        )
            where T : IJsonConvertable<T>
        {
            convertedObject = default;
            return T.FromJsonWithoutCorrection(objectJson, fileVersion, ref convertedObject);
        }

        /// <summary>
        /// Tries to convert the json representation of the object to an object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="extraData">Some extra data to help with the conversion.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        public static bool TryFromJsonExtra<T, TE>(
            JsonDictionary? objectJson,
            TE extraData,
            string fileVersion,
            [NotNullWhen(true)] out T? convertedObject
        )
            where T : IJsonConvertableExtra<T, TE>
        {
            return T.FromJson(objectJson, extraData, fileVersion, out convertedObject);
        }

        /// <summary>
        /// Converts the json representation of the object to an object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="extraData">Some extra data to help with the conversion.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        public static T? FromJsonExtra<T, TE>(JsonDictionary? objectJson, TE extraData, string fileVersion)
            where T : IJsonConvertableExtra<T, TE>
        {
            T.FromJson(objectJson, extraData, fileVersion, out T? convertedObject);
            return convertedObject;
        }

        /// <summary>
        /// FromJson(), but without correcting the json data first.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="extraData">Some extra data to help with the conversion.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        public static T? FromJsonExtraWithoutCorrection<T, TE>(
            JsonDictionary objectJson,
            TE extraData,
            string fileVersion,
            [NotNullWhen(true)] ref T? convertedObject
        )
            where T : IJsonConvertableExtra<T, TE>
        {
            T.FromJsonWithoutCorrection(objectJson, extraData, fileVersion, ref convertedObject);
            return convertedObject;
        }

        /// <summary>
        /// Tries to do FromJson(), but without correcting the json data first.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="extraData">Some extra data to help with the conversion.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        public static bool TryFromJsonExtraWithoutCorrection<T, TE>(
            JsonDictionary objectJson,
            TE extraData,
            string fileVersion,
            [NotNullWhen(true)] out T? convertedObject
        )
            where T : IJsonConvertableExtra<T, TE>
        {
            convertedObject = default;
            return T.FromJsonWithoutCorrection(objectJson, extraData, fileVersion, ref convertedObject);
        }
        #endregion

        #region Logger short
        /// <summary>
        /// Logs a json parsing error for a custom error.
        /// </summary>
        /// <param name="typeName">The name of the type, where the parsing failed.</param>
        /// <param name="message">The error message.</param>
        /// <param name="isError">If the error will result in the parsing function halting.</param>
        public static void LogJsonErrorBase(string typeName, string message, bool isError = false)
        {
            var stackTrace = GetFromJsonCallStackString();
            PACSingletons.Instance.Logger.Log(
                $"{typeName} parse {(isError ? "error" : "warning")}",
                message + (stackTrace is null ? "" : $"\n{stackTrace}"),
                isError ? LogSeverity.ERROR : LogSeverity.WARN
            );
        }

        /// <summary>
        /// Logs a json parsing error for a custom error.
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <param name="message">The error message.</param>
        /// <param name="isError">If the error will result in the parsing function halting.</param>
        public static void LogJsonError<T>(string message, bool isError = false)
        {
            LogJsonErrorBase(typeof(T).ToString(), message, isError);
        }

        /// <summary>
        /// Logs a json parsing error for a parameter parsing error.
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <param name="parameterName">The name of the parameter (or class) that caused the error.</param>
        /// <param name="extraInfo">Some extra information about the error.</param>
        /// <param name="isError">If the error will result in the parsing function halting.</param>
        public static void LogJsonParseError<T>(string parameterName, string? extraInfo = null, bool isError = false)
        {
            LogJsonError<T>($"couldn't parse {parameterName}{(extraInfo is not null ? $", {extraInfo}" : "")}", isError);
        }

        /// <summary>
        /// Logs a json parsing error for a type parsing error.
        /// </summary>
        /// <typeparam name="T">The type that is being parsed.</typeparam>
        /// <param name="extraInfo">Some extra information about the error.</param>
        /// <param name="isError">If the error will result in the parsing function halting.</param>
        public static void LogJsonTypeParseError<T>(string? extraInfo = null, bool isError = false)
        {
            LogJsonErrorBase("Json", $"couldn't parse {typeof(T)} type to json value{(extraInfo is not null ? $", {extraInfo}" : "")}", isError);
        }

        /// <summary>
        /// Logs a json parsing error for a null parameter error.
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <param name="parameterName">The name of the parameter (or class) that caused the error.</param>
        /// <param name="extraInfo">Some extra information about the error.</param>
        /// <param name="isError">If the error will result in the parsing function halting.</param>
        public static void LogJsonNullError<T>(string parameterName, string? extraInfo = null, bool isError = false)
        {
            LogJsonError<T>($"{parameterName} json is null{(extraInfo is not null ? $", {extraInfo}" : "")}", isError);
        }

        /// <summary>
        /// Returns the call stack of "FromJson()" (and json correcter) methods.
        /// </summary>
        public static List<StackFrame?> GetFromJsonCallStack()
        {
            var frames = new StackTrace(true).GetFrames();
            var fromJsonMethodName = "FromJsonWithoutCorrection";
            var correcterMethodName = "CorrectJsonDataVersionPrivate";

            return frames.Where(frame =>
            {
                var frameName = frame.GetMethod()?.Name ?? "";
                return frameName == correcterMethodName || frameName.Contains(fromJsonMethodName);
            })
            .Select(frame =>
                {
                    if ((frame.GetMethod()?.Name ?? "") != correcterMethodName)
                    {
                        return frame;
                    }

                    var frameIndex = frames.ToList().IndexOf(frame) - 2;
                    if (frameIndex >= 0)
                    {
                        return frames[frameIndex];
                    }
                    return null;
            })
            .ToList();
        }

        /// <summary>
        /// Returns the string representation of the call stack of "FromJson()" methods.
        /// </summary>
        public static string? GetFromJsonCallStackString()
        {
            var stackFrames = GetFromJsonCallStack();
            return stackFrames.Count == 0 ? null : string.Join(
                "\n",
                stackFrames.Select(frame =>
                frame is null ? "[ERROR]" : $"\tat {frame.GetMethod()?.DeclaringType?.FullName} in {frame.GetFileName()}:line {frame.GetFileLineNumber()}"
            ));
        }
        #endregion

        #region Json parse short
        /// <summary>
        /// Tries to parse a value to a json value, and logs a warning, if it can't pe parsed.<br/>
        /// You should use an implicit cast from JsonObject wherever you can instead!<br/>
        /// Usable types:<br/>
        /// - bool<br/>
        /// - string<br/>
        /// - numbers<br/>
        /// - enums<br/>
        /// - <see cref="EnumValue{TEnum}"/><br/>
        /// - <see cref="SplittableRandom"/><br/>
        /// - array of JsonObjects
        /// - dictionary of JsonObjects
        /// - any type that has a converter? and can convert from string representation to object (has [type].TryParse()?)<br/>
        /// - null
        /// </summary>
        /// <typeparam name="T">The type to parse from.</typeparam>
        public static JsonObject? ParseToJsonValue<T>(
            T value,
            bool logParseWarnings = true,
            bool isCritical = true
        )
        {
            switch (value)
            {
                case null:
                    return null;
                case int tValue:
                    return tValue;
                case uint tValue:
                    return tValue;
                case long tValue:
                    return tValue;
                case ulong tValue:
                    return tValue;
                case bool tValue:
                    return tValue;
                case string tValue:
                    return tValue;
                case char tValue:
                    return tValue;
                case double tValue:
                    return tValue;
                case float tValue:
                    return tValue;
                case DateTime tValue:
                    return tValue;
                case TimeSpan tValue:
                    return tValue;
                case Guid tValue:
                    return tValue;
                case Enum tValue:
                    return tValue;
                case EnumValueBase tValue:
                    return tValue;
                case SplittableRandom tValue:
                    return tValue;
                case List<JsonObject?> tValue:
                    return tValue;
                case IEnumerable<JsonObject?> tValue:
                    return tValue.ToList();
                case Dictionary<string, JsonObject?> tValue:
                    return tValue;
                case IDictionary<string, JsonObject?> tValue:
                    return tValue.ToDictionary();
            }

            if (logParseWarnings)
            {
                LogJsonTypeParseError<T>(isError: isCritical);
            }
            return new JsonValue(value.ToString()!);
        }

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
        /// <param name="allowNull">Whether to allow null value.</param>
        /// <returns>If the value was sucessfuly parsed.</returns>
        public static bool TryGetJsonObjectValue<T>(
            JsonDictionary objectJson,
            string jsonKey,
            out JsonObject? value,
            bool logParseWarnings = true,
            bool isCritical = false,
            bool allowNull = false
        )
        {
            if (
                objectJson.TryGetValue(jsonKey, out value) &&
                (allowNull || value is not null)
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
        /// <param name="isStraigthCast">If it's just a regular cast of the JsonObject or the value of it.</param>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(JsonDictionary, string, out JsonObject?, bool, bool, bool)"/>
        public static bool TryCastAnyValueForJsonParsing<T, TRes>(
            JsonObject? objectValue,
            [NotNullWhen(true)] out TRes? value,
            [CallerArgumentExpression(nameof(objectValue))] string? parameterName = null,
            bool isCritical = false,
            bool isStraigthCast = false
        )
        {
            value = default;
            var castableValue = isStraigthCast ? objectValue : objectValue?.Value;

            if (castableValue is TRes resultValue)
            {
                value = resultValue;
                return true;
            }

            LogJsonError<T>($"{parameterName ?? "parameter"} is not the expected type (\"{castableValue?.GetType().ToString() ?? "null"}\" instead of \"{typeof(TRes)}\")", isCritical);
            return false;
        }

        /// <summary>
        /// Tries to get a value of a specific type from a json dictionary, and logs a warning, if it doesn't exist, or it can't be cast to the expected type.
        /// </summary>
        /// <param name="isStraigthCast">If it's just a regular cast of the JsonObject or the value of it.</param>
        /// <typeparam name="TRes">The expected type of the result.</typeparam>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(JsonDictionary, string, out JsonObject?, bool, bool, bool)"/>
        /// <param name="overrideNullability">The override of whether to accept a null value as valid.<br/>
        /// Should only be used for nullable string types or similar.</param>
        public static bool TryCastJsonAnyValue<T, TRes>(
            JsonDictionary objectJson,
            string jsonKey,
            [NotNullWhen(true)] out TRes? value,
            bool isCritical = false,
            bool isStraigthCast = false,
            bool? overrideNullability = null
        )
        {
            value = default;
            var isAllowNull = isStraigthCast && (overrideNullability ?? Nullable.GetUnderlyingType(typeof(TRes)) is not null);
            if (!TryGetJsonObjectValue<T>(objectJson, jsonKey, out var result, isCritical: isCritical, allowNull: isAllowNull))
            {
                return false;
            }

            if (isAllowNull && result is null)
            {
                return true;
            }

            return TryCastAnyValueForJsonParsing<T, TRes>(result, out value, jsonKey, isCritical, isStraigthCast);
        }

        /// <summary>
        /// Tries to parse a json value to a type. If the value can't be parsed, it logs a json parse warning.<br/>
        /// Usable types:<br/>
        /// - bool<br/>
        /// - string<br/>
        /// - numbers<br/>
        /// - enums<br/>
        /// - <see cref="EnumValue{TEnum}"/><br/>
        /// - <see cref="EnumTreeValue{TEnum}"/><br/>
        /// - <see cref="SplittableRandom"/><br/>
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
            JsonObject? value,
            [NotNullWhen(true)] out TRes? parsedValue,
            [CallerArgumentExpression(nameof(value))] string? parameterName = null,
            bool logParseWarnings = true,
            string? parameterExtraInfo = null,
            bool isCritical = false
        )
        {
            parsedValue = default;
            var valueText = value?.Value.ToString();

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

            if (actualType == typeof(SplittableRandom))
            {
                if (TryDeserializeRandom(valueText, out var parsedRandom))
                {
                    parsedValue = (TRes)(object)parsedRandom;
                    return true;
                }
            }
            else if (
                actualType.BaseType == typeof(EnumValueBase) &&
                actualType.Assembly == typeof(EnumValue<>).Assembly &&
                actualType.Name == typeof(EnumValue<>).Name &&
                actualType.Namespace == typeof(EnumValue<>).Namespace &&
                actualType.GenericTypeArguments.Length > 0
            )
            {
                var enumType = actualType.GenericTypeArguments[0].BaseType;
                try
                {
                    MethodInfo getValue;
                    try
                    {
                        getValue = enumType!.GetMethod("GetValue", BindingFlags.Static | BindingFlags.Public, [typeof(string)])
                            ?? throw new ArgumentException("Getting reflection method failed!");
                    }
                    catch
                    {
                        PACSingletons.Instance.Logger.Log("Reflection method not found", "AdvancedEnum<T>.GetValue() method not found", LogSeverity.ERROR);
                        throw;
                    }
                    parsedValue = (TRes?)getValue.Invoke(null, [valueText]);
                    if (parsedValue is not null)
                    {
                        return true;
                    }
                }
                catch { }
            }
            else if (
                actualType.BaseType == typeof(EnumTreeValueBase) &&
                actualType.Assembly == typeof(EnumTreeValue<>).Assembly &&
                actualType.Name == typeof(EnumTreeValue<>).Name &&
                actualType.Namespace == typeof(EnumTreeValue<>).Namespace &&
                actualType.GenericTypeArguments.Length > 0
            )
            {
                var enumType = actualType.GenericTypeArguments[0].BaseType;
                try
                {
                    MethodInfo getValue;
                    try
                    {
                        getValue = enumType!.GetMethod("GetValue", BindingFlags.Static | BindingFlags.Public, [typeof(string)])
                            ?? throw new ArgumentException("Getting reflection method failed!");
                    }
                    catch
                    {
                        PACSingletons.Instance.Logger.Log("Reflection method not found", "AdvancedEnumTree<T>.GetValue() method not found", LogSeverity.ERROR);
                        throw;
                    }
                    parsedValue = (TRes?)getValue.Invoke(null, [valueText]);
                    if (parsedValue is not null)
                    {
                        return true;
                    }
                }
                catch { }
            }
            else
            {
                var parseSuccess = true;
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
        /// - <see cref="EnumValue{TEnum}"/><br/>
        /// - <see cref="EnumTreeValue{TEnum}"/><br/>
        /// - <see cref="SplittableRandom"/><br/>
        /// - any type that has a converter? and can convert from string representation to object (has [type].TryParse()?)<br/>
        /// - nullables (will only make the default value null instead of the default value for the type)
        /// </summary>
        /// <typeparam name="TRes">The type to parse to.</typeparam>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(JsonDictionary, string, out JsonObject?, bool, bool, bool)"/>
        public static bool TryParseJsonValue<T, TRes>(
            JsonDictionary objectJson,
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
        /// Tries to parse a value from a json dictionary, and logs a warning, if it can't pe parsed.<br/>
        /// It also accepts null values.
        /// Usable types:<br/>
        /// - bool<br/>
        /// - string<br/>
        /// - numbers<br/>
        /// - enums<br/>
        /// - <see cref="EnumValue{TEnum}"/><br/>
        /// - <see cref="EnumTreeValue{TEnum}"/><br/>
        /// - <see cref="SplittableRandom"/><br/>
        /// - any type that has a converter? and can convert from string representation to object (has [type].TryParse()?)<br/>
        /// - nullables (will only make the default value null instead of the default value for the type)
        /// </summary>
        /// <typeparam name="TRes">The type to parse to.</typeparam>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(JsonDictionary, string, out JsonObject?, bool, bool, bool)"/>
        public static bool TryParseJsonValueNullable<T, TRes>(
            JsonDictionary objectJson,
            string jsonKey,
            out TRes? value,
            bool logParseWarnings = true,
            bool isCritical = false
        )
        {
            value = default;
            if (!TryGetJsonObjectValue<T>(objectJson, jsonKey, out var result, logParseWarnings, isCritical, true))
            {
                return false;
            }

            if (result is null)
            {
                return true;
            }

            return TryParseValueForJsonParsing<T, TRes>(result, out value, jsonKey, logParseWarnings, isCritical: isCritical);
        }

        /// <summary>
        /// Tries to parse an IJsonConvertable value from a json dictionary, and logs a warning, if it can't pe parsed.
        /// </summary>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <typeparam name="TJc">The IJsonConvertable class to convert to.</typeparam>
        /// <returns>If the object was parsed without warnings.</returns>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(JsonDictionary, string, out JsonObject?, bool, bool, bool)"/>
        public static bool TryParseJsonConvertableValue<T, TJc>(
            JsonDictionary objectJson,
            string fileVersion,
            string jsonKey,
            [NotNullWhen(true)] out TJc? value,
            bool isCritical = false
        )
            where TJc : IJsonConvertable<TJc>
        {
            value = default;
            if (!TryCastJsonAnyValue<T, JsonDictionary>(objectJson, jsonKey, out var result, isCritical, true))
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
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(JsonDictionary, string, out JsonObject?, bool, bool, bool)"/>
        /// <returns>If all values where succesfuly parsed.</returns>
        public static bool TryParseListValueForJsonParsing<T, TIn, TRes>(
            IEnumerable<TIn> listValue,
            string listName,
            Func<TIn, (bool success, TRes? result)> parseFunction,
            out List<TRes> value
        )
        {
            value = [];
            var allSuccess = true;
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
                    allSuccess = false;
                }
            }
            return allSuccess;
        }

        /// <summary>
        /// Tries to parse a list value from another list, and logs a warning, if an element can't be parsed.
        /// </summary>
        /// <typeparam name="TRes">The type of the values in the result list.</typeparam>
        /// <param name="listValue">The list to parse the values from.</param>
        /// <param name="listName">The name of the list.</param>
        /// <param name="parseFunction">The function to use, to parse the elemets of the list to the correct type.<br/>
        /// If the success is false or result is null, it will not be added to the list.</param>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(JsonDictionary, string, out JsonObject?, bool, bool, bool)"/>
        /// <returns>If all values where succesfuly parsed.</returns>
        public static bool TryParseListValueForJsonParsing<T, TRes>(
            JsonArray listValue,
            string listName,
            Func<JsonObject?, (bool success, TRes? result)> parseFunction,
            out List<TRes> value
        )
        {
            return TryParseListValueForJsonParsing<T, JsonObject?, TRes>(listValue, listName, parseFunction, out value);
        }

        /// <summary>
        /// Tries to parse a list value from a json dictionary, and logs a warning, if it can't be parsed.
        /// </summary>
        /// <typeparam name="TRes">The type of the values in the result list.</typeparam>
        /// <param name="parseFunction">The function to use, to parse the elemets of the list to the correct type.<br/>
        /// If the success is false or result is null, it will not be added to the list.</param>
        /// <returns>If all values where succesfuly parsed.</returns>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(JsonDictionary, string, out JsonObject?, bool, bool, bool)"/>
        public static bool TryParseJsonListValue<T, TRes>(
            JsonDictionary objectJson,
            string jsonKey,
            Func<JsonObject?, (bool success, TRes? result)> parseFunction,
            [NotNullWhen(true)] out List<TRes>? value,
            bool isCritical = false
        )
        {
            value = null;
            if (!(
                TryCastJsonAnyValue<T, JsonArray>(objectJson, jsonKey, out var resultList, isCritical, true) &&
                resultList is not null
            ))
            {
                return false;
            }

            return TryParseListValueForJsonParsing<T, TRes>(resultList, jsonKey, parseFunction, out value);
        }
        #endregion
        #endregion

        #region Other
        /// <summary>
        /// Gets what the value will be in a KeyValidatorDelegate in a TextField.
        /// </summary>
        /// <inheritdoc cref="ConsoleUI.UIElements.TextField.KeyValidatorDelegate"/>
        public static string GetNewValueForKeyValidatorDelegate(StringBuilder currentValue, ConsoleKeyInfo? inputKey, int cursorPosition)
        {
            if (inputKey is null)
            {
                var sbText = currentValue.ToString();
                return sbText[0..cursorPosition] + sbText[(cursorPosition + 1)..];
            }
            return currentValue.ToString().Insert(cursorPosition, inputKey?.KeyChar.ToString() ?? "");
        }

        /// <summary>
        /// Returns a standard loading text with a spinner. Update the <see cref="LoadingText.Value"/>.
        /// </summary>
        /// <param name="postSpinner"><inheritdoc cref="LoadingText.PostSpinner" path="//summary"/></param>
        /// <param name="preSpinner"><inheritdoc cref="LoadingText.PreSpinner" path="//summary"/></param>
        /// <param name="postValue"><inheritdoc cref="LoadingText.PostValue" path="//summary"/></param>
        /// <param name="precision">The number of digits after the decimal.</param>
        public static LoadingText GetStandardLoadingText(
            string postSpinner,
            string preSpinner = "",
            string postValue = "",
            int precision = 1
        )
        {
            return new LoadingText(
                preSpinner,
                postSpinner,
                postValue + "\u001b[0K", 0,
                valueFormat: $"0.{new string('0', precision)}%"
            );
        }

        /// <summary>
        /// Stops the loading for the standard loading text.
        /// </summary>
        /// <param name="loadingText">The (standard) loading text</param>
        /// <param name="finalValue">The value to display after loading has finished.</param>
        public static void StopLoadingStandard(this LoadingText loadingText, string finalValue = "DONE!")
        {
            loadingText.StopLoading(finalValue);
            Console.WriteLine();
        }
        #endregion
        #endregion

        #region Private functions
        /// <summary>
        /// Loads a json line from a file.
        /// </summary>
        /// <param name="type">The type of the decoding<br/>
        /// 0 - plain json<br/>
        /// 1 - zip + base64<br/>
        /// 2 - <see cref="FileConversion"/> encoded<br/></param>
        /// <param name="filePath">The path and the name of the file without the extension, that will be decoded.</param>
        /// <param name="lineNum">The line, that you want go get back (starting from 0).<br/>
        /// If type is 0, and this is null, it reads the entire text.</param>
        /// <param name="seed">The seed for decoding the file.</param>
        /// <param name="extension">The extension of the file that will be decoded.</param>
        /// <param name="expected">If the file is expected to exist.</param>
        /// <exception cref="FormatException">Exeption thrown, if the file couldn't be decoded.</exception>
        private static JsonDictionary? DecodeSaveAny(
            ushort type,
            string filePath,
            long? seed,
            string extension,
            int? lineNum = 0,
            bool expected = true
        )
        {
            var safeFilePath = Path.GetRelativePath(Constants.ROOT_FOLDER, filePath);
            var fullFilePath = $"{filePath}.{extension}";
            if (!File.Exists(fullFilePath))
            {
                if (Directory.Exists(Path.GetDirectoryName(fullFilePath)))
                {
                    PACSingletons.Instance.Logger.Log("File not found", $"{(expected ? "" : "(but it was expected) ")}file name: {safeFilePath}.{extension}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
                }
                else
                {
                    PACSingletons.Instance.Logger.Log("Folder containing file not found", $"{(expected ? "" : "(but it was expected) ")}file name: {safeFilePath}.{extension}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
                }
                return null;
            }

            string loadedLine;
            try
            {
                if (type == 0)
                {
                    loadedLine = lineNum is not null
                        ? File.ReadLines(fullFilePath, Constants.ENCODING).ElementAt((int)lineNum)
                        : File.ReadAllText(fullFilePath, Constants.ENCODING);
                }
                else if (type == 1)
                {
                    var compressedLine = File.ReadAllLines(fullFilePath, Constants.ENCODING).ElementAt(lineNum ?? 0);
                    loadedLine = Utils.Unzip(Convert.FromBase64String(compressedLine));
                }
                else
                {
                    loadedLine = FileConversion.DecodeFile((long)seed!, filePath, extension, (lineNum ?? 0) + 1, Constants.ENCODING).Last();
                }
            }
            catch (FormatException)
            {
                PACSingletons.Instance.Logger.Log("Decode error", $"file name: {safeFilePath}.{extension}", LogSeverity.ERROR);
                throw;
            }

            try
            {
                return JsonSerializer.DeserializeJson(loadedLine);
            }
            catch (Exception)
            {
                PACSingletons.Instance.Logger.Log("Json decode error", $"file name: {safeFilePath}.{extension}", LogSeverity.ERROR);
                throw;
            }
        }
        #endregion
    }
}