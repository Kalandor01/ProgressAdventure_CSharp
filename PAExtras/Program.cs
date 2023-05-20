using PASaveManager = ProgressAdventure.SaveManager;

namespace PAExtras
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SaveImporter.ImportSave("test");

            PASaveManager.LoadSave("test");

            Console.WriteLine();
        }
    }
}