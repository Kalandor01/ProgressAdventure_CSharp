namespace ProjectAdventure
{
    public class Entity
    {
        string name;
        string full_name = "";
        int base_hp;
        int hp;
        int base_attack;
        int attack;
        int base_defence;
        int defence;
        int base_speed;
        int speed;
        int team;
        bool switched;
        List<Attributes> attributes;
        List<(Items type, int amount)> drops;


        public Entity()
            : this("test")
        {
        }
        public Entity(string name)
            : this(name, 10, 10, 10, 10, 0, false, new List<Attributes>())
        {
        }
        public Entity(string name, int base_hp, int base_attack, int base_defence, int base_speed, int team, bool switched, List<Attributes> attributes)
            : this(name, base_hp, base_attack, base_defence, base_speed, team, switched, attributes, new List<(Items, int)>())
        {
        }
        public Entity(string name, int base_hp, int base_attack, int base_defence, int base_speed, int team, bool switched, List<Attributes> attributes, List<(Items, int)> drops)
        {
            this.name = name;
            this.base_hp = base_hp;
            this.base_attack = base_attack;
            this.base_defence = base_defence;
            this.base_speed = base_speed;
            this.team = team;
            this.switched = switched;
            this.attributes = attributes;
            this.drops = drops;
            // adjust properties
            this.Apply_attributes();
            this.Update_full_name();
        }

        /// <summary>
        /// Modifys the entity's stats acording to the entity's attributes.
        /// </summary>
        private void Apply_attributes()
        {
            this.hp = this.base_hp;
            this.attack = this.base_attack;
            this.defence = this.base_defence;
            this.speed = this.base_speed;
            if (this.attributes.Contains(Attributes.Rare))
            {
                this.hp *= 2;
                this.attack *= 2;
                this.defence *= 2;
                this.speed *= 2;
            }
        }

        /// <summary>
        /// Updates the full name of the entity.
        /// </summary>
        public void Update_full_name()
        {
            string full_name = this.name;
            if (this.attributes.Contains(Attributes.Rare))
            {
                full_name = "Rare " + full_name;
            }
            this.full_name = full_name;
        }

        /// <summary>
        /// Returns a json representation of the `Entity`.
        /// </summary>
        public Dictionary<string, object> To_json()
        {
            // drops
            var drops_json = new List<Dictionary<string, object>>();
            foreach (var item in this.drops)
                drops_json.Add(new Dictionary<string, object> {
                    {"type", item.type.ToString()},
                    {"amount", item.amount}
                });
            // attributes processing
            var attributes_processed = new List<string>();
            foreach (var attribute in this.attributes)
            {
                attributes_processed.Add(attribute.ToString());
            }
            // properties
            var entity_json = new Dictionary<string, object> {
                {"name", this.name},
                {"base_hp", this.base_hp},
                {"base_attack", this.base_attack},
                {"base_defence", this.base_defence},
                {"base_speed", this.base_speed},
                {"team", this.team},
                {"switched", this.switched},
                {"attributes", attributes_processed},
                {"drops", drops_json}
            };
            return entity_json;
        }

        public override string ToString()
        {
            var team = this.team == 0 ? "Player" : this.team.ToString();
            return $"Name: {this.name}\nFull name: {this.full_name}\nHp: {this.hp}\nAttack: {this.attack}\nDefence: {this.defence}\nSpeed: {this.speed}\nAttributes: {this.attributes}\nTeam: {team}\nSwitched sides: {this.switched}\nDrops: {this.drops}";
        }

        public void attack_entity(Entity target)
        {
            target.hp -= this.attack;
        }
    }
}
