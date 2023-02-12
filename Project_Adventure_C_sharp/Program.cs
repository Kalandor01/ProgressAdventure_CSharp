using Save_File_Manager;
using NPrng;
using NPrng.Generators;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Project_Adventure
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

            var rand = new SplittableRandom(100);
            var rand2 = new SplittableRandom(100);

            var encoding = Encoding.GetEncoding("windows-1250");

            var line = EncodeLine("afkjdshfjFGHJSGJHÉÁŰPÚŐőáüűőáüűőűőűŐŰŐŰŐ", rand, encoding);
            var line_de = DecodeLine(line, rand2, encoding);
            Console.WriteLine(line_de);
        }

        private static IEnumerable<byte> EncodeLine(string line, AbstractPseudoRandomGenerator rand, Encoding encoding)
        {
            var encode64 = rand.GenerateInRange(2, 5);
            // encoding into bytes
            var lineEnc = encoding.GetBytes(line);
            // change encoding to utf-8
            var lineUtf8 = Encoding.Convert(encoding, Encoding.UTF8, lineEnc);
            // encode to base64 x times
            var lineBase64 = lineUtf8;
            for (int x = 0; x < encode64; x++)
            {
                lineBase64 = Encoding.UTF8.GetBytes(Convert.ToBase64String(lineBase64));
            }
            // shufling bytes
            var lineEncoded = new List<byte>();
            foreach (var byteBase64 in lineBase64)
            {
                var modByte = (byte)(byteBase64 + (int)rand.GenerateInRange(-32, 134));
                lineEncoded.Add(modByte);
            }
            // + \n
            lineEncoded.Add(10);
            return lineEncoded;
        }

        private static string DecodeLine(IEnumerable<byte> bytes, AbstractPseudoRandomGenerator rand, Encoding encoding)
        {
            var encode64 = rand.GenerateInRange(2, 5);
            // deshufling bytes
            var lineDecoded = new List<byte>();
            foreach (var lineByte in bytes)
            {
                if (lineByte != 10)
                {
                    var modByte = (byte)(lineByte - (int)rand.GenerateInRange(-32, 134));
                    lineDecoded.Add(modByte);
                }
            }
            // encode to base64 x times
            var lineUtf8 = lineDecoded.ToArray();
            for (int x = 0; x < encode64; x++)
            {
                lineUtf8 = Convert.FromBase64String(Encoding.UTF8.GetString(lineUtf8.ToArray()));
            }
            // change encoding from utf-8
            var lineBytes = Encoding.Convert(Encoding.UTF8, encoding, lineUtf8);
            // decode into string
            return encoding.GetString(lineBytes);
        }
    }
}
