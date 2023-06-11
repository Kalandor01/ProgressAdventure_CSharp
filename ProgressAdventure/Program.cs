using ProgressAdventure.Entity;
using ProgressAdventure.Enums;
using ProgressAdventure.SettingsManagement;
using System.Collections;
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
            //Settings.UpdateLoggingLevel(0);

            //SaveManager.CreateSaveData("test", "me");



            //var es = new List<Entity.Entity>
            //{
            //    new Player(),
            //    new Ghoul(2),
            //    new Troll(),
            //    new Caveman(2),
            //    new Caveman(2),
            //    new Caveman(2),
            //    new Caveman(2),
            //    new Caveman(2),
            //    new Dragon(),
            //};

            //var ej = new List<Dictionary<string, object?>>();
            //foreach (var entity in es)
            //{
            //    ej.Add(entity.ToJson());
            //}

            //var es2 = new List<Entity.Entity>();
            //foreach (var entityJson in ej)
            //{
            //    var e = Entity.Entity.AnyEntityFromJson(entityJson);
            //    if (e is null)
            //    {
            //        Console.WriteLine("PARSE ERROR");
            //    }
            //    else
            //    {
            //        es2.Add(e);
            //    }
            //}

            //EntityUtils.Fight(es2);


            var json = new Player().ToJson();


            var gh = (object?)5;
            var gh2 = (object?)new List<object?>();

            var hm1 = gh2 is IEnumerable<object?> gg;
            var hm2 = gh is IDictionary<string, object?> hh;

            var g1 = gh2 as IEnumerable;
            var g2 = gh as IDictionary;



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
