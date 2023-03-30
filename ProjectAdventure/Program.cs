using SaveFileManager;
using System.Text;

namespace ProjectAdventure
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var attributes = new List<Attributes>() { Attributes.Rare };
            //var drops = new List<(Items, int)>() { (Items.Steel_arrow, 5) };
            //var entity = new Entity("you", 12, 1, 1515, 69, 0, false, attributes, drops);
            //Tools.encode_save_s(entity.To_json(), "save");
            //Console.WriteLine(Directory.GetCurrentDirectory());
            //Console.ReadKey();

            //var benis = Encoding.Latin1.GetBytes(new char[] { Console.ReadKey(true).KeyChar });
            //foreach (var item in benis)
            //{
            //    byte gg = 125;
            //    Console.WriteLine(item);
            //}
            //var seed = 3;
            //for (int x = 0; x < 1000; x++)
            //{
            //    var xa = new BigInteger(Math.Abs(x));
            //    var pi = new BigInteger(Math.PI);
            //    var num = SqrtFast(BigInteger.Pow(xa * pi, 73) * (new BigInteger(713853.587) + xa * pi));
            //    var max = new BigInteger(ulong.MaxValue);
            //    max++;
            //    var num2 = (ulong)(num % max);
            //    Console.WriteLine(num2);
            //}

            //EncodeFile("hahalala\nlol");
            //DecodeFile();

            //Directory.CreateDirectory("file/path/deep");

            Console.OutputEncoding = Encoding.UTF8;

            var testLines = new List<string> {
                "testing if this makes a linebreak happen?\néáűúőüóö\n;>Ł$ß¤×÷¸¨<>##@&{@{",
                "éá山ā人é口ŏ刀ā木ù日ì月è日女ǚ子ĭ馬马ǎ鳥鸟ǎ目ù水ǐǐì指事īī一ī二è三ā大à人天ā大小ǎ上à下à本ě木末"
            };
            foreach (var item in testLines)
            {
                Console.WriteLine(item);
            }
            FileConversion.EncodeFile(testLines);
            var lines = FileConversion.DecodeFile();
            foreach (var item in lines)
            {
                Console.WriteLine(item);
            }
            for (var x=0; x < lines.Count(); x++)
            {
                Console.WriteLine(testLines.ElementAt(x) == lines.ElementAt(x));
            }
        }
    }
}
