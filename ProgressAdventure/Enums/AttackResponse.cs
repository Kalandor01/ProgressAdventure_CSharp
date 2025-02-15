namespace ProgressAdventure.Enums
{
    /// <summary>
    /// Possible responses after an entity attacks another entity.
    /// </summary>
    public enum AttackResponse
    {
        /// <summary>
        /// Returned if the target dodged the attack.
        /// </summary>
        TARGET_DOGDED,
        /// <summary>
        /// Returned if the target blocked the attack.
        /// </summary>
        TARGET_BLOCKED,
        /// <summary>
        /// Returned if the target was hit.
        /// </summary>
        TARGET_HIT,
        /// <summary>
        /// Returned if the hit killed the target.
        /// </summary>
        TARGET_KILLED,
        /// <summary>
        /// Returned if the target was already dead before the hit.
        /// </summary>
        TARGET_DEAD,
        /// <summary>
        /// Returned if the attacking entity is dead.
        /// </summary>
        ENTITY_DEAD,
    }
}
