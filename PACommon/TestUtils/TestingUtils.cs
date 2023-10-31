using PACommon.Enums;
using System.Reflection;
using System.Text;

namespace PACommon.TestUtils
{
    public class TestingUtils
    {
        #region Public functions
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

        #region Private functions
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
            Logger.Instance.Log(testName, result.resultType + messageText, result.resultType);
            testsRun++;
            testsSuccessful += passed ? 1 : 0;

            Thread.CurrentThread.Name = Constants.TESTS_THREAD_NAME;
        }
        #endregion
    }
}
