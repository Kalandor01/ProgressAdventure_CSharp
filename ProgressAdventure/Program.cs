using System.Text;
using System.Text.Json.Serialization;
using ConsoleUI;
using ConsoleUI.UIElements;
using PACommon;
using PACommon.ConfigManagement;
using PACommon.ConfigManagement.JsonConverters;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using PACommon.Logging;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.Exceptions;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.Localization;
using ProgressAdventure.SettingsManagement;
using ProgressAdventure.WorldManagement;
using AItem = ProgressAdventure.ItemManagement.AItem;
using Attribute = ProgressAdventure.Enums.Attribute;
using Inventory = ProgressAdventure.ItemManagement.Inventory;
using PACTools = PACommon.Tools;

namespace ProgressAdventure
{
    internal class Program
    {
        /// <summary>
        /// The main function for the program.
        /// </summary>
        private static void MainFunction()
        {
            MenuManager.MainMenu();
            //Settings.UpdateLoggingLevel(0);

            //SaveManager.CreateSaveData("test", "me");

            //var recipeTreeStr = "[SWORD] base: 2, parts(0 blade(1 5 blade_mold?(7)) 0 hilt(1))";
            //var recipeMiniStr = "2(0(1 5(7))0(1))";


            //SaveManager.LoadSave("all items + world");
            //MenuManager.InventoryViewer(SaveData.Instance.player.inventory);


            var backpack = new List<AItem>
            {
                new CompoundItem(ItemType.Misc.SWORD_BLADE, [new MaterialItem(Material.GOLD, 2.3)], 8),
                new CompoundItem(ItemType.Misc.SWORD_HILT, [new MaterialItem(Material.GLASS, 1.75)], 15),
            };
            var ii = ItemUtils.CompleteRecipe(ItemType.Weapon.SWORD, backpack, 7);

            var iii = ItemUtils.CreateCompoundItem(ItemType.Weapon.SWORD, [Material.GOLD, Material.GLASS], 7);
            var mass = ii.RecursiveSelect<AItem, double>(
                i => i is CompoundItem ci ? ci.Parts : new List<AItem>(),
                i => (true, i.MassMultiplier)
            );


            var h = World.RecalculateChunkFileSizes(Constants.CHUNK_SIZE, 7, "all items + world", "Recalculating chunk sizes...");


            var inventory = new Inventory(
            [
                new MaterialItem(Material.WOOD, 620.5),
            ]);
            RecipeCraftableMenu(inventory);

            MenuManager.MainMenu();

            //EntityUtils.RandomFight(2, 100, 20, includePlayer: false);
        }

        private static void RecipeCraftableMenu(Inventory inventory)
        {
            var recipeElements = new List<BaseUI?> { new Toggle() };

            var menu = new OptionsUI(
                recipeElements,
                "Crafting",
                scrollSettings: new ScrollSettings(20, new ScrollIcon("...\n", "..."), 2, 2),
                consoleProxy: PACSingletons.Instance.ConsoleProxy
            );
            menu.BeforeElementsDisplayed += MenuManager.GetAutoHigthAdjusterEventHandler(1.5);

            CalculateCraftables(menu, recipeElements, inventory);

            menu.Display();
        }

        private static void CalculateCraftables(OptionsUI menu, List<BaseUI?> recipeElements, Inventory inventory)
        {
            menu.title = inventory.ToString() + "\n\nCraftnig";
            var selectedValues = recipeElements
                .Select<BaseUI?, int?>(rE => rE is TextField tField ? (int.TryParse(tField.Value, out var amount) ? amount : null) : null)
                .ToList();
            recipeElements.Clear();

            var sValueIndex = 0;
            foreach (var itemRecipe in ItemUtils.ItemRecipes)
            {
                foreach (var recipe in itemRecipe.Value)
                {
                    var recipeAmount = (selectedValues.Count > sValueIndex ? selectedValues[sValueIndex] : null) ?? 1;
                    sValueIndex++;
                    var craftable = ItemUtils.GetRequiredItemsForRecipe(recipe, inventory.items, recipeAmount) is not null;
                    (byte r, byte g, byte b)? color = craftable ? null : Constants.Colors.RED;
                    var rawText = Tools.StylizedText(ItemUtils.ItemTypeToDisplayName(itemRecipe.Key) + " x", color);

                    recipeElements.Add(new TextField(
                        recipeAmount.ToString(),
                        rawText,
                        textValidatorFunction: (inputValue) =>
                        {
                            if (!int.TryParse(inputValue, out var amount))
                            {
                                return (TextFieldValidatorStatus.RETRY, null);
                            }
                            var success = CraftItem(itemRecipe.Key, inventory, recipeElements, recipe, menu, amount);
                            return (success ? TextFieldValidatorStatus.VALID : TextFieldValidatorStatus.INVALID, null);
                        },
                        keyValidatorFunction: (value, key, pos) =>
                        {
                            var good = uint.TryParse(PACTools.GetNewValueForKeyValidatorDelegate(value, key, pos), out var amount) && amount > 0;

                            return good;
                        },
                        overrideDefaultKeyValidatorFunction: false
                    ));
                }
                recipeElements.Add(null);
                sValueIndex++;
            }
        }


