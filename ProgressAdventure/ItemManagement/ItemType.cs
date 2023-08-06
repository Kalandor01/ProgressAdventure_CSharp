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
        public static ItemTypeID MiscItemType
        {
            get => misc;
        }
        #endregion

        private static readonly ItemTypeID all = 1;


        private static readonly ItemTypeID weapon = all[0];
        public static class Weapon
        {
            public static readonly ItemTypeID SWORD = weapon[0];
            public static readonly ItemTypeID BOW = weapon[1];
            public static readonly ItemTypeID ARROW = weapon[2];
            public static readonly ItemTypeID CLUB = weapon[3];
            public static readonly ItemTypeID CLUB_WITH_TEETH = weapon[4];
        }

        private static readonly ItemTypeID defence = all[1];
        public static class Defence
        {
            public static readonly ItemTypeID SHIELD = defence[0];
            public static readonly ItemTypeID HELMET = defence[1];
            public static readonly ItemTypeID CHESTPLATE = defence[2];
            public static readonly ItemTypeID PANTS = defence[3];
            public static readonly ItemTypeID BOOTS = defence[4];
        }

        private static readonly ItemTypeID misc = all[2];
        public static class Misc
        {
            public static readonly ItemTypeID BOTTLE = misc[0];
            public static readonly ItemTypeID HEALTH_POTION = misc[1];
            public static readonly ItemTypeID COIN = misc[2];
            public static readonly ItemTypeID MATERIAL = misc[3];
        }
    }
}
