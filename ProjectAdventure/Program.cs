using ProjectAdventure.Enums;
using System.Text;

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

            //Logger.Log("test", "message", LogSeverity.ERROR, newLine: true);

            var text = "ŰŰŰŰŰŰŰ";
            Console.WriteLine(Utils.StylizedText(text, Constants.Colors.RED, Constants.Colors.BLUE));
        }
    }
}
