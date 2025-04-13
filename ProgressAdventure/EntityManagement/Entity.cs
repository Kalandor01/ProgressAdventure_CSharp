using NPrng.Generators;
using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.WorldManagement;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AItem = ProgressAdventure.ItemManagement.AItem;
using Attribute = ProgressAdventure.Enums.Attribute;
using Inventory = ProgressAdventure.ItemManagement.Inventory;
using PACTools = PACommon.Tools;

namespace ProgressAdventure.EntityManagement
{
    /// <summary>
    /// A representation of an entity.
    /// </summary>
    public class Entity : IJsonConvertableExtra<Entity, (long absoluteX, long absoluteY)?>
    {
        #region Public fields
        /// <summary>
        /// The type of the <see cref="Entity"/>.
        /// </summary>
        public EnumValue<EntityType> type;
        /// <summary>
        /// The name of the <see cref="Entity"/>.
        /// </summary>
        public string name;
        /// <summary>
        /// The base hp of the <see cref="Entity"/>.
        /// </summary>
        public int baseMaxHp;
        /// <summary>
        /// The base attack of the <see cref="Entity"/>.
        /// </summary>
        public int baseAttack;
        /// <summary>
        /// The base defence of the <see cref="Entity"/>.
        /// </summary>
        public int baseDefence;
        /// <summary>
        /// The base agility of the <see cref="Entity"/>.
        /// </summary>
        public int baseAgility;
        /// <summary>
        /// The original team that the <see cref="Entity"/> is a part of.
        /// </summary>
        public int originalTeam;
        /// <summary>
        /// The current team that the <see cref="Entity"/> is a part of.
        /// </summary>
        public int currentTeam;
        /// <summary>
        /// The list of attributes that the <see cref="Entity"/> has.
        /// </summary>
        public List<EnumValue<Attribute>> attributes;
        /// <summary>
        /// The list of items that the <see cref="Entity"/> will drop on death.
        /// </summary>
        public List<AItem> drops;
        /// <summary>
        /// The position of the <see cref="Entity"/> in the world.
        /// </summary>
        public (long x, long y)? Position { get; private set; }
        /// <summary>
        /// The facing direction of the <see cref="Entity"/>.
        /// </summary>
        public Facing facing;

        #region Entity type specific
        /// <summary>
        /// The extra entity type specific data.
        /// </summary>
        private readonly Dictionary<string, object?> extraData;
        #endregion
        #endregion

        #region Public properties
        /// <summary>
        /// The full name of the <see cref="Entity"/>.
        /// </summary>
        public string FullName { get; protected set; }
        /// <summary>
        /// <inheritdoc cref="_maxHp" path="//summary"/>
        /// </summary>
        public int MaxHp
        {
            get;
            protected set => field = value >= 0 ? value : 0;
        }
        /// <summary>
        /// <inheritdoc cref="_currentHp" path="//summary"/>
        /// </summary>
        public int CurrentHp
        {
            get;
            protected set => field = Math.Clamp(value, 0, MaxHp);
        }
        /// <summary>
        /// The current attack of the <see cref="Entity"/>.
        /// </summary>
        public int Attack { get; protected set; }
        /// <summary>
        /// The current defence of the <see cref="Entity"/>.
        /// </summary>
        public int Defence { get; protected set; }
        /// <summary>
        /// The current agility of the <see cref="Entity"/>.
        /// </summary>
        public int Agility { get; protected set; }
        #endregion

        #region Public constructors
        #region Entity type specific constructors
        private static void SpecificConstructorPlayer(
            Entity entity,
            Dictionary<string, object?>? extraData,
            SplittableRandom? generateRandom = null
        )
        {
            entity.name = Tools.CorrectPlayerName(entity.name);
        }
        #endregion

