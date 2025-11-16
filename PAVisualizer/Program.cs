using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using ConsoleUI;
using ConsoleUI.UIElements;
using PACommon;
using PACommon.ConfigManagement;
using PACommon.ConfigManagement.JsonConverters;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using PACommon.Logging;
using PAVisualizer.Windows;
using ProgressAdventure;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.Exceptions;
using ProgressAdventure.Localization;
using ProgressAdventure.SettingsManagement;
using Attribute = ProgressAdventure.Enums.Attribute;
using PAConstants = ProgressAdventure.Constants;
using Utils = PACommon.Utils;

namespace PAVisualizer
{
    internal static class Program
    {
        private static void ConsoleMainFunction()
        {
            var app = BuildAvaloniaApp()
                .SetupWithoutStarting();
            
            PACSingletons.Instance.Logger.DefaultWriteOut = false;

            var elements = new List<BaseUI?>();

            var visualizeSaveElement = new PAButton(UIAction.Create(VisualizeSaveCommand), text: "Save file visualizer");
            elements.Add(visualizeSaveElement);

            var contentDistributionVisualizerElement = new PAButton(UIAction.Create(ContentTypeDistributionVisualizer.Visualize), text: "Content type distribution visualizer");
            elements.Add(contentDistributionVisualizerElement);

            new OptionsUI(
                elements,
                "Select action",
                consoleProxy: PACSingletons.Instance.ConsoleProxy
            ).Display();
        }

        private static void VisualizeSaveCommand()
        {
            const string saveDataFileName = $"{PAConstants.SAVE_FILE_NAME_DATA}.{PAConstants.SAVE_EXT}";
            const string oldSaveDataFileName = $"{PAConstants.SAVE_FILE_NAME_DATA}.{PAConstants.OLD_SAVE_EXT}";
            // TODO: Linux file/folder browser
            // OperatingSystem.IsWindows()
#pragma warning disable CA1416
            var folderPath = Utils.SplitPathToParts(Utils.OpenFileDialog([
                (saveDataFileName, $"Data file ({saveDataFileName})"),
                (oldSaveDataFileName, $"Old data file ({oldSaveDataFileName})")
            ]))?.folderPath;
#pragma warning restore CA1416
            var selectedFolder = VisualizerTools.GetSaveFolderFromPath(folderPath);
            if (selectedFolder is not null)
            {
                ConsoleVisualizer.SaveVisualizer(selectedFolder.Value.saveFolderName, selectedFolder.Value.saveFolderPath);
            }
        }

        public static void AppMain(Application app, string[] args)
        {
            app.Styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
            app.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;

            var window = new MainWindow();
            app.Run(window);
        }

        /// <summary>
        /// Shows the main window.
        /// </summary>
        private static void ShowMainWindow(string[] args)
        {
            BuildAvaloniaApp()
                .Start(AppMain, args);
        }

        private static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<Application>()
                    .UsePlatformDetect()
                    .LogToTrace();
        }

        /// <summary>
        /// The main function for the program.
        /// </summary>
        private static void MainFunction(string[] args)
        {
            if (MenuManager.AskYesNoUIQuestion("Open visualizer GUI?"))
            {
                ShowMainWindow(args);
            }
            else
            {
                ConsoleMainFunction();
            }
        }

        /// <summary>
        /// Function for setting up the enviorment, and initializing global variables.
        /// </summary>
        private static void Preloading()
        {
            Thread.CurrentThread.Name = Constants.VISUALIZER_THREAD_NAME;

            var localizer = Localizer.Initialize(Localizer.DEFAULT_LANGUAGE, false);
            var consoleProxy = new PAConsoleProxy
            {
                Encoding = Encoding.UTF8,
                Title = "Progress Adventure visualizer",
            };
            
            consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.LOADING_0));
            consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.LOADING_COMMON_SINGLETONS_0));

            // initializing PAC singletons
            var loggingStream = new FileLoggerStream(PAConstants.LOGS_FOLDER_PATH, PAConstants.LOG_EXT);

            PACSingletons.Initialize(
                Logger.Initialize(
                    loggingStream,
                    PAConstants.LOG_MS,
                    false,
                    LogSeverity.DEBUG,
                    PAConstants.FORCE_LOG_INTERVAL,
                    false
                ),
                consoleProxy,
                JsonDataCorrecter.Initialize(
                    PAConstants.SAVE_VERSION,
                    PAConstants.ORDER_JSON_CORRECTERS,
                    new Dictionary<string, IList<Type>>
                    {
                        [PAConstants.CONFIG_FORMAT_VERSION] = [typeof(ConfigData)],
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
                    PAConstants.CONFIGS_FOLDER_PATH,
                    PAConstants.CONFIG_EXT,
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
                new Settings(keybinds: new Keybinds(), dontUpdateSettingsIfValueSet: true, isInitializing: true),
                localizer
            );

            PACSingletons.Instance.ConsoleProxy.WriteLine(PASingletons.Instance.Localizer.GetLocalizedString(LocalizationKey.RELOADING_CONFIGS_0));
            ProgressAdventure.Tools.ReloadConfigs(1);
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
        private static void MainErrorHandler(string[] args)
        {
            bool exitGame;
            do
            {
                exitGame = true;
                try
                {
                    PACSingletons.Instance.Logger.Log("Beginning new instance", forceLog: true);
                    MainFunction(args);
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
                    MainErrorHandler(args);
                }
                catch (RestartException re)
                {
                    restartException = re;
                }
                catch (Exception ie)
                {
                    if (ie.InnerException is RestartException)
                    {
                        restartException = ie;
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
