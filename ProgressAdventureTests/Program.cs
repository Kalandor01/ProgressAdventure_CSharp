﻿using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using PACommon.Logging;
using PACommon.SettingsManagement;
using PACommon.TestUtils;
using ProgressAdventure;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.SettingsManagement;
using System.Text;
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
        static void MainFunction()
        {
            //Tools.RunAllTests();
            //Tools.CreateNewTestSaveFromPrevious("2.2.1");
            TestingUtils.RunAllTests(typeof(Tests), Tools.PrepareTest, Tools.DisposeTest);

            Utils.PressKey("DONE!");
        }

        /// <summary>
        /// Function for setting up the enviorment, and initialising global variables.
        /// </summary>
        static void Preloading()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Progress Adventure tests";

            Thread.CurrentThread.Name = PACConstants.TESTS_THREAD_NAME;

            Console.WriteLine("Loading...");

            //initializing PAC singletons
            var loggingStream = new FileLoggerStream(PAConstants.LOGS_FOLDER_PATH, PAConstants.LOG_EXT);

            PACSingletons.Initialize(
                Logger.Initialize(loggingStream, PAConstants.LOG_MS, false, LogSeverity.DEBUG, PAConstants.FORCE_LOG_INTERVAL, false),
                JsonDataCorrecter.Initialize(PAConstants.SAVE_VERSION, PAConstants.ORDER_JSON_CORRECTERS, false)
            );

            if (!Utils.TryEnableAnsiCodes())
            {
                PACSingletons.Instance.Logger.Log("Failed to enable ANSI codes for the terminal", null, LogSeverity.ERROR, forceLog: true);
            }

            // initializing PA singletons
            if (Constants.PRELOAD_GLOBALS_ON_PRELOAD)
            {
                PASingletons.Initialize(
                    new Globals(),
                    new Settings()
                );
                KeybindUtils.colorEnabled = PASingletons.Instance.Settings.EnableColoredText;

                ConfigManager.Initialize(
                    null,
                    PAConstants.CONFIGS_FOLDER_PATH,
                    PAConstants.CONFIG_EXT
                );
            }

            PATools.ReloadConfigs();
            PACSingletons.Instance.Logger.Log("Finished initialization", forceLog: true);
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
                PACSingletons.Instance.Logger.Log("Preloading crashed", e.ToString(), LogSeverity.FATAL, forceLog: true);
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
                    PACSingletons.Instance.Logger.Log("Beginning new instance", forceLog: true);
                    MainFunction();
                    //exit
                    PACSingletons.Instance.Logger.Log("Instance ended succesfuly", forceLog: true);
                    PACSingletons.Instance.Dispose();
                }
                catch (Exception e)
                {
                    PACSingletons.Instance.Logger.Log("Instance crashed", e.ToString(), LogSeverity.FATAL, forceLog: true);
                    if (PAConstants.ERROR_HANDLING)
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
            PreloadingErrorHandler();
            MainErrorHandler();
        }
    }
}
