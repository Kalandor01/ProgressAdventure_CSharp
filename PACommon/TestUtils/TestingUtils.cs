using PACommon.Enums;
using System.Reflection;
using System.Text;

namespace PACommon.TestUtils
{
    public static class TestingUtils
    {
        #region Public functions
        /// <summary>
        /// Runs an individual test.
        /// </summary>
        /// <param name="testFunction">The test to run.</param>
        /// <param name="prepareTestFunction">The function to run before running a test.</param>
        /// <param name="disposeTestFunction">The function to run after running a test.</param>
        /// <param name="newLine">Whether to write a new line in the logs.</param>
        /// <param name="testsRun">The amount of tests that have run so far.</param>
        /// <param name="testsSuccessful">The amount of tests that were successfull so far.</param>
        public static void RunTest(
            Func<TestResultDTO?> testFunction,
            Action? prepareTestFunction,
            Action? disposeTestFunction,
            bool newLine,
            ref int testsRun,
            ref int testsSuccessful
        )
        {
            if (newLine)
            {
                PACSingletons.Instance.Logger.LogNewLine();
            }

            var testName = testFunction.Method.Name;

            Console.OutputEncoding = Encoding.UTF8;
            Thread.CurrentThread.Name = $"{Constants.TESTS_THREAD_NAME}/{testName}";

            prepareTestFunction?.Invoke();

            Console.Write(testName + "...");
            PACSingletons.Instance.Logger.Log("Running...");

            TestResultDTO result;
            try
            {
                result = testFunction.Invoke() ?? new TestResultDTO();
            }
            catch (Exception ex)
            {
                result = new TestResultDTO(LogSeverity.FATAL, ex.ToString());
            }
            finally
            {
                disposeTestFunction?.Invoke();
            }

            EvaluateResult(testName, result, ref testsRun, ref testsSuccessful);
        }

        /// <inheritdoc cref="RunTest(Func{TestResultDTO?}, Action?, Action?, bool, ref int, ref int)"/>
        public static void RunTest(
            Func<TestResultDTO?> testFunction,
            Action? prepareTestFunction,
            Action? disposeTestFunction,
            bool newLine = true
        )
        {
            int _ = 0;
            RunTest(testFunction, prepareTestFunction, disposeTestFunction, newLine, ref _, ref _);
        }

        /// <summary>
        /// Runs all test functions from a class
        /// </summary>
        /// <param name="staticClass"></param>
        /// <inheritdoc cref="RunTest(Func{TestResultDTO?}, Action?, Action?, bool)"/>
        public static void RunAllTests(Type staticClass, Action? prepareTestFunction = null, Action? disposeTestFunction = null)
        {
            if (
                !staticClass.IsAbstract ||
                !staticClass.IsSealed
            )
            {
                PACSingletons.Instance.Logger.Log($"\"{staticClass}\" is not static", null, LogSeverity.ERROR);
                return;
            }

            var testsRun = 0;
            var testsSuccessful = 0;

            PACSingletons.Instance.Logger.LogNewLine();
            PACSingletons.Instance.Logger.Log($"Runing all tests from \"{staticClass}\"");

            var methods = staticClass.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (
                    method.ReturnType == typeof(TestResultDTO?) &&
                    method.GetParameters().Length == 0
                )
                {
                    RunTest(method.CreateDelegate<Func<TestResultDTO?>>(), prepareTestFunction, disposeTestFunction, false, ref testsRun, ref testsSuccessful);
                }
            }

            PACSingletons.Instance.Logger.Log("All tests finished runing");
            var allPassed = testsSuccessful == testsRun;
            var result = Utils.StylizedText($"{testsSuccessful}/{testsRun}", allPassed ? Constants.Colors.GREEN : Constants.Colors.RED);
            Console.WriteLine($"\nFinished running test batch: {result} successful!");
            PACSingletons.Instance.Logger.Log("Finished running test batch", $"{testsSuccessful}/{testsRun} successful", allPassed ? LogSeverity.PASS : LogSeverity.FAIL);
        }

        /// <summary>
        /// Returns the string representation of the result of a test.
        /// </summary>
        /// <param name="result">The result of the test.</param>
        public static string GetResultString(TestResultDTO result)
        {
            var passed = result.resultType == LogSeverity.PASS;
            var typeText = Utils.StylizedText(result.resultType.ToString(), passed ? Constants.Colors.GREEN : Constants.Colors.RED);
            var messageText = result.resultMessage is null ? "" : ": " + result.resultMessage;
            return typeText + messageText;
        }

        /// <summary>
        /// Evaluates the results of a test.
        /// </summary>
        /// <param name="testName">The name of the test.</param>
        /// <param name="result">The result of the test.</param>
        /// <param name="testsRun">The amount of tests that have run so far.</param>
        /// <param name="testsSuccessful">The amount of tests that were successfull so far.</param>
        public static void EvaluateResult(string testName, TestResultDTO result, ref int testsRun, ref int testsSuccessful)
        {
            Console.WriteLine(GetResultString(result));

            var passed = result.resultType == LogSeverity.PASS;
            var messageText = result.resultMessage is null ? "" : ": " + result.resultMessage;
            PACSingletons.Instance.Logger.Log(testName, result.resultType + messageText, result.resultType);
            testsRun++;
            testsSuccessful += passed ? 1 : 0;

            Thread.CurrentThread.Name = Constants.TESTS_THREAD_NAME;
        }
        #endregion
    }
}
