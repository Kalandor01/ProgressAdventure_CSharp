using ProgressAdventure;
using ProgressAdventure.WorldManagement;
using PASaveManager = ProgressAdventure.SaveManager;
using PAConstants = ProgressAdventure.Constants;
using PATools = ProgressAdventure.Tools;

namespace PAExtras
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "PAExtras";
            Logger.LogNewLine();

            //SaveImporter.ImportSave("big_test");
            //SaveImporter.ImportSave("new save");
            //SaveImporter.ImportSave("new save_1");
            //SaveImporter.ImportSave("save2_1");
            //SaveImporter.ImportSave("test");

            //Tools.EncodeSaveFile("data", Path.Join(PAConstants.SAVES_FOLDER_PATH, "test save"));

            PATools.DeleteSave("test");
            SaveImporter.ImportSave("test");

            //PASaveManager.LoadSave("test", false, false);
            //World.LoadAllChunksFromFolder();
            //Tools.FillWorldAreaSegmented((-1400, -4870, -1385, 4857), 16);

            Console.WriteLine();
        }
    }
}