namespace ProgressAdventure
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
        string name;
        /// <summary>
        /// The full name of the <c>Entity</c>.
        /// </summary>
        string fullName = "";
        /// <summary>
        /// The base hp of the <c>Entity</c>.
        /// </summary>
        int baseHp;
        /// <summary>
        /// The current hp of the <c>Entity</c>.
        /// </summary>
        int hp;
        /// <summary>
        /// The base attack of the <c>Entity</c>.
        /// </summary>
        int baseAttack;
        /// <summary>
        /// The current attack of the <c>Entity</c>.
        /// </summary>
        int attack;
        /// <summary>
        /// The base defence of the <c>Entity</c>.
        /// </summary>
        int baseDefence;
        /// <summary>
        /// The current defence of the <c>Entity</c>.
        /// </summary>
        int defence;
        /// <summary>
        /// The base speed of the <c>Entity</c>.
        /// </summary>
        int baseSpeed;
        /// <summary>
        /// The current speed of the <c>Entity</c>.
        /// </summary>
        int speed;
        /// <summary>
        /// The original team that the <c>Entity</c> is a part of.
        /// </summary>
        int originalTeam;
        /// <summary>
        /// The current team that the <c>Entity</c> is a part of.
        /// </summary>
        int team;
        /// <summary>
        /// The list of attributes that the <c>Entity</c> has.
        /// </summary>
        IEnumerable<Enums.Attribute> attributes;
        /// <summary>
        /// The list of items that the <c>Entity</c> will drop on death.
        /// </summary>
        IEnumerable<Item> drops;
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
        public Entity(string name, int baseHp, int baseAttack, int baseDefence, int baseSpeed, int originalTeam = 0, int? team = null, IEnumerable<Enums.Attribute>? attributes = null, IEnumerable<Item>? drops = null)
        {
            this.name = name;
            this.baseHp = baseHp;
            this.baseAttack = baseAttack;
            this.baseDefence = baseDefence;
            this.baseSpeed = baseSpeed;
            this.originalTeam = originalTeam;
            this.team = (int)(team is not null ? team : this.originalTeam);
            this.attributes = attributes is not null ? attributes : Enumerable.Empty<Enums.Attribute>();
            this.drops = drops is not null ? drops : Enumerable.Empty<Item>();
            // adjust properties
            SetupAttributes();
            UpdateFullName();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Updates the full name of the entity.
        /// </summary>
        public void UpdateFullName()
        {
            string fullName = name;
            if (attributes.Contains(Enums.Attribute.Rare))
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
            // drops
            var dropsJson = new List<Dictionary<string, object>>();
            foreach (var drop in drops)
                dropsJson.Add(new Dictionary<string, object> {
                    {"type", drop.type.ToString()},
                    {"amount", drop.amount}
                });
            // attributes processing
            var attributesProcessed = new List<string>();
            foreach (var attribute in attributes)
            {
                attributesProcessed.Add(attribute.ToString());
            }
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
            if (attributes.Contains(Enums.Attribute.Rare))
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
