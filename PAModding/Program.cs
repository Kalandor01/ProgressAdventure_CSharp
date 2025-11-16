using System.Text;
using System.Text.Json.Serialization;
using PACommon;
using PACommon.ConfigManagement;
using PACommon.ConfigManagement.JsonConverters;
using PACommon.Enums;
using PACommon.JsonUtils;
using PACommon.Logging;
using ProgressAdventure;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.Exceptions;
using ProgressAdventure.Localization;
using ProgressAdventure.SettingsManagement;
using Attribute = ProgressAdventure.Enums.Attribute;
using Constants = ProgressAdventure.Constants;
using Tools = ProgressAdventure.Tools;

namespace PAModding
{
    internal class Program
    {
        /// <summary>
        /// The main function for the program.
        /// </summary>
        private static void MainFunction()
        {
            MenuManager.MainMenu();

            //EntityUtils.RandomFight(2, 100, 20, includePlayer: false);
        }

        /// <summary>
        /// Function for setting up the enviorment, and initializing global variables.
        /// </summary>
        private static void Preloading()
        {
            Thread.CurrentThread.Name = Constants.MAIN_THREAD_NAME;

            var localizer = Localizer.Initialize(Localizer.DEFAULT_LANGUAGE, false);
            var consoleProxy = new PAConsoleProxy
            {
                Encoding = Encoding.UTF8,
                Title = localizer.GetLocalizedString(LocalizationKey.APPLICATION_TITLE_0),
            };

            consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.LOADING_0));
            consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.LOADING_COMMON_SINGLETONS_0));
            
            // initializing PAC singletons
            var loggingStream = new FileLoggerStream(Constants.LOGS_FOLDER_PATH, Constants.LOG_EXT);

            PACSingletons.Initialize(
                Logger.Initialize(
                    loggingStream,
                    Constants.LOG_MS,
                    false,
                    LogSeverity.DEBUG,
                    Constants.FORCE_LOG_INTERVAL,
                    false
                ),
                consoleProxy,
                JsonDataCorrecter.Initialize(
                    Constants.SAVE_VERSION,
                    Constants.ORDER_JSON_CORRECTERS,
                    new Dictionary<string, IList<Type>>
                    {
                        [Constants.CONFIG_FORMAT_VERSION] = [typeof(ConfigData)],
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
                    Constants.CONFIGS_FOLDER_PATH,
                    Constants.CONFIG_EXT,
                    false
                )
            );

            consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.ACTIVATING_ANSI_0));
            if (!PACSingletons.Instance.ConsoleProxy.TryEnableAnsiCodes())
            {
                PACSingletons.Instance.Logger.Log("Failed to enable ANSI codes for the terminal", null, LogSeverity.ERROR, forceLog: true);
            }

            consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.LOADING_PA_SINGLETONS_0));
            // initializing PA singletons
            // special loading order to avoid unintended errors because of complicated self references
            SettingsUtils.LoadDefaultConfigs();
            PASingletons.Initialize(
                new Globals(),
                new Settings(keybinds: new Keybinds(), currentLanguage: localizer.CurrentLanguage, dontUpdateSettingsIfValueSet: true),
                localizer
            );

            PACSingletons.Instance.ConsoleProxy.WriteLine(PASingletons.Instance.Localizer.GetLocalizedString(LocalizationKey.RELOADING_CONFIGS_0));
            Tools.ReloadConfigs(1);
            PASingletons.Instance.Settings.Keybinds = PASingletons.Instance.Settings.GetKeybins();
            
            PACSingletons.Instance.ConsoleProxy.WriteLine(PASingletons.Instance.Localizer.GetLocalizedString(LocalizationKey.DONE_0));
            PACSingletons.Instance.Logger.Log("Finished initialization");
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
