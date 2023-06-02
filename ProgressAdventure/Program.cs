using Newtonsoft.Json.Linq;
using ProgressAdventure.Entity;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.SettingsManagement;
using SaveFileManager;
using System.Text;

namespace ProgressAdventure
{
    internal class Program
    {
        /// <summary>
        /// The main function for the program.
        /// </summary>
        static void MainFunction()
        {
            Settings.UpdateLoggingLevel(1);

            SaveManager.CreateSaveData("test", "me");


            //var entities = new List<Entity.Entity>
            //{
            //    SaveData.player,
            //    new Troll(0),
            //    new Troll(0),
            //    new Troll(0),

            //    new Caveman(1),
            //    new Ghoul(1),
            //    new Troll(1),

            //    new Caveman(2),
            //    new Troll(2),
            //    new Ghoul(2),

            //    new Caveman(3),
            //    new Ghoul(3),
            //    new Troll(3),
            //};

            //EntityUtils.Fight(entities);

            


            Console.WriteLine();
        }

        /// <summary>
        /// Function for setting up the enviorment, and initialising global variables.
        /// </summary>
        static void Preloading()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Thread.CurrentThread.Name = Constants.MAIN_THREAD_NAME;
            Logger.LogNewLine();
            Console.WriteLine("Loading...");
            Logger.Log("Preloading global variables");
            // GLOBAL VARIABLES
            SettingsManagement.Settings.Initialise();
            Globals.Initialise();
        }

        /// <summary>
        /// The error handler, for the preloading.
        /// </summary>
        static void PreloadingErrorHandler()
        {
            try
            {
                Preloading();
            }
            catch (Exception e)
            {
                Logger.Log("Preloading crashed", e.ToString(), LogSeverity.FATAL);
                if (Constants.ERROR_HANDLING)
                {
                    Utils.PressKey("ERROR: " + e.Message);
                }
                throw;
            }
        }

        /// <summary>
        /// The error handler, for the main function.
        /// </summary>
        static void MainErrorHandler()
        {
            // general crash handler (release only)

            bool exitGame;
            do
            {
                exitGame = true;
                try
                {
                    Logger.Log("Beginning new instance");
                    MainFunction();
                    //exit
                    Logger.Log("Instance ended succesfuly");
                }
                catch (Exception e)
                {
                    Logger.Log("Instance crashed", e.ToString(), LogSeverity.FATAL);
                    if (Constants.ERROR_HANDLING)
                    {
                        Console.WriteLine("ERROR: " + e.Message);
                        var ans = Utils.Input("Restart?(Y/N): ");
                        if (ans is not null && ans.ToUpper() == "Y")
                        {
                            Logger.Log("Restarting instance");
                            exitGame = false;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            while (!exitGame);
        }

        static void Main(string[] args)
        {
            PreloadingErrorHandler();
            MainErrorHandler();
        }
    }
}
