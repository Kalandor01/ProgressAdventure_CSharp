using ConsoleUI;
using ConsoleUI.UIElements;
using PACommon;
using PACommon.ConfigManagement;
using PACommon.ConfigManagement.JsonConverters;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using PACommon.Logging;
using PACommon.SettingsManagement;
using ProgressAdventure;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.Exceptions;
using ProgressAdventure.SettingsManagement;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows;
using Attribute = ProgressAdventure.Enums.Attribute;
using PAConstants = ProgressAdventure.Constants;
using Utils = PACommon.Utils;

namespace PAVisualizer
{
    internal static class Program
    {
        static void ConsoleMainFunction()
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
            var oldSaveDataFileName = $"{PAConstants.SAVE_FILE_NAME_DATA}.{PAConstants.OLD_SAVE_EXT}";
            string? folderPath = Utils.SplitPathToParts(Utils.OpenFileDialog([
                (saveDataFileName, $"Data file ({saveDataFileName})"),
                (oldSaveDataFileName, $"Old data file ({oldSaveDataFileName})")
            ]))?.folderPath;
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
        /// The main function for the program.
        /// </summary>
        static void MainFunction()
        {
            if (MenuManager.AskYesNoUIQuestion("Open visualizer GUI?"))
            {
                ShowMainWindow();
            }
            else
            {
                ConsoleMainFunction();
            }
        }

        /// <summary>
        /// Function for setting up the enviorment, and initialising global variables.
        /// </summary>
        static void Preloading()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Thread.CurrentThread.Name = Constants.VISUALIZER_THREAD_NAME;
            Console.WriteLine("Loading...");

            // initializing PAC singletons
            var loggingStream = new FileLoggerStream(PAConstants.LOGS_FOLDER_PATH, PAConstants.LOG_EXT);

            PACSingletons.Initialize(
                Logger.Initialize(loggingStream, PAConstants.LOG_MS, false, LogSeverity.DEBUG, PAConstants.FORCE_LOG_INTERVAL, false),
                JsonDataCorrecter.Initialize(
                    PAConstants.SAVE_VERSION,
                    PAConstants.ORDER_JSON_CORRECTERS,
                    new Dictionary<string, IList<Type>>
                    {
                        [PAConstants.CONFIG_VERSION] = [typeof(ConfigData)],
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
            // TODO: configs for more dicts, namespaces for more (keys?) + in correcters???
            ProgressAdventure.Tools.ReloadConfigs(1);
            PASingletons.Instance.Settings.Keybinds = Settings.GetKeybins();
            PACSingletons.Instance.Logger.Log("Finished initialization");
        }


        /// <summary>
        /// The error handler, for the preloading.
        /// </summary>
        static void PreloadingErrorHandler()
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
        static void MainErrorHandler()
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
