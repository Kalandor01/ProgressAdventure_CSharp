﻿using ProgressAdventure.Enums;
using ProgressAdventure.Extensions;
using ProgressAdventure.SettingsManagement;
using SaveFileManager;
using System.Text;
using SFMUtils = SaveFileManager.Utils;

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


            //MenuManager.MainMenu();

            //EntityUtils.RandomFight(2, 100, 20, includePlayer: false);

            var autoSaveElement = new Toggle(false, "Auto save: ");


            var fps = new TextField(
                57.ToString(),
                "fps: ",
                "Fps",
                oldValueAsStartingValue:true,
                maxInputLength:4,
                validatorFunction:new TextField.ValidatorDelegate(IsFps)
            );


            // logging
            var loggingNames = new List<string> { "he", "hohohoho", "hahahahaha\nhahah" };
            var loggingElement = new PAChoice(loggingNames, 0, "Logging: ");
            var coloredTextElement = new Toggle(true, "Colored text: ", "enabled", "disabled");

            // menu elements
            var menuElements = new List<BaseUI?> { autoSaveElement, fps, loggingElement, coloredTextElement, null};
            var response = SFMUtils.OptionsUI(menuElements, " Other options", keybinds: Settings.Keybinds.KeybindList);

            Console.WriteLine();
        }

        public static bool IsFps(string fps)
        {
            var success = int.TryParse(fps, out var fpsValue);
            return success && fpsValue > 0 && fpsValue <= 1000;
        }

        /// <summary>
        /// Function for setting up the enviorment, and initialising global variables.
        /// </summary>
        static void Preloading()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Progress Adventure";

            Thread.CurrentThread.Name = Constants.MAIN_THREAD_NAME;

            Logger.LogNewLine();
            Console.WriteLine("Loading...");

            if (!Utils.TryEnableAnsiCodes())
            {
                Logger.Log("Failed to enable ANSI codes for the non-debug terminal", null, LogSeverity.ERROR);
            }

            Logger.Log("Preloading global variables");
            // GLOBAL VARIABLES
            SettingsManagement.Settings.Initialise();
            Globals.Initialise();
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
                    Logger.Log("Beginning new instance");
                    MainFunction();
                    //exit
                    Logger.Log("Instance ended succesfuly");
                }
                catch (Exception e)
                {
                    Logger.Log("Instance crashed", e.ToString(), LogSeverity.FATAL);
                    if (Constants.ERROR_HANDLING)
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
