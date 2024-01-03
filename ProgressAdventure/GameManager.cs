using PACommon;
using PACommon.Enums;
using ProgressAdventure.Entity;
using ProgressAdventure.Enums;
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
            var backupChoice = PASingletons.Instance.Settings.DefBackupAction == -1;
            var automaticBackup = PASingletons.Instance.Settings.DefBackupAction == 1;
            SaveManager.LoadSave(saveName, backupChoice, automaticBackup);
            GameLoop();
        }

        /// <summary>
        /// Initiates a random fight, that includes the player.
        /// </summary>
        public static void InitiateFight()
        {
            PASingletons.Instance.Globals.inFight = true;
            EntityUtils.RandomFight(3, 5);
            PASingletons.Instance.Globals.inFight = false;
        }

        /// <summary>
        /// Creates the currently loaded save.
        /// </summary>
        public static void SaveGame()
        {
            PASingletons.Instance.Globals.saving = true;
            SaveManager.MakeSave();
            PACSingletons.Instance.Logger.Log("Game saved", $"save name: \"{SaveData.Instance.saveName}\", player name: \"{SaveData.Instance.player.FullName}\"");
            PASingletons.Instance.Globals.saving = false;
        }

        /// <summary>
        /// The main game loop of the game.
        /// </summary>
        public static void GameLoop()
        {
            PASingletons.Instance.Globals.inGameLoop = true;
            // GAME LOOP
            PACSingletons.Instance.Logger.Log("Game loop started");
            // TRHEADS
            // user actions
            RunLoopingTaskWithErrorHandling(Constants.USER_ACTIONS_THREAD_NAME, UserActionsThreadFunction);
            // auto saver
            if (PASingletons.Instance.Settings.AutoSave)
            {
                RunLoopingTaskWithErrorHandling(Constants.AUTO_SAVE_THREAD_NAME, AutoSaveThreadFunction);
            }
            // GAME
            SaveData.Instance.player.Stats();
            Console.WriteLine("Wandering...");
            for (var x = 0; x < 0; x++)
            {
                if (!PASingletons.Instance.Globals.exiting)
                {
                    Thread.Sleep(100);
                    SaveData.Instance.player.WeightedTurn();
                    SaveData.Instance.player.Move();
                    var position = SaveData.Instance.player.position;
                    World.TryGetChunkAll(position, out Chunk chunk);
                    chunk.FillChunk();
                }
            }
            if (!PASingletons.Instance.Globals.exiting)
            {
                Thread.Sleep(1000);
            }
            if (!PASingletons.Instance.Globals.exiting)
            {
                //InitiateFight();
                SaveGame();
            }
            // SaveGame() maybe instead of the auto save
            // ENDING
            PASingletons.Instance.Globals.exiting = false;
            PASingletons.Instance.Globals.inGameLoop = false;
            Utils.PressKey("Exiting...Press key!");
            PACSingletons.Instance.Logger.Log("Game loop ended");
        }
        #endregion

        #region Thread functions
        /// <summary>
        /// Runs a function in a seperate thread, with error handling.
        /// </summary>
        /// <param name="threadName">The name of the new thread.</param>
        /// <param name="threadFunction">The function to run in the thread.</param>
        public static void RunTaskWithErrorHandling(string threadName, Action threadFunction)
        {
            Task.Run(() => {
                Thread.CurrentThread.Name = threadName;
                try
                {
                    threadFunction();
                }
                catch (Exception e)
                {
                    PACSingletons.Instance.Logger.Log("Thread crashed", e.ToString(), LogSeverity.FATAL);
                    throw;
                }
            });
        }

        /// <summary>
        /// Runs a looping function in a seperate thread, with error handling.
        /// </summary>
        /// <param name="threadName">The name of the new thread.</param>
        /// <param name="threadFunction">The function to run in the thread. If it returns true, it ends the thread.</param>
        /// <param name="loopDelay">The number of milliseconds to suspent the thread inbetween loops.</param>
        public static void RunLoopingTaskWithErrorHandling(string threadName, Func<bool> threadFunction, int loopDelay = 0)
        {
            RunTaskWithErrorHandling(threadName, () => {
                while (!threadFunction())
                {
                    if (loopDelay > 0)
                    {
                        Thread.Sleep(loopDelay);
                    }
                }
            });
        }

        /// <summary>
        /// Function that runs in the auto saver thread.
        /// </summary>
        /// <returns>Whether the thread should exit.</returns>
        public static bool AutoSaveThreadFunction()
        {
            Thread.Sleep(Constants.AUTO_SAVE_INTERVAL);
            if (PASingletons.Instance.Globals.inGameLoop)
            {
                var saved = false;
                while (!saved)
                {
                    if (PASingletons.Instance.Globals.inGameLoop)
                    {
                        if (!PASingletons.Instance.Globals.saving && !PASingletons.Instance.Globals.inFight)
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
                        return true;
                    }
                }
            }
            if (!PASingletons.Instance.Globals.inGameLoop)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Function that runs in the user actions thread.
        /// </summary>
        /// <returns>Whether the thread should exit.</returns>
        public static bool UserActionsThreadFunction()
        {
            if (!PASingletons.Instance.Globals.inGameLoop)
            {
                return true;
            }


            // extra keys into settings

            // PAUSE: EXIT (WITHOUT) SAVING, SAVE (ESC)
            // STATS: INVENTORY (E/I)
            // SAVE (S)

            // pause all other threads while paused/in stats window
            //var threads = Process.GetCurrentProcess().Threads;
            //foreach (var thread in threads)
            //{
            //    Thread.Sleep(99999999);
            //}


            if (PASingletons.Instance.Settings.Keybinds.GetActionKey(ActionType.ESCAPE)?.IsKey() != true)
            {
                return false;
            }

            if (PASingletons.Instance.Globals.inFight || PASingletons.Instance.Globals.saving)
            {
                Console.WriteLine("You can't exit now!");
            }

            PACSingletons.Instance.Logger.Log("Beginning manual save", $"save name: {SaveData.Instance.saveName}");
            PASingletons.Instance.Globals.exiting = true;
            SaveGame();
            PASingletons.Instance.Globals.inGameLoop = false;
            return true;
        }
        #endregion
    }
}
