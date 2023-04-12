using ProjectAdventure.Enums;
using System.Text;
using System.Drawing;
using ProjectAdventure.Settings;
using System.Collections;
using SaveFileManager;

namespace ProjectAdventure
{
    internal class Program
    {
        static void MainFunction()
        {
            //var attributes = new List<Enums.Attribute>() { Enums.Attribute.Rare };
            //var drops = new List<Item>() { new Item(ItemType.STEEL_ARROW, 5) };
            //var entity = new Entity("you", 12, 1, 1515, 69, 0, null, attributes, drops);
            //Tools.EncodeSaveShort(entity.ToJson(), "save");
            //var dec = Tools.DecodeSaveShort("save");
            //Console.WriteLine(dec);
            //Console.WriteLine(dec.Value<string>("name"));

            //ulong seed = 10;
            //var baseDivision = 200;

            //var sampleSize = 2048;

            //var noise1 = new PerlinNoise(seed);
            //var noise2 = new PerlinNoise(seed);
            //var noise3 = new PerlinNoise(seed);
            //var noise4 = new PerlinNoise(seed);
            //var noise5 = new PerlinNoise(seed);

            //var bitmap = new Bitmap(sampleSize, sampleSize);

            //for (var x = 0; x < bitmap.Width; x++)
            //{
            //    for (var y = 0; y < bitmap.Height; y++)
            //    {
            //        var point = noise1.Generate(x, y, 16.0 / baseDivision) * 1;
            //        point += noise2.Generate(x, y, 8.0 / baseDivision) * 2;
            //        point += noise3.Generate(x, y, 4.0 / baseDivision) * 4;
            //        point += noise4.Generate(x, y, 2.0 / baseDivision) * 8;
            //        point += noise5.Generate(x, y, 1.0 / baseDivision) * 16;
            //        point /= 31;
            //        point = point * 128 + 128;
            //        bitmap.SetPixel(x, y, Color.FromArgb(255, (int)point, (int)point, (int)point));
            //    }
            //}

            //bitmap.Save("test_image.png");



            //Logger.Log("test", "message", LogSeverity.ERROR, newLine: true);

            //var text = "ŰŰŰŰŰŰŰ";
            //Console.WriteLine(Utils.StylizedText(text, Constants.Colors.RED, Constants.Colors.BLUE));

            //var ck = new List<ActionKey>();
            //foreach (var aType in Enum.GetValues(typeof(ActionType)))
            //{
            //    var key1 = Console.ReadKey(true);
            //    var key2 = Console.ReadKey(true);
            //    ck.Add(new ActionKey((ActionType)aType, new List<ConsoleKeyInfo> { key1, key2 }));
            //}
            var kb = new Keybinds(KeybindUtils.GetDefaultKeybindList());

            Utils.RecursiveWrite(kb.ToJson());

            Tools.EncodeSaveShort((IDictionary)kb.ToJson(), "kbsave");
            var kbJson = Tools.DecodeSaveShort("kbsave");

            var kb2 = Keybinds.KeybindsFromJson(kbJson.Root);
            Utils.RecursiveWrite(kb2.ToJson());
        }

        static void ErrorHandler()
        {
            // general crash handler (release only)

            bool exitGame;
            do
            {
                exitGame = true;
                try
                {
                    Logger.LogNewLine();
                    Logger.Log("Beginning new instance");
                    MainFunction();
                    //exit
                    Logger.Log("Instance ended succesfuly");
                }
                catch (Exception ex)
                {
                    Logger.Log("Instance crashed", ex.ToString(), LogSeverity.FATAL);
                    if (Constants.ERROR_HANDLING)
                    {
                        Console.WriteLine("ERROR: " + ex.Message);
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
            Console.OutputEncoding = Encoding.UTF8;

            Thread.CurrentThread.Name = Constants.MAIN_THREAD_NAME;

            ErrorHandler();
        }
    }
}
