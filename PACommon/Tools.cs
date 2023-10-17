using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPrng;
using NPrng.Generators;
using NPrng.Serializers;
using PACommon.Enums;
using PACommon.Extensions;
using SaveFileManager;
using System.Collections;
using System.Reflection;
using System.Text;

namespace PACommon
{
    /// <summary>
    /// Contains project specific useful functions.
    /// </summary>
    public static class Tools
    {
        #region "Constants"
        /// <summary>
        /// The current save version.
        /// </summary>
        public static string SAVE_VERSION = "2.2";
        #endregion

        #region Public functions
        #region Short
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
            // convert from json to string
            var JsonDataList = new List<string>();
            foreach (var data in dataList)
            {
                JsonDataList.Add(JsonConvert.SerializeObject(data));
            }
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
                Logger.Instance.Log("Decode error", $"file name: {safeFilePath}.{extension}", LogSeverity.ERROR);
                throw;
            }
            catch (FileNotFoundException)
            {
                Logger.Instance.Log("File not found", $"{(expected ? "" : "(but it was expected) ")}file name: {safeFilePath}.{extension}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
                throw;
            }
            catch (DirectoryNotFoundException)
            {
                Logger.Instance.Log("Folder containing file not found", $"{(expected ? "" : "(but it was expected) ")}file name: {safeFilePath}.{extension}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
                throw;
            }
            try
            {
                return DeserializeJson(decodedLine);
            }
            catch (Exception)
            {
                Logger.Instance.Log("Json decode error", $"file name: {safeFilePath}.{extension}", LogSeverity.ERROR);
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
                Logger.Instance.Log($"Recreating {displayName} folder");
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Deserialize data
        /// <summary>
        /// Turns the json string into a dictionary.
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        public static Dictionary<string, object?>? DeserializeJson(string jsonString)
        {
            var partialDict = JsonConvert.DeserializeObject<Dictionary<string, object?>>(jsonString);
            return partialDict is not null ? DeserializePartialJTokenDict(partialDict) : null;
        }

        /// <summary>
        /// Returns the value of the JToken
        /// </summary>
        /// <param name="token">The JToken to deserialize.</param>
        public static object? DeserializeJToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.None:
                    return null;
                case JTokenType.Object:
                    var partialDict = token.ToObject<Dictionary<string, object?>>();
                    return partialDict is not null ? DeserializePartialJTokenDict(partialDict) : null;
                case JTokenType.Array:
                    var partialList = token.ToObject<List<object?>>();
                    return partialList is not null ? DeserializePartialJTokenList(partialList) : null;
                case JTokenType.Constructor:
                    return ((JConstructor)token).ToString();
                case JTokenType.Property:
                    var prop = (JProperty)token;
                    return DeserializeJToken(prop.Value);
                case JTokenType.Comment:
                    return token.ToString();
                case JTokenType.Integer:
                    return (long)token;
                case JTokenType.Float:
                    return (double)token;
                case JTokenType.String:
                    return token.ToString();
                case JTokenType.Boolean:
                    return (bool)token;
                case JTokenType.Null:
                    return null;
                case JTokenType.Undefined:
                    Logger.Instance.Log("Undefined JToken value", token.ToString(), LogSeverity.WARN);
                    return null;
                case JTokenType.Date:
                    return (DateTime)token;
                case JTokenType.Raw:
                    return token.ToString();
                case JTokenType.Bytes:
                    return (byte)token;
                case JTokenType.Guid:
                    return (Guid)token;
                case JTokenType.Uri:
                    return token.ToString();
                case JTokenType.TimeSpan:
                    return (TimeSpan)token;
                default:
                    return token;
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
                Logger.Instance.Log("Random parse error", "random seed is null", LogSeverity.WARN);
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
                    Logger.Instance.Log("Random parse error", "cannot parse random generator from seed string", LogSeverity.WARN);
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

        #region String corrector
        /// <summary>
        /// A function to return the corrected verson of a string, that the user inputed.
        /// </summary>
        /// <param name="rawText">The raw user input to correct.</param>
        public delegate string StringCorrectorDelegate(string? rawText);

        /// <summary>
        /// Temporary field for the string corrector function.
        /// </summary>
        private static StringCorrectorDelegate _stringCorrectorFunction;
        /// <summary>
        /// Temporary field for the string corrector <c>TextField</c>.
        /// </summary>
        private static TextField _correctorTextField;

        /// <summary>
        /// Displays a <c>TextField</c>, where the user can input a string, that will be corrected as the user types it.<br/>
        /// NOT THREAD SAFE!!!
        /// </summary>
        /// <param name="preValue">The text to display before the part, where the user inputs the string.</param>
        /// <param name="stringCorrector"></param>
        /// <param name="postValue">The text displayed immediately after the user's inputed text.</param>
        /// <param name="startingValue">The starting value of the input field.</param>
        /// <param name="clearScreen">Whether to "clear" the screen, before displaying the text, or not.</param>
        /// <param name="keybindList">The keybind list to use.</param>
        /// <returns>The uncorrected version of the final string.</returns>
        public static string GetRealTimeCorrectedString(string preValue, StringCorrectorDelegate stringCorrector, string postValue = "", string startingValue = "", bool clearScreen = true, IEnumerable<KeyAction>? keybindList = null)
        {
            _stringCorrectorFunction = stringCorrector;
            _correctorTextField = new TextField(
                "",
                preValue,
                postValue + " -> " + _stringCorrectorFunction(startingValue),
                oldValueAsStartingValue: true,
                keyValidatorFunction: new TextField.KeyValidatorDelegate(StringCorrectorKeyValidator)
            );
            new BaseUIDisplay(_correctorTextField, autoEnter: true, clearScreen: clearScreen).Display(keybindList);
            Console.WriteLine();
            return _correctorTextField.Value;
        }

        private static bool StringCorrectorKeyValidator(StringBuilder text, ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Enter ||
                key.KeyChar == '\0' ||
                key.Key == ConsoleKey.Escape
            )
            {
                return true;
            }

            string newText = "";
            if (key.Key == ConsoleKey.Backspace)
            {
                if (text.Length > 0)
                {
                    newText = text.ToString()[0..(text.Length - 1)];
                }
            }
            else
            {
                newText = text.ToString() + key.KeyChar;
            }

            _correctorTextField.PostValue = " -> " + _stringCorrectorFunction(newText);
            return true;
        }
        #endregion

        #region Tests
        /// <summary>
        /// Runs an individual test.
        /// </summary>
        /// <param name="testFunction">The test to run.</param>
        /// <param name="prepareTestFunction">The function to run before running a test.</param>
        /// <param name="newLine">Whether to write a new line in the logs.</param>
        /// <param name="testsRun">The amount of tests that have run so far.</param>
        /// <param name="testsSuccessful">The amount of tests that were successfull so far.</param>
        public static void RunTest(Func<TestResultDTO?> testFunction, Action? prepareTestFunction, bool newLine, ref int testsRun, ref int testsSuccessful)
        {
            if (newLine)
            {
                Logger.Instance.LogNewLine();
            }

            var testName = testFunction.Method.Name;

            Console.OutputEncoding = Encoding.UTF8;
            Thread.CurrentThread.Name = $"{Constants.TESTS_THREAD_NAME}/{testName}";

            prepareTestFunction?.Invoke();

            Console.Write(testName + "...");
            Logger.Instance.Log("Running...");

            TestResultDTO result;
            try
            {
                result = testFunction.Invoke() ?? new TestResultDTO();
            }
            catch (Exception ex)
            {
                EvaluateResult(testName, new TestResultDTO(LogSeverity.FATAL, ex.ToString()), ref testsRun, ref testsSuccessful);
                return;
            }
            EvaluateResult(testName, result, ref testsRun, ref testsSuccessful);
        }

        /// <summary>
        /// Runs an individual test.
        /// </summary>
        /// <param name="testFunction">The test to run.</param>
        /// <param name="prepareTestFunction">The function to run before running a test.</param>
        /// <param name="newLine">Whether to write a new line in the logs.</param>
        public static void RunTest(Func<TestResultDTO?> testFunction, Action? prepareTestFunction, bool newLine = true)
        {
            int _ = 0;
            RunTest(testFunction, prepareTestFunction, newLine, ref _, ref _);
        }

        /// <summary>
        /// Runs all test functions from a class
        /// </summary>
        /// <param name="staticClass"></param>
        /// <param name="prepareTestFunction">The function to run before running a test.</param>
        public static void RunAllTests(Type staticClass, Action? prepareTestFunction = null)
        {
            if (
                !staticClass.IsAbstract ||
                !staticClass.IsSealed
            )
            {
                Logger.Instance.Log($"\"{staticClass}\" is not static", null, LogSeverity.ERROR);
                return;
            }

            var testsRun = 0;
            var testsSuccessful = 0;

            Logger.Instance.LogNewLine();
            Logger.Instance.Log($"Runing all tests from \"{staticClass}\"");

            var methods = staticClass.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (
                    method.ReturnType == typeof(TestResultDTO?) &&
                    method.GetParameters().Length == 0
                )
                {
                    RunTest(method.CreateDelegate<Func<TestResultDTO?>>(), prepareTestFunction, false, ref testsRun, ref testsSuccessful);
                }
            }

            Logger.Instance.Log("All tests finished runing");
            var allPassed = testsSuccessful == testsRun;
            var result = Utils.StylizedText($"{testsSuccessful}/{testsRun}", allPassed ? Constants.Colors.GREEN : Constants.Colors.RED);
            Console.WriteLine($"\nFinished running test batch: {result} successful!");
            Logger.Instance.Log("Finished running test batch", $"{testsSuccessful}/{testsRun} successful", allPassed ? LogSeverity.PASS : LogSeverity.FAIL);
        }
        #endregion

        #region Misc
        /// <summary>
        /// Searches for all public static fields in a (static) class, and all of its nested (public static) classes, and returns their values.
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
        /// Tries to convert the json representation of the object to an object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        public static bool TryFromJson<T>(IDictionary<string, object?>? objectJson, string fileVersion, out T? convertedObject)
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
        /// Corrects the json data to a specific version.
        /// </summary>
        /// <param name="objectName">The name of the object to convert.</param>
        /// <param name="objectJsonCorrecter">The correcter function.</param>
        /// <param name="objectJson">The json data to correct.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="newFileVersion">The version number, this function will correct the json data to.</param>
        public static void CorrectJsonDataVersion(string objectName, Action<IDictionary<string, object?>> objectJsonCorrecter, ref IDictionary<string, object?> objectJson, ref string fileVersion, string newFileVersion)
        {
            if (!Utils.IsUpToDate(newFileVersion, fileVersion))
            {
                objectJsonCorrecter(objectJson);

                Logger.Instance.Log($"Corrected {objectName} json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                fileVersion = newFileVersion;
            }
        }

        /// <summary>
        /// Tries to correct the json data of the object, if it's out of date.
        /// </summary>
        /// <param name="objectName">The name of the object to convert.</param>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="correcters">The list of function to use, to correct the old json data.</param>
        public static void CorrectJsonData(
            string objectName,
            ref IDictionary<string, object?> objectJson,
            List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion
        )
        {
            if (!correcters.Any() || Utils.IsUpToDate(SAVE_VERSION, fileVersion) || Utils.IsUpToDate(correcters.Last().newFileVersion, fileVersion))
            {
                return;
            }

            Logger.Instance.Log($"{objectName} json data is old", "correcting data");
            foreach (var (objectJsonCorrecter, newFileVersion) in correcters)
            {
                CorrectJsonDataVersion(objectName, objectJsonCorrecter, ref objectJson, ref fileVersion, newFileVersion);
            }
            Logger.Instance.Log($"{objectName} json data corrected");
        }

        /// <summary>
        /// Tries to correct the json data of the object, if it's out of date.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="correcters">The list of function to use, to correct the old json data.</param>
        public static void CorrectJsonData<T>(
            ref IDictionary<string, object?> objectJson,
            List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion
        )
        {
            CorrectJsonData(typeof(T).ToString(), ref objectJson, correcters, fileVersion);
        }
        #endregion
        #endregion

        #region Private functions
        /// <summary>
        /// Turns the JTokens in a dictionary into the value of the JToken.
        /// </summary>
        /// <param name="partialJsonDict">The dictionary containing values, including JTokens.</param>
        private static Dictionary<string, object?> DeserializePartialJTokenDict(IDictionary<string, object?> partialJsonDict)
        {
            var jsonDict = new Dictionary<string, object?>();
            foreach (var kvPair in partialJsonDict)
            {
                object? kvValue;
                if (
                    kvPair.Value is not null &&
                    typeof(JToken).IsAssignableFrom(kvPair.Value.GetType())
                )
                {
                    kvValue = DeserializeJToken((JToken)kvPair.Value);
                }
                else
                {
                    kvValue = kvPair.Value;
                }
                jsonDict.Add(kvPair.Key, kvValue);
            }
            return jsonDict;
        }

        /// <summary>
        /// Turns the JTokens in a list into the value of the JToken. 
        /// </summary>
        /// <param name="partialJsonList">The list containing values, including JTokens.</param>
        private static List<object?> DeserializePartialJTokenList(IEnumerable<object?> partialJsonList)
        {
            var jsonList = new List<object?>();
            foreach (var element in partialJsonList)
            {
                object? value;
                if (
                    element is not null &&
                    typeof(JToken).IsAssignableFrom(element.GetType())
                )
                {
                    value = DeserializeJToken((JToken)element);
                }
                else
                {
                    value = element;
                }
                jsonList.Add(value);
            }
            return jsonList;
        }

        /// <summary>
        /// Evaluates the results of a test.
        /// </summary>
        /// <param name="testName">The name of the test.</param>
        /// <param name="result">The result of the test.</param>
        /// <param name="testsRun">The amount of tests that have run so far.</param>
        /// <param name="testsSuccessful">The amount of tests that were successfull so far.</param>
        private static void EvaluateResult(string testName, TestResultDTO result, ref int testsRun, ref int testsSuccessful)
        {
            var passed = result.resultType == LogSeverity.PASS;
            var typeText = Utils.StylizedText(result.resultType.ToString(), passed ? Constants.Colors.GREEN : Constants.Colors.RED);
            var messageText = result.resultMessage is null ? "" : ": " + result.resultMessage;

            Console.WriteLine(typeText + messageText);
            Logger.Instance.Log(testName, result.resultType + (messageText), result.resultType);
            testsRun++;
            testsSuccessful += passed ? 1 : 0;

            Thread.CurrentThread.Name = Constants.TESTS_THREAD_NAME;
        }
        #endregion
    }
}