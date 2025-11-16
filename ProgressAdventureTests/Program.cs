using System.Text;
using System.Text.Json.Serialization;
using PACommon;
using PACommon.ConfigManagement;
using PACommon.ConfigManagement.JsonConverters;
using PACommon.Enums;
using PACommon.JsonUtils;
using PACommon.Logging;
using PACommon.TestUtils;
using ProgressAdventure;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.Exceptions;
using ProgressAdventure.Localization;
using ProgressAdventure.SettingsManagement;
using Attribute = ProgressAdventure.Enums.Attribute;
using PACConstants = PACommon.Constants;
using PAConstants = ProgressAdventure.Constants;
using PATools = ProgressAdventure.Tools;

namespace ProgressAdventureTests
{
    internal class Program
    {
        /// <summary>
        /// The main function for the program.
        /// </summary>
        private static void MainFunction()
        {
            //PATools.LoadDefaultConfigs();
            // PATools.ReloadConfigs();
            //Tools.RunAllTests();
            //Tools.CreateNewTestSaveFromPrevious("2.5");
            TestingUtils.RunAllTests(typeof(Tests), Tools.PrepareTest, Tools.DisposeTest);

            PACSingletons.Instance.ConsoleProxy.PressKey("DONE!");
        }

        /// <summary>
        /// Function for setting up the enviorment, and initializing global variables.
        /// </summary>
        private static void Preloading()
        {
            Thread.CurrentThread.Name = PACConstants.TESTS_THREAD_NAME;

            var localizer = Localizer.Initialize(Localizer.DEFAULT_LANGUAGE, false);
            var consoleProxy = new PAConsoleProxy
            {
                Encoding = Encoding.UTF8,
                Title = "Progress Adventure tests",
            };
            consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.LOADING_0));
            consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.LOADING_COMMON_SINGLETONS_0));

            // initializing PAC singletons
            var loggingStream = new FileLoggerStream(PAConstants.LOGS_FOLDER_PATH, PAConstants.LOG_EXT);

            PACSingletons.Initialize(
                Logger.Initialize(
                    loggingStream,
                    PAConstants.LOG_MS,
                    false,
                    LogSeverity.DEBUG,
                    PAConstants.FORCE_LOG_INTERVAL,
                    false
                ),
                consoleProxy,
                JsonDataCorrecter.Initialize(
                    PAConstants.SAVE_VERSION,
                    PAConstants.ORDER_JSON_CORRECTERS,
                    new Dictionary<string, IList<Type>>
                    {
                        [PAConstants.CONFIG_FORMAT_VERSION] = [typeof(ConfigData)],
                    },
                    false
                ),
                ConfigManager.Initialize(
                    [
                        new JsonStringEnumConverter(allowIntegerValues: false),
                        new TypeConverter(),
                        new AdvancedEnumConverter<Attribute>(),
                        new AdvancedEnumConverter<Material>(),
                        new AdvancedEnumConverter<EntityType>(),
                        new AdvancedEnumConverter<LocalizationKey>(),
                        new AdvancedEnumTreeConverter<ItemType>(),
                        new MaterialItemAttributesDTOConverter(),
                        new AIngredientDTOConverter(),
                        new ConsoleKeyInfoConverter(),
                    ],
                    PAConstants.CONFIGS_FOLDER_PATH,
                    PAConstants.CONFIG_EXT,
                    false
                )
            );

            consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.ACTIVATING_ANSI_0));
            if (!PACSingletons.Instance.ConsoleProxy.TryEnableAnsiCodes())
            {
                PACSingletons.Instance.Logger.Log("Failed to enable ANSI codes for the terminal", null, LogSeverity.ERROR, forceLog: true);
            }

            // initializing PA singletons
            // special loading order to avoid unintended errors because of complicated self references
            if (Constants.PRELOAD_GLOBALS_ON_PRELOAD)
            {
                consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.LOADING_PA_SINGLETONS_0));
                SettingsUtils.LoadDefaultConfigs();
                PASingletons.Initialize(
                    new Globals(),
                    new Settings(keybinds: new Keybinds(), currentLanguage: localizer.CurrentLanguage, dontUpdateSettingsIfValueSet: true, isInitializing: true),
                    localizer
                );
            }

            if (Constants.PRELOAD_GLOBALS_ON_PRELOAD)
            {
                consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.RELOADING_CONFIGS_0));
                PATools.ReloadConfigs();
                PASingletons.Instance.Settings.Keybinds = PASingletons.Instance.Settings.GetKeybins();
            }
            
            consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.DONE_0));
            PACSingletons.Instance.Logger.Log("Finished initialization", forceLog: true);
        }

        /// <summary>
        /// The error handler, for the preloading.
        /// </summary>
        private static void PreloadingErrorHandler()
        {
            bool exitPreloading;
            do
            {
                exitPreloading = true;
                try
                {
                    Preloading();
                }
                catch (Exception e)
                {
                    if (MenuManager.HandleErrorMenu(e, true))
                    {
                        throw;
                    }
                    exitPreloading = false;
                }
            }
            while (!exitPreloading);
        }

        /// <summary>
        /// The error handler, for the main function.
        /// </summary>
        private static void MainErrorHandler()
        {
            // general crash handler (release only)

            bool exitGame;
            do
            {
                exitGame = true;
                try
                {
                    PACSingletons.Instance.Logger.Log("Beginning new instance", forceLog: true);
                    MainFunction();
                    //exit
                    PACSingletons.Instance.Logger.Log("Instance ended succesfuly", forceLog: true);
                    PACSingletons.Instance.Dispose();
                }
                catch (Exception e)
                {
                    if (MenuManager.HandleErrorMenu(e, false))
                    {
                        throw;
                    }
                    exitGame = false;
                }
            }
            while (!exitGame);
        }

        private static void Main(string[] args)
        {
            bool exitGame;
            do
            {
                Exception? restartException = null;
                exitGame = true;
                try
                {
                    PreloadingErrorHandler();
                    MainErrorHandler();
                }
                catch (RestartException re)
                {
                    restartException = re;
                }
                catch (Exception ex)
                {
                    if (!MenuManager.TryGetRestartException(ex, out var re))
                    {
                        throw;
                    }
                    
                    restartException = ex;
                }

                if (restartException is not null)
                {
                    PACSingletons.Instance.Logger.Log("Instance restart requested", restartException.ToString(), forceLog: true);
                    exitGame = false;
                }
            }
            while (!exitGame);
        }
    }
}
