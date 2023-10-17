using PACommon;
using PACommon.Enums;
using PACommon.SettingsManagement;
using ProgressAdventure.SettingsManagement;
using System.Text;
using PACTools = PACommon.Tools;

namespace ProgressAdventure
{
    internal class Program
    {
        /// <summary>
        /// The main function for the program.
        /// </summary>
        static void MainFunction()
        {
            //Settings.UpdateLoggingLevel(0);

            //SaveManager.CreateSaveData("test", "me");

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

            Logger.Initialize(Constants.ROOT_FOLDER, Constants.LOGS_FOLDER, Constants.LOG_EXT, Constants.LOG_MS, false, LogSeverity.DEBUG);
            Console.WriteLine("Loading...");

            if (!Utils.TryEnableAnsiCodes())
            {
                Logger.Instance.Log("Failed to enable ANSI codes for the non-debug terminal", null, LogSeverity.ERROR);
            }

            Logger.Instance.Log("Preloading global variables");
            // GLOBAL VARIABLES
            Settings.Initialize();
            KeybindUtils.colorEnabled = Settings.EnableColoredText;
            PACTools.SAVE_VERSION = Constants.SAVE_VERSION;
            Globals.Initialize();
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
            catch (Exception e)
            {
                Logger.Instance.Log("Preloading crashed", e.ToString(), LogSeverity.FATAL);
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
                    Logger.Instance.Log("Beginning new instance");
                    MainFunction();
                    //exit
                    Logger.Instance.Log("Instance ended succesfuly");
                }
                catch (Exception e)
                {
                    Logger.Instance.Log("Instance crashed", e.ToString(), LogSeverity.FATAL);
                    if (Constants.ERROR_HANDLING)
                    {
                        Console.WriteLine("ERROR: " + e.Message);
                        var ans = Utils.Input("Restart?(Y/N): ");
                        if (ans is not null && ans.ToUpper() == "Y")
                        {
                            Logger.Instance.Log("Restarting instance");
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
            PreloadingErrorHandler();
            MainErrorHandler();
        }
    }
}
