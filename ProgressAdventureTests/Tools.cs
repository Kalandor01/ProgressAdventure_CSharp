using PACommon;
using PACommon.Enums;
using PACommon.SettingsManagement;
using PACommon.TestUtils;
using ProgressAdventure;
using ProgressAdventure.SettingsManagement;
using ProgressAdventure.WorldManagement;
using System.IO.Compression;
using PAConstants = ProgressAdventure.Constants;
using PATools = ProgressAdventure.Tools;
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
                PACSingletons.Instance.Logger.LogNewLine();
            }
            PACSingletons.Instance.Logger.Log("Runing all dictionary tests");

            RunTestInternal(Tests.EntityUtilsFacingToMovementVectorDictionaryCheck);
            RunTestInternal(Tests.EntityUtilsAttributeStatsChangeDictionaryCheck);
            RunTestInternal(Tests.ItemUtilsMaterialItemAttributesDictionaryCheck);
            RunTestInternal(Tests.ItemUtilsCompoundItemAttributesDictionaryCheck);
            RunTestInternal(Tests.SettingsUtilsActionTypeAttributesDictionaryCheck);
            RunTestInternal(Tests.SettingsUtilsSpecialKeyNameDictionaryCheck);
            RunTestInternal(Tests.SettingsUtilsSettingValueTypeMapDictionaryCheck);
            RunTestInternal(Tests.SettingsUtilsDefaultSettingsDictionaryCheck);
            RunTestInternal(Tests.WorldUtilsTileNoiseOffsetsDictionaryCheck);

            PACSingletons.Instance.Logger.Log("All dictionary tests finished runing");

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
            PACSingletons.Instance.Logger.LogNewLine();
            ResetTestCounters();
            PACSingletons.Instance.Logger.Log("Runing all tests");

            RunAllDictionaryTests(false, false);

            RunTestInternal(Tests.AllMaterialItemTypesExistAndLoadable);
            RunTestInternal(Tests.AllCompoundItemTypesExistAndLoadable);
            RunTestInternal(Tests.AllEntitiesLoadable);

            RunTestInternal(Tests.BasicJsonConvertTest);

            PACSingletons.Instance.Logger.Log("All tests finished runing");
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
            PACSingletons.Instance.Logger.Log("Finished running test batch", $"{testsSuccessful}/{testsRun} successful", allPassed ? LogSeverity.PASS : LogSeverity.FAIL);
        }
        #endregion

        #region Internal functions
        /// <summary>
        /// Prepares the test enviorment.
        /// </summary>
        internal static void PrepareTest()
        {
            SettingsUtils.LoadDefaultConfigs();
            PASingletons.Initialize(
                new Globals(),
                new Settings(keybinds: new Keybinds(), dontUpdateSettingsIfValueSet: true)
            );
            PASingletons.Instance.Settings.LoggingLevel = LogSeverity.WARN;
            KeybindUtils.colorEnabled = PASingletons.Instance.Settings.EnableColoredText;

            PATools.ReloadConfigs();
            PASingletons.Instance.Settings.Keybinds = Settings.GetKeybins();
        }

        /// <summary>
        /// Disposes the test enviorment.
        /// </summary>
        internal static void DisposeTest()
        {
            PASingletons.Instance.Dispose();
            KeybindUtils.colorEnabled = true;
            PASingletons.Instance.Settings.LoggingLevel = LogSeverity.DEBUG;
        }

        /// <summary>
        /// Runs a test that is part of a batch.
        /// </summary>
        /// <param name="testFunction">The test to run.</param>
        internal static void RunTestInternal(Func<TestResultDTO?> testFunction)
        {
            TestingUtils.RunTest(testFunction, PrepareTest, DisposeTest, false, ref testsRun, ref testsSuccessful);
        }

        /// <summary>
        /// Gets a test save backup, loads it, saves it and creates a new test save from it, depending on the current save version.
        /// </summary>
        /// <param name="saveVersion">The save version/name of the save, to update.</param>
        internal static void CreateNewTestSaveFromPrevious(string saveVersion)
        {
            PATools.DeleteSave(saveVersion);
            ZipFile.ExtractToDirectory(Path.Join(Constants.TEST_REFERENCE_SAVES_FOLDER_PATH, $"{saveVersion}.{PAConstants.BACKUP_EXT}"), PATools.GetSaveFolderPath(saveVersion));
            SaveManager.LoadSave(saveVersion, false, false);
            World.LoadAllChunksFromFolder(null, "Loading...");
            SaveManager.MakeSave(showProgressText: "Saving...");
            var testBackupFilePath = Path.Join(Constants.TEST_REFERENCE_SAVES_FOLDER_PATH, $"{PAConstants.SAVE_VERSION}.{PAConstants.BACKUP_EXT}");
            File.Delete(testBackupFilePath);
            ZipFile.CreateFromDirectory(PATools.GetSaveFolderPath(saveVersion), testBackupFilePath);
            PATools.DeleteSave(saveVersion);
        }
        #endregion
    }
}
