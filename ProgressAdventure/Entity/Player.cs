using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using Attribute = ProgressAdventure.Enums.Attribute;

namespace ProgressAdventure.Entity
{
    public class Player : Entity
    {
        #region Public fields
        public Inventory inventory;
        public (int x, int y) position;
        public Facing facing;
        #endregion

        #region Public constructors
        public Player(string name = "You", Inventory? inventory = null, (int x, int y)? position = null, Facing? facing = null)
            :this(
                EntityUtils.EntityManager(
                    (14, 20, 26),
                    (7, 10, 13),
                    (7, 10, 13),
                    (1, 10, 20),
                    0,
                    0,
                    0,
                    name
                ),
                inventory,
                position,
                facing
            )
        { }

        public Player(
            string name,
            int baseHp,
            int baseAttack,
            int baseDefence,
            int baseSpeed,
            Inventory? inventory = null,
            (int x, int y)? position = null,
            Facing? facing = null
        )
            : base(name ?? "You", baseHp, baseAttack, baseDefence, baseSpeed)
        {
            this.inventory = inventory ?? new Inventory();
            this.position = position is not null ? (position.Value.x,  position.Value.y) : (0, 0);
            this.facing = facing ?? Facing.NORTH;
            UpdateFullName();
        }
        #endregion

        #region Private constructors
        private Player(
            (string name, int baseHpValue, int baseAttackValue, int baseDefenceValue, int baseSpeedValue, int originalTeam, int currentTeam, List<Attribute> attributes) stats,
            Inventory? inventory = null,
            (int x, int y)? position = null,
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
            if (SaveData.MainRandom.GenerateDouble() < 0.2)
            {
                var oldFacing = facing;
                var movementVector = EntityUtils.facingToMovementVectorMapping[facing];
                Facing? newFacing;
                // back
                if (SaveData.MainRandom.GenerateDouble() < 0.2)
                {
                    var (x, y) = Utils.VectorMultiply(movementVector, (-1, -1));
                    newFacing = EntityUtils.MovementVectorToFacing(((int)x, (int)y));
                }
                // side / diagonal
                else
                {
                    // side
                    if (SaveData.MainRandom.GenerateDouble() < 0.2)
                    {
                        if (SaveData.MainRandom.GenerateDouble() < 0.5)
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
                            var diagonalDir = SaveData.MainRandom.GenerateDouble() < 0.5 ? 1 : -1;
                            newFacing = EntityUtils.MovementVectorToFacing((
                                movementVector.x == 0 ? diagonalDir : movementVector.x,
                                movementVector.y == 0 ? diagonalDir : movementVector.y
                            ));
                        }
                        // diagonal to straight
                        else
                        {
                            var resetX = SaveData.MainRandom.GenerateDouble() < 0.5;
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
            position = ((int x, int y))Utils.VectorAdd(position, move, true);
            Logger.Log("Player moved", $"{oldPosition} -> {position}", LogSeverity.DEBUG);
        }

        /// <summary>
        /// Returns a json representation of the <c>Entity</c>.
        /// </summary>
        public new Dictionary<string, object?> ToJson()
        {
            var playerJson = base.ToJson();
            playerJson["x_pos"] = position.x;
            playerJson["y_pos"] = position.y;
            playerJson["rotation"] = (int)facing;
            playerJson["inventory"] = inventory.ToJson();
            return playerJson;
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
