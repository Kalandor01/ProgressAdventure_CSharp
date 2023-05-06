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
        public int Visited { get; private set; }
        /// <summary>
        /// The terrain layer of the tile.
        /// </summary>
        public readonly TerrainContent terrain;
        /// <summary>
        /// The structure layer of the tile.
        /// </summary>
        public readonly StructureContent structure;
        /// <summary>
        /// The population on this tile.
        /// </summary>
        public readonly PopulationContent population;
        #endregion
    }
}