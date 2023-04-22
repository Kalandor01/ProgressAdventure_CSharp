using ProgressAdventure.Enums;
using System.Diagnostics;
using Attribute = ProgressAdventure.Enums.Attribute;

namespace ProgressAdventure.Entity
{
    public static class EntityUtils
    {
        #region Config dictionaries
        /// <summary>
        /// The dictionary pairing up facing types, to their vaector equivalents.
        /// </summary>
        public static readonly Dictionary<Facing, (int x, int y)> facingToMovementVectorMapping = new()
        {
            [Facing.NORTH] = (0, 1),
            [Facing.SOUTH] = (0, -1),
            [Facing.WEST] = (-1, 0),
            [Facing.EAST] = (1, 0),
            [Facing.NORTH_WEST] = (-1, 1),
            [Facing.NORTH_EAST] = (1, 1),
            [Facing.SOUTH_WEST] = (-1, -1),
            [Facing.SOUTH_EAST] = (1, -1)
        };
        #endregion

        #region Public fuctions
        /// <summary>
        /// Function to create the stats for an entity object.<br/>
        /// All values calculated from ranges will be calcualted with a trangular distribution. 
        /// </summary>
        /// <param name="baseHp">The base HP of the entity.</param>
        /// <param name="baseAttack">The base attack damage of the entity.</param>
        /// <param name="baseDefence">The base defence value of the entity.</param>
        /// <param name="baseSpeed">The base speed of the entity.</param>
        /// <param name="negativeFluctuation">The value, that will offset all of the base stat values, it the negative direction.</param>
        /// <param name="positiveFluctuation">The value, that will offset all of the base stat values, it the positive direction.</param>
        /// <param name="rareChance">The chance of the entitiy having the rare attribute. (1 = 100%)</param>
        /// <param name="originalTeam">The original team of the entity.</param>
        /// <param name="teamChangeChange">The chance of the entitiy changing its team to the player's team. (1 = 100%)</param>
        /// <param name="name">The name of the entity. If its null, the name of the calling class will be used, as the name.</param>
        public static (string name, int baseHpValue, int baseAttackValue, int baseDefenceValue, int baseSpeedValue, int originalTeam, int currentTeam, List<Attribute> attributes) EntityManager(
            int baseHp,
            int baseAttack,
            int baseDefence,
            int baseSpeed,
            int negativeFluctuation = 2,
            int positiveFluctuation = 3,
            double rareChance = 0.02,
            int originalTeam = 1,
            double teamChangeChange = 0.005,
            string? name = null
        )
        {
            if (name == null)
            {
                try
                {
                    var frame = new StackTrace().GetFrame(1);
                    var method = frame.GetMethod();
                    name = method.ReflectedType.Name;
                }
                catch (NullReferenceException)
                {
                    Logger.Log("Trying to create entity with no known name", "Using default name", LogSeverity.WARN);
                    name = "[Unknown entity]";
                }
            }
            return EntityManager(
                (baseHp - negativeFluctuation, baseHp, baseHp + positiveFluctuation),
                (baseAttack - negativeFluctuation, baseAttack, baseAttack + positiveFluctuation),
                (baseDefence - negativeFluctuation, baseDefence, baseDefence + positiveFluctuation),
                (baseSpeed - negativeFluctuation, baseSpeed, baseSpeed + positiveFluctuation),
                rareChance,
                originalTeam,
                teamChangeChange,
                name
            );
        }

        /// <summary>
        /// Function to create the stats for an entity object.<br/>
        /// All values calculated from ranges will be calcualted with a trangular distribution. 
        /// </summary>
        /// <param name="baseHp">The base HP range of the entity.</param>
        /// <param name="baseAttack">The base attack damage range of the entity.</param>
        /// <param name="baseDefence">The base defence value range of the entity.</param>
        /// <param name="baseSpeed">The base speed range of the entity.</param>
        /// <param name="rareChance">The chance of the entitiy having the rare attribute. (1 = 100%)</param>
        /// <param name="originalTeam">The original team of the entity.</param>
        /// <param name="teamChangeChange">The chance of the entitiy changing its team to the player's team. (1 = 100%)</param>
        /// <param name="name">The name of the entity. If its null, the name of the calling class will be used, as the name.</param>
        public static (string name, int baseHpValue, int baseAttackValue, int baseDefenceValue, int baseSpeedValue, int originalTeam, int currentTeam, List<Attribute> attributes) EntityManager(
            (int lower, int middle, int upper) baseHp,
            (int lower, int middle, int upper) baseAttack,
            (int lower, int middle, int upper) baseDefence,
            (int lower, int middle, int upper) baseSpeed,
            double rareChance = 0.02,
            int originalTeam = 1,
            double teamChangeChange = 0.005,
            string? name = null
        )
        {
            if (name is null)
            {
                try
                {
                    var frame = new StackTrace().GetFrame(1);
                    var method = frame.GetMethod();
                    name = method.ReflectedType.Name;
                }
                catch (NullReferenceException)
                {
                    Logger.Log("Trying to create entity with no known name", "Using default name", LogSeverity.WARN);
                    name = "[Unknown entity]";
                }
            }
            name = name.Replace("_", " ");
            var baseHpValue = ConfigureStat(baseHp);
            var baseAttackValue = ConfigureStat(baseAttack);
            var baseDefenceValue = ConfigureStat(baseDefence);
            var baseSpeedValue = ConfigureStat(baseSpeed);
            var attributes = new List<Attribute>();
            if (SaveData.MainRandom.GenerateDouble() < rareChance)
            {
                attributes.Add(Attribute.RARE);
            }
            if (baseHpValue <= 0)
            {
                baseHpValue = 1;
            }
            // team
            int currentTeam = originalTeam;
            if (SaveData.MainRandom.GenerateDouble() < teamChangeChange)
            {
                currentTeam = 0;
            }
            return (name, baseHpValue, baseAttackValue, baseDefenceValue, baseSpeedValue, originalTeam, currentTeam, attributes);
        }

        /// <summary>
        /// Converts the vector into the equivalent <c>Facing</c> enum, if there is one.<br/>
        /// Otherwise returns null.
        /// </summary>
        /// <param name="vector">The movement vector.</param>
        public static Facing? MovementVectorToFacing((int x, int y) vector)
        {
            if (facingToMovementVectorMapping.ContainsValue(vector))
            {
                return facingToMovementVectorMapping.First(facing => facing.Value == vector).Key;
            }
            return null;
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Returns the actual value of the stat, roller using a triangular distribution, from the range.<br/>
        /// The returned value cannot be less than 0.
        /// </summary>
        /// <param name="statRange">The range of the values to use.</param>
        private static int ConfigureStat((int lower, int middle, int upper) statRange)
        {
            int statValue;
            // fluctuation
            if (statRange.lower == statRange.upper)
            {
                statValue = statRange.lower;
            }
            else
            {
                statValue = (int)Math.Round(SaveData.MainRandom.Triangular(statRange.lower, statRange.middle, statRange.upper));
            }
            if (statValue < 0)
            {
                statValue = 0;
            }
            return statValue;
        }
        #endregion
    }
}
