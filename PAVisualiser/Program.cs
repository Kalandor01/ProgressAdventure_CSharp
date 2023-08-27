using ProgressAdventure;
using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement;
using System;
using System.Text;
using System.Threading;
using System.Windows;
using Logger = ProgressAdventure.Logger;
using PAConstants = ProgressAdventure.Constants;
using PAUtils = ProgressAdventure.Utils;

namespace PAVisualiser
{
    internal static class Program
    {
        /// <summary>
        /// The main function for the program.
        /// </summary>
        static void MainFunction()
        {
            Logger.DefaultWriteOut = false;
            LegacyVisualiser.SaveVisualizer("test save");
        }

        /// <summary>
        /// Shows the main window.
        /// </summary>
        static void ShowMainWindow()
        {
            var windowThread = new Thread(ShowMainWindowThread);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        [STAThread]
        static void ShowMainWindowThread()
        {
            Thread.CurrentThread.Name = Constants.VISUALISER_WINDOW_THREAD_NAME;
            var mainWindow = new MainWindow();
            var application = new Application();
            application.Run(mainWindow);
        }

        /// <summary>
        /// Function for setting up the enviorment, and initialising global variables.
        /// </summary>
        static void Preloading()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Thread.CurrentThread.Name = Constants.VISUALIZER_THREAD_NAME;
            Logger.LogNewLine();
            Logger.DefaultWriteOut = true;
            Console.WriteLine("Loading...");

            if (!PAUtils.TryEnableAnsiCodes())
            {
                Logger.Log("Failed to enable ANSI codes for the non-debug terminal", null, LogSeverity.ERROR);
            }

            Logger.Log("Preloading global variables");
            // GLOBAL VARIABLES
            ProgressAdventure.SettingsManagement.Settings.Initialise();
            ProgressAdventure.Globals.Initialise();
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
        
        public static void Main()
        {
            PreloadingErrorHandler();

            if (MenuManager.AskYesNoUIQuestion("Open visualiser GUI?"))
            {
                ShowMainWindow();
            }
            else
            {
                MainErrorHandler();
            }
        }
    }
}
