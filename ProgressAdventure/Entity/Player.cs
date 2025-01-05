using PACommon.JsonUtils;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.WorldManagement;
using Attribute = ProgressAdventure.Enums.Attribute;
using PACTools = PACommon.Tools;

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
            : this(
                Tools.CorrectPlayerName(name),
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
            : base(
                name ?? GetDefaultName(),
                new EntityManagerStatsDTO(
                    stats.baseMaxHp,
                    stats.baseAttack,
                    stats.baseDefence,
                    stats.baseAgility,
                    0,
                    0,
                    []
                ),
                []
            )
        {
            this.inventory = inventory ?? new Inventory();
            if (position is not null)
            {
                this.position = ((long x, long y))position;
            }
            if (facing is not null)
            {
                this.facing = (Facing)facing;
            }
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
            if (updateWorld)
            {
                World.TryGetTileAll(position, out var tile);
                tile.Visit();
            }
        }

        /// <summary>
        /// Displays the player's stats.
        /// </summary>
        /// <param name="displayInventory">If the inventory should be displayed.</param>
        public void Stats(bool displayInventory = true)
        {
            Console.WriteLine($"\nName: {FullName}\n\nSTATS:");
            Console.WriteLine($"HP: {CurrentHp}/{MaxHp}\nAttack: {Attack}\nDefence: {Defence}\nAgility: {Agility}\n");
            if(displayInventory )
            {
                Console.WriteLine(inventory);
            }
        }
        #endregion

        #region Public overrides
        public override string ToString()
        {
            return $"{base.ToString()}\n{inventory}\nPosition: {position}\nRotation: {facing}";
        }
        #endregion

        #region JsonConvertable
        #region Protected properties
        protected static List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> MiscVersionCorrecters { get; } =
        [
            // 2.0 -> 2.0.1
            (oldJson =>
            {
                // inventory items in dictionary
                oldJson.TryGetValue("inventory", out var inventoryJson);
                oldJson["inventory"] = new JsonDictionary() { ["items"] = inventoryJson };
            }, "2.0.1"),
        ];
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Player"/><br/>
        /// Can be used for loading the <c>Player</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        protected Player(GenericEntityConstructorParametersDTO entityData)
            : base(entityData) { }
        #endregion

        #region Methods
        public override JsonDictionary ToJson()
        {
            var playerJson = base.ToJson();
            playerJson[Constants.JsonKeys.Player.INVENTORY] = inventory.ToJson();
            return playerJson;
        }

        protected override bool FromMiscJson(JsonDictionary miscJson, string fileVersion)
        {
            var success = PACTools.TryParseJsonConvertableValue<Player, Inventory>(miscJson, fileVersion, Constants.JsonKeys.Player.INVENTORY, out var inventory);
            this.inventory = inventory ?? new Inventory();
            return success;
        }
        #endregion
        #endregion
    }
}
