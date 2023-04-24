namespace ProgressAdventure.ItemManagement
{
    public static class ItemType
    {
        #region Public properties
        public static ItemTypeID AllItemType
        {
            get => all;
        }
        public static ItemTypeID WeaponItemType
        {
            get => weapon;
        }
        public static ItemTypeID DefenceItemType
        {
            get => defence;
        }
        public static ItemTypeID MaterialItemType
        {
            get => material;
        }
        public static ItemTypeID MiscItemType
        {
            get => misc;
        }
        #endregion

        private static readonly ItemTypeID all = 1;


        private static readonly ItemTypeID weapon = all[0];
        public static class Weapon
        {
            public static readonly ItemTypeID WOODEN_SWORD = weapon[0];
            public static readonly ItemTypeID STONE_SWORD = weapon[1];
            public static readonly ItemTypeID STEEL_SWORD = weapon[2];
            public static readonly ItemTypeID WOODEN_BOW = weapon[3];
            public static readonly ItemTypeID STEEL_ARROW = weapon[4];
            public static readonly ItemTypeID WOODEN_CLUB = weapon[5];
            public static readonly ItemTypeID CLUB_WITH_TEETH = weapon[6];
        }

        private static readonly ItemTypeID defence = all[1];
        public static class Defence
        {
            public static readonly ItemTypeID WOODEN_SHIELD = defence[0];
            public static readonly ItemTypeID LEATHER_CAP = defence[1];
            public static readonly ItemTypeID LEATHER_TUNIC = defence[2];
            public static readonly ItemTypeID LEATHER_PANTS = defence[3];
            public static readonly ItemTypeID LEATHER_BOOTS = defence[4];
        }

        private static readonly ItemTypeID material = all[2];
        public static class Material
        {
            public static readonly ItemTypeID BOOTLE = material[0];
            public static readonly ItemTypeID WOOL = material[1];
            public static readonly ItemTypeID CLOTH = material[2];
            public static readonly ItemTypeID WOOD = material[3];
            public static readonly ItemTypeID STONE = material[4];
            public static readonly ItemTypeID STEEL = material[5];
            public static readonly ItemTypeID GOLD = material[6];
            public static readonly ItemTypeID TEETH = material[7];
        }

        private static readonly ItemTypeID misc = all[3];
        public static class Misc
        {
            public static readonly ItemTypeID HEALTH_POTION = misc[0];
            public static readonly ItemTypeID GOLD_COIN = misc[1];
            public static readonly ItemTypeID SILVER_COIN = misc[2];
            public static readonly ItemTypeID COPPER_COIN = misc[3];
            public static readonly ItemTypeID ROTTEN_FLESH = misc[4];
        }
    }
}
