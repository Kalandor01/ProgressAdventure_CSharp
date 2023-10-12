﻿using PACommon;
using PACommon.Enums;
using ProgressAdventure;
using ProgressAdventure.SettingsManagement;
using PAConstants = ProgressAdventure.Constants;
using PACTools = PACommon.Tools;
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

            RunTestInternal(Tests.EntityUtilsFacingToMovementVectorDictionaryCheck);
            RunTestInternal(Tests.EntityUtilsAttributeStatsChangeDictionaryCheck);
            RunTestInternal(Tests.ItemUtilsMaterialPropertiesDictionaryCheck);
            RunTestInternal(Tests.ItemUtilsMaterialItemAttributesDictionaryCheck);
            RunTestInternal(Tests.ItemUtilsCompoundItemAttributesDictionaryCheck);
            RunTestInternal(Tests.SettingsUtilsActionTypeIgnoreMappingDictionaryCheck);
            RunTestInternal(Tests.SettingsUtilsActionTypeResponseMappingDictionaryCheck);
            RunTestInternal(Tests.SettingsUtilsSpecialKeyNameDictionaryCheck);
            RunTestInternal(Tests.SettingsUtilsSettingValueTypeMapDictionaryCheck);
            RunTestInternal(Tests.SettingsUtilsDefaultSettingsDictionaryCheck);
            RunTestInternal(Tests.WorldUtilsTileNoiseOffsetsDictionaryCheck);

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

            RunTestInternal(Tests.AllMaterialItemTypesExistAndLoadable);
            RunTestInternal(Tests.AllCompoundItemTypesExistAndLoadable);
            RunTestInternal(Tests.AllEntitiesLoadable);

            RunTestInternal(Tests.BasicJsonConvertTest);

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
        internal static void PrepareTest()
        {
            Settings.LoggingLevel = 0;

            Logger.Log("Preloading global variables");
            // GLOBAL VARIABLES
            Settings.Initialise();
            Globals.Initialise();
        }

        /// <summary>
        /// Runs a test that is part of a batch.
        /// </summary>
        /// <param name="testFunction">The test to run.</param>
        internal static void RunTestInternal(Func<TestResultDTO?> testFunction)
        {
            PACTools.RunTest(testFunction, PrepareTest, false, ref testsRun, ref testsSuccessful);
        }
        #endregion
    }
}
