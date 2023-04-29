namespace ProgressAdventure.WorldManagement.Content
{
    public class Tile
    {
        #region Public fields
        /// <summary>
        /// The x position of the tile.
        /// </summary>
        public readonly long x;
        /// <summary>
        /// The y position of the tile.
        /// </summary>
        public readonly long y;
        /// <summary>
        /// How many times the tile has been visited.
        /// </summary>
        public int visited;
        #endregion
    }
}