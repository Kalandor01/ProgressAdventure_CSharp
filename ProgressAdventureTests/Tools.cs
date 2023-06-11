using ProgressAdventure;
using ProgressAdventure.Enums;
using ProgressAdventure.SettingsManagement;
using System.Text;
using Logger = ProgressAdventure.Logger;

namespace ProgressAdventureTests
{
    /// <summary>
    /// Contains project specific useful functions.
    /// </summary>
    public static class Tools
    {
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
        /// Evaluates the results of a test.
        /// </summary>
        /// <param name="testName">The name of the test.</param>
        /// <param name="resultType">The result type of the test.</param>
        /// <param name="exeption">The error/thrown exeption, if the test failed.</param>
        private static void EvaluateResult(string testName, LogSeverity resultType, string? exeption = null)
        {
            Console.WriteLine(resultType);
            Logger.Log(testName, resultType + (resultType == LogSeverity.PASS ? "" : ": " + exeption), resultType);
            Thread.CurrentThread.Name = Constants.TESTS_THREAD_NAME;
        }

        /// <summary>
        /// Runs an individual test.
        /// </summary>
        /// <param name="testFunction">The test to run.</param>
        /// <param name="newLine">Whether to write a new line in the logs.</param>
        public static void RunTest(Delegate testFunction, bool newLine = true)
        {
            var testName = PrepareTest(testFunction, newLine);

            (LogSeverity resultType, string? exeption) result;
            try
            {
                result = ((LogSeverity resultType, string? exeption)?)testFunction.DynamicInvoke() ?? (LogSeverity.PASS, null);
            }
            catch (Exception ex)
            {
                EvaluateResult(testName, LogSeverity.FATAL, ex.ToString());
                return;
            }
            EvaluateResult(testName, result.resultType, result.exeption);
        }

        /// <summary>
        /// Runs all tests.
        /// </summary>
        public static void RunAllTests()
        {
            Logger.LogNewLine();
            Logger.Log("Runing all tests");
            RunTest(Tests.AllItemTypesExistAndLoadable, false);
            RunTest(Tests.AllEntitiesLoadable, false);
        }
    }
}