        /// <summary>
        /// <inheritdoc cref="Entity"/><br/>
        /// Can be used for loading the <see cref="Entity"/> from json.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="name"></param>
        /// <param name="baseMaxHp"></param>
        /// <param name="currentHp"></param>
        /// <param name="baseAttack"></param>
        /// <param name="baseDefence"></param>
        /// <param name="baseAgility"></param>
        /// <param name="originalTeam"></param>
        /// <param name="currentTeam"></param>
        /// <param name="attributes"></param>
        /// <param name="drops"></param>
        /// <param name="position"></param>
        /// <param name="facing"></param>
        /// <param name="extraData"></param>
        /// <param name="generationRandom">The random generator to use to generate the entity's properties.</param>
        /// <exception cref="ArgumentException">Thrown if the entity type is invalid.</exception>
        private Entity(
            EnumValue<EntityType> entityType,
            (long x, long y)? position,
            string? name = null,
            int? baseMaxHp = null,
            int? currentHp = null,
            int? baseAttack = null,
            int? baseDefence = null,
            int? baseAgility = null,
            int? originalTeam = null,
            int? currentTeam = null,
            List<EnumValue<Attribute>>? attributes = null,
            List<AItem>? drops = null,
            Facing? facing = null,
            Dictionary<string, object?>? extraData = null,
            SplittableRandom? generationRandom = null
        )
        {
            if (!EntityUtils.EntityPropertiesMap.TryGetValue(entityType, out var properties))
            {
                throw new ArgumentException("Entity type doesn't have properties!", nameof(entityType));
            }

            this.type = entityType;
            this.name = name ?? GenerateEntityName(generationRandom);
            
            int actualCurrentHp;
            if (
                baseMaxHp is null ||
                currentHp is null ||
                baseAttack is null ||
                baseDefence is null ||
                baseAgility is null ||
                originalTeam is null ||
                currentTeam is null ||
                attributes is null
            )
            {
                var stats = EntityUtils.EntityManager(
                    properties.maxHp,
                    properties.attack,
                    properties.defence,
                    properties.agility,
                    properties.attributeChances,
                    originalTeam ?? properties.originalTeam,
                    properties.teamChangeChange,
                    generationRandom
                );

                this.baseMaxHp = baseMaxHp ?? stats.baseMaxHp;
                this.CurrentHp = currentHp ?? stats.baseMaxHp;
                this.baseAttack = baseAttack ?? stats.baseAttack;
                this.baseDefence = baseDefence ?? stats.baseDefence;
                this.baseAgility = baseAgility ?? stats.baseAgility;
                this.originalTeam = originalTeam ?? stats.originalTeam;
                this.currentTeam = currentTeam ?? stats.currentTeam;
                this.attributes = attributes ?? stats.attributes;

                actualCurrentHp = currentHp ?? stats.baseMaxHp;
            }
            else
            {
                this.baseMaxHp = (int)baseMaxHp;
                this.CurrentHp = (int)currentHp;
                this.baseAttack = (int)baseAttack;
                this.baseDefence = (int)baseDefence;
                this.baseAgility = (int)baseAgility;
                this.originalTeam = (int)originalTeam;
                this.currentTeam = (int)currentTeam;
                this.attributes = attributes;

                actualCurrentHp = (int)currentHp;
            }

            this.drops = drops ?? LootFactory.GenerateLoot(properties.loot, generationRandom);
            Position = position;
            this.facing = facing ?? Facing.NORTH;

            this.extraData = [];
            if (properties.hasInventory)
            {
                SetExtraDataValueFromConstructor(this, extraData, Constants.JsonKeys.Entity.INVENTORY, new Inventory());
            }

            // specific constructors
            if (type == EntityType.PLAYER)
            {
                SpecificConstructorPlayer(this, extraData, generationRandom);
            }

            SetupStats(actualCurrentHp);
            UpdateFullName();
        }

        /// <summary>
        /// <inheritdoc cref="Entity"/><br/>
        /// </summary>
        /// <param name="entityType"><inheritdoc cref="type" path="//summary"/></param>
        /// <param name="name"><inheritdoc cref="name" path="//summary"/></param>
        /// <param name="facing"><inheritdoc cref="facing" path="//summary"/></param>
        /// <param name="teamOverwrite">Overwrites the original team of the <see cref="Entity"/>.</param>
        /// <param name="generationRandom">The random generator to use to generate the entity's properties.</param>
        /// <exception cref="ArgumentException"></exception>
        public Entity(
            EnumValue<EntityType> entityType,
            string? name = null,
            Facing? facing = null,
            int? teamOverwrite = null,
            Dictionary<string, object?>? extraData = null,
            SplittableRandom? generationRandom = null
        )
            :this(
                 entityType,
                 position: null,
                 name,
                 originalTeam: teamOverwrite,
                 facing: facing,
                 extraData: extraData,
                 generationRandom: generationRandom
            )
        {

        }
        #endregion

