namespace ProgressAdventure.Entity
{
    /// <summary>
    /// Struct used for storing the chances of an entity getting a specific attribute, when creating it's stats in the EntityManager function (1 = 100%).
    /// </summary>
    public struct AttributeChances
    {
        #region Fields
        public double rareChance;
        public double crippledChance;
        public double healthyChance;
        public double sickChance;
        public double strongChance;
        public double weakChance;
        public double toughChance;
        public double frailChance;
        public double agileChance;
        public double slowChance;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="AttributeChances"/>
        /// </summary>
        public AttributeChances()
            : this(0.02) { }

        /// <summary>
        /// <inheritdoc cref="AttributeChances"/>
        /// </summary>
        /// <param name="rareChance"></param>
        /// <param name="crippledChance"></param>
        /// <param name="healthyChance"></param>
        /// <param name="sickChance"></param>
        /// <param name="strongChance"></param>
        /// <param name="weakChance"></param>
        /// <param name="toughChance"></param>
        /// <param name="frailChance"></param>
        /// <param name="agileChance"></param>
        /// <param name="slowChance"></param>
        public AttributeChances(
            double rareChance = 0.02,
            double crippledChance = 0.02,
            double healthyChance = 0.1,
            double sickChance = 0.1,
            double strongChance = 0.1,
            double weakChance = 0.1,
            double toughChance = 0.1,
            double frailChance = 0.1,
            double agileChance = 0.1,
            double slowChance = 0.1
        )
        {
            this.rareChance = Math.Clamp(rareChance, 0, 1);
            this.crippledChance = Math.Clamp(crippledChance, 0, 1);
            this.healthyChance = Math.Clamp(healthyChance, 0, 1);
            this.sickChance = Math.Clamp(sickChance, 0, 1);
            this.strongChance = Math.Clamp(strongChance, 0, 1);
            this.weakChance = Math.Clamp(weakChance, 0, 1);
            this.toughChance = Math.Clamp(toughChance, 0, 1);
            this.frailChance = Math.Clamp(frailChance, 0, 1);
            this.agileChance = Math.Clamp(agileChance, 0, 1);
            this.slowChance = Math.Clamp(slowChance, 0, 1);
        }
        #endregion
    }
}
