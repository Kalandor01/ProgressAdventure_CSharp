using NPrng.Generators;
using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using PACommon.Logging;
using System.Collections;
using PACTools = PACommon.Tools;

namespace ProgressAdventure.WorldManagement
{
    /// <summary>
    /// An object representing a chunk, containing a list of tiles.
    /// </summary>
    public class Chunk : IJsonConvertable<Chunk>
    {
        #region Public fields
        /// <summary>
        /// The absolute position of the base of the chunk.
        /// </summary>
        public readonly (long x, long y) basePosition;
        /// <summary>
        /// The list of tiles in the chunk.
        /// </summary>
        public readonly Dictionary<string, Tile> tiles;
        #endregion

        #region Public properties
        /// <summary>
        /// This chunk's random generator.
        /// </summary>
        public SplittableRandom ChunkRandomGenerator { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Chunk"/>
        /// </summary>
        /// <param name="basePosition">The absolute position of the chunk.</param>
        /// <param name="tiles"><inheritdoc cref="tiles" path="//summary"/></param>
        /// <param name="chunkRandom">The chunk's random generator.</param>
        public Chunk((long x, long y) basePosition, Dictionary<string, Tile>? tiles = null, SplittableRandom? chunkRandom = null)
        {
            var baseX = Utils.FloorRound(basePosition.x, Constants.CHUNK_SIZE);
            var baseY = Utils.FloorRound(basePosition.y, Constants.CHUNK_SIZE);
            this.basePosition = (baseX, baseY);
            Logger.Instance.Log("Creating chunk", $"baseX: {this.basePosition.x} , baseY: {this.basePosition.y}");
            ChunkRandomGenerator = chunkRandom ?? GetChunkRandom(basePosition);
            this.tiles = tiles ?? new Dictionary<string, Tile>();
            FillChunk(tiles is not null);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns the <c>Tile</c> if it exists, or null.
        /// </summary>
        /// <param name="position">The position of the tile.</param>
        public Tile? FindTile((long x, long y) position)
        {
            return FindTile(GetTileDictName(position));
        }

        /// <summary>
        /// Generates a new <c>Tile</c> at a specific position.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the tile.</param>
        public Tile GenerateTile((long x, long y) absolutePosition)
        {
            var tileKey = GetTileDictName(absolutePosition);
            var tile = new Tile(absolutePosition.x, absolutePosition.y, ChunkRandomGenerator);
            tiles[tileKey] = tile;
            var posX = Utils.Mod(absolutePosition.x, Constants.CHUNK_SIZE);
            var posY = Utils.Mod(absolutePosition.y, Constants.CHUNK_SIZE);
            Logger.Instance.Log("Created tile", $"x: {posX}, y: {posY}, terrain: {WorldUtils.terrainContentTypeIDTextMap[tile.terrain.subtype]}, structure: {WorldUtils.structureContentSubtypeIDTextMap[tile.structure.subtype]}, population: {WorldUtils.populationContentSubtypeIDTextMap[tile.population.subtype]}", LogSeverity.DEBUG);
            return tile;
        }

        /// <summary>
        /// Tries to find a tile at a specific location, and creates one, if doesn't exist.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the tile.</param>
        /// <param name="tile">The tile that was found or created.</param>
        public bool TryGetTile((long x, long y) absolutePosition, out Tile tile)
        {
            var res = FindTile(absolutePosition);
            tile = res ?? GenerateTile(absolutePosition);
            return res is not null;
        }

        /// <summary>
        /// Saves the chunk's data into a file in the save folder.
        /// </summary>
        /// <param name="saveFolderName">If null, it will use the save name in <c>SaveData</c>.</param>
        public void SaveToFile(string? saveFolderName = null)
        {
            saveFolderName ??= SaveData.Instance.saveName;
            Tools.RecreateChunksFolder(saveFolderName);
            var chunkJson = ToJson();
            var chunkFileName = GetChunkFileName(basePosition);
            Tools.EncodeSaveShort(chunkJson, GetChunkFilePath(chunkFileName, saveFolderName));
            Logger.Instance.Log("Saved chunk", $"{chunkFileName}.{Constants.SAVE_EXT}");
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
        /// <param name="position">The position of the chunk.</param>
        /// <param name="saveFolderName">The name of the save folder.<br/>
        /// If null, it will make one using the save name in <c>SaveData</c>.</param>
        /// <param name="expected">If the chunk is expected to exist.<br/>
        /// ONLY ALTERS THE LOGS DISPLAYED, IF THE CHUNK DOESN'T EXIST.</param>
        public static bool FromFile((long x, long y) position, out Chunk? chunk, string? saveFolderName = null, bool expected = true)
        {
            saveFolderName ??= SaveData.Instance.saveName;
            var chunkFileName = GetChunkFileName(position);
            Dictionary<string, object?>? chunkJson;
            chunk = null;

            try
            {
                chunkJson = Tools.DecodeSaveShort(GetChunkFilePath(chunkFileName, saveFolderName), expected: expected);
            }
            catch (Exception e)
            {
                if (e is FormatException)
                {
                    Logger.Instance.Log("Chunk parse error", "chunk couldn't be parsed", LogSeverity.ERROR);
                    return false;
                }
                else if (e is FileNotFoundException)
                {
                    Logger.Instance.Log("Chunk file not found", $"{(expected ? "" : "(but it was expected) ")}x: {position.x}, y: {position.y}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
                    return false;
                }
                else if (e is DirectoryNotFoundException)
                {
                    Logger.Instance.Log("Chunk folder not found", $"{(expected ? "" : "(but it was expected) ")}x: {position.x}, y: {position.y}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
                    return false;
                }
                throw;
            }

            if (chunkJson is null)
            {
                Logger.Instance.Log("Chunk parse error", "chunk json is null", LogSeverity.ERROR);
                return false;
            }

            //file version
            string? fileVersion = null;
            if (chunkJson.TryGetValue("file_version", out object? versionValue) && versionValue is not null)
            {
                fileVersion = versionValue.ToString();
            }
            else if (chunkJson.TryGetValue("saveVersion", out object? versionValueBackup) && versionValueBackup is not null)
            {
                Logger.Instance.Log("Old style chunk version (< 2.2)", $"chunk name: {chunkFileName}", LogSeverity.INFO);

                fileVersion = versionValueBackup.ToString();
            }

            if (fileVersion is null)
            {
                Logger.Instance.Log("Chunk parse error", $"couldn't parse file version, assuming minimum, chunk file name: {chunkFileName}", LogSeverity.WARN);
                fileVersion = Constants.OLDEST_SAVE_VERSION;
            }

            chunkJson.Add("position_x", position.x);
            chunkJson.Add("position_y", position.y);

            var success = PACTools.TryFromJson(chunkJson, fileVersion, out chunk);
            Logger.Instance.Log("Loaded chunk from file", $"{chunkFileName}.{Constants.SAVE_EXT}");
            return success;
        }

        /// <summary>
        /// Generates the chunk random genrator for a chunk.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the chunk.</param>
        public static SplittableRandom GetChunkRandom((long x, long y) absolutePosition)
        {
            var posX = Utils.FloorRound(absolutePosition.x, Constants.CHUNK_SIZE);
            var posY = Utils.FloorRound(absolutePosition.y, Constants.CHUNK_SIZE);
            var noiseValues = WorldUtils.GetNoiseValues(posX, posY);
            var noiseNum = noiseValues.Count;
            var seedNumSize = 19.0;
            var noiseTenMulti = (int)Math.Floor(seedNumSize / noiseNum);
            var noiseMulti = Math.Pow(10, noiseTenMulti);
            ulong seed = 1;
            for (int x = 0; x < noiseValues.Count; x++)
            {
                var noiseVal = noiseValues.ElementAt(x).Value;
                seed *= (ulong)(noiseVal * noiseMulti);
            }
            seed = (ulong)(seed * RandomStates.Instance.ChunkSeedModifier);
            return new SplittableRandom(seed);
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
        public static string GetChunkFilePath(string chunkFileName, string saveFolderName)
        {
            var saveFolderPath = Path.Join(Constants.SAVES_FOLDER_PATH, saveFolderName);
            return Path.Join(saveFolderPath, Constants.SAVE_FOLDER_NAME_CHUNKS, chunkFileName);
        }

        /// <summary>
        /// Gets the name of the chunk file.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the chunk.</param>
        public static string GetChunkFileName((long x, long y) absolutePosition)
        {
            var baseX = Utils.FloorRound(absolutePosition.x, Constants.CHUNK_SIZE);
            var baseY = Utils.FloorRound(absolutePosition.y, Constants.CHUNK_SIZE);
            return $"{Constants.CHUNK_FILE_NAME}{Constants.CHUNK_FILE_NAME_SEP}{baseX}{Constants.CHUNK_FILE_NAME_SEP}{baseY}";
        }

        /// <summary>
        /// Tries to load the list of tiles, from a chunk file's json, and returns if it succeded without warning.
        /// </summary>
        /// <param name="chunkRandom">The chunk's random generator.</param>
        /// <param name="absolutePosition">The absolute position of the chunk.</param>
        /// <param name="tileListJson">A list of tiles is a json representation.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="tiles">The decoded dictionary of tile objects.</param>
        private static bool TilesFromJson(SplittableRandom? chunkRandom, (long x, long y) absolutePosition, IEnumerable tileListJson, string fileVersion, out Dictionary<string, Tile> tiles)
        {
            chunkRandom ??= GetChunkRandom(absolutePosition);
            tiles = new Dictionary<string, Tile>();
            var success = true;

            foreach (var tileJson in tileListJson)
            {
                success &= Tile.FromJson(chunkRandom, tileJson as IDictionary<string, object?>, fileVersion, out Tile? tile);
                if (tile is not null)
                {
                    tiles.Add(GetTileDictName(tile.relativePosition), tile);
                }
            }
            var totalTileNum = Constants.CHUNK_SIZE * Constants.CHUNK_SIZE;
            Logger.Instance.Log("Loaded chunk tiles from json", $"loaded tiles: {tiles.Count}/{totalTileNum} {(tiles.Count < totalTileNum ? "Remaining tiles will be regenerated" : "")}", tiles.Count < totalTileNum ? LogSeverity.WARN : LogSeverity.INFO);

            return success;
        }
        #endregion

        #region JsonConvert
        static List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<Chunk>.VersionCorrecters { get; } = new()
        {
            // 2.1.1 -> 2.2
            (oldJson =>
            {
                // snake case rename
                if (oldJson.TryGetValue("chunkRandom", out var crRename))
                {
                    oldJson["chunk_random"] = crRename;
                }
            }, "2.2"),
        };

        public Dictionary<string, object?> ToJson()
        {
            var tilesJson = new List<Dictionary<string, object?>>();
            foreach (var tile in tiles)
            {
                tilesJson.Add(tile.Value.ToJson());
            }
            return new Dictionary<string, object?>
            {
                ["file_version"] = Constants.SAVE_VERSION,
                ["chunk_random"] = PACTools.SerializeRandom(ChunkRandomGenerator),
                ["tiles"] = tilesJson,
            };
        }

        static bool IJsonConvertable<Chunk>.FromJsonWithoutCorrection(IDictionary<string, object?> chunkJson, string fileVersion, ref Chunk? chunkObject)
        {
            var success = true;

            // position
            if (
                !chunkJson.TryGetValue("position_x", out object? posXString) ||
                !chunkJson.TryGetValue("position_y", out object? posYString)
            )
            {
                Logger.Instance.Log("Chunk parse error", "chunk position is null", LogSeverity.ERROR);
                return false;
            }

            if (
                !long.TryParse(posXString?.ToString(), out long posX) ||
                !long.TryParse(posYString?.ToString(), out long posY)
            )
            {
                Logger.Instance.Log("Chunk parse error", "chunk position couldn't be parsed", LogSeverity.ERROR);
                return false;
            }

            (long x, long y) position = (posX, posY);

            // chunk seed
            SplittableRandom? chunkRandomGenerator = null;
            if (chunkJson.TryGetValue("chunk_random", out object? chunkRandom))
            {
                if (PACTools.TryDeserializeRandom(chunkRandom?.ToString(), out SplittableRandom? chunkRandomValue))
                {
                    chunkRandomGenerator = chunkRandomValue;
                }
                else
                {
                    Logger.Instance.Log("Chunk parse error", "chunk seed couldn't be parsed", LogSeverity.WARN);
                    success = false;
                }
            }
            else
            {
                Logger.Instance.Log("Chunk parse error", "chunk seed is null", LogSeverity.WARN);
                success = false;
            }
            chunkRandomGenerator ??= GetChunkRandom(position);

            // tiles
            if (
                !chunkJson.TryGetValue("tiles", out object? tilesListValue) ||
                tilesListValue is not IEnumerable tilesList
            )
            {
                Logger.Instance.Log("Chunk parse error", "tiles couldn't be parsed", LogSeverity.ERROR);
                return false;
            }

            success &= TilesFromJson(chunkRandomGenerator, position, tilesList, fileVersion, out Dictionary<string, Tile> tiles);
            chunkObject = new Chunk(position, tiles, chunkRandomGenerator);
            return success;
        }
        #endregion
    }
}
