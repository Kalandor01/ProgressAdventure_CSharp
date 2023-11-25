using PACommon;
using PACommon.Enums;
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
            PACSingletons.Instance.Logger.Log("Created save", $"save name: \"{SaveData.Instance.saveName}\", player name: \"{SaveData.Instance.player.FullName}\"");
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
            Globals.Instance.inFight = true;
            EntityUtils.RandomFight(3, 5);
            Globals.Instance.inFight = false;
        }

        /// <summary>
        /// Creates the currently loaded save.
        /// </summary>
        public static void SaveGame()
        {
            Globals.Instance.saving = true;
            SaveManager.MakeSave();
            PACSingletons.Instance.Logger.Log("Game saved", $"save name: \"{SaveData.Instance.saveName}\", player name: \"{SaveData.Instance.player.FullName}\"");
            Globals.Instance.saving = false;
        }

        /// <summary>
        /// The main game loop of the game.
        /// </summary>
        public static void GameLoop()
        {
            Globals.Instance.inGameLoop = true;
            // GAME LOOP
            PACSingletons.Instance.Logger.Log("Game loop started");
            // TRHEADS
            // manual quit
            Task.Run(ManualQuitThreadFunction);
            // auto saver
            if (Settings.AutoSave)
            {
                Task.Run(AutoSaveThreadFunction);
            }
            // GAME
            SaveData.Instance.player.Stats();
            Console.WriteLine("Wandering...");
            for (var x = 0; x < 0; x++)
            {
                if (!Globals.Instance.exiting)
                {
                    Thread.Sleep(100);
                    SaveData.Instance.player.WeightedTurn();
                    SaveData.Instance.player.Move();
                    var position = SaveData.Instance.player.position;
                    World.TryGetChunkAll(position, out Chunk chunk);
                    chunk.FillChunk();
                }
            }
            if (!Globals.Instance.exiting)
            {
                Thread.Sleep(1000);
            }
            if (!Globals.Instance.exiting)
            {
                //InitiateFight();
                SaveGame();
            }
            // SaveGame() maybe instead of the auto save
            // ENDING
            Globals.Instance.exiting = false;
            Globals.Instance.inGameLoop = false;
            Utils.PressKey("Exiting...Press key!");
            PACSingletons.Instance.Logger.Log("Game loop ended");
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
                    if (Globals.Instance.inGameLoop)
                    {
                        var saved = false;
                        while (!saved)
                        {
                            if (Globals.Instance.inGameLoop)
                            {
                                if (!Globals.Instance.saving && !Globals.Instance.inFight)
                                {
                                    PACSingletons.Instance.Logger.Log("Beginning auto save", $"save name: {SaveData.Instance.saveName}");
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
                    if (!Globals.Instance.inGameLoop)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                PACSingletons.Instance.Logger.Log("Thread crashed", e.ToString(), LogSeverity.FATAL);
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
                    if (Globals.Instance.inGameLoop)
                    {
                        if (Settings.Keybinds.GetActionKey(ActionType.ESCAPE).IsKey())
                        {
                            if (!Globals.Instance.inFight && !Globals.Instance.saving)
                            {
                                PACSingletons.Instance.Logger.Log("Beginning manual save", $"save name: {SaveData.Instance.saveName}");
                                Globals.Instance.exiting = true;
                                SaveGame();
                                Globals.Instance.inGameLoop = false;
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
                PACSingletons.Instance.Logger.Log("Thread crashed", e.ToString(), LogSeverity.FATAL);
                throw;
            }
        }
        #endregion
    }
}
