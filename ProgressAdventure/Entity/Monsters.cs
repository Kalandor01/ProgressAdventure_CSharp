using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;

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
        public static List<AItem> GetDefaultDrops()
        {
            return LootFactory.LootManager([
                new(ItemType.Weapon.CLUB, Material.WOOD, 0.3),
                new(ItemUtils.MATERIAL_ITEM_TYPE, Material.CLOTH, 0.15, 0, 1, 3),
                new(ItemType.Misc.COIN, Material.COPPER, 0.35, 0, 4, 3)
            ]);
        }
        #endregion

        #region JsonConverter
        /// <summary>
        /// <inheritdoc cref="Caveman"/><br/>
        /// Can be used for loading the <c>Caveman</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        protected Caveman(GenericEntityConstructorParametersDTO entityData)
            : base(entityData) { }
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
        public static List<AItem> GetDefaultDrops()
        {
            return LootFactory.LootManager([
                new(ItemType.Weapon.SWORD, Material.STONE, 0.2),
                new(ItemUtils.MATERIAL_ITEM_TYPE, Material.ROTTEN_FLESH, 0.55, 0, 3),
                new(ItemType.Misc.COIN, Material.COPPER, 0.4, 0, 5, 4)
            ]);
        }
        #endregion

        #region JsonConverter
        /// <summary>
        /// <inheritdoc cref="Ghoul"/><br/>
        /// Can be used for loading the <c>Ghoul</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        protected Ghoul(GenericEntityConstructorParametersDTO entityData)
            : base(entityData) { }
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
        public static List<AItem> GetDefaultDrops()
        {
            return LootFactory.LootManager([
                new(ItemType.Weapon.CLUB_WITH_TEETH, Material.WOOD, 0.25),
                new(ItemUtils.MATERIAL_ITEM_TYPE, Material.CLOTH, 0.25, 1, 3, 2),
                new(ItemUtils.MATERIAL_ITEM_TYPE, Material.TEETH, 0.35, 1, 5, 2),
                new(ItemType.Misc.COIN, Material.SILVER, 0.3, 1, 3, 3)
            ]);
        }
        #endregion

        #region JsonConverter
        /// <summary>
        /// <inheritdoc cref="Troll"/><br/>
        /// Can be used for loading the <c>Troll</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        protected Troll(GenericEntityConstructorParametersDTO entityData)
            : base(entityData) { }
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
        public static List<AItem> GetDefaultDrops()
        {
            return LootFactory.LootManager([
                new(ItemType.Misc.COIN, Material.GOLD, 0.8, 1, 10, 10),
                new(ItemType.Misc.COIN, Material.SILVER, 0.9, 5, 15, 8),
                new(ItemType.Misc.COIN, Material.COPPER, 1, 10, 20, 5),
                new(ItemType.Weapon.SWORD, Material.STEEL, 1, 0, 10),
                new(ItemType.Defence.SHIELD, Material.WOOD, 1, 0, 5),
            ]);
        }
        #endregion

        #region JsonConverter
        /// <summary>
        /// <inheritdoc cref="Dragon"/><br/>
        /// Can be used for loading the <c>Dragon</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        protected Dragon(GenericEntityConstructorParametersDTO entityData)
            : base(entityData) { }
        #endregion
    }
    #endregion
    #endregion
}
