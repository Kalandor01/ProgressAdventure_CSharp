using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.WorldManagement;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// A representation of an entity.
    /// </summary>
    public abstract class Entity
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
        public List<Enums.Attribute> attributes;
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
        public string FullName { get; private set; }
        /// <summary>
        /// The maximum hp of the <c>Entity</c>.
        /// </summary>
        public int MaxHp { get; private set; }
        /// <summary>
        /// The current hp of the <c>Entity</c>.
        /// </summary>
        public int CurrentHp { get; private set; }
        /// <summary>
        /// The current attack of the <c>Entity</c>.
        /// </summary>
        public int Attack { get; private set; }
        /// <summary>
        /// The current defence of the <c>Entity</c>.
        /// </summary>
        public int Defence { get; private set; }
        /// <summary>
        /// The current speed of the <c>Entity</c>.
        /// </summary>
        public int Speed { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Entity"/><br/>
        /// Can be used for loading the <c>Entity</c> from json.
        /// </summary>
        /// <param name="name"><inheritdoc cref="name" path="//summary"/></param>
        /// <param name="baseMaxHp"><inheritdoc cref="baseMaxHp" path="//summary"/></param>
        /// <param name="baseAttack"><inheritdoc cref="baseAttack" path="//summary"/></param>
        /// <param name="baseDefence"><inheritdoc cref="baseDefence" path="//summary"/></param>
        /// <param name="baseSpeed"><inheritdoc cref="baseSpeed" path="//summary"/></param>
        /// <param name="originalTeam"><inheritdoc cref="originalTeam" path="//summary"/></param>
        /// <param name="currentTeam"><inheritdoc cref="currentTeam" path="//summary"/></param>
        /// <param name="attributes"><inheritdoc cref="attributes" path="//summary"/></param>
        /// <param name="drops"><inheritdoc cref="drops" path="//summary"/></param>
        /// <param name="position"><inheritdoc cref="position" path="//summary"/></param>
        /// <param name="facing"><inheritdoc cref="facing" path="//summary"/></param>
        public Entity(
            string name,
            int baseMaxHp,
            int baseAttack,
            int baseDefence,
            int baseSpeed,
            int? currentHp = null,
            int originalTeam = 0,
            int? currentTeam = null,
            List<Enums.Attribute>? attributes = null,
            List<Item>? drops = null,
            (long x, long y) ? position = null,
            Facing? facing = null
        )
        {
            this.name = name;
            this.baseMaxHp = baseMaxHp;
            this.baseAttack = baseAttack;
            this.baseDefence = baseDefence;
            this.baseSpeed = baseSpeed;
            this.originalTeam = originalTeam;
            this.currentTeam = currentTeam ?? this.originalTeam;
            this.attributes = attributes ?? new List<Enums.Attribute>();
            this.drops = drops ?? new List<Item>();
            this.position = position ?? (0, 0);
            this.facing = facing ?? Facing.NORTH;
            // adjust properties
            SetupAttributes(currentHp);
            UpdateFullName();
        }

        /// <summary>
        /// <inheritdoc cref="Entity"/><br/>
        /// Can be used for creating a new <c>Entity</c>, from the result of the EntityManager function.
        /// </summary>
        /// <param name="stats">The tuple of stats, representin all other values from the other constructor, other than drops.</param>
        /// <param name="drops"><inheritdoc cref="drops" path="//summary"/></param>
        public Entity(
            string name,
            (
                int baseMaxHp,
                int baseAttack,
                int baseDefence,
                int baseSpeed,
                int originalTeam,
                int? team,
                List<Enums.Attribute>? attributes
            ) stats,
            List<Item>? drops = null
        )
            :this(
                 name,
                 stats.baseMaxHp,
                 stats.baseAttack,
                 stats.baseDefence,
                 stats.baseSpeed,
                 null,
                 stats.originalTeam,
                 stats.team,
                 stats.attributes,
                 drops
            ) { }
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
            FullName = fullName;
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
        /// Returns a json representation of the <c>Entity</c>.
        /// </summary>
        public Dictionary<string, object?> ToJson()
        {
            // attributes
            var attributesProcessed = attributes.Select(a => a.ToString()).ToList();
            // drops
            var dropsJson = drops.Select(drop => new Dictionary<string, object>
                {
                    ["type"] = drop.Type.ToString(),
                    ["amount"] = drop.amount,
                }).ToList();
            // properties
            var entityJson = new Dictionary<string, object?>
            {
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
            return entityJson;
        }

        /// <summary>
        /// Makes the entity attack another one.
        /// </summary>
        /// <param name="target">The target entity.</param>
        public void AttackEntity(Entity target)
        {
            target.CurrentHp -= Attack;
            if (target.CurrentHp < 0)
            {
                CurrentHp = 0;
            }
        }
        #endregion

        #region Protected functions
        protected static (string? name, (long x, long y)? position, Facing? facing, int? currentHp, int? baseMaxHp, int? baseAttack, int? baseDefence, int? baseSpeed)? FromJsonInternal(IDictionary<string, object?>? entityJson)
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
            return (name, position, facing, currentHp, baseMaxHp, baseAttack, baseDefence, baseSpeed);
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
            return $"Name: {name}\nFull name: {FullName}\nHp: {MaxHp}\nAttack: {Attack}\nDefence: {Defence}\nSpeed: {Speed}\nAttributes: {attributes}\nOriginal team: {originalTeamStr}\nCurrent team: {teamStr}\nDrops: {drops}";
        }
        #endregion
    }
}