        #region Public methods
        /// <summary>
        /// Updates the full name of the entity.
        /// </summary>
        public void UpdateFullName()
        {
            var attributeNames = new StringBuilder();
            foreach (var attribute in attributes)
            {
                attributeNames.Append(ConfigUtils.RemoveNamespace(attribute.Name) + " ");
            }
            FullName = attributeNames.ToString().Capitalize() + name;
        }

        /// <summary>
        /// Turns the entity in a random direction, that is weighted in the direction that it's already going towards.
        /// </summary>
        public void WeightedTurn()
        {
            if (RandomStates.Instance.MainRandom.GenerateDouble() < 0.2)
            {
                var oldFacing = facing;
                var angle = RandomStates.Instance.MainRandom.Triangular(-180, 0, 180);
                var newFacing = EntityUtils.RotateFacing(oldFacing, angle);

                if (newFacing is not null && newFacing != oldFacing)
                {
                    facing = (Facing)newFacing;
                    PACSingletons.Instance.Logger.Log("Entity turned", $"name: {FullName}, {oldFacing} -> {facing}", LogSeverity.DEBUG);
                }
            }
        }

        /// <summary>
        /// Moves the entity in the direction it's facing.
        /// </summary>
        /// <param name="multiplierVector">The multiplier to move the entity by.</param>
        /// <param name="facing">If not null, it will move in that direction instead.</param>
        /// <returns>If the move was successful.</returns>
        public bool Move((double x, double y)? multiplierVector = null, Facing? facing = null)
        {
            var moveRaw = EntityUtils.FacingToMovementVectorMap[facing ?? this.facing];
            var move = Utils.VectorMultiply(moveRaw, multiplierVector ?? (1, 1));
            return EntityUtils.EntityMover(this, ((long x, long y))move);
        }

        /// <summary>
        /// Modifies the world position of the entity.
        /// </summary>
        /// <param name="position">The position to move to.</param>
        /// <param name="updateWorld">Whether to update the tile, the entitiy is on.</param>
        /// <returns>If the entity position was successfuly set.</returns>
        public bool SetPosition((long x, long y) position, bool? updateWorld = null)
        {
            bool success;
            if (Position is null)
            {
                success = AddPosition(position);
            }
            else
            {
                success = MovePosition(position);
            }

            if (
                success &&
                Position is not null &&
                (updateWorld ?? EntityUtils.EntityPropertiesMap[type].updatesWorldWhenMoving)
            )
            {
                World.TryGetTileAll(Position.Value, out var tile);
                if (type == EntityType.PLAYER)
                {
                    tile.Visit();
                }
            }
            return success;
        }

        /// <summary>
        /// Adds the <see cref="Entity"/> to the given <see cref="PopulationManager"/>. 
        /// </summary>
        /// <param name="populationManager">The <see cref="PopulationManager"/> that the <see cref="Entity"/> should be added to.</param>
        /// <returns>If the addition was successful.</returns>
        public bool AddPosition(PopulationManager populationManager, bool log = true)
        {
            if (Position is not null)
            {
                return false;
            }

            Position = populationManager.absolutePosition;
            if (!populationManager.AddEntity(this))
            {
                Position = null;
                return false;
            }

            if (log)
            {
                PACSingletons.Instance.Logger.Log(
                    $"{(type == EntityType.PLAYER ? "Player" : "Entity")} position added",
                    $"name: {FullName}, {populationManager.absolutePosition}",
                    LogSeverity.DEBUG
                );
            }
            return true;
        }

        /// <summary>
        /// Adds a world position to the <see cref="Entity"/>.
        /// </summary>
        /// <param name="absolutePosition">The position to set.</param>
        /// <returns>If the addition was successful.</returns>
        public bool AddPosition((long x, long y) absolutePosition, bool log = true)
        {
            if (Position is not null)
            {
                return false;
            }

            World.TryGetTileAll(absolutePosition, out var tile);
            return AddPosition(tile.populationManager, log);
        }

