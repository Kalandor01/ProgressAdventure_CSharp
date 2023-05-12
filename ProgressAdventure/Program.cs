using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement;
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
            SaveManager.CreateSaveData("test", "me");

            World.GenerateChunk((0, 0));
            World.GenerateChunk((-42, -9));
            World.GenerateChunk((9, 342));
            World.GenerateChunk((0, 10));

            //var ch = World.Chunks;
            //var pl = SaveData.player;

            //SaveManager.MakeSave(false, "Loading...");

            //var sn = SaveData.saveName;

            //SaveManager.LoadSave(sn);
            //World.LoadAllChunksFromFolder();

            //var ch2 = World.Chunks;
            //var pl2 = SaveData.player;

            //var s1 = Chunk.GetChunkRandom((0, 0));

            //var s2 = Chunk.GetChunkRandom((1, 8));

            //var s3 = Chunk.GetChunkRandom((1, -15));

            //var s4 = Chunk.GetChunkRandom((-4523, 17214232));

            ulong seed = 10;

            var sampleSize = 2048;

            var noise = RandomStates.TileTypeNoiseGenerators[TileNoiseType.HEIGHT];

            var bitmap = new Bitmap(sampleSize, sampleSize);

            for (long x = -1000; x < bitmap.Width - 1000; x++)
            {
                for (long y = -1000; y < bitmap.Height - 1000; y++)
                {
                    var point = noise.Generate(x, y, 16.0 / Constants.TILE_NOISE_DIVISION) * 1;
                    point += noise.Generate(x, y, 8.0 / Constants.TILE_NOISE_DIVISION) * 2;
                    point += noise.Generate(x, y, 4.0 / Constants.TILE_NOISE_DIVISION) * 4;
                    point += noise.Generate(x, y, 2.0 / Constants.TILE_NOISE_DIVISION) * 8;
                    point += noise.Generate(x, y, 1.0 / Constants.TILE_NOISE_DIVISION) * 16;
                    point /= 31;
                    var val = point;
                    point *= 255;
                    bitmap.SetPixel((int)x + 1000, (int)y + 1000, Color.FromArgb(255, (int)point, (int)point, (int)point));
                    //Console.WriteLine(val);
                }
            }

            bitmap.Save("test_image.png");

            var v = WorldUtils.GetNoiseValues(0, 0);


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
