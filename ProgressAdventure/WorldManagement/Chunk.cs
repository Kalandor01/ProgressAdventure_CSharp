using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;

namespace ProgressAdventure.WorldManagement
{
    /// <summary>
    /// An object representing a chunk, containing a list of tiles.
    /// </summary>
    public class Chunk
    {
        /// <summary>
        /// The absolute position of the base of the chunk.
        /// </summary>
        public readonly (long x, long y) basePosition;
        /// <summary>
        /// The list of tiles in the chunk.
        /// </summary>
        public readonly Dictionary<string, Tile> tiles;

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Chunk"/>
        /// </summary>
        /// <param name="basePosition">The absolute position of the chunk.</param>
        /// <param name="tiles"><inheritdoc cref="tiles" path="//summary"/></param>
        public Chunk((long x, long y) basePosition, Dictionary<string, Tile>? tiles = null)
        {
            var baseX = Utils.FloorRound(basePosition.x, Constants.CHUNK_SIZE);
            var baseY = Utils.FloorRound(basePosition.y, Constants.CHUNK_SIZE);
            this.basePosition = (baseX, baseY);
            Logger.Log("Creating chunk", $"baseX: {this.basePosition.x} , baseY: {this.basePosition.y}");
            this.tiles = tiles ?? new Dictionary<string, Tile>();
            FillChunk(tiles is not null);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns a json representation of the <c>Chunk</c>.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToJson()
        {
            var tilesJson = new List<Dictionary<string, object?>>();
            foreach (var tile in tiles)
            {
                tilesJson.Add(tile.Value.ToJson());
            }
            return new Dictionary<string, object> {
                ["tiles"] = tilesJson,
            };
        }

        /// <summary>
        /// Returns the <c>Tile</c> if it exists, or null.
        /// </summary>
        /// <param name="position">The position of the tile.</param>
        public Tile? FindTile((long x, long y) position)
        {
            return FindTile(GetTileDictName(position));
        }

        /// <summary>
        /// Generates a new tile at a specific position.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the tile.</param>
        public Tile GenerateTile((long x, long y) absolutePosition)
        {
            var tileKey = GetTileDictName(absolutePosition);
            var tile = new Tile(absolutePosition.x, absolutePosition.y);
            tiles[tileKey] = tile;
            var posX = Utils.Mod(absolutePosition.x, Constants.CHUNK_SIZE);
            var posY = Utils.Mod(absolutePosition.y, Constants.CHUNK_SIZE);
            Logger.Log("Created tile", $"x: {posX} , y: {posY}", Enums.LogSeverity.DEBUG);
            return tile;
        }

        /// <summary>
        /// Tries to find a tile at a specific location, and creates one, if doesn't exist.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the tile.</param>
        /// <param name="tile">The tile that was found or created</param>
        public bool TryGetTile((long x, long y) absolutePosition, out Tile tile)
        {
            var res = FindTile(absolutePosition);
            tile = res ?? GenerateTile(absolutePosition);
            return res is not null;
        }

        /// <summary>
        /// Saves the chunk's data into a file in the save folder.
        /// </summary>
        /// <param name="saveFolderName">If null, it will make one using the save name in <c>SaveData</c>.</param>
        public void SaveToFile(string? saveFolderName = null)
        {
            saveFolderName ??= SaveData.saveName;
            Tools.RecreateChunksFolder(saveFolderName);
            var chunkJson = ToJson();
            var chunkFileName = GetChunkFileName(basePosition);
            Tools.EncodeSaveShort(chunkJson, GetChunkFilePath(chunkFileName, saveFolderName));
            Logger.Log("Saved chunk", $"{chunkFileName}.{Constants.SAVE_EXT}");
        }

        /// <summary>
        /// Generates ALL not yet generated tiles.
        /// </summary>
        public void FillChunk()
        {
            FillChunk(true);
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Loads a Chunk object from a chunk file, or null if the file is not found.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the chunk.</param>
        /// <param name="saveFolderName">The name of the save folder.<br/>
        /// If null, it will make one using the save name in <c>SaveData</c>.</param>
        public static Chunk? FromFile((long x, long y) absolutePosition, string? saveFolderName = null)
        {
            saveFolderName ??= SaveData.saveName;
            var chunkFileName = GetChunkFileName(absolutePosition);
            Dictionary<string, object?>? chunkJson;
            try
            {
                chunkJson = Tools.DecodeSaveShort(GetChunkFilePath(chunkFileName, saveFolderName));
            }
            catch (Exception e)
            {
                if (e is FormatException)
                {
                    Logger.Log("Chunk parse error", "Chunk couldn't be parsed", LogSeverity.ERROR);
                    return null;
                }
                else if (e is FileNotFoundException)
                {
                    Logger.Log("Chunk file not found", null, LogSeverity.ERROR);
                    return null;
                }
                else if (e is DirectoryNotFoundException)
                {
                    Logger.Log("Chunk folder not found", null, LogSeverity.ERROR);
                    return null;
                }
                throw;
            }
            if (!(
                chunkJson is not null &&
                chunkJson.TryGetValue("tiles", out object? tilesList) &&
                tilesList is not null
            ))
            {
                return null;
            }
            var chunk = FromJson(absolutePosition, (IEnumerable<object?>)tilesList);
            Logger.Log("Loaded chunk from file", $"{chunkFileName}.{Constants.SAVE_EXT}");
            return chunk;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Returns the <c>Tile</c> if it exists, or null.
        /// </summary>
        /// <param name="tileKey">The name of the tile in the distionary.</param>
        private Tile? FindTile(string tileKey)
        {
            tiles.TryGetValue(tileKey, out Tile? tile);
            return tile;
        }

        /// <summary>
        /// Generates ALL not yet generated tiles.
        /// </summary>
        /// <param name="checkExisting">If it should check, if tile already exists before creating it.</param>
        private void FillChunk(bool checkExisting)
        {
            for (int x = 0; x < Constants.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                {
                    if (checkExisting)
                    {
                        TryGetTile((basePosition.x + x, basePosition.y + y), out _);
                    }
                    else
                    {
                        GenerateTile((basePosition.x + x, basePosition.y + y));
                    }
                }
            }
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Converts the position of the tile into it's dictionary key name.
        /// </summary>
        /// <param name="position">The position of the tile.</param>
        private static string GetTileDictName((long x, long y) position)
        {
            return $"{Utils.Mod(position.x, Constants.CHUNK_SIZE)}_{Utils.Mod(position.y, Constants.CHUNK_SIZE)}";
        }

        /// <summary>
        /// Gets the path of the chunk file.
        /// </summary>
        /// <param name="chunkFileName">The name of the chunk file.</param>
        /// <param name="saveFolderName">The name of the save folder.</param>
        private static string GetChunkFilePath(string chunkFileName, string saveFolderName)
        {
            var saveFolderPath = Path.Join(Constants.SAVES_FOLDER_PATH, saveFolderName);
            return Path.Join(saveFolderPath, Constants.SAVE_FOLDER_NAME_CHUNKS, chunkFileName);
        }

        /// <summary>
        /// Gets the name of the chunk file.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the chunk.</param>
        private static string GetChunkFileName((long x, long y) absolutePosition)
        {
            var baseX = Utils.FloorRound(absolutePosition.x, Constants.CHUNK_SIZE);
            var baseY = Utils.FloorRound(absolutePosition.y, Constants.CHUNK_SIZE);
            return $"{Constants.CHUNK_FILE_NAME}{Constants.CHUNK_FILE_NAME_SEP}{baseX}{Constants.CHUNK_FILE_NAME_SEP}{baseY}";
        }

        /// <summary>
        /// Loads a Chunk object from a chunk file's json.
        /// </summary>
        /// <param name="tileListJson">The json representation of the chunk.</param>
        private static Chunk FromJson((long x, long y) absolutePosition, IEnumerable<object?> tileListJson)
        {
            var tiles = new Dictionary<string, Tile>();
            foreach (var tileJson in tileListJson)
            {
                if (tileJson is not null)
                {
                    var tileDict = (IDictionary<string, object?>)tileJson;
                    Tile? tile = null;
                    try
                    {
                        tile = Tile.FromJson(tileDict);
                    }
                    catch (ArgumentException)
                    {
                        Logger.Log("Tile parse error", "Tile couldn't be parsed (corrupted coordinates?)", LogSeverity.ERROR);
                    }
                    if (tile is not null)
                    {
                        tiles.Add(GetTileDictName(tile.relativePosition), tile);
                    }
                }
            }
            var totalTileNum = Constants.CHUNK_SIZE * Constants.CHUNK_SIZE;
            Logger.Log("Loaded chunk from json", $"loaded tiles: {tiles.Count}/{totalTileNum} {(tiles.Count < totalTileNum ? "Remaining tiles will be regenerated" : "")}", tiles.Count < totalTileNum ? LogSeverity.WARN : LogSeverity.INFO);
            return new Chunk(absolutePosition, tiles);
        }
        #endregion
    }
}
