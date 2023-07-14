using ProgressAdventure;

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



            //PATools.DeleteSave("test");
            //SaveImporter.ImportSave("test");

            //PASaveManager.LoadSave("test", false, false);
            //World.LoadAllChunksFromFolder();
            //Tools.FillWorldAreaSegmented((-1400, -4870, -1385, 4857), 16);



            //var zipFileName = "hmm;2023-06-15;20-35-20";

            //Tools.Unzip(Path.Join(PAConstants.BACKUPS_FOLDER_PATH, zipFileName), PAConstants.SAVES_FOLDER_PATH, true, zipFileName.Split(";").First());



            Tools.LoadBackupMenu();

            Console.WriteLine();
        }
    }
}