using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.WorldManagement;
using Attribute = ProgressAdventure.Enums.Attribute;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// The player entity.
    /// </summary>
    public class Player : Entity<Player>
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
                    new AttributeChancesDTO(0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
                    0,
                    0
                ),
                inventory,
                position,
                facing
            )
        { }
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
            EntityManagerStatsDTO stats,
            Inventory? inventory = null,
            (long x, long y)? position = null,
            Facing? facing = null
        )
            : this(
                (
                    name,
                    stats.baseMaxHp,
                    stats.baseMaxHp,
                    stats.baseAttack,
                    stats.baseDefence,
                    stats.baseAgility,
                    0,
                    0,
                    new List<Attribute>(),
                    new List<Item>(),
                    position,
                    facing
                ),
                null,
                Constants.SAVE_VERSION
            )
        {
            this.inventory = inventory ?? new Inventory();
        }
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
        /// Displays the player's stats.
        /// </summary>
        public void Stats()
        {
            Console.WriteLine($"\nName: {FullName}\n\nSTATS:");
            Console.WriteLine($"HP: {CurrentHp}/{MaxHp}\nAttack: {Attack}\nDefence: {Defence}\nAgility: {Agility}\n");
            Console.WriteLine(inventory);
        }
        #endregion

        #region Public overrides
        public override string ToString()
        {
            return $"{base.ToString()}\n{inventory}\nPosition: {position}\nRotation: {facing}";
        }
        #endregion

        #region JsonConvertable
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Player"/><br/>
        /// Can be used for loading the <c>Player</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        /// <param name="miscData">The json data, that can be used for loading extra data, specific to an entity type.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        protected Player((
            string? name,
            int? baseMaxHp,
            int? currentHp,
            int? baseAttack,
            int? baseDefence,
            int? baseAgility,
            int? originalTeam,
            int? currentTeam,
            List<Attribute>? attributes,
            List<Item>? drops,
            (long x, long y)? position,
            Facing? facing
        ) entityData, IDictionary<string, object?>? miscData, string fileVersion)
            : base(entityData, miscData, fileVersion) { }
        #endregion

        #region Methods
        public override Dictionary<string, object?> ToJson()
        {
            var playerJson = base.ToJson();
            playerJson["inventory"] = inventory.ToJson();
            return playerJson;
        }

        protected override void FromMiscJson(IDictionary<string, object?> miscJson, string fileVersion)
        {
            //correct data
            if (!Tools.IsUpToDate(Constants.SAVE_VERSION, fileVersion))
            {
                Logger.Log("Player json data is old", "correcting data");
                // 2.0 -> 2.0.1
                var newFileVersion = "2.0.1";
                if (!Tools.IsUpToDate(newFileVersion, fileVersion))
                {
                    // inventory items in dictionary
                    miscJson.TryGetValue("inventory", out object? inventoryJson);
                    miscJson["inventory"] = new Dictionary<string, object?>() { ["items"] = inventoryJson };

                    Logger.Log("Corrected player json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                    fileVersion = newFileVersion;
                }
                Logger.Log("Player json data corrected");
            }

            //convert
            Inventory? inventoryTemp = null;
            if (
                miscJson.TryGetValue("inventory", out var inventoryValue)
            )
            {
                Inventory.FromJson(inventoryValue as IDictionary<string, object?>, fileVersion, out inventoryTemp);
            }
            else
            {
                Logger.Log("Player parse error", "couldn't parse player inventory", LogSeverity.WARN);
            }
            inventory = inventoryTemp ?? new Inventory();
        }
        #endregion
        #endregion
    }
}
