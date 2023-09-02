using PAConstants = ProgressAdventure.Constants;

namespace PAExtras
{
    /// <summary>
    /// Object for storing constants.
    /// </summary>
    public static class Constants
    {

        #region Save importer
        /// <summary>
        /// The name of the exported saves folder.
        /// </summary>
        public const string EXPORTED_FOLDER = "exported";
        /// <summary>
        /// The path to the exported saves folder.
        /// </summary>
        public static readonly string EXPORTED_FOLDER_PATH = Path.Join(PAConstants.ROOT_FOLDER, EXPORTED_FOLDER);
        /// <summary>
        /// The save version to expect for an exported save.
        /// </summary>
        public const string NEWEST_PYTHON_SAVE_VERSION = "1.5.3";
        /// <summary>
        /// The save version to import into.
        /// </summary>
        public const string IMPORT_SAVE_VERSION = "2.0";
        /// <summary>
        /// The save version to expect for an exported save.
        /// </summary>
        public const string EXPORTED_SAVE_EXT = "json";
        /// <summary>
        /// The name of the data file in a save file.
        /// </summary>
        public const string SAVE_FILE_NAME_DATA = "data";
        /// <summary>
        /// The name of the chunks folder in a save file.
        /// </summary>
        public const string SAVE_FOLDER_NAME_CHUNKS = "chunks";
        /// <summary>
        /// The beggining of the name of a chunk file.
        /// </summary>
        public const string CHUNK_FILE_NAME = "chunk";
        /// <summary>
        /// The separation string used in the name of the chunk file name.
        /// </summary>
        public const string CHUNK_FILE_NAME_SEP = "_";
        /// <summary>
        /// The size of the grid, containing the tile in a chunk.
        /// </summary>
        public const int CHUNK_SIZE = 10;
        /// <summary>
        /// The division to use in tile noises.
        /// </summary>
        public const long TILE_NOISE_DIVISION = 200;
        #endregion
    }
}