        /*
            CompountItemPrpertiesDTO has Material items?
            IngredientDTO.unit can be null???
            Item crafting/creating is wrong?
         */


        private static bool CraftItem(
            EnumTreeValue<ItemType> targetItem,
            Inventory inventory,
            List<BaseUI?> recipeElements,
            RecipeDTO targetRecipe,
            OptionsUI menu,
            int amount
        )
        {
            var item = ItemUtils.CompleteRecipe(targetItem, inventory.items, amount, targetRecipe);
            if (item is null)
            {
                return false;
            }

            inventory.Add(item);
            inventory.ClearFalseItems();
            CalculateCraftables(menu, recipeElements, inventory);
            return true;
        }

        /// <summary>
        /// Function for setting up the enviorment, and initializing global variables.
        /// </summary>
        private static void Preloading()
        {
            Thread.CurrentThread.Name = Constants.MAIN_THREAD_NAME;

            var localizer = Localizer.Initialize(Localizer.DEFAULT_LANGUAGE, false);
            var consoleProxy = new PAConsoleProxy
            {
                Encoding = Encoding.UTF8,
                Title = localizer.GetLocalizedString(LocalizationKey.APPLICATION_TITLE_0),
            };
            
            consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.LOADING_0));
            consoleProxy.WriteLine(localizer.GetLocalizedString(LocalizationKey.LOADING_COMMON_SINGLETONS_0));

            // initializing PAC singletons
            var loggingStream = new FileLoggerStream(Constants.LOGS_FOLDER_PATH, Constants.LOG_EXT);

            PACSingletons.Initialize(
                Logger.Initialize(
                    loggingStream,
                    Constants.LOG_MS,
                    false,
                    LogSeverity.DEBUG,
                    Constants.FORCE_LOG_INTERVAL,
                    false
                ),
                consoleProxy,
                JsonDataCorrecter.Initialize(
                    Constants.SAVE_VERSION,
                    Constants.ORDER_JSON_CORRECTERS,
                    new Dictionary<string, IList<Type>>
                    {
                        [Constants.CONFIG_FORMAT_VERSION] = [typeof(ConfigData)],
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
                    Constants.CONFIGS_FOLDER_PATH,
                    Constants.CONFIG_EXT,
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
                new Settings(keybinds: new Keybinds(), currentLanguage: localizer.CurrentLanguage, dontUpdateSettingsIfValueSet: true, isInitializing: true),
                localizer
            );

            PACSingletons.Instance.ConsoleProxy.WriteLine(PASingletons.Instance.Localizer.GetLocalizedString(LocalizationKey.RELOADING_CONFIGS_0));
            // TODO: configs for more dicts, namespaces for more (keys?) + in correcters???
            Tools.ReloadConfigs(1);
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
        private static void MainErrorHandler()
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
                    MainErrorHandler();
                }
                catch (RestartException re)
                {
                    restartException = re;
                }
                catch (Exception ex)
                {
                    if (!MenuManager.TryGetRestartException(ex, out var re))
                    {
                        throw;
                    }
                    
                    restartException = ex;
                }

                if (restartException is not null)
                {
                    PACSingletons.Instance.Logger.Log("Instance restart requested", restartException.ToString(), forceLog: true);
                    exitGame = false;
                }
            }
            while (!exitGame);
        }
    }
}