        /// <summary>
        /// Removes the <see cref="Entity"/> from the given <see cref="PopulationManager"/>. 
        /// </summary>
        /// <param name="populationManager">The <see cref="PopulationManager"/> that the <see cref="Entity"/> is in.</param>
        /// <param name="log">Whether to log the action.</param>
        /// <returns>If the removal was successful.</returns>
        public bool RemovePosition(PopulationManager populationManager, bool log = true)
        {
            if (Position is null)
            {
                return false;
            }

            var oldPos = Position.Value;
            Position = null;
            if (!populationManager.RemoveEntity(this))
            {
                Position = oldPos;
                return false;
            }

            if (log)
            {
                PACSingletons.Instance.Logger.Log(
                    $"{(type == EntityType.PLAYER ? "Player" : "Entity")} position removed",
                    $"name: {FullName}, old position: {oldPos}",
                    LogSeverity.DEBUG
                );
            }
            return true;
        }

        /// <summary>
        /// Removes the <see cref="Entity"/>'s world position.
        /// </summary>
        /// <param name="log">Whether to log the action.</param>
        /// <returns>If the removal was successful.</returns>
        public bool RemovePosition(bool log = true)
        {
            if (
                Position is null ||
                World.FindTileAll(Position.Value)?.populationManager is not PopulationManager popMan
            )
            {
                return false;
            }
            return RemovePosition(popMan, log);
        }


        /// <summary>
        /// Moves the <see cref="Entity"/> from one <see cref="PopulationManager"/> to another.
        /// </summary>
        /// <param name="originPopulationManager">The <see cref="PopulationManager"/> to move the <see cref="Entity"/> from.</param>
        /// <param name="destinationPopulationManager">The <see cref="PopulationManager"/> to move the <see cref="Entity"/> to.</param>
        /// <returns>If the entity was successfuly moved.</returns>
        public bool MovePosition(PopulationManager originPopulationManager, PopulationManager destinationPopulationManager)
        {
            if (
                originPopulationManager == destinationPopulationManager ||
                !RemovePosition(originPopulationManager, false)
            )
            {
                return false;
            }

            if (!AddPosition(destinationPopulationManager, false))
            {
                AddPosition(originPopulationManager);
                return false;
            }

            PACSingletons.Instance.Logger.Log(
                $"{(type == EntityType.PLAYER ? "Player" : "Entity")} moved",
                $"name: {FullName}, {originPopulationManager.absolutePosition} -> {destinationPopulationManager.absolutePosition}",
                LogSeverity.DEBUG
            );
            return true;
        }


        /// <summary>
        /// Moves the <see cref="Entity"/> to another position.
        /// </summary>
        /// <param name="destinationAbsolutePosition">The position to move the <see cref="Entity"/> to.</param>
        /// <returns>If the entity was successfuly moved.</returns>
        public bool MovePosition((long x, long y) destinationAbsolutePosition)
        {
            var oldPos = Position;
            if (
                Position == destinationAbsolutePosition ||
                !RemovePosition(false)
            )
            {
                return false;
            }

            if (
                !AddPosition(destinationAbsolutePosition, false) &&
                oldPos is not null
            )
            {
                AddPosition(((long x, long y))oldPos);
                return false;
            }

            PACSingletons.Instance.Logger.Log(
                $"{(type == EntityType.PLAYER ? "Player" : "Entity")} moved",
                $"name: {FullName}, {oldPos} -> {destinationAbsolutePosition}",
                LogSeverity.DEBUG
            );
            return true;
        }

        /// <summary>
        /// Makes the entity attack another one.
        /// </summary>
        /// <param name="target">The target entity.</param>
        public AttackResponse AttackEntity(Entity target)
        {
            PACSingletons.Instance.Logger.Log("Attack log", $"{FullName} attacked {target.FullName}");
            // attacker dead
            if (CurrentHp == 0)
            {
                PACSingletons.Instance.Logger.Log("Attack log", $"{FullName}(attacker) is dead");
                return AttackResponse.ENTITY_DEAD;
            }
            // enemy dead
            else if (target.CurrentHp == 0)
            {
                PACSingletons.Instance.Logger.Log("Attack log", $"{FullName}(attacked) is already dead");
                return AttackResponse.TARGET_DEAD;
            }
            // enemy dodge
            else if (RandomStates.Instance.MiscRandom.GenerateDouble() > Agility * 1.0 / target.Agility - 0.1)
            {
                PACSingletons.Instance.Logger.Log("Attack log", $"{target.FullName} dodged");
                return AttackResponse.TARGET_DOGDED;
            }
            // attack
            else
            {
                var attack = (int)RandomStates.Instance.MiscRandom.GenerateInRange(1, 7) + Attack;
                var damage = attack - target.Defence;
                // block
                if (damage <= 0)
                {
                    PACSingletons.Instance.Logger.Log("Attack log", $"{target.FullName} blocked attack");
                    return AttackResponse.TARGET_BLOCKED;
                }
                // hit
                else
                {
                    target.TakeDamage(damage);
                    PACSingletons.Instance.Logger.Log("Attack log", $"{target.FullName} took {damage} damage ({target.CurrentHp})");
                    // kill
                    if (target.CurrentHp == 0)
                    {
                        PACSingletons.Instance.Logger.Log("Attack log", $"{FullName} defeated {target.FullName}");
                        return AttackResponse.TARGET_KILLED;
                    }
                    return AttackResponse.TARGET_HIT;
                }
            }
        }

