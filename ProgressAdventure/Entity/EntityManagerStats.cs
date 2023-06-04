using Attribute = ProgressAdventure.Enums.Attribute;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// Struct used for storing stats returned by the EntityManager function.
    /// </summary>
    public struct EntityManagerStats
    {
        #region Fields
        public int baseMaxHp;
        public int baseAttack;
        public int baseDefence;
        public int baseAgility;
        public int originalTeam;
        public int currentTeam;
        public List<Attribute> attributes;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="EntityManagerStats"/>
        /// </summary>
        public EntityManagerStats()
            : this(1, 1, 1, 1, 1, 1, new List<Attribute>()) { }

        /// <summary>
        /// <inheritdoc cref="EntityManagerStats"/>
        /// </summary>
        /// <param name="baseMaxHp"></param>
        /// <param name="baseAttack"></param>
        /// <param name="baseDefence"></param>
        /// <param name="baseAgility"></param>
        /// <param name="originalTeam"></param>
        /// <param name="currentTeam"></param>
        /// <param name="attributes"></param>
        public EntityManagerStats(
            int baseMaxHp,
            int baseAttack,
            int baseDefence,
            int baseAgility,
            int originalTeam,
            int currentTeam,
            List<Attribute> attributes
        )
        {
            this.baseMaxHp = baseMaxHp;
            this.baseAttack = baseAttack;
            this.baseDefence = baseDefence;
            this.baseAgility = baseAgility;
            this.originalTeam = originalTeam;
            this.currentTeam = currentTeam;
            this.attributes = attributes;
        }
        #endregion
    }
}
