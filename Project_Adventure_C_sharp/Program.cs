namespace Project_Adventure_C_sharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var attributes = new List<Attributes>() { Attributes.Rare };
            var drops = new List<(Items, int)>() { (Items.Steel_arrow, 5) };
            var entity = new Entity("you", 12, 1, 1515, 69, 0, false, attributes, drops);
            Tools.encode_save_s(entity.To_json(), "save");
            Console.WriteLine(Directory.GetCurrentDirectory());
            Console.ReadKey();
        }
    }
}
