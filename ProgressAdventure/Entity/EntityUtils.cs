using ProgressAdventure.Enums;
using System.Diagnostics;
using Attribute = ProgressAdventure.Enums.Attribute;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// Utils for entities.
    /// </summary>
    public static class EntityUtils
    {
        #region Config dictionaries
        /// <summary>
        /// The dictionary pairing up facing types, to their vector equivalents.
        /// </summary>
        internal static readonly Dictionary<Facing, (int x, int y)> facingToMovementVectorMapping = new()
        {
            [Facing.NORTH] = (0, 1),
            [Facing.SOUTH] = (0, -1),
            [Facing.WEST] = (-1, 0),
            [Facing.EAST] = (1, 0),
            [Facing.NORTH_WEST] = (-1, 1),
            [Facing.NORTH_EAST] = (1, 1),
            [Facing.SOUTH_WEST] = (-1, -1),
            [Facing.SOUTH_EAST] = (1, -1),
        };
        /// <summary>
        /// The dictionary pairing up attribute types, to stat modifiers.
        /// </summary>
        internal static readonly Dictionary<Attribute, (double maxHp, double attack, double defence, double speed)> attributeStatChangeMap = new()
        {
            [Attribute.RARE] = (2, 2, 2, 2),
        };
        #endregion

        #region Public fuctions
        /// <summary>
        /// Function to create the stats for an entity object.<br/>
        /// All values calculated from ranges will be calcualted with a trangular distribution. 
        /// </summary>
        /// <param name="baseMaxHp">The base max HP of the entity.</param>
        /// <param name="baseAttack">The base attack damage of the entity.</param>
        /// <param name="baseDefence">The base defence value of the entity.</param>
        /// <param name="baseSpeed">The base speed of the entity.</param>
        /// <param name="negativeFluctuation">The value, that will offset all of the base stat values, it the negative direction.</param>
        /// <param name="positiveFluctuation">The value, that will offset all of the base stat values, it the positive direction.</param>
        /// <param name="rareChance">The chance of the entitiy having the rare attribute. (1 = 100%)</param>
        /// <param name="originalTeam">The original team of the entity.</param>
        /// <param name="teamChangeChange">The chance of the entitiy changing its team to the player's team. (1 = 100%)</param>
        public static (
            int baseMaxHpValue,
            int baseAttackValue,
            int baseDefenceValue,
            int baseSpeedValue,
            int originalTeam,
            int currentTeam,
            List<Attribute> attributes
        ) EntityManager(
            int baseMaxHp,
            int baseAttack,
            int baseDefence,
            int baseSpeed,
            int negativeFluctuation = 2,
            int positiveFluctuation = 3,
            double rareChance = 0.02,
            int originalTeam = 1,
            double teamChangeChange = 0.005
        )
        {
            return EntityManager(
                (baseMaxHp - negativeFluctuation, baseMaxHp, baseMaxHp + positiveFluctuation),
                (baseAttack - negativeFluctuation, baseAttack, baseAttack + positiveFluctuation),
                (baseDefence - negativeFluctuation, baseDefence, baseDefence + positiveFluctuation),
                (baseSpeed - negativeFluctuation, baseSpeed, baseSpeed + positiveFluctuation),
                rareChance,
                originalTeam,
                teamChangeChange
            );
        }

        /// <summary>
        /// Function to create the stats for an entity object.<br/>
        /// All values calculated from ranges will be calcualted with a trangular distribution. 
        /// </summary>
        /// <param name="baseMaxHp">The base max HP range of the entity.</param>
        /// <param name="baseAttack">The base attack damage range of the entity.</param>
        /// <param name="baseDefence">The base defence value range of the entity.</param>
        /// <param name="baseSpeed">The base speed range of the entity.</param>
        /// <param name="rareChance">The chance of the entitiy having the rare attribute. (1 = 100%)</param>
        /// <param name="originalTeam">The original team of the entity.</param>
        /// <param name="teamChangeChange">The chance of the entitiy changing its team to the player's team. (1 = 100%)</param>
        public static (
            int baseMaxHpValue,
            int baseAttackValue,
            int baseDefenceValue,
            int baseSpeedValue,
            int originalTeam,
            int currentTeam,
            List<Attribute> attributes
        ) EntityManager(
            (int lower, int middle, int upper) baseMaxHp,
            (int lower, int middle, int upper) baseAttack,
            (int lower, int middle, int upper) baseDefence,
            (int lower, int middle, int upper) baseSpeed,
            double rareChance = 0.02,
            int originalTeam = 1,
            double teamChangeChange = 0.005
        )
        {
            var baseMaxHpValue = ConfigureStat(baseMaxHp);
            var baseAttackValue = ConfigureStat(baseAttack);
            var baseDefenceValue = ConfigureStat(baseDefence);
            var baseSpeedValue = ConfigureStat(baseSpeed);
            var attributes = new List<Attribute>();
            if (RandomStates.MainRandom.GenerateDouble() < rareChance)
            {
                attributes.Add(Attribute.RARE);
            }
            if (baseMaxHpValue <= 0)
            {
                baseMaxHpValue = 1;
            }
            // team
            int currentTeam = originalTeam;
            if (RandomStates.MainRandom.GenerateDouble() < teamChangeChange)
            {
                currentTeam = 0;
            }
            return (baseMaxHpValue, baseAttackValue, baseDefenceValue, baseSpeedValue, originalTeam, currentTeam, attributes);
        }

        /// <summary>
        /// Gets the name of the entity from the name of the calling class.
        /// </summary>
        public static string GetEntityNameFromClass()
        {
            string name;
            try
            {
                var frame = new StackTrace().GetFrame(1);
                var method = frame.GetMethod();
                name = method.ReflectedType.Name;
                name = name.Replace("_", " ");
            }
            catch (NullReferenceException)
            {
                Logger.Log("Trying to create entity with no known name", "Using default name", LogSeverity.WARN);
                name = "[Unknown entity]";
            }
            return name;
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
                statValue = (int)Math.Round(RandomStates.MainRandom.Triangular(statRange.lower, statRange.middle, statRange.upper));
            }
            if (statValue < 0)
            {
                statValue = 0;
            }
            return statValue;
        }

        public static void EntityMover(Entity entity, (long x, long y) relativeMovementVector, bool? updateWorld = null, (long x, long y)? startingPosition = null)
        {
            if (startingPosition is not null)
            {
                if (updateWorld is not null)
                {
                    entity.SetPosition(((long x, long y))startingPosition, (bool)updateWorld);
                }
                else
                {
                    entity.SetPosition(((long x, long y))startingPosition);
                }
            }
            (long x, long y) endPosition = (entity.position.x + relativeMovementVector.x, entity.position.y + relativeMovementVector.y);
            while (entity.position != endPosition)
            {
                var xPosDif = entity.position.x - endPosition.x;
                var yPosDif = entity.position.y - endPosition.y;
                var newPos = entity.position;
                if (Math.Abs(xPosDif) > Math.Abs(yPosDif) && xPosDif != 0)
                {
                    newPos.x += xPosDif > 0 ? -1 : 1;
                }
                else if (yPosDif != 0)
                {
                    newPos.y += yPosDif > 0 ? -1 : 1;
                }
                if (updateWorld is not null)
                {
                    entity.SetPosition(newPos, (bool)updateWorld);
                }
                else
                {
                    entity.SetPosition(newPos);
                }
            }
        }

        /// <summary>
        /// Creates teams based on the team number of the entities in the list.
        /// </summary>
        /// <param name="entities">The list of entities, the fight should happen between.</param>
        public static Dictionary<string, List<Entity>> CreateTeams(IEnumerable<Entity> entities)
        {
            var teams = new Dictionary<string, List<Entity>>();
            foreach (var entity in entities)
            {
                var teamName = entity.currentTeam == 0 ? "Player" : entity.currentTeam.ToString();
                if (teams.ContainsKey(teamName))
                {
                    teams[teamName].Add(entity);
                }
                else
                {
                    teams[teamName] = new List<Entity> { entity };
                }
            }
            return teams;
        }

        /// <summary>
        /// Initiates a fight between multiple entities.
        /// </summary>
        /// <param name="entities">The list of entities, the fight should happen between.</param>
        /// <param name="writeOut">Whether to write out, what is happening with the fight.</param>
        public static void Fight(IEnumerable<Entity> entities, bool writeOut = true)
        {
            Logger.Log("Fight log", $"fight initiated with {entities.Count()} entities");
            var teams = CreateTeams(entities);
            ForcedFight(teams, writeOut);
        }

        /// <summary>
        /// Initiates a fight between multiple teams, where the team aliance, doesn't matter.
        /// </summary>
        /// <param name="teams">The teams of entities, the fight should happen between.</param>
        /// <param name="writeOut">Whether to write out, what is happening with the fight.</param>
        public static void ForcedFight(Dictionary<string, List<Entity>> teams, bool writeOut = true)
        {
            var (teamsPrepared, playerTeam) = PrepareTeams(teams);
            if (teamsPrepared.Count == 0)
            {
                Logger.Log("Fight log", "no entities in fight");
                if (writeOut)
                {
                    Console.WriteLine("There is no one to fight.");
                }
            }
            else if (teamsPrepared.Count == 1)
            {
                Logger.Log("Fight log", "only 1 team in fight");
                if (writeOut)
                {
                    Console.WriteLine("There is only 1 team in the fight. There is no reason to fight.");
                }
            }
            else
            {
                UnpreparedFight(teamsPrepared, playerTeam, writeOut);
            }
            Logger.Log("Fight log", "fight ended");
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Filters out invalid teams from the teams list, and returns which team the player is in.
        /// </summary>
        /// <param name="teamsRaw">The teams of entities.</param>
        private static (Dictionary<string, List<Entity>> teams, string? playerTeam) PrepareTeams(Dictionary<string, List<Entity>> teamsRaw)
        {
            string? playerTeam = null;
            var teams = new Dictionary<string, List<Entity>>();
            foreach (var team in teamsRaw)
            {
                if (team.Value.Count > 0)
                {
                    var entityList = new List<Entity>();
                    foreach (var entity in team.Value)
                    {
                        entityList.Add(entity);
                        if (entity.GetType() == typeof(Player))
                        {
                            playerTeam = team.Key;
                        }
                    }
                    teams.Add(team.Key, entityList);
                }
                else
                {
                    Logger.Log("Fight log", $"empty team: {team.Key}", LogSeverity.WARN);
                }
            }
            return (teams, playerTeam);
        }

        /// <summary>
        /// Gets the total number of entities inthe teams list.
        /// </summary>
        /// <param name="teams">The teams of entities.</param>
        private static Dictionary<string, int> GetTeamEntityCounts(Dictionary<string, List<Entity>> teams)
        {
            var teamCounts = new Dictionary<string, int>();
            foreach (var team in teams)
            {
                teamCounts.Add(team.Key, team.Value.Count);
            }
            return teamCounts;
        }

        /// <summary>
        /// Gets the total number of entities inthe teams list.
        /// </summary>
        /// <param name="teamCounts">The entity counts for teams.</param>
        private static int GetTotalEntityCount(Dictionary<string, int> teamCounts)
        {
            int count = 0;
            foreach (var teamCount in teamCounts)
            {
                count += teamCount.Value;
            }
            return count;
        }

        /// <summary>
        /// Creates a fight between multiple teams, but it doesn't check for correctnes of the teams.
        /// </summary>
        /// <param name="teams">The teams of entities, the fight should happen between.</param>
        /// <param name="playerTeam">The team that the player is in, or null.</param>
        /// <param name="writeOut">Whether to write out, what is happening with the fight.</param>
        private static void UnpreparedFight(Dictionary<string, List<Entity>> teams, string? playerTeam, bool writeOut)
        {
            var teamCounts = GetTeamEntityCounts(teams);
            var totalCount = GetTotalEntityCount(teamCounts);
            Logger.Log("Fight log", $"fight started with {teams.Count} teams, and {totalCount} entities");
            // entities write out
            if (writeOut)
            {
                foreach (var team in teams)
                {
                    Console.WriteLine($"\nTeam {team.Key}:\n");
                    foreach (var entity in team.Value)
                    {
                        Console.Write($"\t{entity.FullName}");
                        if (entity.originalTeam != entity.currentTeam)
                        {
                            Console.Write(" (Switched to this side!)");
                        }
                        Console.WriteLine($"\n\tHP: {entity.CurrentHp}\n\tAttack: {entity.Attack}\n\tDefence: {entity.Defence}\n\tSpeed: {entity.Speed}\n");
                    }
                }
                Console.WriteLine();
            }
            // fight
            while (teamCounts.Count > 1)
            {
                for (var teamNum = 0; teamNum < teams.Count; teamNum++)
                {
                    var team = teams.ElementAt(teamNum);
                    for (var entityNum = 0; entityNum < team.Value.Count; entityNum++)
                    {
                        var entity = team.Value[entityNum];
                        if (entity.CurrentHp > 0)
                        {
                            // get target
                            var targetTeamNum = (int)RandomStates.MiscRandom.GenerateInRange(0, teams.Count - 2);
                            if (targetTeamNum >= teamNum)
                            {
                                targetTeamNum++;
                            }
                            var targetTeam = teams.ElementAt(targetTeamNum);
                            Entity targetEntity;
                            do
                            {
                                var targetEntityNum = RandomStates.MiscRandom.GenerateInRange(0, targetTeam.Value.Count - 1);
                                targetEntity = targetTeam.Value.ElementAt((int)targetEntityNum);
                            }
                            while (targetEntity.CurrentHp <= 0);
                            // attack
                            var targetOldHp = targetEntity.CurrentHp;
                            var attackResponse = entity.AttackEntity(targetEntity);
                            if (writeOut)
                            {
                                Console.WriteLine($"{entity.FullName} attacked {targetEntity.FullName}");
                                string? writeText = null;
                                switch (attackResponse)
                                {
                                    case AttackResponse.ENEMY_DOGDED:
                                        writeText = "DODGED!";
                                        break;
                                    case AttackResponse.ENEMY_BLOCKED:
                                        writeText = "BLOCKED!";
                                        break;
                                    case AttackResponse.HIT:
                                        writeText = $"dealt {targetOldHp - targetEntity.CurrentHp} damage ({targetEntity.CurrentHp})";
                                        break;
                                }
                                if (writeText is not null)
                                {
                                    Console.WriteLine(writeText);
                                }
                            }
                            // kill
                            if (attackResponse == AttackResponse.KILLED)
                            {
                                if (writeOut)
                                {
                                    Console.WriteLine($"dealt {targetOldHp - targetEntity.CurrentHp} damage (DEAD)");
                                    Console.WriteLine($"{entity.FullName} defeated {targetEntity.FullName}");
                                }
                                var targetTeamKey = teamCounts.ElementAt(targetTeamNum).Key;
                                teamCounts[targetTeamKey]--;
                                // loot?
                                if (entity.GetType() == typeof(Player))
                                {
                                    ((Player)entity).inventory.Loot(targetEntity, writeOut ? entity.FullName : null);
                                }
                                if (teamCounts[targetTeamKey] <= 0)
                                {
                                    teamCounts.Remove(targetTeamKey);
                                    teams.Remove(targetTeamKey);
                                    Logger.Log("Fight log", $"team {targetTeamKey} defeated");
                                    if (writeOut)
                                    {
                                        Console.WriteLine($"team {targetTeamKey} defeated");
                                    }
                                    if (teamCounts.Count <= 1)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (writeOut)
                            {
                                Thread.Sleep(500);
                            }
                        }
                    }
                    if (teamCounts.Count <= 1)
                    {
                        break;
                    }
                }
            }
            // outcome
            var winTeamName = teamCounts.First().Key;
            if (playerTeam is not null)
            {
                // player team dead
                if (winTeamName != playerTeam)
                {
                    Logger.Log("Fight log", "player team defeated");
                    if (writeOut)
                    {
                        Console.WriteLine($"{SaveData.player.FullName}'s team was defeated");
                    }
                }
                // player team won
                else
                {
                    Logger.Log("Fight log", "player team won");
                    if (writeOut)
                    {
                        Console.WriteLine($"{SaveData.player.FullName}'s team won");
                    }
                    if (SaveData.player.CurrentHp == 0)
                    {
                        Logger.Log("Fight log", "player died");
                        if (writeOut)
                        {
                            Console.WriteLine($"{SaveData.player.FullName} died");
                        }
                    }
                    // loot
                    else
                    {
                        foreach (var team in teams)
                        {
                            foreach (var entity in team.Value)
                            {
                                if (entity.CurrentHp <= 0 && !entity.Equals(SaveData.player))
                                {
                                    SaveData.player.inventory.Loot(entity.drops, writeOut ? SaveData.player.FullName : null);
                                }
                            }
                        }
                    }
                    if (writeOut)
                    {
                        SaveData.player.Stats();
                    }
                }
            }
            else
            {
                Logger.Log("Fight log", $"team {winTeamName} won");
                if (writeOut)
                {
                    Console.WriteLine($"team {winTeamName} won");
                }
            }
        }
        #endregion
    }
}
