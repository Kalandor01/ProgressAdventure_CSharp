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
        /// Can be used for creating a new <c>Player</c>.
        /// </summary>
        /// <param name="name"><inheritdoc cref="Entity.name" path="//summary"/></param>
        /// <param name="inventory"><inheritdoc cref="inventory" path="//summary"/></param>
        /// <param name="position"><inheritdoc cref="Entity.position" path="//summary"/></param>
        /// <param name="facing"><inheritdoc cref="Entity.facing" path="//summary"/></param>
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
        /// <inheritdoc cref="Player"/><br/>
        /// Can be used for loading the <c>Player</c> from json.
        /// </summary>
        /// <param name="name"><inheritdoc cref="Entity.name" path="//summary"/></param>
        /// <param name="baseMaxHp"><inheritdoc cref="Entity.baseMaxHp" path="//summary"/></param>
        /// <param name="currentHp"><inheritdoc cref="Entity.CurrentHp" path="//summary"/></param>
        /// <param name="baseAttack"><inheritdoc cref="Entity.baseAttack" path="//summary"/></param>
        /// <param name="baseDefence"><inheritdoc cref="Entity.baseDefence" path="//summary"/></param>
        /// <param name="baseSpeed"><inheritdoc cref="Entity.baseSpeed" path="//summary"/></param>
        /// <param name="inventory"><inheritdoc cref="inventory" path="//summary"/></param>
        /// <param name="position"><inheritdoc cref="Entity.position" path="//summary"/></param>
        /// <param name="facing"><inheritdoc cref="Entity.facing" path="//summary"/></param>
        public Player(
            string? name,
            int baseMaxHp,
            int currentHp,
            int baseAttack,
            int baseDefence,
            int baseSpeed,
            Inventory? inventory = null,
            (long x, long y)? position = null,
            Facing? facing = null
        )
            : base(string.IsNullOrWhiteSpace(name) ? "You" : name, baseMaxHp, baseAttack, baseDefence, baseSpeed, currentHp, position:position, facing:facing)
        {
            this.inventory = inventory ?? new Inventory();
        }
        #endregion

        #region Private constructors
        /// <summary>
        /// <inheritdoc cref="Player"/><br/>
        /// Can be used for creating a new <c>Player</c>, from the result of the EntityManager function.
        /// </summary>
        /// <param name="stats">The tuple of stats, representin all stat values.</param>
        /// <param name="inventory"><inheritdoc cref="inventory" path="//summary"/></param>
        /// <param name="position"><inheritdoc cref="position" path="//summary"/></param>
        /// <param name="facing"><inheritdoc cref="facing" path="//summary"/></param>
        private Player(
            string? name,
            (int baseMaxHpValue, int baseAttackValue, int baseDefenceValue, int baseSpeedValue, int originalTeam, int currentTeam, List<Attribute> attributes) stats,
            Inventory? inventory = null,
            (long x, long y)? position = null,
            Facing? facing = null
        )
            : this(
                  name,
                  stats.baseMaxHpValue,
                  stats.baseMaxHpValue,
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
                entityData.baseMaxHp is not null &&
                entityData.currentHp is not null &&
                entityData.baseAttack is not null &&
                entityData.baseDefence is not null &&
                entityData.baseSpeed is not null
            )
            {
                player = new Player(
                    entityData.name,
                    (int)entityData.baseMaxHp,
                    (int)entityData.currentHp,
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