        /// <summary>
        /// Makes the entity take damage.
        /// </summary>
        /// <param name="damage">The amount of damage to take.</param>
        public void TakeDamage(int damage)
        {
            CurrentHp -= damage;
        }

        /// <summary>
        /// Returns the full name of the entity with their species in parentheses, unless it's a player.
        /// </summary>
        public string GetFullNameWithSpecies()
        {
            return FullName + (type != EntityType.PLAYER ? $" ({EntityUtils.EntityPropertiesMap[type].displayName})" : "");
        }

        #region Entity type specific methods
        /// <summary>
        /// Returns the (player) inventory.
        /// </summary>
        public Inventory? TryGetInventory()
        {
            if (EntityUtils.EntityPropertiesMap[type].hasInventory)
            {
                return extraData[Constants.JsonKeys.Entity.INVENTORY] as Inventory;
            }
            return null;
        }

        /// <summary>
        /// Sets a value for the (player) inventory.
        /// </summary>
        /// <param name="inventory">The new inventory value.</param>
        public bool TrySetInventory(Inventory inventory)
        {
            if (EntityUtils.EntityPropertiesMap[type].hasInventory)
            {
                extraData[Constants.JsonKeys.Entity.INVENTORY] = inventory;
                return true;
            }
            return false;
        }


        /// <summary>
        /// Displays the player's stats.
        /// </summary>
        /// <param name="displayInventory">If the inventory should be displayed.</param>
        public void Stats(bool displayInventory = true)
        {
            Console.WriteLine($"\nName: {FullName}\n\nSTATS:");
            Console.WriteLine($"HP: {CurrentHp}/{MaxHp}\nAttack: {Attack}\nDefence: {Defence}\nAgility: {Agility}\n");
            if (displayInventory)
            {
                Console.WriteLine(TryGetInventory());
            }
        }
        #endregion
        #endregion

        #region Private methods
        /// <summary>
        /// Sets up the entity's stats acording to its base attributes.
        /// </summary>
        private void SetupStats(int? currentHp = null)
        {
            double tempMaxHp = baseMaxHp;
            double tempAttack = baseAttack;
            double tempDefence = baseDefence;
            double tempAgility = baseAgility;

            foreach (var attribute in attributes)
            {
                var (maxHp, attack, defence, agility) = EntityUtils.AttributeStatChangeMap[attribute];
                tempMaxHp *= maxHp;
                tempAttack *= attack;
                tempDefence *= defence;
                tempAgility *= agility;
            }
            MaxHp = (int)Math.Clamp(tempMaxHp, int.MinValue, int.MaxValue);
            CurrentHp = currentHp ?? MaxHp;
            Attack = (int)Math.Clamp(tempAttack, int.MinValue, int.MaxValue);
            Defence = (int)Math.Clamp(tempDefence, int.MinValue, int.MaxValue);
            Agility = (int)Math.Clamp(tempAgility, int.MinValue, int.MaxValue);
        }

        /// <summary>
        /// Sets the extra value for the given key, if it exists in the given extra data dict, or the default value.
        /// </summary>
        /// <param name="entity">The entity to set the extra value.</param>
        /// <param name="extraData">The extra data to use.</param>
        /// <param name="key">The key of the data.</param>
        /// <param name="defaultValue">The default value for the data.</param>
        private static void SetExtraDataValueFromConstructor(
            Entity entity,
            Dictionary<string, object?>? extraData,
            string key,
            object? defaultValue
        )
        {
            entity.extraData[key] =
                extraData is not null && extraData.TryGetValue(key, out var value)
                    ? value
                    : defaultValue;
        }
        #endregion

