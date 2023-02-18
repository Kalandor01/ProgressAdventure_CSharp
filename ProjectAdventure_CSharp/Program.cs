using SaveFileManager;
using NPrng;
using NPrng.Generators;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Numerics;
using System.IO;

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

            fileConversion.EncodeFile(new List<string> { "testing if this makes a linebreak happen?\néáűúőüóö\n;>Ł$ß¤×÷¸¨<>##@&{@{"});
            var lines = fileConversion.DecodeFile();
            foreach (var item in lines)
            {
                Console.WriteLine(item);
            }
        }
    }
}
