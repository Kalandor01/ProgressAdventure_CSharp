using ProgressAdventure.ItemManagement;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// A representation of an entity.
    /// </summary>
    public class Entity
    {
        #region Public fields
        /// <summary>
        /// The name of the <c>Entity</c>.
        /// </summary>
        public string name;
        /// <summary>
        /// The full name of the <c>Entity</c>.
        /// </summary>
        public string fullName = "";
        /// <summary>
        /// The base hp of the <c>Entity</c>.
        /// </summary>
        public int baseHp;
        /// <summary>
        /// The current hp of the <c>Entity</c>.
        /// </summary>
        public int hp;
        /// <summary>
        /// The base attack of the <c>Entity</c>.
        /// </summary>
        public int baseAttack;
        /// <summary>
        /// The current attack of the <c>Entity</c>.
        /// </summary>
        public int attack;
        /// <summary>
        /// The base defence of the <c>Entity</c>.
        /// </summary>
        public int baseDefence;
        /// <summary>
        /// The current defence of the <c>Entity</c>.
        /// </summary>
        public int defence;
        /// <summary>
        /// The base speed of the <c>Entity</c>.
        /// </summary>
        public int baseSpeed;
        /// <summary>
        /// The current speed of the <c>Entity</c>.
        /// </summary>
        public int speed;
        /// <summary>
        /// The original team that the <c>Entity</c> is a part of.
        /// </summary>
        public int originalTeam;
        /// <summary>
        /// The current team that the <c>Entity</c> is a part of.
        /// </summary>
        public int team;
        /// <summary>
        /// The list of attributes that the <c>Entity</c> has.
        /// </summary>
        public List<Enums.Attribute> attributes;
        /// <summary>
        /// The list of items that the <c>Entity</c> will drop on death.
        /// </summary>
        public List<Item> drops;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Entity"/>
        /// </summary>
        /// <param name="name"><inheritdoc cref="name" path="//summary"/></param>
        /// <param name="baseHp"><inheritdoc cref="baseHp" path="//summary"/></param>
        /// <param name="baseAttack"><inheritdoc cref="baseAttack" path="//summary"/></param>
        /// <param name="baseDefence"><inheritdoc cref="baseDefence" path="//summary"/></param>
        /// <param name="baseSpeed"><inheritdoc cref="baseSpeed" path="//summary"/></param>
        /// <param name="originalTeam"><inheritdoc cref="originalTeam" path="//summary"/></param>
        /// <param name="team"><inheritdoc cref="team" path="//summary"/></param>
        /// <param name="attributes"><inheritdoc cref="attributes" path="//summary"/></param>
        /// <param name="drops"><inheritdoc cref="drops" path="//summary"/></param>
        public Entity(string name, int baseHp, int baseAttack, int baseDefence, int baseSpeed, int originalTeam = 0, int? team = null, List<Enums.Attribute>? attributes = null, List<Item>? drops = null)
        {
            this.name = name;
            this.baseHp = baseHp;
            this.baseAttack = baseAttack;
            this.baseDefence = baseDefence;
            this.baseSpeed = baseSpeed;
            this.originalTeam = originalTeam;
            this.team = (int)(team is not null ? team : this.originalTeam);
            this.attributes = attributes is not null ? attributes : new List<Enums.Attribute>();
            this.drops = drops is not null ? drops : new List<Item>();
            // adjust properties
            SetupAttributes();
            UpdateFullName();
        }

        /// <summary>
        /// <inheritdoc cref="Entity"/>
        /// </summary>
        /// <param name="stats">The tuple of stats, representin all other values from the other constructor, other than drops.</param>
        /// <param name="drops"><inheritdoc cref="drops" path="//summary"/></param>
        public Entity((string name, int baseHp, int baseAttack, int baseDefence, int baseSpeed, int originalTeam, int? team, List<Enums.Attribute>? attributes) stats, List<Item>? drops = null)
            :this(stats.name, stats.baseHp, stats.baseAttack, stats.baseDefence, stats.baseSpeed, stats.originalTeam, stats.team, stats.attributes, drops) { }
        #endregion

        #region Public methods
        /// <summary>
        /// Updates the full name of the entity.
        /// </summary>
        public void UpdateFullName()
        {
            string fullName = name;
            if (attributes.Contains(Enums.Attribute.RARE))
            {
                fullName = "Rare " + fullName;
            }
            this.fullName = fullName;
        }

        /// <summary>
        /// Returns a json representation of the <c>Entity</c>.
        /// </summary>
        public Dictionary<string, object?> ToJson()
        {
            // attributes
            var attributesProcessed = attributes.Select(a => a.ToString()).ToList();
            // drops
            var dropsJson = drops.Select(drop => new Dictionary<string, object> {
                    {"type", drop.Type.ToString()},
                    {"amount", drop.amount}
                }).ToList();
            // properties
            var entityJson = new Dictionary<string, object?> {
                {"name", name},
                {"baseHp", baseHp},
                {"baseAttack", baseAttack},
                {"baseDefence", baseDefence},
                {"baseSpeed", baseSpeed},
                {"originalTeam", originalTeam},
                {"team", team},
                {"attributes", attributesProcessed},
                {"drops", dropsJson}
            };
            return entityJson;
        }

        /// <summary>
        /// Makes the entity attack another one.
        /// </summary>
        /// <param name="target">The target entity.</param>
        public void Attack(Entity target)
        {
            target.hp -= attack;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Sets up the entity's stats acording to its base attributes.
        /// </summary>
        private void SetupAttributes()
        {
            hp = baseHp;
            attack = baseAttack;
            defence = baseDefence;
            speed = baseSpeed;
            if (attributes.Contains(Enums.Attribute.RARE))
            {
                hp *= 2;
                attack *= 2;
                defence *= 2;
                speed *= 2;
            }
        }
        #endregion

        #region Public overrides
        public override string ToString()
        {
            var originalTeamStr = originalTeam == 0 ? "Player" : originalTeam.ToString();
            var teamStr = team == 0 ? "Player" : team.ToString();
            return $"Name: {name}\nFull name: {fullName}\nHp: {hp}\nAttack: {attack}\nDefence: {defence}\nSpeed: {speed}\nAttributes: {attributes}\nOriginal team: {originalTeamStr}\nCurrent team: {teamStr}\nDrops: {drops}";
        }
        #endregion
    }
}
