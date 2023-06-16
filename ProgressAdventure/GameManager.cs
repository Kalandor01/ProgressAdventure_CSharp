using ProgressAdventure.Enums;
using ProgressAdventure.SettingsManagement;

namespace ProgressAdventure
{
    /// <summary>
    /// Class for managing the things, happening, while the game is running.
    /// </summary>
    public static class GameManager
    {
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
        /// The main game loop of the game.
        /// </summary>
        public static void GameLoop()
        {
            Utils.PressKey("\n\nGAME LOOP NOT DONE YET!!!\n\n");
            Logger.Log("GAME LOOP PLACEHOLDER", "yes", LogSeverity.FAIL);
        }
    }
}
