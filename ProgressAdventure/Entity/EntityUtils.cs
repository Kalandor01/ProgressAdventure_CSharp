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
        /// The dictionary pairing up entity type strings, to entity types.
        /// </summary>
        internal static readonly Dictionary<string, Type> entityTypeMap = new()
        {
            ["player"] = typeof(Player),
            ["caveman"] = typeof(Caveman),
            ["ghoul"] = typeof(Ghoul),
            ["troll"] = typeof(Troll),
            ["dragon"] = typeof(Dragon),
        };

        /// <summary>
        /// The dictionary pairing up attribute types, to stat modifiers.
        /// </summary>
        internal static readonly Dictionary<Attribute, (double maxHp, double attack, double defence, double agility)> attributeStatChangeMap = new()
        {
            [Attribute.RARE] = (2, 2, 2, 2),
            [Attribute.CRIPPLED] = (0.5, 0.5, 0.5, 0.5),
            [Attribute.HEALTHY] = (2, 1, 1, 1),
            [Attribute.SICK] = (0.5, 1, 1, 1),
            [Attribute.STRONG] = (1, 2, 1, 1),
            [Attribute.WEAK] = (1, 0.5, 1, 1),
            [Attribute.TOUGH] = (1, 1, 2, 1),
            [Attribute.FRAIL] = (1, 1, 0.5, 1),
            [Attribute.AGILE] = (1, 1, 1, 2),
            [Attribute.SLOW] = (1, 1, 1, 0.5),
        };

        /// <summary>
        /// The dictionary pairing up attributes, to their display name.
        /// </summary>
        internal static readonly Dictionary<Attribute, string> attributeNameMap = new()
        {
            [Attribute.RARE] = "Rare",
            [Attribute.CRIPPLED] = "Crippled",
            [Attribute.HEALTHY] = "Healthy",
            [Attribute.SICK] = "Sick",
            [Attribute.STRONG] = "Strong",
            [Attribute.WEAK] = "Weak",
            [Attribute.TOUGH] = "Tough",
            [Attribute.FRAIL] = "Frail",
            [Attribute.AGILE] = "Agile",
            [Attribute.SLOW] = "Slow",
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
        /// <param name="baseAgility">The base agility of the entity.</param>
        /// <param name="negativeFluctuation">The value, that will offset all of the base stat values, it the negative direction.</param>
        /// <param name="positiveFluctuation">The value, that will offset all of the base stat values, it the positive direction.</param>
        /// <param name="attributeChances">The chances of the entitiy having a specific attribute.</param>
        /// <param name="originalTeam">The original team of the entity.</param>
        /// <param name="teamChangeChange">The chance of the entitiy changing its team to the player's team. (1 = 100%)</param>
        public static EntityManagerStatsDTO EntityManager(
            int baseMaxHp,
            int baseAttack,
            int baseDefence,
            int baseAgility,
            int negativeFluctuation = 2,
            int positiveFluctuation = 3,
            AttributeChancesDTO? attributeChances = null,
            int originalTeam = 1,
            double teamChangeChange = 0.005
        )
        {
            return EntityManager(
                (baseMaxHp - negativeFluctuation, baseMaxHp, baseMaxHp + positiveFluctuation),
                (baseAttack - negativeFluctuation, baseAttack, baseAttack + positiveFluctuation),
                (baseDefence - negativeFluctuation, baseDefence, baseDefence + positiveFluctuation),
                (baseAgility - negativeFluctuation, baseAgility, baseAgility + positiveFluctuation),
                attributeChances,
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
        /// <param name="baseAgility">The base agility range of the entity.</param>
        /// <param name="attributeChances">The chances of the entitiy having a specific attribute.</param>
        /// <param name="originalTeam">The original team of the entity.</param>
        /// <param name="teamChangeChange">The chance of the entitiy changing its team to the player's team. (1 = 100%)</param>
        public static EntityManagerStatsDTO EntityManager(
            (int lower, int middle, int upper) baseMaxHp,
            (int lower, int middle, int upper) baseAttack,
            (int lower, int middle, int upper) baseDefence,
            (int lower, int middle, int upper) baseAgility,
            AttributeChancesDTO? attributeChances = null,
            int originalTeam = 1,
            double teamChangeChange = 0.005
        )
        {
            var baseMaxHpValue = ConfigureStat(baseMaxHp);
            var baseAttackValue = ConfigureStat(baseAttack);
            var baseDefenceValue = ConfigureStat(baseDefence);
            var baseAgilityValue = ConfigureStat(baseAgility);
            var attributes = GenerateEntityAttributes(attributeChances);
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
            return new EntityManagerStatsDTO(baseMaxHpValue, baseAttackValue, baseDefenceValue, baseAgilityValue, originalTeam, currentTeam, attributes);
        }

        public static List<Attribute> GenerateEntityAttributes(AttributeChancesDTO? attributeChances)
        {
            var attrChances = attributeChances ?? new AttributeChancesDTO();
            var attributes = new List<Attribute>();
            // all attributes
            if (RandomStates.MainRandom.GenerateDouble() < attrChances.rareChance)
            {
                attributes.Add(Attribute.RARE);
            }
            else if (RandomStates.MainRandom.GenerateDouble() < attrChances.crippledChance)
            {
                attributes.Add(Attribute.CRIPPLED);
            }
            // health
            if (RandomStates.MainRandom.GenerateDouble() < attrChances.healthyChance)
            {
                attributes.Add(Attribute.HEALTHY);
            }
            else if (RandomStates.MainRandom.GenerateDouble() < attrChances.sickChance)
            {
                attributes.Add(Attribute.SICK);
            }
            // attack
            if (RandomStates.MainRandom.GenerateDouble() < attrChances.strongChance)
            {
                attributes.Add(Attribute.STRONG);
            }
            else if (RandomStates.MainRandom.GenerateDouble() < attrChances.weakChance)
            {
                attributes.Add(Attribute.WEAK);
            }
            // defence
            if (RandomStates.MainRandom.GenerateDouble() < attrChances.toughChance)
            {
                attributes.Add(Attribute.TOUGH);
            }
            else if (RandomStates.MainRandom.GenerateDouble() < attrChances.frailChance)
            {
                attributes.Add(Attribute.FRAIL);
            }
            // agility
            if (RandomStates.MainRandom.GenerateDouble() < attrChances.agileChance)
            {
                attributes.Add(Attribute.AGILE);
            }
            else if (RandomStates.MainRandom.GenerateDouble() < attrChances.slowChance)
            {
                attributes.Add(Attribute.SLOW);
            }
            return attributes;
        }

        /// <summary>
        /// Returns the name of the type of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public static string? GetEntityTypeName(Entity entity)
        {
            foreach (var entityType in entityTypeMap)
            {
                if (entityType.Value == entity.GetType())
                {
                    return entityType.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the name of the entity from the name of the calling class.
        /// </summary>
        /// <param name="extraDepth">Should be increased by 1, for every extra method, that called this one, that isn't the target entity class.</param>
        public static string GetEntityNameFromClass(uint extraDepth = 0)
        {
            string? name = null;
            try
            {
                var frame = new StackTrace().GetFrame((int)(1 + extraDepth));
                var method = frame?.GetMethod();
                name = method?.ReflectedType?.Name;
                name = name?.Replace("_", " ");
            }
            catch (NullReferenceException)
            {
                Logger.Log("Tried to create entity with no known name", null, LogSeverity.WARN);
            }
            if (name is null)
            {
                Logger.Log("Couldn't get entity name from class", "Using default name", LogSeverity.WARN);
            }
            return name ?? "[Unknown entity]";
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

        /// <summary>
        /// Moves an entity from one position to another, one tile at a time.
        /// </summary>
        /// <param name="entity">The entity to move.</param>
        /// <param name="relativeMovementVector">The relative movement vector to move the entity by.</param>
        /// <param name="updateWorld">Whether to update the world with the new position of the entity, while moving.<br/>
        /// If null, the default is used.</param>
        /// <param name="startingPosition">The starting position of the entity.<br/>
        /// If null, the default is used.</param>
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
            var noTeamNumber = 0;
            foreach (var entity in entities)
            {
                // -1 = no team, 0 = player team
                var teamName = entity.currentTeam == 0 ? "Player" : entity.currentTeam.ToString();
                if (entity.currentTeam == -1)
                {
                    teamName = entity.FullName + noTeamNumber;
                    noTeamNumber++;
                }
                if (entity.currentTeam != -1 && teams.TryGetValue(teamName, out List<Entity>? value))
                {
                    value.Add(entity);
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
            var (teamsPrepared, playerTeam, player) = PrepareTeams(teams);
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
                UnpreparedFight(teamsPrepared, playerTeam, player, writeOut);
            }
            Logger.Log("Fight log", "fight ended");
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

        /// <summary>
        /// Filters out invalid teams from the teams list, and returns which team the player is in.
        /// </summary>
        /// <param name="teamsRaw">The teams of entities.</param>
        private static (Dictionary<string, List<Entity>> teams, string? playerTeam, Player? player) PrepareTeams(Dictionary<string, List<Entity>> teamsRaw)
        {
            string? playerTeam = null;
            Player? player = null;
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
                            player = (Player)entity;
                        }
                    }
                    teams.Add(team.Key, entityList);
                }
                else
                {
                    Logger.Log("Fight log", $"empty team: {team.Key}", LogSeverity.WARN);
                }
            }
            return (teams, playerTeam, player);
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
        /// <param name="player">The (first) player in the fight, or null.</param>
        private static void UnpreparedFight(Dictionary<string, List<Entity>> teams, string? playerTeam, Player? player, bool writeOut)
        {
            var teamCounts = GetTeamEntityCounts(teams);
            var totalCount = GetTotalEntityCount(teamCounts);
            var isPlayerInTeam = playerTeam is not null && teams[playerTeam].Count > 1;
            Logger.Log("Fight log", $"fight started with {teams.Count} teams, and {totalCount} entities");
            if (player is not null)
            {
                Logger.Log("Fight log", $"player is in the fight, team: {playerTeam}");
            }
            
            // entities write out
            if (writeOut)
            {
                var oneEntityTeamExists = false;
                if (teams.Count < totalCount)
                {
                    foreach (var team in teams)
                    {
                        if (team.Value.Count > 1)
                        {
                            Console.WriteLine($"\nTeam {team.Key}:\n");
                            foreach (var entity in team.Value)
                            {
                                Console.Write($"\t{entity.GetFullNameWithSpecies()}");
                                if (entity.originalTeam != entity.currentTeam)
                                {
                                    Console.Write(" (Switched to this side!)");
                                }
                                Console.WriteLine($"\n\tHP: {entity.CurrentHp}\n\tAttack: {entity.Attack}\n\tDefence: {entity.Defence}\n\tAgility: {entity.Agility}\n");
                            }
                        }
                        else
                        {
                            oneEntityTeamExists = true;
                        }
                    }
                    Console.WriteLine("Other entities:\n");
                }
                else
                {
                    oneEntityTeamExists = true;
                }
                if (oneEntityTeamExists)
                {
                    foreach (var team in teams)
                    {
                        if (team.Value.Count == 1)
                        {
                            foreach (var entity in team.Value)
                            {
                                Console.Write($"{entity.GetFullNameWithSpecies()}");
                                if (entity.originalTeam != entity.currentTeam)
                                {
                                    Console.Write(" (Switched to this side!)");
                                }
                                Console.WriteLine($"\nHP: {entity.CurrentHp}\nAttack: {entity.Attack}\nDefence: {entity.Defence}\nAgility: {entity.Agility}\n");
                            }
                        }
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
                                    if (teams[targetTeamKey].Count > 1)
                                    {
                                        Logger.Log("Fight log", $"team {targetTeamKey} defeated");
                                        if (writeOut)
                                        {
                                            Console.WriteLine($"team {targetTeamKey} defeated");
                                        }
                                    }
                                    teamCounts.Remove(targetTeamKey);
                                    teams.Remove(targetTeamKey);
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
            if (writeOut)
            {
                Console.WriteLine("\nResults:\n");
            }
            var winTeamName = teamCounts.First().Key;
            if (playerTeam is null || winTeamName != playerTeam)
            {
                if (teams[winTeamName].Count > 1)
                {
                    Logger.Log("Fight log", $"team {winTeamName} won");
                    if (writeOut)
                    {
                        Console.WriteLine($"team {winTeamName} won");
                    }
                }
                else
                {
                    Logger.Log("Fight log", $"entity {teams[winTeamName].First().FullName} won");
                    if (writeOut)
                    {
                        Console.WriteLine($"{teams[winTeamName].First().FullName} won");
                    }
                }
            }
            if (playerTeam is not null)
            {
                // player team dead
                if (winTeamName != playerTeam)
                {
                    if (isPlayerInTeam)
                    {
                        Logger.Log("Fight log", "player team defeated");
                        if (writeOut)
                        {
                            Console.WriteLine($"{player?.FullName}'s team was defeated");
                        }
                    }
                    else
                    {
                        Logger.Log("Fight log", "player defeated");
                        if (writeOut)
                        {
                            Console.WriteLine($"{player?.FullName} was defeated");
                        }
                    }
                }
                // player team won
                else
                {
                    if (isPlayerInTeam)
                    {
                        Logger.Log("Fight log", "player team won");
                        if (writeOut)
                        {
                            Console.WriteLine($"{player?.FullName}'s team won");
                        }
                    }
                    else
                    {
                        Logger.Log("Fight log", "player won");
                        if (writeOut)
                        {
                            Console.WriteLine($"{player?.FullName} won");
                        }
                    }
                    if (player?.CurrentHp == 0)
                    {
                        Logger.Log("Fight log", "player died");
                        if (writeOut)
                        {
                            Console.WriteLine($"{player.FullName} died");
                        }
                    }
                    // loot
                    else
                    {
                        foreach (var team in teams)
                        {
                            foreach (var entity in team.Value)
                            {
                                if (entity.CurrentHp <= 0 && !entity.Equals(player))
                                {
                                    player?.inventory.Loot(entity.drops, writeOut ? player.FullName : null);
                                }
                            }
                        }
                    }
                    if (writeOut)
                    {
                        player?.Stats();
                    }
                }
            }
        }
        #endregion
    }
}
