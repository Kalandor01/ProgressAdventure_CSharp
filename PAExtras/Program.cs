using ProgressAdventure;
using ProgressAdventure.WorldManagement;
using PASaveManager = ProgressAdventure.SaveManager;
using PAConstants = ProgressAdventure.Constants;

namespace PAExtras
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "PAExtras";
            Logger.LogNewLine();

            SaveImporter.ImportSave("big_test");
            //SaveImporter.ImportSave("new save");
            //SaveImporter.ImportSave("new save_1");
            //SaveImporter.ImportSave("save2_1");
            //SaveImporter.ImportSave("test");

            //Tools.EncodeSaveFile("data", Path.Join(PAConstants.SAVES_FOLDER_PATH, "test save"));

            Console.WriteLine();
        }
    }
}