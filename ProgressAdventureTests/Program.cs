using ProgressAdventure.Enums;
using System.Text;
using PAConstants = ProgressAdventure.Constants;
using PAUtils = ProgressAdventure.Utils;
using Logger = ProgressAdventure.Logger;
using ProgressAdventure.Entity;
using Moq;
using System.Reflection;
using System.Transactions;

namespace ProgressAdventureTests
{
    internal class Program
    {
        /// <summary>
        /// The main function for the program.
        /// </summary>
        static void MainFunction()
        {
            Tools.RunAllTests();

            Console.WriteLine();
        }

        /// <summary>
        /// Function for setting up the enviorment, and initialising global variables.
        /// </summary>
        static void Preloading()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Thread.CurrentThread.Name = Constants.TESTS_THREAD_NAME;
            Logger.LogNewLine();
            Console.WriteLine("Loading...");
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
                    PAUtils.PressKey("ERROR: " + e.Message);
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
                        var ans = PAUtils.Input("Restart?(Y/N): ");
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
