using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.SettingsManagement;
using ProgressAdventure;
using ProgressAdventure.SettingsManagement;
using SaveFileManager;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using PAConstants = ProgressAdventure.Constants;
using Utils = PACommon.Utils;

namespace PAVisualizer
{
    internal static class Program
    {
        /// <summary>
        /// The main function for the program.
        /// </summary>
        static void MainFunction()
        {
            PACSingletons.Instance.Logger.DefaultWriteOut = false;

            var elements = new List<BaseUI>();

            var visualizeSaveElement = new PAButton(new UIAction(VisualizeSaveCommand), text: "Save file visualizer");
            elements.Add(visualizeSaveElement);

            var contentDistributionVisualizerElement = new PAButton(new UIAction(ContentTypeDistributionVisualizer.Visualize), text: "Content type distribution visualizer");
            elements.Add(contentDistributionVisualizerElement);

            new OptionsUI(elements, "Select action").Display();
        }

        static void VisualizeSaveCommand()
        {
            var saveDataFileName = $"{PAConstants.SAVE_FILE_NAME_DATA}.{PAConstants.SAVE_EXT}";
            string? folderPath = Utils.SplitPathToParts(Utils.OpenFileDialog(new List<(string regex, string displayName)> { (saveDataFileName, $"Data file ({saveDataFileName})") }))?.folderPath;
            var selectedFolder = VisualizerTools.GetSaveFolderFromPath(folderPath);
            if (selectedFolder is not null)
            {
                ConsoleVisualizer.SaveVisualizer(selectedFolder.Value.saveFolderName, selectedFolder.Value.saveFolderPath);
            }
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
            Thread.CurrentThread.Name = Constants.VISUALIZER_WINDOW_THREAD_NAME;
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
            PACSingletons.Instance.Logger.LogNewLine();
            PACSingletons.Instance.Logger.DefaultWriteOut = true;
            Console.WriteLine("Loading...");

            if (!Utils.TryEnableAnsiCodes())
            {
                PACSingletons.Instance.Logger.Log("Failed to enable ANSI codes for the non-debug terminal", null, LogSeverity.ERROR);
            }

            // initializing PA singletons
            PASingletons.Initialize(
                new Globals(),
                new Settings()
            );
            KeybindUtils.colorEnabled = PASingletons.Instance.Settings.EnableColoredText;
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
                PACSingletons.Instance.Logger.Log("Preloading crashed", e.ToString(), LogSeverity.FATAL);
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
                    PACSingletons.Instance.Logger.Log("Beginning new instance");
                    MainFunction();
                    //exit
                    PACSingletons.Instance.Logger.Log("Instance ended succesfuly");
                }
                catch (Exception e)
                {
                    PACSingletons.Instance.Logger.Log("Instance crashed", e.ToString(), LogSeverity.FATAL);
                    if (PAConstants.ERROR_HANDLING)
                    {
                        Console.WriteLine("ERROR: " + e.Message);
                        var ans = Utils.Input("Restart?(Y/N): ");
                        if (ans is not null && ans.ToUpper() == "Y")
                        {
                            PACSingletons.Instance.Logger.Log("Restarting instance");
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

            if (MenuManager.AskYesNoUIQuestion("Open visualizer GUI?"))
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
