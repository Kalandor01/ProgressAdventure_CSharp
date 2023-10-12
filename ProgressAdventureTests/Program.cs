using PACommon;
using PACommon.Enums;
using System.Text;
using PAConstants = ProgressAdventure.Constants;
using PACConstants = PACommon.Constants;
using PACTools = PACommon.Tools;

namespace ProgressAdventureTests
{
    internal class Program
    {
        /// <summary>
        /// The main function for the program.
        /// </summary>
        static void MainFunction()
        {
            //Tools.RunAllTests();
            PACTools.RunAllTests(typeof(Tests), Tools.PrepareTest);


            Utils.PressKey("DONE!");
        }

        /// <summary>
        /// Function for setting up the enviorment, and initialising global variables.
        /// </summary>
        static void Preloading()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Thread.CurrentThread.Name = PACConstants.TESTS_THREAD_NAME;
            Logger.LogNewLine();
            Console.WriteLine("Loading...");

            if (!Utils.TryEnableAnsiCodes())
            {
                Logger.Log("Failed to enable ANSI codes for the non-debug terminal", null, LogSeverity.ERROR);
            }

            Logger.Log("Preloading global variables");
            // GLOBAL VARIABLES
            if (Constants.PRELOAD_GLOBALS_ON_PRELOAD)
            {
                ProgressAdventure.SettingsManagement.Settings.Initialise();
                ProgressAdventure.Globals.Initialise();
            }
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
                Logger.Log("Preloading crashed", e.ToString(), LogSeverity.FATAL);
                if (PAConstants.ERROR_HANDLING)
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
                    Logger.Log("Beginning new instance");
                    MainFunction();
                    //exit
                    Logger.Log("Instance ended succesfuly");
                }
                catch (Exception e)
                {
                    Logger.Log("Instance crashed", e.ToString(), LogSeverity.FATAL);
                    if (PAConstants.ERROR_HANDLING)
                    {
                        Console.WriteLine("ERROR: " + e.Message);
                        var ans = Utils.Input("Restart?(Y/N): ");
                        if (ans is not null && ans.ToUpper() == "Y")
                        {
                            Logger.Log("Restarting instance");
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
