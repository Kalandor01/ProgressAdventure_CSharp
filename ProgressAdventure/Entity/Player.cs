using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using Attribute = ProgressAdventure.Enums.Attribute;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// The player entity.
    /// </summary>
    public class Player : Entity
    {
        #region Public fields
        /// <summary>
        /// The inventory of the palyer.
        /// </summary>
        public Inventory inventory;
        /// <summary>
        /// The position of the player in the world.
        /// </summary>
        public (long x, long y) position;
        /// <summary>
        /// The facing direction of the player.
        /// </summary>
        public Facing facing;
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="Player"/>
        /// </summary>
        /// <param name="inventory"><inheritdoc cref="inventory" path="//summary"/></param>
        /// <param name="position"><inheritdoc cref="position" path="//summary"/></param>
        /// <param name="facing"><inheritdoc cref="facing" path="//summary"/></param>
        /// <inheritdoc cref="Entity(string, int, int, int , int , int , int? , List{Attribute}?, List{Item}?)"/>
        public Player(string? name = null, Inventory? inventory = null, (long x, long y)? position = null, Facing? facing = null)
            :this(
                EntityUtils.EntityManager(
                    (14, 20, 26),
                    (7, 10, 13),
                    (7, 10, 13),
                    (1, 10, 20),
                    0,
                    0,
                    0,
                    string.IsNullOrWhiteSpace(name) ? "You" : name
                ),
                inventory,
                position,
                facing
            )
        { }

        /// <summary>
        /// <inheritdoc cref="Player"/>
        /// </summary>
        /// <param name="inventory"><inheritdoc cref="inventory" path="//summary"/></param>
        /// <param name="position"><inheritdoc cref="position" path="//summary"/></param>
        /// <param name="facing"><inheritdoc cref="facing" path="//summary"/></param>
        /// <inheritdoc cref="Entity(string, int, int, int , int , int , int? , List{Attribute}?, List{Item}?)"/>
        public Player(
            string? name,
            int baseHp,
            int baseAttack,
            int baseDefence,
            int baseSpeed,
            Inventory? inventory = null,
            (long x, long y)? position = null,
            Facing? facing = null
        )
            : base(string.IsNullOrWhiteSpace(name) ? "You" : name, baseHp, baseAttack, baseDefence, baseSpeed)
        {
            this.inventory = inventory ?? new Inventory();
            this.position = position is not null ? (position.Value.x,  position.Value.y) : (0, 0);
            this.facing = facing ?? Facing.NORTH;
            UpdateFullName();
        }
        #endregion

        #region Private constructors
        /// <summary>
        /// <inheritdoc cref="Player"/>
        /// </summary>
        /// <param name="stats">The tuple of stats, representin all stat values.</param>
        /// <param name="inventory"><inheritdoc cref="inventory" path="//summary"/></param>
        /// <param name="position"><inheritdoc cref="position" path="//summary"/></param>
        /// <param name="facing"><inheritdoc cref="facing" path="//summary"/></param>
        private Player(
            (string name, int baseHpValue, int baseAttackValue, int baseDefenceValue, int baseSpeedValue, int originalTeam, int currentTeam, List<Attribute> attributes) stats,
            Inventory? inventory = null,
            (long x, long y)? position = null,
            Facing? facing = null
        )
            : this(
                  stats.name,
                  stats.baseHpValue,
                  stats.baseAttackValue,
                  stats.baseDefenceValue,
                  stats.baseSpeedValue,
                  inventory,
                  position,
                  facing
                )
        { }
        #endregion

        #region Public methods
        /// <summary>
        /// Turns the player in a random direction, that is weighted in the direction that it's already going towards.
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
                    Logger.Log("Player turned", $"{oldFacing} -> {facing}", LogSeverity.DEBUG);
                }
            }
        }

        /// <summary>
        /// Moves the player in the direction it's facing.
        /// </summary>
        /// <param name="multiplierVector">The multiplier to move the player by.</param>
        /// <param name="facing">If not null, it will move in that direction instead.</param>
        public void Move((double x, double y)? multiplierVector = null, Facing? facing = null)
        {
            var oldPosition = position;
            var moveRaw = EntityUtils.facingToMovementVectorMapping[facing ?? this.facing];
            var move = Utils.VectorMultiply(moveRaw, multiplierVector ?? (1, 1));
            position = ((long x, long y))Utils.VectorAdd(position, move, true);
            Logger.Log("Player moved", $"{oldPosition} -> {position}", LogSeverity.DEBUG);
        }

        /// <summary>
        /// Returns a json representation of the <c>Entity</c>.
        /// </summary>
        public new Dictionary<string, object?> ToJson()
        {
            var playerJson = base.ToJson();
            playerJson["xPos"] = position.x;
            playerJson["yPos"] = position.y;
            playerJson["facing"] = (int)facing;
            playerJson["inventory"] = inventory.ToJson();
            return playerJson;
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Converts the <c>Player</c> json to object format.
        /// </summary>
        /// <param name="playerJson">The json representation of the <c>Player</c> object.</param>
        public static Player? FromJson(IDictionary<string, object?>? playerJson)
        {
            if (playerJson is null)
            {
                Logger.Log("Player parse error", "player json is null", LogSeverity.ERROR);
                return null;
            }
            // name
            string name;
            if (playerJson.TryGetValue("name", out var playerName) &&
                playerName is not null &&
                playerName.ToString() is not null
            )
            {
                name = playerName.ToString();
            }
            else
            {
                Logger.Log("Player parse error", "couldn't parse player name", LogSeverity.WARN);
                name = "You";
            }
            // inventory
            Inventory? inventory = null;
            if (
                playerJson.TryGetValue("inventory", out var inventoryValue)
            )
            {
                inventory = Inventory.FromJson((IEnumerable<object>)inventoryValue);
            }
            else
            {
                Logger.Log("Player parse error", "couldn't parse player inventory", LogSeverity.WARN);
            }
            // position
            (long x, long y)? position = null;
            if (
                long.TryParse(playerJson.TryGetValue("xPos", out var xPositionValue) ? xPositionValue.ToString() : null, out long xPosition) &&
                long.TryParse(playerJson.TryGetValue("yPos", out var yPositionValue) ? yPositionValue.ToString() : null, out long yPosition)
            )
            {
                position = (xPosition, yPosition);
            }
            else
            {
                Logger.Log("Couldn't parse player position from Player JSON", severity: LogSeverity.WARN);
            }
            // facing
            Facing? facing = null;
            if (
                Enum.TryParse(typeof(Facing), playerJson.TryGetValue("facing", out var facingValue) ? facingValue.ToString() : null, out object? facingEnum) &&
                Enum.IsDefined(typeof(Facing), (Facing)facingEnum)
            )
            {
                facing = (Facing)facingEnum;
            }
            else
            {
                Logger.Log("Couldn't parse player facing from Player JSON", severity: LogSeverity.WARN);
            }
            Player player;
            // stats
            if (
                int.TryParse(playerJson.TryGetValue("baseHp", out var baseHpValue) ? baseHpValue.ToString() : null, out int baseHp) &&
                int.TryParse(playerJson.TryGetValue("baseAttack", out var baseAttackValue) ? baseAttackValue.ToString() : null, out int baseAttack) &&
                int.TryParse(playerJson.TryGetValue("baseDefence", out var baseDefenceValue) ? baseDefenceValue.ToString() : null, out int baseDefence) &&
                int.TryParse(playerJson.TryGetValue("baseSpeed", out var baseSpeedValue) ? baseSpeedValue.ToString() : null, out int baseSpeed)
            )
            {
                player = new Player(name, baseHp, baseAttack, baseDefence, baseSpeed, inventory, position, facing);
            }
            else
            {
                Logger.Log("Couldn't parse player stats from Player JSON", severity: LogSeverity.WARN);
                player = new Player(name, inventory, position, facing);
            }
            return player;
        }
        #endregion

        #region Public overrides
        public override string ToString()
        {
            return $"{base.ToString()}\n{this.inventory}\nPosition: {this.position}\nRotation: {this.facing}";
        }
        #endregion
    }
}
