using ProgressAdventure.Entity;
using ProgressAdventure.Enums;
using ProgressAdventure.SettingsManagement;
using ProgressAdventure.WorldManagement;

namespace ProgressAdventure
{
    /// <summary>
    /// Class for managing the things, happening, while the game is running.
    /// </summary>
    public static class GameManager
    {
        #region Public functions
        /// <summary>
        /// Creates a new save.
        /// </summary>
        public static void NewSave()
        {
            SaveManager.CreateSaveData();
            SaveManager.MakeSave();
            Logger.Log("Created save", $"save name: {SaveData.saveName}, player name: \"{SaveData.player.FullName}\"");
            GameLoop();
        }

        /// <summary>
        /// Loads an existing save.
        /// </summary>
        public static void LoadSave(string saveName)
        {
            var backupChoice = Settings.DefBackupAction == -1;
            var automaticBackup = Settings.DefBackupAction == 1;
            SaveManager.LoadSave(saveName, backupChoice, automaticBackup);
            GameLoop();
        }

        /// <summary>
        /// Initiates a random fight, that includes the player.
        /// </summary>
        public static void InitiateFight()
        {
            Globals.inFight = true;
            EntityUtils.RandomFight(3, 5);
            Globals.inFight = false;
        }

        /// <summary>
        /// Creates the currently loaded save.
        /// </summary>
        public static void SaveGame()
        {
            Globals.saving = true;
            SaveManager.MakeSave();
            Logger.Log("Game saved", $"save name: {SaveData.saveName}, player name: \"{SaveData.player.FullName}\"");
            Globals.saving = false;
        }

        /// <summary>
        /// The main game loop of the game.
        /// </summary>
        public static void GameLoop()
        {
            Globals.inGameLoop = true;
            // GAME LOOP
            Logger.Log("Game loop started");
            // TRHEADS
            // manual quit
            Task.Run(ManualQuitThreadFunction);
            // auto saver
            if (Settings.AutoSave)
            {
                Task.Run(AutoSaveThreadFunction);
            }
            // GAME
            SaveData.player.Stats();
            Console.WriteLine("Wandering...");
            for (var x = 0; x < 0; x++)
            {
                if (!Globals.exiting)
                {
                    Thread.Sleep(100);
                    SaveData.player.WeightedTurn();
                    SaveData.player.Move();
                    var position = SaveData.player.position;
                    World.TryGetChunkAll(position, out Chunk chunk);
                    chunk.FillChunk();
                }
            }
            if (!Globals.exiting)
            {
                Thread.Sleep(1000);
            }
            if (!Globals.exiting)
            {
                //InitiateFight();
                SaveGame();
            }
            // SaveGame() maybe instead of the auto save
            // ENDING
            Globals.exiting = false;
            Globals.inGameLoop = false;
            Utils.PressKey("Exiting...Press key!");
            Logger.Log("Game loop ended");
        }
        #endregion

        #region Thread functions
        /// <summary>
        /// Function that runs in the auto saver thread.
        /// </summary>
        public static void AutoSaveThreadFunction()
        {
            Thread.CurrentThread.Name = Constants.AUTO_SAVE_THREAD_NAME;
            try
            {
                while (true)
                {
                    Thread.Sleep(Constants.AUTO_SAVE_INTERVAL);
                    if (Globals.inGameLoop)
                    {
                        var saved = false;
                        while (!saved)
                        {
                            if (Globals.inGameLoop)
                            {
                                if (!Globals.saving && !Globals.inFight)
                                {
                                    Logger.Log("Beginning auto save", $"save name: {SaveData.saveName}");
                                    SaveGame();
                                    saved = true;
                                }
                                else
                                {
                                    Thread.Sleep(Constants.AUTO_SAVE_DELAY);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (!Globals.inGameLoop)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log("Thread crashed", e.ToString(), LogSeverity.FATAL);
                throw;
            }
        }

        /// <summary>
        /// Function that runs in the quit game thread.
        /// </summary>
        public static void ManualQuitThreadFunction()
        {
            Thread.CurrentThread.Name = Constants.MANUAL_SAVE_THREAD_NAME;
            try
            {
                while (true)
                {
                    if (Globals.inGameLoop)
                    {
                        if (Settings.Keybinds.GetActionKey(ActionType.ESCAPE).IsKey())
                        {
                            if (!Globals.inFight && !Globals.saving)
                            {
                                Logger.Log("Beginning manual save", $"save name: {SaveData.saveName}");
                                Globals.exiting = true;
                                SaveGame();
                                Globals.inGameLoop = false;
                                break;
                            }
                            else
                            {
                                Console.WriteLine("You can't exit now!");
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log("Thread crashed", e.ToString(), LogSeverity.FATAL);
                throw;
            }
        }
        #endregion
    }
}
