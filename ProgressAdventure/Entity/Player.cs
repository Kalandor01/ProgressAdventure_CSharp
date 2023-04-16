using Attribute = ProgressAdventure.Enums.Attribute;

namespace ProgressAdventure.Entity
{
    public class Player : Entity
    {
        public Player(string name = "You")
            : base(name, 1, 2, 3, 4, 0, 1, new List<Attribute>(), new List<Item>()) { }
    }
}
