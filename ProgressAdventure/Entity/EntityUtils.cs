using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using ProgressAdventure.ConfigManagement;
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
        #region Default config values
        /// <summary>
        /// The default value for the config used for the values of <see cref="Attribute"/>.
        /// </summary>
        private static readonly List<EnumValue<Attribute>> _defaultAttributes =
        [
            Attribute.RARE,
            Attribute.CRIPPLED,
            Attribute.HEALTHY,
            Attribute.SICK,
            Attribute.STRONG,
            Attribute.WEAK,
            Attribute.TOUGH,
            Attribute.FRAIL,
            Attribute.AGILE,
            Attribute.SLOW,
        ];

        /// <summary>
        /// The default value for the config used for the value of <see cref="EntityTypeMap"/>.
        /// </summary>
        private static readonly Dictionary<string, Type> _defaultEntityTypeMap = new()
        {
            ["player"] = typeof(Player),
            ["caveman"] = typeof(Caveman),
            ["ghoul"] = typeof(Ghoul),
            ["troll"] = typeof(Troll),
            ["dragon"] = typeof(Dragon),
        };

        /// <summary>
        /// The default value for the config used for the value of <see cref="FacingToMovementVectorMap"/>.
        /// </summary>
        private static readonly Dictionary<Facing, (int x, int y)> _defaultFacingToMovementVectorMap = new()
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
        /// The default value for the config used for the value of <see cref="AttributeStatChangeMap"/>.
        /// </summary>
        private static readonly Dictionary<EnumValue<Attribute>, (double maxHp, double attack, double defence, double agility)> _defaultAttributeStatChangeMap = new()
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
        #endregion

        #region Config dictionaries
        /// <summary>
        /// The dictionary pairing up entity type strings, to entity types.
        /// </summary>
        internal static Dictionary<string, Type> EntityTypeMap { get; private set; }

        /// <summary>
        /// The dictionary pairing up facing types, to their vector equivalents.
        /// </summary>
        internal static Dictionary<Facing, (int x, int y)> FacingToMovementVectorMap { get; private set; }

        /// <summary>
        /// The dictionary pairing up attribute types, to stat modifiers.
        /// </summary>
        internal static Dictionary<EnumValue<Attribute>, (double maxHp, double attack, double defence, double agility)> AttributeStatChangeMap { get; private set; }
        #endregion

        #region Constructors
        static EntityUtils()
        {
            LoadDefaultConfigs();
        }
        #endregion

        #region Public fuctions
        #region Configs
        #region Write default config or get reload common data
        private static (string configName, bool paddingData) WriteDefaultConfigOrGetReloadDataEntityTypeMap(bool isWriteConfig)
        {
            var basePath = Path.Join(Constants.CONFIGS_ENTITY_SUBFOLDER_NAME, "entity_type_map");
            if (!isWriteConfig)
            {
                return (basePath, false);
            }

            PACSingletons.Instance.ConfigManager.SetConfig(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultEntityTypeMap
                );
            return default;
        }

        private static (
            string configName,
            Func<Facing, string> serializeKeys,
            Func<(int x, int y), Dictionary<string, int>> serializeValues
        ) WriteDefaultConfigOrGetReloadDataFacingToMovementVectorMap(bool isWriteConfig)
        {
            var basePath = Path.Join(Constants.CONFIGS_ENTITY_SUBFOLDER_NAME, "facing_to_movement_vector_map");
            static string KeySerializer(Facing key) => key.ToString();
            static Dictionary<string, int> ValueSerializer((int x, int y) key) => new()
            {
                [nameof(key.x)] = key.x,
                [nameof(key.y)] = key.y,
            };
            if (!isWriteConfig)
            {
                return (basePath, KeySerializer, ValueSerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultFacingToMovementVectorMap,
                    ValueSerializer
                );
            return default;
        }

        private static (string configName, bool paddingData) WriteDefaultConfigOrGetReloadDataAttributes(bool isWriteConfig)
        {
            var basePath = Path.Join(Constants.CONFIGS_ENTITY_SUBFOLDER_NAME, "attributes");
            if (!isWriteConfig)
            {
                return (basePath, false);
            }

            PACSingletons.Instance.ConfigManager.SetConfig(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultAttributes
                );
            return default;
        }

        private static (
            string configName,
            Func<EnumValue<Attribute>, string> serializeKeys,
            Func<(double maxHp, double attack, double defence, double agility), Dictionary<string, double>> serializeValues
        ) WriteDefaultConfigOrGetReloadDataAttributeStatChangeMap(bool isWriteConfig)
        {
            var basePath = Path.Join(Constants.CONFIGS_ENTITY_SUBFOLDER_NAME, "attribute_stat_change_map");
            static string KeySerializer(EnumValue<Attribute> key) => key.Name;
            static Dictionary<string, double> ValueSerializer((double maxHp, double attack, double defence, double agility) key) => new()
            {
                [nameof(key.maxHp)] = key.maxHp,
                [nameof(key.attack)] = key.attack,
                [nameof(key.defence)] = key.defence,
                [nameof(key.agility)] = key.agility,
            };
            if (!isWriteConfig)
            {
                return (basePath, KeySerializer, ValueSerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultAttributeStatChangeMap,
                    ValueSerializer,
                    KeySerializer
                );
            return default;
        }
        #endregion

        /// <summary>
        /// Resets all variables that come from configs.
        /// </summary>
        public static void LoadDefaultConfigs()
        {
            EntityTypeMap = _defaultEntityTypeMap;
            FacingToMovementVectorMap = _defaultFacingToMovementVectorMap;
            Tools.LoadDefultAdvancedEnum(_defaultAttributes);
            AttributeStatChangeMap = _defaultAttributeStatChangeMap;
        }

        /// <summary>
        /// Resets all config files to their default states.
        /// </summary>
        public static void WriteDefaultConfigs()
        {
            WriteDefaultConfigOrGetReloadDataEntityTypeMap(true);
            WriteDefaultConfigOrGetReloadDataFacingToMovementVectorMap(true);
            WriteDefaultConfigOrGetReloadDataAttributes(true);
            WriteDefaultConfigOrGetReloadDataAttributeStatChangeMap(true);
        }

        /// <summary>
        /// Reloads all values that come from configs.
        /// </summary>
        /// <param name="namespaceFolders">The name of the currently active config folders.</param>
        /// <param name="isVanillaInvalid">If the vanilla config is valid.</param>
        /// <param name="showProgressIndentation">If not null, shows the progress of loading the configs on the console.</param>
        public static void ReloadConfigs(
            List<(string folderName, string namespaceName)> namespaceFolders,
            bool isVanillaInvalid,
            int? showProgressIndentation = null
        )
        {
            Tools.ReloadConfigsFolderDisplayProgress(Constants.CONFIGS_ENTITY_SUBFOLDER_NAME, showProgressIndentation);
            showProgressIndentation = showProgressIndentation + 1 ?? null;

            EntityTypeMap = ConfigUtils.ReloadConfigsAggregateDict(
                WriteDefaultConfigOrGetReloadDataEntityTypeMap(false).configName,
                namespaceFolders,
                _defaultEntityTypeMap,
                isVanillaInvalid,
                showProgressIndentation
            );

            var facingToMovementVectorMapData = WriteDefaultConfigOrGetReloadDataFacingToMovementVectorMap(false);
            FacingToMovementVectorMap = ConfigUtils.ReloadConfigsAggregateDict(
                facingToMovementVectorMapData.configName,
                namespaceFolders,
                _defaultFacingToMovementVectorMap,
                facingToMovementVectorMapData.serializeValues,
                move => (move["x"], move["y"]),
                facingToMovementVectorMapData.serializeKeys,
                Enum.Parse<Facing>,
                isVanillaInvalid,
                showProgressIndentation
            );

            ConfigUtils.ReloadConfigsAggregateAdvancedEnum(
                WriteDefaultConfigOrGetReloadDataAttributes(false).configName,
                namespaceFolders,
                _defaultAttributes,
                isVanillaInvalid,
                showProgressIndentation,
                true
            );

            var attributeStatChangeMapData = WriteDefaultConfigOrGetReloadDataAttributeStatChangeMap(false);
            AttributeStatChangeMap = ConfigUtils.ReloadConfigsAggregateDict(
                attributeStatChangeMapData.configName,
                namespaceFolders,
                _defaultAttributeStatChangeMap,
                attributeStatChangeMapData.serializeValues,
                stats => (stats["maxHp"], stats["attack"], stats["defence"], stats["agility"]),
                attributeStatChangeMapData.serializeKeys,
                key => Attribute.GetValue(ConfigUtils.GetNameapacedString(key)),
                isVanillaInvalid,
                showProgressIndentation
            );
        }
        #endregion

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
            if (RandomStates.Instance.MainRandom.GenerateDouble() < teamChangeChange)
            {
                currentTeam = 0;
            }
            return new EntityManagerStatsDTO(baseMaxHpValue, baseAttackValue, baseDefenceValue, baseAgilityValue, originalTeam, currentTeam, attributes);
        }

        public static List<EnumValue<Attribute>> GenerateEntityAttributes(AttributeChancesDTO? attributeChances)
        {
            var attrChances = attributeChances ?? new AttributeChancesDTO();
            var attributes = new List<EnumValue<Attribute>>();
            // all attributes
            RandomStates.Instance.MainRandom.GenerateTriChance(() =>
            {
                attributes.Add(Attribute.CRIPPLED);
            },
            () =>
            {
                attributes.Add(Attribute.RARE);
            }, attrChances.crippledChance, attrChances.rareChance);
            // health
            RandomStates.Instance.MainRandom.GenerateTriChance(() =>
            {
                attributes.Add(Attribute.SICK);
            },
            () =>
            {
                attributes.Add(Attribute.HEALTHY);
            }, attrChances.sickChance, attrChances.healthyChance);
            // attack
            RandomStates.Instance.MainRandom.GenerateTriChance(() =>
            {
                attributes.Add(Attribute.WEAK);
            },
            () =>
            {
                attributes.Add(Attribute.STRONG);
            }, attrChances.weakChance, attrChances.strongChance);
            // defence
            RandomStates.Instance.MainRandom.GenerateTriChance(() =>
            {
                attributes.Add(Attribute.FRAIL);
            },
            () =>
            {
                attributes.Add(Attribute.TOUGH);
            }, attrChances.frailChance, attrChances.toughChance);
            // agility
            RandomStates.Instance.MainRandom.GenerateTriChance(() =>
            {
                attributes.Add(Attribute.SLOW);
            },
            () =>
            {
                attributes.Add(Attribute.AGILE);
            }, attrChances.slowChance, attrChances.agileChance);
            return attributes;
        }

        /// <summary>
        /// Returns the name of the type of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public static string? GetEntityTypeName(Entity entity)
        {
            foreach (var entityType in EntityTypeMap)
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
                PACSingletons.Instance.Logger.Log("Tried to create entity with no known name", null, LogSeverity.WARN);
            }
            if (name is null)
            {
                PACSingletons.Instance.Logger.Log("Couldn't get entity name from class", "Using default name", LogSeverity.WARN);
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
            if (FacingToMovementVectorMap.ContainsValue(vector))
            {
                return FacingToMovementVectorMap.First(facing => facing.Value == vector).Key;
            }
            return null;
        }

        /// <summary>
        /// Rotates the facing. (clockwise)
        /// </summary>
        /// <param name="facing">The vector to rotate.</param>
        /// <param name="angle">The angle to rotate by.</param>
        public static Facing? RotateFacing(Facing facing, double angle)
        {
            var facingVector = FacingToMovementVectorMap[facing];

            var radian = -1 * angle * (Math.PI / 180);

            var cos = Math.Cos(radian);
            var sin = Math.Sin(radian);

            var x = facingVector.x * cos - facingVector.y * sin;
            var y = facingVector.x * sin + facingVector.y * cos;

            var finalVector = ((int)Math.Round(x), (int)Math.Round(y));

            return MovementVectorToFacing(finalVector);
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
                if (entity.CurrentHp > 0)
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
                        teams[teamName] = [entity];
                    }
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
            PACSingletons.Instance.Logger.Log("Fight log", $"fight initiated with {entities.Count()} entities");
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
                PACSingletons.Instance.Logger.Log("Fight log", "no entities in fight");
                if (writeOut)
                {
                    Console.WriteLine("There is no one to fight.");
                }
            }
            else if (teamsPrepared.Count == 1)
            {
                PACSingletons.Instance.Logger.Log("Fight log", "only 1 team in fight");
                if (writeOut)
                {
                    Console.WriteLine("There is only 1 team in the fight. There is no reason to fight.");
                }
            }
            else
            {
                UnpreparedFight(teamsPrepared, playerTeam, player, writeOut);
            }
            PACSingletons.Instance.Logger.Log("Fight log", "fight ended");
        }

        public static void RandomFight(int entityNumber = 1, int totalCost = 1, int minPower = 1, int maxPower = -1, bool roundUp = false, bool includePlayer = true)
        {
            var entities = new List<Entity>();

            var remainingEntityNumber = entityNumber;
            for (var _ = 0; _ < entityNumber; _++)
            {
                // max cost calculation
                double nonRoundedCost = totalCost * 1.0 / remainingEntityNumber;
                int maxCost = (int)(roundUp ? Math.Ceiling(nonRoundedCost) : Math.Round(nonRoundedCost));
                if (maxCost < minPower)
                {
                    maxCost = minPower;
                }

                // cost calculation
                var entityCost = remainingEntityNumber > 1 ? RandomStates.Instance.MiscRandom.GenerateInRange(minPower, maxCost) : totalCost;

                // cost adjustment
                if (entityCost < minPower)
                {
                    entityCost = minPower;
                }
                if (maxPower != -1 && entityCost > maxPower)
                {
                    entityCost = maxPower;
                }

                // monster choice
                Entity? entity = null;
                if (entityCost >= 10)
                {
                    var entityNum = RandomStates.Instance.MiscRandom.GenerateInRange(0, 0);
                    switch (entityNum)
                    {
                        case 0:
                            entity = new Dragon();
                            break;
                    }
                    totalCost -= 10;
                }
                else if (entityCost >= 3)
                {
                    var entityNum = RandomStates.Instance.MiscRandom.GenerateInRange(0, 0);
                    switch (entityNum)
                    {
                        case 0:
                            entity = new Troll();
                            break;
                    }
                    totalCost -= 3;
                }
                else if (entityCost >= 2)
                {
                    var entityNum = RandomStates.Instance.MiscRandom.GenerateInRange(0, 0);
                    switch (entityNum)
                    {
                        case 0:
                            entity = new Ghoul();
                            break;
                    }
                    totalCost -= 2;
                }
                else
                {
                    var entityNum = RandomStates.Instance.MiscRandom.GenerateInRange(0, 0);
                    switch (entityNum)
                    {
                        case 0:
                            entity = new Caveman();
                            break;
                    }
                    totalCost -= 1;
                }

                if (entity is not null)
                {
                    entities.Add(entity);
                    remainingEntityNumber--;
                }
            }

            if (includePlayer)
            {
                entities.Add(SaveData.Instance.player);
            }

            PACSingletons.Instance.Logger.Log("Fight log", "random fight initiated");
            Fight(entities);
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
                statValue = (int)Math.Round(RandomStates.Instance.MainRandom.Triangular(statRange.lower, statRange.middle, statRange.upper));
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
                if (team.Value.Count == 0)
                {
                    PACSingletons.Instance.Logger.Log("Fight log", $"empty team: {team.Key}", LogSeverity.WARN);
                    continue;
                }

                var entityList = new List<Entity>();
                foreach (var entity in team.Value)
                {
                    if (entity.CurrentHp > 0)
                    {
                        entityList.Add(entity);
                        if (entity.GetType() == typeof(Player))
                        {
                            playerTeam = team.Key;
                            player = (Player)entity;
                        }
                    }
                }

                if (entityList.Count == 0)
                {
                    PACSingletons.Instance.Logger.Log("Fight log", $"all entities are dead in team: {team.Key}", LogSeverity.WARN);
                    continue;
                }

                teams.Add(team.Key, entityList);
            }
            return (teams, playerTeam, player);
        }

        /// <summary>
        /// Gets the number of entities in each team, in the teams dictionary.
        /// </summary>
        /// <param name="teams">The teams of entities.</param>
        /// <param name="countDead">If dead entities should also be counted.</param>
        private static Dictionary<string, int> GetTeamEntityCounts(Dictionary<string, List<Entity>> teams, bool countDead = false)
        {
            var teamCounts = new Dictionary<string, int>();
            foreach (var team in teams)
            {
                if (countDead)
                {
                    teamCounts.Add(team.Key, team.Value.Count);
                }
                else
                {
                    var teamCount = 0;
                    foreach (var entity in team.Value)
                    {
                        if (entity.CurrentHp > 0)
                        {
                            teamCount++;
                        }
                    }
                    if (teamCount > 0)
                    {
                        teamCounts.Add(team.Key, teamCount);
                    }
                }
            }
            return teamCounts;
        }

        /// <summary>
        /// Gets the total number of entities in the team counts dictionary.
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
        /// Writes out the teams, and entities in those teams.
        /// </summary>
        /// <param name="teams">The teams of entities, the fight should happen between.</param>
        /// <param name="totalCount">The total number of entities.</param>
        private static void WriteOutFightTeams(Dictionary<string, List<Entity>> teams, int totalCount)
        {
            var oneEntityTeamExists = false;
            var multiEntityTeamExists = false;
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
                        multiEntityTeamExists = true;
                    }
                    else
                    {
                        oneEntityTeamExists = true;
                    }
                }
            }
            else
            {
                oneEntityTeamExists = true;
            }
            if (oneEntityTeamExists)
            {
                Console.WriteLine($"{(multiEntityTeamExists ? "Other e" : "E")}ntities:\n");
                foreach (var team in teams)
                {
                    if (team.Value.Count == 0)
                    {
                        PACSingletons.Instance.Logger.Log("Fight log", $"no entity in team \"{team.Key}\", did you call {nameof(UnpreparedFight)} directly???", LogSeverity.ERROR);
                        return;
                    }

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

        /// <summary>
        /// Creates a fight between multiple teams, but it doesn't check for correctnes of the teams.
        /// </summary>
        /// <param name="teams">The teams of entities, the fight should happen between.</param>
        /// <param name="playerTeam">The team that the player is in, or null.</param>
        /// <param name="writeOut">Whether to write out, what is happening with the fight.</param>
        /// <param name="player">The (first) player in the fight, or null.</param>
        private static void UnpreparedFight(Dictionary<string, List<Entity>> teams, string? playerTeam, Player? player, bool writeOut)
        {
            if (teams.Count == 0)
            {
                PACSingletons.Instance.Logger.Log("Fight log", $"no teams in fight, did you call {nameof(UnpreparedFight)} directly???", LogSeverity.ERROR);
                return;
            }

            var teamCounts = GetTeamEntityCounts(teams);
            var totalCount = GetTotalEntityCount(teamCounts);
            var isPlayerInTeam = playerTeam is not null && teams[playerTeam].Count > 1;
            PACSingletons.Instance.Logger.Log("Fight log", $"fight started with {teams.Count} teams, and {totalCount} entities");
            if (player is not null)
            {
                PACSingletons.Instance.Logger.Log("Fight log", $"player is in the fight, team: {playerTeam}");
            }

            // entities write out
            if (writeOut)
            {
                WriteOutFightTeams(teams, totalCount);
            }

            // fight
            var no_damage_in_x_turns = 0;
            while (teamCounts.Count > 1 && no_damage_in_x_turns < Constants.FIGHT_GIVE_UP_TURN_NUMBER)
            {
                no_damage_in_x_turns++;
                for (var teamNum = 0; teamNum < teams.Count; teamNum++)
                {
                    var team = teams.ElementAt(teamNum);
                    for (var entityNum = 0; entityNum < team.Value.Count; entityNum++)
                    {
                        var entity = team.Value[entityNum];
                        if (entity.CurrentHp > 0)
                        {
                            // get target
                            var targetTeamNum = (int)RandomStates.Instance.MiscRandom.GenerateInRange(0, teams.Count - 2);
                            if (targetTeamNum >= teamNum)
                            {
                                targetTeamNum++;
                            }
                            var targetTeam = teams.ElementAt(targetTeamNum);
                            Entity targetEntity;
                            do
                            {
                                var targetEntityNum = RandomStates.Instance.MiscRandom.GenerateInRange(0, targetTeam.Value.Count - 1);
                                targetEntity = targetTeam.Value.ElementAt((int)targetEntityNum);
                            }
                            while (targetEntity.CurrentHp == 0);
                            // attack
                            var targetOldHp = targetEntity.CurrentHp;
                            var attackResponse = entity.AttackEntity(targetEntity);
                            if (writeOut)
                            {
                                Console.WriteLine($"{entity.FullName} attacked {targetEntity.FullName}");
                                string? writeText = null;
                                switch (attackResponse)
                                {
                                    case AttackResponse.TARGET_DOGDED:
                                        writeText = "DODGED!";
                                        break;
                                    case AttackResponse.TARGET_BLOCKED:
                                        writeText = "BLOCKED!";
                                        break;
                                    case AttackResponse.TARGET_HIT:
                                        writeText = $"dealt {targetOldHp - targetEntity.CurrentHp} damage ({targetEntity.CurrentHp})";
                                        break;
                                }
                                if (writeText is not null)
                                {
                                    Console.WriteLine(writeText);
                                }
                            }
                            if (attackResponse == AttackResponse.TARGET_HIT || attackResponse == AttackResponse.TARGET_KILLED)
                            {
                                no_damage_in_x_turns = 0;
                            }
                            // kill
                            if (attackResponse == AttackResponse.TARGET_KILLED)
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
                                        PACSingletons.Instance.Logger.Log("Fight log", $"team {targetTeamKey} defeated");
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
            // teams gave up
            if (no_damage_in_x_turns >= Constants.FIGHT_GIVE_UP_TURN_NUMBER)
            {
                PACSingletons.Instance.Logger.Log("Fight log", $"no damage was dealt for {no_damage_in_x_turns} turns, so the fight automaticaly ended");
                if (writeOut)
                {
                    Console.WriteLine("Everyone got bored, so the fight ends in a stalemate.");
                }
                return;
            }

            // winning team/entity
            var winTeamName = teamCounts.First().Key;
            if (playerTeam is null || winTeamName != playerTeam)
            {
                if (teams[winTeamName].Count > 1)
                {
                    PACSingletons.Instance.Logger.Log("Fight log", $"team {winTeamName} won");
                    if (writeOut)
                    {
                        Console.WriteLine($"team {winTeamName} won");
                    }
                }
                else
                {
                    PACSingletons.Instance.Logger.Log("Fight log", $"entity {teams[winTeamName].First().FullName} won");
                    if (writeOut)
                    {
                        Console.WriteLine($"{teams[winTeamName].First().FullName} won");
                    }
                }
            }

            if (playerTeam is null)
            {
                return;
            }

            // player team dead
            if (winTeamName != playerTeam)
            {
                if (isPlayerInTeam)
                {
                    PACSingletons.Instance.Logger.Log("Fight log", "player team defeated");
                    if (writeOut)
                    {
                        Console.WriteLine($"{player?.FullName}'s team was defeated");
                    }
                }
                else
                {
                    PACSingletons.Instance.Logger.Log("Fight log", "player defeated");
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
                    PACSingletons.Instance.Logger.Log("Fight log", "player team won");
                    if (writeOut)
                    {
                        Console.WriteLine($"{player?.FullName}'s team won");
                    }
                }
                else
                {
                    PACSingletons.Instance.Logger.Log("Fight log", "player won");
                    if (writeOut)
                    {
                        Console.WriteLine($"{player?.FullName} won");
                    }
                }
                if (player?.CurrentHp == 0)
                {
                    PACSingletons.Instance.Logger.Log("Fight log", "player died");
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
                            if (entity.CurrentHp == 0 && !entity.Equals(player))
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
        #endregion
    }
}
