using Attribute = ProgressAdventure.Enums.Attribute;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// DTO used for storing stats returned by the EntityManager function.
    /// </summary>
    public class EntityManagerStatsDTO
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
        /// <inheritdoc cref="EntityManagerStatsDTO"/>
        /// </summary>
        /// <param name="baseMaxHp"></param>
        /// <param name="baseAttack"></param>
        /// <param name="baseDefence"></param>
        /// <param name="baseAgility"></param>
        /// <param name="originalTeam"></param>
        /// <param name="currentTeam"></param>
        /// <param name="attributes"></param>
        public EntityManagerStatsDTO(
            int baseMaxHp,
            int baseAttack,
            int baseDefence,
            int baseAgility,
            int originalTeam = 1,
            int? currentTeam = null,
            List<Attribute>? attributes = null
        )
        {
            this.baseMaxHp = baseMaxHp;
            this.baseAttack = baseAttack;
            this.baseDefence = baseDefence;
            this.baseAgility = baseAgility;
            this.originalTeam = originalTeam;
            this.currentTeam = currentTeam ?? originalTeam;
            this.attributes = attributes ?? new List<Attribute>();
        }
        #endregion
    }
}
