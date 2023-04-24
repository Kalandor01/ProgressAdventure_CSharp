using ProgressAdventure.ItemManagement;

namespace ProgressAdventure.Entity
{
    #region Monster classes
    /// <summary>
    /// Caveman entity.
    /// </summary>
    public class Caveman : Entity
    {
        /// <summary>
        /// <inheritdoc cref="Caveman"/>
        /// </summary>
        public Caveman()
            : base(
                EntityUtils.EntityManager(7, 7, 7, 7),
                LootFactory.LootManager(new List<LootFactory> {
                    new LootFactory(ItemType.Weapon.WOODEN_CLUB, 0.3),
                    new LootFactory(ItemType.Material.CLOTH, 0.15, 0, 1, 3),
                    new LootFactory(ItemType.Misc.COPPER_COIN, 0.35, 0, 4, 3)
                })
            )
        { }
    }

    /// <summary>
    /// Ghoul entity.
    /// </summary>
    public class Ghoul : Entity
    {
        /// <summary>
        /// <inheritdoc cref="Ghoul"/>
        /// </summary>
        public Ghoul()
            : base(
                EntityUtils.EntityManager(11, 9, 9, 9),
                LootFactory.LootManager(new List<LootFactory> {
                    new LootFactory(ItemType.Weapon.STONE_SWORD, 0.2),
                    new LootFactory(ItemType.Misc.ROTTEN_FLESH, 0.55, 0, 3),
                    new LootFactory(ItemType.Misc.COPPER_COIN, 0.4, 0, 5, 4)
                })
            )
        { }
    }

    /// <summary>
    /// Troll entity.
    /// </summary>
    public class Troll : Entity
    {
        /// <summary>
        /// <inheritdoc cref="Troll"/>
        /// </summary>
        public Troll()
            : base(
                EntityUtils.EntityManager(13, 11, 11, 5),
                LootFactory.LootManager(new List<LootFactory> {
                    new LootFactory(ItemType.Weapon.CLUB_WITH_TEETH, 0.25),
                    new LootFactory(ItemType.Material.CLOTH, 0.25, 1, 3, 2),
                    new LootFactory(ItemType.Material.TEETH, 0.35, 1, 5, 2),
                    new LootFactory(ItemType.Misc.SILVER_COIN, 0.3, 1, 3, 3)
                })
            )
        { }
    }
    #endregion
}
