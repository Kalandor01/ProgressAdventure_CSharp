namespace ProgressAdventure.ItemManagement
{
    public struct ItemAttributes
    {
        #region Fields
        public readonly string typeName;
        public readonly string? displayName;
        public readonly bool? consumable;
        #endregion

        #region Constructor
        public ItemAttributes()
        {
            throw new ArgumentNullException(nameof(typeName));
        }

        public ItemAttributes(string typeName, string? displayName, bool? consumable)
        {
            this.typeName = typeName;
            this.displayName = displayName;
            this.consumable = consumable;
        }
        #endregion
    }
}
