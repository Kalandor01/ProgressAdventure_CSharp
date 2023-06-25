using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using Attribute = ProgressAdventure.Enums.Attribute;

namespace ProgressAdventure.Entity
{
    #region Monster classes
    #region Caveman
    /// <summary>
    /// Caveman entity.
    /// </summary>
    public class Caveman : Entity<Caveman>
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Caveman"/>
        /// </summary>
        /// <param name="team">The team of the entity.</param>
        public Caveman(int team = 1)
            : base(
                GetDefaultName(),
                GetBaseStats(team),
                GetDefaultDrops()
            )
        { }
        #endregion

        #region Functions
        /// <summary>
        /// Returns the newly rolled stats, specific to this entity type.
        /// </summary>
        public static EntityManagerStatsDTO GetBaseStats(int team = 1)
        {
            return EntityUtils.EntityManager(7, 7, 7, 7, originalTeam: team);
        }

        /// <summary>
        /// Returns the newly generated drops, specific to this entity type.
        /// </summary>
        public static List<Item> GetDefaultDrops()
        {
            return LootFactory.LootManager(new List<LootFactory> {
                new LootFactory(ItemType.Weapon.WOODEN_CLUB, 0.3),
                new LootFactory(ItemType.Material.CLOTH, 0.15, 0, 1, 3),
                new LootFactory(ItemType.Misc.COPPER_COIN, 0.35, 0, 4, 3)
            });
        }
        #endregion

        #region JsonConverter
        /// <summary>
        /// <inheritdoc cref="Caveman"/><br/>
        /// Can be used for loading the <c>Caveman</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        /// <param name="miscData">The json data, that can be used for loading extra data, specific to an entity type.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        protected Caveman((
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
    }
    #endregion

    #region Ghoul
    /// <summary>
    /// Ghoul entity.
    /// </summary>
    public class Ghoul : Entity<Ghoul>
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Ghoul"/>
        /// </summary>
        /// <param name="team">The team of the entity.</param>
        public Ghoul(int team = 1)
            : base(
                GetDefaultName(),
                GetBaseStats(team),
                GetDefaultDrops()
            )
        { }
        #endregion

        #region Functions
        /// <summary>
        /// Returns the newly rolled stats, specific to this entity type.
        /// </summary>
        public static EntityManagerStatsDTO GetBaseStats(int team = 1)
        {
            return EntityUtils.EntityManager(11, 9, 9, 9, originalTeam: team);
        }

        /// <summary>
        /// Returns the newly generated drops, specific to this entity type.
        /// </summary>
        public static List<Item> GetDefaultDrops()
        {
            return LootFactory.LootManager(new List<LootFactory> {
                new LootFactory(ItemType.Weapon.STONE_SWORD, 0.2),
                new LootFactory(ItemType.Misc.ROTTEN_FLESH, 0.55, 0, 3),
                new LootFactory(ItemType.Misc.COPPER_COIN, 0.4, 0, 5, 4)
            });
        }
        #endregion

        #region JsonConverter
        /// <summary>
        /// <inheritdoc cref="Ghoul"/><br/>
        /// Can be used for loading the <c>Ghoul</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        /// <param name="miscData">The json data, that can be used for loading extra data, specific to an entity type.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        protected Ghoul((
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
    }
    #endregion

    #region Troll
    /// <summary>
    /// Troll entity.
    /// </summary>
    public class Troll : Entity<Troll>
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Troll"/>
        /// </summary>
        /// <param name="team">The team of the entity.</param>
        public Troll(int team = 1)
            : base(
                GetDefaultName(),
                GetBaseStats(team),
                GetDefaultDrops()
            )
        { }
        #endregion

        #region Functions
        /// <summary>
        /// Returns the newly rolled stats, specific to this entity type.
        /// </summary>
        public static EntityManagerStatsDTO GetBaseStats(int team = 1)
        {
            return EntityUtils.EntityManager(13, 11, 11, 5, originalTeam: team);
        }

        /// <summary>
        /// Returns the newly generated drops, specific to this entity type.
        /// </summary>
        public static List<Item> GetDefaultDrops()
        {
            return LootFactory.LootManager(new List<LootFactory> {
                new LootFactory(ItemType.Weapon.CLUB_WITH_TEETH, 0.25),
                new LootFactory(ItemType.Material.CLOTH, 0.25, 1, 3, 2),
                new LootFactory(ItemType.Material.TEETH, 0.35, 1, 5, 2),
                new LootFactory(ItemType.Misc.SILVER_COIN, 0.3, 1, 3, 3)
            });
        }
        #endregion

        #region JsonConverter
        /// <summary>
        /// <inheritdoc cref="Troll"/><br/>
        /// Can be used for loading the <c>Troll</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        /// <param name="miscData">The json data, that can be used for loading extra data, specific to an entity type.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        protected Troll((
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
    }
    #endregion

    #region Dragon
    /// <summary>
    /// ragon entity.
    /// </summary>
    public class Dragon : Entity<Dragon>
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Dragon"/>
        /// </summary>
        /// <param name="team">The team of the entity.</param>
        public Dragon(int team = -1)
            : base(
                GetDefaultName(),
                GetBaseStats(team),
                GetDefaultDrops()
            )
        { }
        #endregion

        #region Functions
        /// <summary>
        /// Returns the newly rolled stats, specific to this entity type.
        /// </summary>
        public static EntityManagerStatsDTO GetBaseStats(int team = -1)
        {
            return EntityUtils.EntityManager(
                100, 50, 50, 20,
                10, 20,
                new AttributeChancesDTO(
                    crippledChance: 0,
                    sickChance: 0,
                    weakChance: 0,
                    frailChance: 0,
                    slowChance: 0
                ),
                team, 0
            );
        }

        /// <summary>
        /// Returns the newly generated drops, specific to this entity type.
        /// </summary>
        public static List<Item> GetDefaultDrops()
        {
            return LootFactory.LootManager(new List<LootFactory> {
                new LootFactory(ItemType.Misc.GOLD_COIN, 0.8, 1, 10, 10),
                new LootFactory(ItemType.Misc.SILVER_COIN, 0.9, 5, 15, 8),
                new LootFactory(ItemType.Misc.COPPER_COIN, 1, 10, 20, 5),
                new LootFactory(ItemType.Weapon.STEEL_SWORD, 1, 0, 10),
                new LootFactory(ItemType.Defence.WOODEN_SHIELD, 1, 0, 5),
            });
        }
        #endregion

        #region JsonConverter
        /// <summary>
        /// <inheritdoc cref="Dragon"/><br/>
        /// Can be used for loading the <c>Dragon</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        /// <param name="miscData">The json data, that can be used for loading extra data, specific to an entity type.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        protected Dragon((
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
    }
    #endregion
    #endregion
}
