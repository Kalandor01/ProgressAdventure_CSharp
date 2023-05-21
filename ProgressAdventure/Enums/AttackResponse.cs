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
        ENEMY_DOGDED,
        /// <summary>
        /// Returned if the enemy blocked the attack.
        /// </summary>
        ENEMY_BLOCKED,
        /// <summary>
        /// Returned if the enemy was hit.
        /// </summary>
        HIT,
        /// <summary>
        /// Returned if the hit killed the enemy.
        /// </summary>
        KILLED,
        /// <summary>
        /// Returned if the enemy was already dead before the hit.
        /// </summary>
        ENEMY_DEAD,
        /// <summary>
        /// Returned if the attacking entity is dead.
        /// </summary>
        ATTACKER_DEAD,
    }
}
