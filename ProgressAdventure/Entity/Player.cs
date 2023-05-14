using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.WorldManagement;
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
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="Player"/>
        /// </summary>
        /// <param name="inventory"><inheritdoc cref="inventory" path="//summary"/></param>
        /// <inheritdoc cref="Entity(string, int, int, int , int , int , int? , List{Attribute}?, List{Item}?)"/>
        public Player(
            string? name = null,
            Inventory? inventory = null,
            (long x, long y)? position = null,
            Facing? facing = null
        )
            :this(
                string.IsNullOrWhiteSpace(name) ? "You" : name,
                EntityUtils.EntityManager(
                    (14, 20, 26),
                    (7, 10, 13),
                    (7, 10, 13),
                    (1, 10, 20),
                    0,
                    0,
                    0
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
            string? name,
            (int baseHpValue, int baseAttackValue, int baseDefenceValue, int baseSpeedValue, int originalTeam, int currentTeam, List<Attribute> attributes) stats,
            Inventory? inventory = null,
            (long x, long y)? position = null,
            Facing? facing = null
        )
            : this(
                  name,
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
        public override void SetPosition((long x, long y) position)
        {
            SetPosition(position, true);
        }

        public override void SetPosition((long x, long y) position, bool updateWorld)
        {
            base.SetPosition(position, updateWorld);
            if (updateWorld )
            {
                World.TryGetTileAll(position, out var tile);
                tile.Visit();
            }
        }

        /// <summary>
        /// Returns a json representation of the <c>Entity</c>.
        /// </summary>
        public new Dictionary<string, object?> ToJson()
        {
            var playerJson = base.ToJson();
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
            var entityDataRaw = FromJsonInternal(playerJson);
            if (entityDataRaw is null)
            {
                return null;
            }
            var entityData = entityDataRaw.Value;
            entityData.name ??= "You";
            Inventory? inventory = null;
            if (
                playerJson.TryGetValue("inventory", out var inventoryValue)
            )
            {
                inventory = Inventory.FromJson((IEnumerable<object?>?)inventoryValue);
            }
            else
            {
                Logger.Log("Player parse error", "couldn't parse player inventory", LogSeverity.WARN);
            }
            // player
            Player player;
            if (
                entityData.baseHp is not null &&
                entityData.baseAttack is not null &&
                entityData.baseDefence is not null &&
                entityData.baseSpeed is not null
            )
            {
                player = new Player(
                    entityData.name,
                    (int)entityData.baseHp,
                    (int)entityData.baseAttack,
                    (int)entityData.baseDefence,
                    (int)entityData.baseSpeed,
                    inventory,
                    entityData.position,
                    entityData.facing
                );
            }
            else
            {
                player = new Player(entityData.name, inventory, entityData.position, entityData.facing);
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
