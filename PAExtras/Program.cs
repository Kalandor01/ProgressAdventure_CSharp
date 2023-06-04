using ProgressAdventure;
using ProgressAdventure.WorldManagement;
using PASaveManager = ProgressAdventure.SaveManager;

namespace PAExtras
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "PAExtras";
            Logger.LogNewLine();

            var saveName = "test";

            SaveImporter.ImportSave(saveName);

            PASaveManager.LoadSave(saveName);

            World.LoadAllChunksFromFolder(null, "Loading chunks...");

            var ch = World.Chunks;
            var p = SaveData.player;

            PASaveManager.MakeSave();

            PASaveManager.LoadSave(saveName);

            Console.WriteLine();
        }
    }
}