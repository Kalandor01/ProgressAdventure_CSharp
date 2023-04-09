using ProjectAdventure.Enums;
using System.Text;
using System.Drawing;

namespace ProjectAdventure
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Thread.CurrentThread.Name = Constants.MAIN_THREAD_NAME;


            var attributes = new List<Enums.Attribute>() { Enums.Attribute.Rare };
            var drops = new List<Item>() { new Item(ItemType.STEEL_ARROW, 5) };
            var entity = new Entity("you", 12, 1, 1515, 69, 0, null, attributes, drops);
            Tools.EncodeSaveShort(entity.ToJson(), "save");
            var dec = Tools.DecodeSaveShort("save");
            Console.WriteLine(dec);
            Console.WriteLine(dec.Value<string>("name"));

            ulong seed = 10;
            var baseDivision = 200;

            var sampleSize = 2048;

            var noise1 = new PerlinNoise(seed);
            var noise2 = new PerlinNoise(seed);
            var noise3 = new PerlinNoise(seed);
            var noise4 = new PerlinNoise(seed);
            var noise5 = new PerlinNoise(seed);

            var bitmap = new Bitmap(sampleSize, sampleSize);

            for (var x = 0; x < bitmap.Width; x++)
            {
                for (var y = 0; y < bitmap.Height; y++)
                {
                    var point = noise1.Generate(x, y, 16.0 / baseDivision) * 1;
                    point += noise2.Generate(x, y, 8.0 / baseDivision) * 2;
                    point += noise3.Generate(x, y, 4.0 / baseDivision) * 4;
                    point += noise4.Generate(x, y, 2.0 / baseDivision) * 8;
                    point += noise5.Generate(x, y, 1.0 / baseDivision) * 16;
                    point /= 31;
                    point = point * 128 + 128;
                    bitmap.SetPixel(x, y, Color.FromArgb(255, (int)point, (int)point, (int)point));
                }
            }

            bitmap.Save("test_image.png");



            //Logger.Log("test", "message", LogSeverity.ERROR, newLine: true);

            var text = "ŰŰŰŰŰŰŰ";
            Console.WriteLine(Utils.StylizedText(text, Constants.Colors.RED, Constants.Colors.BLUE));
        }
    }
}
