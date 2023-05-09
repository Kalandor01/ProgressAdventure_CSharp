using ProgressAdventure.Entity;
using ProgressAdventure.Enums;
using System.Drawing;
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
            ulong seed = 10;
            var baseDivision = 200;

            var sampleSize = 2048;

            var noise = new PerlinNoise(seed);

            var bitmap = new Bitmap(sampleSize, sampleSize);

            for (var x = -1000; x < bitmap.Width - 1000; x++)
            {
                for (var y = -1000; y < bitmap.Height - 1000; y++)
                {
                    var point = noise.Generate(x, y, 16.0 / baseDivision) * 1;
                    point += noise.Generate(x, y, 8.0 / baseDivision) * 2;
                    point += noise.Generate(x, y, 4.0 / baseDivision) * 4;
                    point += noise.Generate(x, y, 2.0 / baseDivision) * 8;
                    point += noise.Generate(x, y, 1.0 / baseDivision) * 16;
                    point /= 31;
                    point = point * 128 + 128;
                    bitmap.SetPixel(x + 1000, y + 1000, Color.FromArgb(255, (int)point, (int)point, (int)point));
                }
            }

            bitmap.Save("test_image.png");



            SaveData.Initialise("test");


            

            Console.WriteLine();

            //Utils.RecursiveWrite(kb.ToJson());


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
