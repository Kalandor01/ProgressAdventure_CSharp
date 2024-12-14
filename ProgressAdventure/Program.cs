using ConsoleUI;
using ConsoleUI.UIElements;
using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using PACommon.Logging;
using PACommon.SettingsManagement;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.SettingsManagement;
using ProgressAdventure.WorldManagement;
using System.Text;
using AItem = ProgressAdventure.ItemManagement.AItem;
using Inventory = ProgressAdventure.ItemManagement.Inventory;
using PACTools = PACommon.Tools;
using Utils = PACommon.Utils;

namespace ProgressAdventure
{
    internal class Program
    {
        /// <summary>
        /// The main function for the program.
        /// </summary>
        static void MainFunction()
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

        static void RecipeCraftableMenu(Inventory inventory)
        {
            var recipeElements = new List<BaseUI?> { new Toggle() };

            var menu = new OptionsUI(recipeElements, "Crafting", scrollSettings: new ScrollSettings(20, new ScrollIcon("...\n", "..."), 2, 2));

            CalculateCraftables(menu, recipeElements, inventory);

            menu.Display();
        }

        static void CalculateCraftables(OptionsUI menu, List<BaseUI?> recipeElements, Inventory inventory)
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
                    var rawText = Tools.StylizedText(ItemUtils.ItemIDToDisplayName(itemRecipe.Key) + " x", color);

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


        static bool CraftItem(ItemTypeID targetItem, Inventory inventory, List<BaseUI?> recipeElements, RecipeDTO targetRecipe, OptionsUI menu, int amount)
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
        /// Function for setting up the enviorment, and initialising global variables.
        /// </summary>
        static void Preloading()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Progress Adventure";

            Thread.CurrentThread.Name = Constants.MAIN_THREAD_NAME;

            Console.WriteLine("Loading...");

            // initializing PAC singletons
            var loggingStream = new FileLoggerStream(Constants.LOGS_FOLDER_PATH, Constants.LOG_EXT);

            PACSingletons.Initialize(
                Logger.Initialize(loggingStream, Constants.LOG_MS, false, LogSeverity.DEBUG, Constants.FORCE_LOG_INTERVAL, false),
                JsonDataCorrecter.Initialize(Constants.SAVE_VERSION, Constants.ORDER_JSON_CORRECTERS, false)
            );

            if (!Utils.TryEnableAnsiCodes())
            {
                PACSingletons.Instance.Logger.Log("Failed to enable ANSI codes for the terminal", null, LogSeverity.ERROR, forceLog: true);
            }

            // initializing PA singletons
            PASingletons.Initialize(
                new Globals(),
                new Settings()
            );

            KeybindUtils.colorEnabled = PASingletons.Instance.Settings.EnableColoredText;

            ConfigManager.Initialize(
                null,
                Constants.CONFIGS_FOLDER_PATH,
                Constants.CONFIG_EXT
            );

            Tools.ReloadConfigs();
            PACSingletons.Instance.Logger.Log("Finished initialization");
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
                    PACSingletons.Instance.Logger.Log("Beginning new instance", forceLog: true);
                    MainFunction();
                    //exit
                    PACSingletons.Instance.Logger.Log("Instance ended succesfuly", forceLog: true);
                    PACSingletons.Instance.Dispose();
                }
                catch (Exception e)
                {
                    PACSingletons.Instance.Logger.Log("Instance crashed", e.ToString(), LogSeverity.FATAL, forceLog: true);
                    if (Constants.ERROR_HANDLING)
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
