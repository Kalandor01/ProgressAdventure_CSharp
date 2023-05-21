using ProgressAdventure;
using ProgressAdventure.WorldManagement;
using PASaveManager = ProgressAdventure.SaveManager;

namespace PAExtras
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Logger.LogNewLine();

            SaveImporter.ImportSave("new save");

            PASaveManager.LoadSave("new save");

            World.LoadAllChunksFromFolder(null, "Loading chunks...");

            var ch = World.Chunks;

            Console.WriteLine();
        }
    }
}