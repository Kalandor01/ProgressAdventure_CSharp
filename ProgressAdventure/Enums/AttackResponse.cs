namespace ProgressAdventure.Enums
{
    /// <summary>
    /// Possible responses afted an entity atacks another entity.
    /// </summary>
    public enum AttackResponse
    {
        /// <summary>
        /// Returned if the enemy dogded the attack.
        /// </summary>
        TARGET_DOGDED,
        /// <summary>
        /// Returned if the enemy blocked the attack.
        /// </summary>
        TARGET_BLOCKED,
        /// <summary>
        /// Returned if the enemy was hit.
        /// </summary>
        TARGET_HIT,
        /// <summary>
        /// Returned if the hit killed the enemy.
        /// </summary>
        TARGET_KILLED,
        /// <summary>
        /// Returned if the attacked entity is already dead before the hit.
        /// </summary>
        TARGET_DEAD,
        /// <summary>
        /// Returned if the attacking entity is dead.
        /// </summary>
        ENTITY_DEAD,
    }
}
