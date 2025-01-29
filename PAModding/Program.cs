using PACommon;
using PACommon.ConfigManagement;
using PACommon.ConfigManagement.JsonConverters;
using PACommon.Enums;
using PACommon.JsonUtils;
using PACommon.Logging;
using PACommon.SettingsManagement;
using ProgressAdventure;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.Exceptions;
using ProgressAdventure.SettingsManagement;
using System.Text;
using System.Text.Json.Serialization;
using Constants = ProgressAdventure.Constants;
using Tools = ProgressAdventure.Tools;

namespace PAModding
{
    internal class Program
    {
        /// <summary>
        /// The main function for the program.
        /// </summary>
        static void MainFunction()
        {
            MenuManager.MainMenu();

            //EntityUtils.RandomFight(2, 100, 20, includePlayer: false);
        }

        /// <summary>
        /// Function for setting up the enviorment, and initialising global variables.
        /// </summary>
        static void Preloading()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Progress Adventure";

            Thread.CurrentThread.Name = Constants.MAIN_THREAD_NAME;

            Console.WriteLine("Loading...");

            // initializing PAC singletons
            var loggingStream = new FileLoggerStream(Constants.LOGS_FOLDER_PATH, Constants.LOG_EXT);

            PACSingletons.Initialize(
                Logger.Initialize(loggingStream, Constants.LOG_MS, false, LogSeverity.DEBUG, Constants.FORCE_LOG_INTERVAL, false),
                JsonDataCorrecter.Initialize(Constants.SAVE_VERSION, Constants.ORDER_JSON_CORRECTERS, false),
                ConfigManager.Initialize(
                    [
                        new JsonStringEnumConverter(allowIntegerValues: false),
                        new TypeConverter(),
                        new AdvancedEnumTreeConverter<ItemType>(),
                        new MaterialItemAttributesDTOConverter(),
                        new ConsoleKeyInfoConverter(),
                    ],
                    Constants.CONFIGS_FOLDER_PATH,
                    Constants.CONFIG_EXT,
                    false
                )
            );

            if (!Utils.TryEnableAnsiCodes())
            {
                PACSingletons.Instance.Logger.Log("Failed to enable ANSI codes for the terminal", null, LogSeverity.ERROR, forceLog: true);
            }

            // initializing PA singletons
            // special loading order to avoid unintended errors because of complicated self references
            SettingsUtils.LoadDefaultConfigs();
            PASingletons.Initialize(
                new Globals(),
                new Settings(keybinds: new Keybinds(), dontUpdateSettingsIfValueSet: true)
            );

            KeybindUtils.colorEnabled = PASingletons.Instance.Settings.EnableColoredText;

            Console.WriteLine("Reloading configs...");
            Tools.ReloadConfigs(1);
            PASingletons.Instance.Settings.Keybinds = Settings.GetKeybins();
            PACSingletons.Instance.Logger.Log("Finished initialization");
        }

        /// <summary>
        /// The error handler, for the preloading.
        /// </summary>
        static void PreloadingErrorHandler()
        {
            try
            {
                Preloading();
            }
            catch (RestartException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (e.InnerException is RestartException)
                {
                    throw;
                }

                PACSingletons.Instance.Logger.Log("Preloading crashed", e.ToString(), LogSeverity.FATAL, forceLog: true);
                if (Constants.ERROR_HANDLING)
                {
                    Utils.PressKey("ERROR: " + e.Message);
                }
                throw;
            }
        }

        /// <summary>
        /// The error handler, for the main function.
        /// </summary>
        static void MainErrorHandler()
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
                catch (RestartException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    if (e.InnerException is RestartException)
                    {
                        throw;
                    }

                    PACSingletons.Instance.Logger.Log("Instance crashed", e.ToString(), LogSeverity.FATAL, forceLog: true);
                    if (Constants.ERROR_HANDLING)
                    {
                        Console.WriteLine("ERROR: " + e.Message);
                        var ans = Utils.Input("Restart?(Y/N): ");
                        if (ans is not null && ans.ToUpper() == "Y")
                        {
                            PACSingletons.Instance.Logger.Log("Restarting instance", forceLog: true);
                            exitGame = false;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            while (!exitGame);
        }

        static void Main(string[] args)
        {
            bool exitGame;
            do
            {
                RestartException? restartException = null;
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
                catch (Exception ie)
                {
                    if (ie.InnerException is RestartException re)
                    {
                        restartException = re;
                    }
                    else
                    {
                        throw;
                    }
                }

                if (restartException is not null)
                {
                    PACSingletons.Instance.Logger.Log("Instance restart requested", restartException.ToString(), LogSeverity.INFO, forceLog: true);
                    exitGame = false;
                }
            }
            while (!exitGame);
        }
    }
}