        #region Public overrides
        public override string ToString()
        {
            var typeLine = type != EntityType.PLAYER ? $"\nSpecies: {EntityUtils.EntityPropertiesMap[type].displayName}" : "";
            var attributesStr = string.Join(", ", attributes);
            var originalTeamStr = originalTeam == 0 ? "Player" : originalTeam.ToString();
            var teamStr = currentTeam == 0 ? "Player" : currentTeam.ToString();
            var dropsStr = string.Join("\n, ", drops.Select(d => $"\t{d}"));
            var entityStr = $"Name: {name}{typeLine}\n" +
                $"Full name: {FullName}\n" +
                $"Hp: {MaxHp}\n" +
                $"Attack: {Attack}\n" +
                $"Defence: {Defence}\n" +
                $"Agility: {Agility}\n" +
                $"Attributes: {attributesStr}\n" +
                $"Original team: {originalTeamStr}\n" +
                $"Current team: {teamStr}\n" +
                $"Drops: {dropsStr}\n" +
                $"{(EntityUtils.EntityPropertiesMap[type].hasInventory ? $"{TryGetInventory()}\n" : "")}" +
                $"Position: {Position}\n" +
                $"Rotation: {facing}";
            return entityStr;
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Generates an entity name.
        /// </summary>
        /// <param name="randomGenrator">The genrator to use.</param>
        public static string GenerateEntityName(SplittableRandom? randomGenrator = null)
        {
            return SentenceGenerator.GenerateNameSequence((2, 3), randomGenerator: randomGenrator ?? RandomStates.Instance.MiscRandom);
        }
        #endregion

        #region JsonConvert
        static List<(Action<JsonDictionary, (long absoluteX, long absoluteY)?> objectJsonCorrecter, string newFileVersion)> IJsonConvertableExtra<Entity, (long absoluteX, long absoluteY)?>.VersionCorrecters { get; } =
        [
            // 2.0 -> 2.0.1
            ((oldJson, extraData) =>
            {
                // player inventory items in dictionary
                if (
                    oldJson.TryGetValue("type", out var entityType) &&
                    entityType?.Value.ToString() == "player"
                )
                {
                    oldJson.TryGetValue("inventory", out var inventoryJson);
                    oldJson["inventory"] = new JsonDictionary() { ["items"] = inventoryJson };
                }
            }, "2.0.1"),
            // 2.1 -> 2.1.1
            ((oldJson, extraData) =>
            {
                // renamed speed to agility
                JsonDataCorrecterUtils.RenameKeyIfExists(oldJson, "baseSpeed", "baseAgility");
            }, "2.1.1"),
            // 2.1.1 -> 2.2
            ((oldJson, extraData) =>
            {
                // snake case keys
                JsonDataCorrecterUtils.RemapKeysIfExist(oldJson, new Dictionary<string, string> {
                    ["baseMaxHp"] = "base_max_hp",
                    ["currentHp"] = "current_hp",
                    ["baseAttack"] = "base_attack",
                    ["baseDefence"] = "base_defence",
                    ["baseAgility"] = "base_agility",
                    ["originalTeam"] = "original_team",
                    ["currentTeam"] = "current_team",
                    ["xPos"] = "x_position",
                    ["yPos"] = "y_position",
                });
            }, "2.2"),
            // 2.4 -> 2.4.1
            ((oldJson, extraData) =>
            {
                // namespaced type
                JsonDataCorrecterUtils.TransformValue<Entity, string>(
                    oldJson, "type", (value) => (true, ConfigUtils.GetSpecificNamespacedString(value))
                );
            }, "2.4.1"),
        ];

        static bool IJsonConvertableExtra<Entity, (long absoluteX, long absoluteY)?>.FromJsonWithoutCorrection(
            JsonDictionary entityJson,
            (long absoluteX, long absoluteY)? extraData,
            string fileVersion,
            [NotNullWhen(true)] ref Entity? entityObject
        )
        {
            if (
                !PACTools.TryParseJsonValue<Entity, EnumValue<EntityType>>(
                    entityJson,
                    Constants.JsonKeys.Entity.TYPE,
                    out var entityType,
                    isCritical: true
                ) ||
                !EntityUtils.EntityPropertiesMap.TryGetValue(entityType, out var properties)
            )
            {
                PACTools.LogJsonError<Entity>("invalid entity type", true);
                return false;
            }

            var success = true;
            success &= PACTools.TryParseJsonValue<Entity, string?>(entityJson, Constants.JsonKeys.Entity.NAME, out var name);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.BASE_MAX_HP, out var baseMaxHp);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.CURRENT_HP, out var currentHp);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.BASE_ATTACK, out var baseAttack);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.BASE_DEFENCE, out var baseDefence);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.BASE_AGILITY, out var baseAgility);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.ORIGINAL_TEAM, out var originalTeam);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.CURRENT_TEAM, out var currentTeam);
            success &= PACTools.TryParseJsonListValue<Entity, EnumValue<Attribute>>(entityJson, Constants.JsonKeys.Entity.ATTRIBUTES,
                attribute => {
                    var attributeSuccess = PACTools.TryParseValueForJsonParsing<Entity, EnumValue<Attribute>>(attribute, out var value, logParseWarnings: false);
                    success &= attributeSuccess;
                    return (attributeSuccess, value);
                },
                out var attributes);
            success &= PACTools.TryParseJsonListValue<Entity, AItem>(entityJson, Constants.JsonKeys.Entity.DROPS,
                dropJson => {
                    var dropSuccess = PACTools.TryFromJson(dropJson as JsonDictionary, fileVersion, out AItem? dropObject);
                    success &= dropSuccess;
                    return (dropSuccess, dropObject);
                },
                out var drops);

            success &= PACTools.TryParseJsonValue<Entity, Facing?>(entityJson, Constants.JsonKeys.Entity.FACING, out var facing);

            // specific extra constructor data
            var  extraConstData = new Dictionary<string, object?>();
            if (properties.hasInventory)
            {
                success &= PACTools.TryParseJsonConvertableValue<Entity, Inventory>(
                    entityJson,
                    fileVersion,
                    Constants.JsonKeys.Entity.INVENTORY,
                    out var inventory
                );
                extraConstData[Constants.JsonKeys.Entity.INVENTORY] = inventory ?? new Inventory();
            }

            entityObject = new Entity(
                entityType,
                extraData,
                name,
                baseMaxHp,
                currentHp,
                baseAttack,
                baseDefence,
                baseAgility,
                originalTeam,
                currentTeam,
                attributes,
                drops,
                facing,
                extraConstData
            );

            entityObject.SetupStats(entityObject.CurrentHp);
            entityObject.UpdateFullName();
            return success && entityObject is not null;
        }

        #region Methods
        public virtual JsonDictionary ToJson()
        {
            // attributes
            var attributesProcessed = attributes.Select(a => (JsonObject?)a).ToList();
            // drops
            var dropsJson = drops.Select(drop => (JsonObject?)drop.ToJson()).ToList();
            // properties
            var entityJson = new JsonDictionary
            {
                [Constants.JsonKeys.Entity.TYPE] = type,
                [Constants.JsonKeys.Entity.NAME] = name,
                [Constants.JsonKeys.Entity.BASE_MAX_HP] = baseMaxHp,
                [Constants.JsonKeys.Entity.CURRENT_HP] = CurrentHp,
                [Constants.JsonKeys.Entity.BASE_ATTACK] = baseAttack,
                [Constants.JsonKeys.Entity.BASE_DEFENCE] = baseDefence,
                [Constants.JsonKeys.Entity.BASE_AGILITY] = baseAgility,
                [Constants.JsonKeys.Entity.ORIGINAL_TEAM] = originalTeam,
                [Constants.JsonKeys.Entity.CURRENT_TEAM] = currentTeam,
                [Constants.JsonKeys.Entity.ATTRIBUTES] = attributesProcessed,
                [Constants.JsonKeys.Entity.DROPS] = dropsJson,
                [Constants.JsonKeys.Entity.FACING] = (int)facing,
            };

            var properties = EntityUtils.EntityPropertiesMap[type];
            if (properties.hasInventory)
            {
                entityJson[Constants.JsonKeys.Entity.INVENTORY] = TryGetInventory()!.ToJson();
            }

            return entityJson;
        }
        #endregion
        #endregion
    }
}
