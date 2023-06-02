using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.WorldManagement;
using System.Reflection;
using Attribute = ProgressAdventure.Enums.Attribute;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// A representation of an entity.<br/>
    /// Classes implementing this class MUST create a (protected) constructor, with signiture protected Type([return type from "FromJsonInternal()"] entityData, IDictionary<string, object?>? miscData) for FromJson<T>() to work.
    /// </summary>
    public abstract class Entity : IJsonReadable
    {
        #region Public fields
        /// <summary>
        /// The name of the <c>Entity</c>.
        /// </summary>
        public string name;
        /// <summary>
        /// The base hp of the <c>Entity</c>.
        /// </summary>
        public int baseMaxHp;
        /// <summary>
        /// The base attack of the <c>Entity</c>.
        /// </summary>
        public int baseAttack;
        /// <summary>
        /// The base defence of the <c>Entity</c>.
        /// </summary>
        public int baseDefence;
        /// <summary>
        /// The base speed of the <c>Entity</c>.
        /// </summary>
        public int baseSpeed;
        /// <summary>
        /// The original team that the <c>Entity</c> is a part of.
        /// </summary>
        public int originalTeam;
        /// <summary>
        /// The current team that the <c>Entity</c> is a part of.
        /// </summary>
        public int currentTeam;
        /// <summary>
        /// The list of attributes that the <c>Entity</c> has.
        /// </summary>
        public List<Attribute> attributes;
        /// <summary>
        /// The list of items that the <c>Entity</c> will drop on death.
        /// </summary>
        public List<Item> drops;
        /// <summary>
        /// The position of the entity in the world.
        /// </summary>
        public (long x, long y) position;
        /// <summary>
        /// The facing direction of the entity.
        /// </summary>
        public Facing facing;
        #endregion

        #region Public properties
        /// <summary>
        /// The full name of the <c>Entity</c>.
        /// </summary>
        public string FullName { get; protected set; }
        /// <summary>
        /// The maximum hp of the <c>Entity</c>.
        /// </summary>
        public int MaxHp { get; protected set; }
        /// <summary>
        /// The current hp of the <c>Entity</c>.
        /// </summary>
        public int CurrentHp { get; protected set; }
        /// <summary>
        /// The current attack of the <c>Entity</c>.
        /// </summary>
        public int Attack { get; protected set; }
        /// <summary>
        /// The current defence of the <c>Entity</c>.
        /// </summary>
        public int Defence { get; protected set; }
        /// <summary>
        /// The current speed of the <c>Entity</c>.
        /// </summary>
        public int Speed { get; protected set; }
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="Entity"/><br/>
        /// Can be used for creating a new <c>Entity</c>, from the result of the EntityManager function.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="stats">The tuple of stats, representin all other values from the other constructor, other than drops.</param>
        /// <param name="drops"><inheritdoc cref="drops" path="//summary"/></param>
        public Entity(
            string name,
            EntityManagerStats stats,
            List<Item>? drops = null
        )
            :this(
                (
                    name,
                    stats.baseMaxHp,
                    null,
                    stats.baseAttack,
                    stats.baseDefence,
                    stats.baseSpeed,
                    stats.originalTeam,
                    stats.currentTeam,
                    stats.attributes,
                    drops,
                    null,
                    null
                ),
                null
            ) { }
        #endregion

        #region Public methods
        /// <summary>
        /// Updates the full name of the entity.
        /// </summary>
        public void UpdateFullName()
        {
            string attributeNames = "";
            foreach (var attribute in attributes)
            {
                attributeNames += EntityUtils.attributeNameMap[attribute] + " ";
            }
            FullName = attributeNames + name;
            FullName = FullName[0].ToString().ToUpper() + FullName[1..].ToLower();
        }

        /// <summary>
        /// Turns the entity in a random direction, that is weighted in the direction that it's already going towards.
        /// </summary>
        public void WeightedTurn()
        {
            // turn
            if (RandomStates.MainRandom.GenerateDouble() < 0.2)
            {
                var oldFacing = facing;
                var movementVector = EntityUtils.facingToMovementVectorMapping[facing];
                Facing? newFacing;
                // back
                if (RandomStates.MainRandom.GenerateDouble() < 0.2)
                {
                    var (x, y) = Utils.VectorMultiply(movementVector, (-1, -1));
                    newFacing = EntityUtils.MovementVectorToFacing(((int)x, (int)y));
                }
                // side / diagonal
                else
                {
                    // side
                    if (RandomStates.MainRandom.GenerateDouble() < 0.2)
                    {
                        if (RandomStates.MainRandom.GenerateDouble() < 0.5)
                        {
                            newFacing = EntityUtils.MovementVectorToFacing((movementVector.y, movementVector.x));
                        }
                        else
                        {
                            var (x, y) = Utils.VectorMultiply(movementVector, (-1, -1));
                            newFacing = EntityUtils.MovementVectorToFacing(((int)y, (int)x));
                        }
                    }
                    // diagonal
                    else
                    {
                        // straight to diagonal
                        if (movementVector.x == 0 || movementVector.y == 0)
                        {
                            var diagonalDir = RandomStates.MainRandom.GenerateDouble() < 0.5 ? 1 : -1;
                            newFacing = EntityUtils.MovementVectorToFacing((
                                movementVector.x == 0 ? diagonalDir : movementVector.x,
                                movementVector.y == 0 ? diagonalDir : movementVector.y
                            ));
                        }
                        // diagonal to straight
                        else
                        {
                            var resetX = RandomStates.MainRandom.GenerateDouble() < 0.5;
                            newFacing = EntityUtils.MovementVectorToFacing((
                                resetX ? 0 : movementVector.x,
                                !resetX ? 0 : movementVector.y
                            ));
                        }
                    }

                }
                if (newFacing is not null)
                {
                    facing = (Facing)newFacing;
                    Logger.Log("Entity turned", $"name: {FullName}, {oldFacing} -> {facing}", LogSeverity.DEBUG);
                }
            }
        }

        /// <summary>
        /// Moves the entity in the direction it's facing.
        /// </summary>
        /// <param name="multiplierVector">The multiplier to move the entity by.</param>
        /// <param name="facing">If not null, it will move in that direction instead.</param>
        public void Move((double x, double y)? multiplierVector = null, Facing? facing = null)
        {
            var moveRaw = EntityUtils.facingToMovementVectorMapping[facing ?? this.facing];
            var move = Utils.VectorMultiply(moveRaw, multiplierVector ?? (1, 1));
            EntityUtils.EntityMover(this, ((long x, long y))move);
        }

        /// <summary>
        /// Modifies the position of the entity.
        /// </summary>
        /// <param name="position">The position to move to.</param>
        public virtual void SetPosition((long x, long y) position)
        {
            SetPosition(position, false);
        }

        /// <summary>
        /// Modifies the position of the entity.
        /// </summary>
        /// <param name="position">The position to move to.</param>
        /// <param name="updateWorld">Whether to update the tile, the entitiy is on.</param>
        public virtual void SetPosition((long x, long y) position, bool updateWorld)
        {
            var oldPosition = this.position;
            this.position = position;
            Logger.Log("Entity moved", $"name: {FullName}, {oldPosition} -> {this.position}", LogSeverity.DEBUG);
            if (updateWorld)
            {
                World.TryGetTileAll(this.position, out _);
            }
        }

        /// <summary>
        /// Makes the entity attack another one.
        /// </summary>
        /// <param name="target">The target entity.</param>
        public AttackResponse AttackEntity(Entity target)
        {
            Logger.Log("Attack log", $"{FullName} attacked {target.FullName}");
            // attacker dead
            if (CurrentHp <= 0)
            {
                Logger.Log("Attack log", $"{FullName}(attacker) is dead");
                return AttackResponse.ATTACKER_DEAD;
            }
            // enemy dead
            else if (target.CurrentHp <= 0)
            {
                Logger.Log("Attack log", $"{FullName}(attacked) is already dead");
                return AttackResponse.ENEMY_DEAD;
            }
            // enemy dodge
            else if (RandomStates.MiscRandom.GenerateDouble() > Speed * 1.0 / target.Speed - 0.1)
            {
                Logger.Log("Attack log", $"{target.FullName} dodged");
                return AttackResponse.ENEMY_DOGDED;
            }            
            // attack
            else
            {
                var attack = (int)RandomStates.MiscRandom.GenerateInRange(1, 7) + Attack;
                var damage = attack - target.Defence;
                // block
                if (damage <= 0)
                {
                    Logger.Log("Attack log", $"{target.FullName} blocked attack");
                    return AttackResponse.ENEMY_BLOCKED;
                }
                // hit
                else
                {
                    target.DamageEntity(damage);
                    Logger.Log("Attack log", $"{target.FullName} took {damage} damage ({target.CurrentHp})");
                    // kill
                    if (target.CurrentHp == 0)
                    {
                        Logger.Log("Attack log", $"{FullName} defeated {target.FullName}");
                        return AttackResponse.KILLED;
                    }
                    return AttackResponse.HIT;
                }
            }
        }

        /// <summary>
        /// Makes the entity take damage.
        /// </summary>
        /// <param name="damage">The amount of damage to take.</param>
        public void DamageEntity(int damage)
        {
            CurrentHp -= damage;
            if (CurrentHp < 0)
            {
                CurrentHp = 0;
            }
        }

        /// <summary>
        /// Returns the full name of the entity with their species in parentheses, unless it's a player.
        /// </summary>
        public string GetFullNameWithSpecies()
        {
            return FullName + (GetType() != typeof(Player) ? $" ({EntityUtils.GetEntityTypeName(this)})" : "");
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Sets up the entity's stats acording to its base attributes.
        /// </summary>
        private void SetupAttributes(int? currentHp = null)
        {
            double tempMaxHp = baseMaxHp;
            double tempAttack = baseAttack;
            double tempDefence = baseDefence;
            double tempSpeed = baseSpeed;

            foreach (var attribute in attributes)
            {
                var (maxHp, attack, defence, speed) = EntityUtils.attributeStatChangeMap[attribute];
                tempMaxHp *= maxHp;
                tempAttack *= attack;
                tempDefence *= defence;
                tempSpeed *= speed;
            }
            MaxHp = (int)Math.Clamp(tempMaxHp, int.MinValue, int.MaxValue);
            CurrentHp = currentHp ?? MaxHp;
            CurrentHp = Math.Clamp(CurrentHp, 0, MaxHp);
            Attack = (int)Math.Clamp(tempAttack, int.MinValue, int.MaxValue);
            Defence = (int)Math.Clamp(tempDefence, int.MinValue, int.MaxValue);
            Speed = (int)Math.Clamp(tempSpeed, int.MinValue, int.MaxValue);
        }
        #endregion

        #region Public overrides
        public override string ToString()
        {
            var originalTeamStr = originalTeam == 0 ? "Player" : originalTeam.ToString();
            var teamStr = currentTeam == 0 ? "Player" : currentTeam.ToString();
            var typeLine = GetType() != typeof(Player) ? $"\nSpecies: {EntityUtils.GetEntityTypeName(this)}" : "";
            return $"Name: {name}{typeLine}\nFull name: {FullName}\nHp: {MaxHp}\nAttack: {Attack}\nDefence: {Defence}\nSpeed: {Speed}\nAttributes: {attributes}\nOriginal team: {originalTeamStr}\nCurrent team: {teamStr}\nDrops: {drops}";
        }
        #endregion

        #region JsonConvert
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Entity"/><br/>
        /// Can be used for loading the <c>Entity</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        /// <param name="miscData">The json data, that can be used for loading extra data, specific to an entity type.<br/>
        /// Should only be null, if entity creation called this constructor.</param>
        protected Entity((
            string? name,
            int? baseMaxHp,
            int? currentHp,
            int? baseAttack,
            int? baseDefence,
            int? baseSpeed,
            int? originalTeam,
            int? currentTeam,
            List<Attribute>? attributes,
            List<Item>? drops,
            (long x, long y)? position,
            Facing? facing
        ) entityData, IDictionary<string, object?>? miscData)
        {
            name = entityData.name ?? GetDefaultName();
            position = entityData.position ?? (0, 0);
            facing = entityData.facing ?? Facing.NORTH;
            if (
                entityData.baseMaxHp is null ||
                entityData.baseAttack is null ||
                entityData.baseDefence is null ||
                entityData.baseSpeed is null ||
                entityData.originalTeam is null ||
                entityData.currentTeam is null ||
                entityData.attributes is null
            )
            {
                var ems = GetBaseStats();
                baseMaxHp = entityData.baseMaxHp ?? ems.baseMaxHp;
                baseAttack = entityData.baseAttack ?? ems.baseAttack;
                baseDefence = entityData.baseDefence ?? ems.baseDefence;
                baseSpeed = entityData.baseSpeed ?? ems.baseSpeed;
                originalTeam = entityData.originalTeam ?? ems.originalTeam;
                currentTeam = entityData.currentTeam ?? ems.currentTeam;
                attributes = entityData.attributes ?? ems.attributes;
            }
            else
            {
                baseMaxHp = (int)entityData.baseMaxHp;
                baseAttack = (int)entityData.baseAttack;
                baseDefence = (int)entityData.baseDefence;
                baseSpeed = (int)entityData.baseSpeed;
                originalTeam = (int)entityData.originalTeam;
                currentTeam = (int)entityData.currentTeam;
                attributes = entityData.attributes;
            }
            drops = entityData.drops ?? GetDefaultDrops();
            // not new entity call
            if (miscData is not null)
            {
                FromMiscJson(miscData);
            }
            // adjust properties
            SetupAttributes(entityData.currentHp);
            UpdateFullName();
        }
        #endregion

        #region Methods
        public virtual Dictionary<string, object?> ToJson()
        {
            // attributes
            var attributesProcessed = attributes.Select(a => a.ToString()).ToList();
            // drops
            var dropsJson = drops.Select(drop => drop.ToJson()).ToList();
            // properties
            return new Dictionary<string, object?>
            {
                ["type"] = EntityUtils.GetEntityTypeName(this),
                ["name"] = name,
                ["baseMaxHp"] = baseMaxHp,
                ["currentHp"] = CurrentHp,
                ["baseAttack"] = baseAttack,
                ["baseDefence"] = baseDefence,
                ["baseSpeed"] = baseSpeed,
                ["originalTeam"] = originalTeam,
                ["currentTeam"] = currentTeam,
                ["attributes"] = attributesProcessed,
                ["drops"] = dropsJson,
                ["xPos"] = position.x,
                ["yPos"] = position.y,
                ["facing"] = (int)facing,
            };
        }

        /// <summary>
        /// Converts the misc data of the entity from a json version.
        /// </summary>
        /// <param name="miscJson">The json representation of the misc data for the specific entity type.</param>
        protected virtual void FromMiscJson(IDictionary<string, object?> miscJson) { }
        #endregion

        #region Functions
        /// <summary>
        /// Converts the json representation of the entity to a specific entity object.
        /// </summary>
        /// <typeparam name="T">The type of the entity to try to convert into.</typeparam>
        /// <param name="entityJson">The json representation of the entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity type couldn't be converted from json with the required constructor.</exception>
        public static T? FromJson<T>(IDictionary<string, object?>? entityJson)
            where T : Entity<T>
        {
            return (T?)AnyEntityFromJsonPrivate(typeof(T), entityJson);
        }

        /// <summary>
        /// Converts any entity json, from a json format, into an entity object (if it implements the nececary protected constructor).
        /// </summary>
        /// <param name="entityJson">The json representation of an entity.</param>
        public static Entity? AnyEntityFromJson(IDictionary<string, object?>? entityJson)
        {
            if (entityJson is null)
            {
                Logger.Log("Entity parse error", "entity json is null", LogSeverity.ERROR);
                return null;
            }
            if (
                entityJson.TryGetValue("type", out object? entityTypeValue) &&
                entityTypeValue is not null
            )
            {
                if (
                    EntityUtils.entityTypeMap.TryGetValue(entityTypeValue.ToString() ?? "", out Type? entityType) &&
                    entityType is not null
                )
                {
                    return AnyEntityFromJsonPrivate(entityType, entityJson);
                }
                else
                {
                    Logger.Log("Entity parse error", "invalid entity type", LogSeverity.ERROR);
                    return null;
                }
            }
            else
            {
                Logger.Log("Entity parse error", "entity type json is null", LogSeverity.ERROR);
                return null;
            }
        }

        /// <summary>
        /// Returns a newly generated name of this entity, specific to this entity type.
        /// </summary>
        public static string GetDefaultName()
        {
            return EntityUtils.GetEntityNameFromClass(1);
        }

        /// <summary>
        /// Returns the newly rolled stats, specific to this entity type.
        /// </summary>
        public static EntityManagerStats GetBaseStats()
        {
            return EntityUtils.EntityManager(5, 5, 5, 5);
        }

        /// <summary>
        /// Returns the newly generated drops, specific to this entity type.
        /// </summary>
        public static List<Item> GetDefaultDrops()
        {
            return new List<Item> { new Item(ItemType.Misc.COPPER_COIN) };
        }

        /// <summary>
        /// Converts the json representation of the entity to a specific entity object.
        /// </summary>
        /// <param name="entityType">The type of the entity to try to convert into.</param>
        /// <param name="entityJson">The json representation of the entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity type couldn't be converted from json with the required constructor.</exception>
        private static Entity? AnyEntityFromJsonPrivate(Type entityType, IDictionary<string, object?>? entityJson)
        {
            var entityData = FromJsonPrivate(entityJson);
            if (entityData is null || entityJson is null)
            {
                return null;
            }

            // get entity
            var constructor = entityType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance,
                null,
                new[] { entityData.GetType(), entityJson.GetType() },
                null
            ) ?? throw new ArgumentNullException(message: $"Couldn't find required entity constructor for type \"{entityType}\"!", null);
            var entity = constructor.Invoke(new object[] { entityData, entityJson }) ?? throw new ArgumentNullException(message: $"Couldn't create entity object from type \"{entityType}\"!", null);
            return (Entity?)entity;
        }

        /// <summary>
        /// Converts the json representation of the <c>Entity</c> to a format that can easily be turned to an <c>Entity</c> object.
        /// </summary>
        /// <param name="entityJson">The json representation of the <c>Entity</c>.</param>
        private static (
            string? name,
            int? baseMaxHp,
            int? currentHp,
            int? baseAttack,
            int? baseDefence,
            int? baseSpeed,
            int? originalTeam,
            int? currentTeam,
            List<Attribute>? attributes,
            List<Item>? drops,
            (long x, long y)? position,
            Facing? facing
        )? FromJsonPrivate(IDictionary<string, object?>? entityJson)
        {
            if (entityJson is null)
            {
                Logger.Log("Entity parse error", "entity json is null", LogSeverity.ERROR);
                return null;
            }
            // name
            string? name = null;
            if (
                entityJson.TryGetValue("name", out var playerName) &&
                playerName is not null
            )
            {
                name = playerName.ToString();
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity name", LogSeverity.WARN);
            }
            // stats
            // max hp
            int? baseMaxHp = null;
            if (
                entityJson.TryGetValue("baseMaxHp", out var baseMaxHpValue) &&
                int.TryParse(baseMaxHpValue?.ToString(), out int maxHpValue)
            )
            {
                baseMaxHp = maxHpValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity base max hp", LogSeverity.WARN);
            }
            // current hp
            int? currentHp = null;
            if (
                entityJson.TryGetValue("currentHp", out var currentHpValueStr) &&
                int.TryParse(currentHpValueStr?.ToString(), out int currentHpValue)
            )
            {
                currentHp = currentHpValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity current hp", LogSeverity.WARN);
            }
            // attack
            int? baseAttack = null;
            if (
                entityJson.TryGetValue("baseAttack", out var baseAttackValue) &&
                int.TryParse(baseAttackValue?.ToString(), out int attackValue)
            )
            {
                baseAttack = attackValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity base attack", LogSeverity.WARN);
            }
            // defence
            int? baseDefence = null;
            if (
                entityJson.TryGetValue("baseDefence", out var baseDefenceValue) &&
                int.TryParse(baseDefenceValue?.ToString(), out int defenceValue)
            )
            {
                baseDefence = defenceValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity base defence", LogSeverity.WARN);
            }
            // speed
            int? baseSpeed = null;
            if (
                entityJson.TryGetValue("baseSpeed", out var baseSpeedValue) &&
                int.TryParse(baseSpeedValue?.ToString(), out int speedValue)
            )
            {
                baseSpeed = speedValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity base speed", LogSeverity.WARN);
            }
            // original team
            int? originalTeam = null;
            if (
                entityJson.TryGetValue("originalTeam", out var originalTeamValueStr) &&
                int.TryParse(originalTeamValueStr?.ToString(), out int originalTeamValue)
            )
            {
                originalTeam = originalTeamValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity original team", LogSeverity.WARN);
            }
            // current team
            int? currentTeam = null;
            if (
                entityJson.TryGetValue("currentTeam", out var currentTeamValueStr) &&
                int.TryParse(currentTeamValueStr?.ToString(), out int currentTeamValue)
            )
            {
                currentTeam = currentTeamValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity current team", LogSeverity.WARN);
            }
            // attributes
            List<Attribute>? attributes = null;
            if (
                entityJson.TryGetValue("attributes", out var attributesJson) &&
                attributesJson is not null
            )
            {
                attributes = new List<Attribute>();
                var attributeList = (IEnumerable<object?>)attributesJson;
                foreach (var attribute in attributeList)
                {
                    if (
                        Enum.TryParse(attribute?.ToString(), out Attribute attributeEnum) &&
                        Enum.IsDefined(attributeEnum)
                    )
                    {
                        attributes.Add(attributeEnum);
                    }
                    else
                    {
                        Logger.Log("Entity parse error", "entity attribute parse error", LogSeverity.WARN);
                    }
                }
            }
            else
            {
                Logger.Log("Entity parse error", "entity attributes json doesn't exist", LogSeverity.WARN);
            }
            // drops
            List<Item>? drops = null;
            if (
                entityJson.TryGetValue("drops", out var dropsJson) &&
                dropsJson is not null
            )
            {
                drops = new List<Item>();
                var dropList = (IEnumerable<object?>)dropsJson;
                foreach (var dropJson in dropList)
                {
                    var dropItem = Item.FromJson((IDictionary<string, object?>?)dropJson);
                    if (dropItem is not null)
                    {
                        drops.Add(dropItem);
                    }
                }
            }
            else
            {
                Logger.Log("Entity parse error", "entity drops json doesn't exist", LogSeverity.WARN);
            }
            // position
            (long x, long y)? position = null;
            if (
                entityJson.TryGetValue("xPos", out var xPositionValue) &&
                entityJson.TryGetValue("yPos", out var yPositionValue) &&
                long.TryParse(xPositionValue?.ToString(), out long xPosition) &&
                long.TryParse(yPositionValue?.ToString(), out long yPosition)
            )
            {
                position = (xPosition, yPosition);
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity position", LogSeverity.WARN);
            }
            // facing
            Facing? facing = null;
            if (
                entityJson.TryGetValue("facing", out var facingValue) &&
                Enum.TryParse(typeof(Facing), facingValue?.ToString(), out object? facingEnum) &&
                Enum.IsDefined(typeof(Facing), (Facing)facingEnum)
            )
            {
                facing = (Facing)facingEnum;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity facing", severity: LogSeverity.WARN);
            }
            return (
                name,
                baseMaxHp,
                currentHp,
                baseAttack,
                baseDefence,
                baseSpeed,
                originalTeam,
                currentTeam,
                attributes,
                drops,
                position,
                facing
            );
        }
        #endregion
        #endregion
    }
}
