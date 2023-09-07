using PACommon;
using PACommon.Enums;
using ProgressAdventure;
using ProgressAdventure.SettingsManagement;
using System.Reflection;
using System.Text;
using PAConstants = ProgressAdventure.Constants;
using Utils = PACommon.Utils;

namespace ProgressAdventureTests
{
    /// <summary>
    /// Contains project specific useful functions.
    /// </summary>
    public static class Tools
    {
        #region Private fields
        private static int testsRun = 0;
        private static int testsSuccessful = 0;
        #endregion

        #region Public functions
        /// <summary>
        /// Runs an individual test.
        /// </summary>
        /// <param name="testFunction">The test to run.</param>
        /// <param name="newLine">Whether to write a new line in the logs.</param>
        public static void RunTest(Delegate testFunction, bool newLine = true)
        {
            var testName = PrepareTest(testFunction, newLine);

            TestResultDTO result;
            try
            {
                result = (TestResultDTO?)testFunction.DynamicInvoke() ?? new TestResultDTO();
            }
            catch (Exception ex)
            {
                EvaluateResult(testName, new TestResultDTO(LogSeverity.FATAL, ex.ToString()));
                return;
            }
            EvaluateResult(testName, result);
        }

        /// <summary>
        /// Runs all dictionary tests.
        /// </summary>
        /// <param name="newLine">Whether to write a new line in the logs.</param>
        public static void RunAllDictionaryTests(bool newLine = true, bool newBatch = true)
        {
            if (newBatch)
            {
                ResetTestCounters();
            }

            if (newLine)
            {
                Logger.LogNewLine();
            }
            Logger.Log("Runing all dictionary tests");

            RunTest(Tests.LoggerLoggingValuesDictionaryCheck, false);
            RunTest(Tests.EntityUtilsFacingToMovementVectorDictionaryCheck, false);
            RunTest(Tests.EntityUtilsAttributeStatsChangeDictionaryCheck, false);
            RunTest(Tests.ItemUtilsMaterialPropertiesDictionaryCheck, false);
            RunTest(Tests.ItemUtilsMaterialItemAttributesDictionaryCheck, false);
            RunTest(Tests.ItemUtilsCompoundItemAttributesDictionaryCheck, false);
            RunTest(Tests.SettingsUtilsActionTypeIgnoreMappingDictionaryCheck, false);
            RunTest(Tests.SettingsUtilsActionTypeResponseMappingDictionaryCheck, false);
            RunTest(Tests.SettingsUtilsSpecialKeyNameDictionaryCheck, false);
            RunTest(Tests.WorldUtilsTileNoiseOffsetsDictionaryCheck, false);

            Logger.Log("All dictionary tests finished runing");

            if (newBatch)
            {
                PrintSummary();
            }
        }

        /// <summary>
        /// Runs all tests.
        /// </summary>
        public static void RunAllTests()
        {
            Logger.LogNewLine();
            ResetTestCounters();
            Logger.Log("Runing all tests");

            RunAllDictionaryTests(false, false);

            RunTest(Tests.AllMaterialItemTypesExistAndLoadable, false);
            RunTest(Tests.AllCompoundItemTypesExistAndLoadable, false);
            RunTest(Tests.AllEntitiesLoadable, false);

            RunTest(Tests.BasicJsonConvertTest, false);

            Logger.Log("All tests finished runing");
            PrintSummary();
            ResetTestCounters();
        }

        /// <summary>
        /// Resets the counters, used to count test statistics.
        /// </summary>
        public static void ResetTestCounters()
        {
            testsRun = 0;
            testsSuccessful = 0;
        }

        /// <summary>
        /// Prints out the summary of a test batch, based on the test counters.
        /// </summary>
        public static void PrintSummary()
        {
            var allPassed = testsSuccessful == testsRun;
            var result = Utils.StylizedText($"{testsSuccessful}/{testsRun}", allPassed ? PAConstants.Colors.GREEN : PAConstants.Colors.RED);
            Console.WriteLine($"\nFinished running test batch: {result} successful!");
            Logger.Log("Finished running test batch", $"{testsSuccessful}/{testsRun} successful", allPassed ? LogSeverity.PASS : LogSeverity.FAIL);
        }
        #endregion

        #region Internal functions
        /// <summary>
        /// Prepares the test enviorment.
        /// </summary>
        /// <param name="testFunction">The test to run.</param>
        /// <param name="newLine">Whether to write a new line in the logs.</param>
        internal static string PrepareTest(Delegate testFunction, bool newLine = true)
        {
            var testName = testFunction.Method.Name;

            Settings.LoggingLevel = 0;
            Console.OutputEncoding = Encoding.UTF8;
            Thread.CurrentThread.Name = $"{Constants.TESTS_THREAD_NAME}/{testName}";

            if (newLine)
            {
                Logger.LogNewLine();
            }

            Logger.Log("Preloading global variables");
            // GLOBAL VARIABLES
            Settings.Initialise();
            Globals.Initialise();

            Console.Write(testName + "...");
            Logger.Log("Running...");
            return testName;
        }

        /// <summary>
        /// Gets an internal field from a non-static class.
        /// </summary>
        /// <typeparam name="T">The type of the internal field.</typeparam>
        /// <typeparam name="TClass">The type of the class to get the field from.</typeparam>
        /// <param name="fieldName">The name of the internal field.</param>
        /// <param name="instance">The instance to get the field from.</param>
        /// <exception cref="ArgumentNullException">Thrown if the field is null.</exception>
        internal static T GetInternalFieldFromNonStaticClass<T, TClass>(TClass instance, string fieldName)
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
        internal static T GetInternalFieldFromStaticClass<T>(Type classType, string fieldName)
        {
            return GetInternalFieldFromClass<T>(classType, fieldName);
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Evaluates the results of a test.
        /// </summary>
        /// <param name="testName">The name of the test.</param>
        /// <param name="result">The result of the test.</param>
        private static void EvaluateResult(string testName, TestResultDTO result)
        {
            var passed = result.resultType == LogSeverity.PASS;
            var typeText = Utils.StylizedText(result.resultType.ToString(), passed ? PAConstants.Colors.GREEN : PAConstants.Colors.RED);
            var messageText = result.resultMessage is null ? "" : ": " + result.resultMessage;

            Console.WriteLine(typeText + messageText);
            Logger.Log(testName, result.resultType + (messageText), result.resultType);
            testsRun++;
            testsSuccessful += passed ? 1 : 0;

            Thread.CurrentThread.Name = Constants.TESTS_THREAD_NAME;
        }

        /// <summary>
        /// Gets an internal field from a class.
        /// </summary>
        /// <typeparam name="T">The type of the internal field.</typeparam>
        /// <param name="fieldName">The name of the internal field.</param>
        /// <param name="instance">The instance to get the field from. If null, it assumes, that the class in a static class.</param>
        /// <exception cref="ArgumentNullException">Thrown if the field is null.</exception>
        private static T GetInternalFieldFromClass<T>(Type classType, string fieldName, object? instance = null)
        {
            var field = classType?.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(instance);
            return field is null ? throw new ArgumentNullException("The internal filed is null.") : (T)field;
        }
        #endregion
    }
}
