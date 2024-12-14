using NPrng.Generators;
using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using System.Diagnostics.CodeAnalysis;
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
            PACSingletons.Instance.Logger.Log("Creating chunk", $"baseX: {this.basePosition.x} , baseY: {this.basePosition.y}");
            ChunkRandomGenerator = chunkRandom ?? GetChunkRandom(basePosition);
            this.tiles = tiles ?? [];
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
            PACSingletons.Instance.Logger.Log("Created tile", $"x: {posX}, y: {posY}, terrain: {WorldUtils.TerrainContentTypeMap[tile.terrain.subtype].typeName}, structure: {WorldUtils.StructureContentTypeMap[tile.structure.subtype].typeName}, population: {WorldUtils.PopulationContentTypeMap[tile.population.subtype].typeName}", LogSeverity.DEBUG);
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
            chunkJson.Remove(Constants.JsonKeys.Chunk.POSITION_X);
            chunkJson.Remove(Constants.JsonKeys.Chunk.POSITION_Y);
            var chunkFileName = GetChunkFileName(basePosition);
            Tools.EncodeSaveShort(chunkJson, GetChunkFilePath(chunkFileName, saveFolderName));
            PACSingletons.Instance.Logger.Log("Saved chunk", $"{chunkFileName}.{Constants.SAVE_EXT}");
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
        /// Tries to load a Chunk from a chunk file, and return it, if it was successfuly parsed.
        /// </summary>
        /// <param name="position">The position of the chunk.</param>
        /// <param name="saveFolderName">The name of the save folder.<br/>
        /// If null, it will make one using the save name in <c>SaveData</c>.</param>
        /// <param name="expected">If the chunk is expected to exist.<br/>
        /// ONLY ALTERS THE LOGS DISPLAYED, IF THE CHUNK DOESN'T EXIST.</param>
        /// <returns>If the parsing was succesfull without any warnings.</returns>
        public static bool FromFile((long x, long y) position, out Chunk? chunk, string? saveFolderName = null, bool expected = true)
        {
            saveFolderName ??= SaveData.Instance.saveName;
            var chunkFileName = GetChunkFileName(position);
            chunk = null;

            var chunkJson = Tools.DecodeSaveShortExpected<Chunk>(
                GetChunkFilePath(chunkFileName, saveFolderName),
                expected: expected,
                extraFileInformation: $"x: {position.x}, y: {position.y}"
            );

            if (chunkJson is null)
            {
                return false;
            }

            var fileVersion = SaveManager.GetSaveVersion<Chunk>(chunkJson, Constants.JsonKeys.Chunk.FILE_VERSION, chunkFileName);
            if (fileVersion is null)
            {
                PACTools.LogJsonParseError<Chunk>(Constants.JsonKeys.Chunk.FILE_VERSION, $"assuming minimum, chunk file name: {chunkFileName}");
                fileVersion = Constants.OLDEST_SAVE_VERSION;
            }

            chunkJson.Add(Constants.JsonKeys.Chunk.POSITION_X, position.x);
            chunkJson.Add(Constants.JsonKeys.Chunk.POSITION_Y, position.y);

            var success = PACTools.TryFromJson(chunkJson, fileVersion, out chunk);
            PACSingletons.Instance.Logger.Log("Loaded chunk from file", $"{chunkFileName}.{Constants.SAVE_EXT}");
            return success;
        }

        /// <summary>
        /// Generates the chunk random genrator for a chunk.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the chunk.</param>
        public static SplittableRandom GetChunkRandom((long x, long y) absolutePosition)
        {
            return GetChunkRandom(absolutePosition, Constants.CHUNK_SIZE);
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
        /// <param name="chunkSize">The chunk size to round the position to.</param>
        public static string GetChunkFileName((long x, long y) absolutePosition, int chunkSize)
        {
            var baseX = Utils.FloorRound(absolutePosition.x, chunkSize);
            var baseY = Utils.FloorRound(absolutePosition.y, chunkSize);
            return $"{Constants.CHUNK_FILE_NAME}{Constants.CHUNK_FILE_NAME_SEP}{baseX}{Constants.CHUNK_FILE_NAME_SEP}{baseY}";
        }

        /// <inheritdoc cref="GetChunkFileName(ValueTuple{long, long}, int)"/>
        public static string GetChunkFileName((long x, long y) absolutePosition)
        {
            return GetChunkFileName(absolutePosition, Constants.CHUNK_SIZE);
        }

        /// <summary>
        /// Generates the chunk random genrator for a chunk.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the chunk.</param>
        /// <param name="chunkSize">The size of a chunk.</param>
        internal static SplittableRandom GetChunkRandom((long x, long y) absolutePosition, int chunkSize)
        {
            var posX = Utils.FloorRound(absolutePosition.x, chunkSize);
            var posY = Utils.FloorRound(absolutePosition.y, chunkSize);
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

        #region JsonConvert
        static List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<Chunk>.VersionCorrecters { get; } =
        [
            // 2.1.1 -> 2.2
            (oldJson =>
            {
                // snake case rename
                if (oldJson.TryGetValue("chunkRandom", out var crRename))
                {
                    oldJson["chunk_random"] = crRename;
                }
            }, "2.2"),
        ];

        public JsonDictionary ToJson()
        {
            var tilesJson = new List<JsonObject?>();
            foreach (var tile in tiles)
            {
                tilesJson.Add(tile.Value.ToJson());
            }
            return new JsonDictionary
            {
                [Constants.JsonKeys.Chunk.POSITION_X] = basePosition.x,
                [Constants.JsonKeys.Chunk.POSITION_Y] = basePosition.y,
                [Constants.JsonKeys.Chunk.FILE_VERSION] = Constants.SAVE_VERSION,
                [Constants.JsonKeys.Chunk.CHUNK_RANDOM] = PACTools.SerializeRandom(ChunkRandomGenerator),
                [Constants.JsonKeys.Chunk.TILES] = tilesJson,
            };
        }

        static bool IJsonConvertable<Chunk>.FromJsonWithoutCorrection(JsonDictionary chunkJson, string fileVersion, [NotNullWhen(true)] ref Chunk? chunkObject)
        {
            var success = true;

            // position
            if (!(
                PACTools.TryParseJsonValue<Chunk, long>(chunkJson, Constants.JsonKeys.Chunk.POSITION_X, out var posX, isCritical: true) &&
                PACTools.TryParseJsonValue<Chunk, long>(chunkJson, Constants.JsonKeys.Chunk.POSITION_Y, out var posY, isCritical: true)
            ))
            {
                return false;
            }
            (long x, long y) position = (posX, posY);

            success &= PACTools.TryParseJsonValue<Chunk, SplittableRandom>(chunkJson, Constants.JsonKeys.Chunk.CHUNK_RANDOM, out var chunkRandom);
            chunkRandom ??= GetChunkRandom(position);

            if (!PACTools.TryParseJsonListValue<Chunk, KeyValuePair<string, Tile>>(chunkJson, Constants.JsonKeys.Chunk.TILES, tileJson => {
                if (!PACTools.TryCastAnyValueForJsonParsing<Tile, JsonDictionary>(tileJson, out var tileJsonValue, nameof(tileJson), isStraigthCast: true))
                {
                    success = false;
                    return (false, default);
                }
                success &= Tile.FromJson(chunkRandom, tileJsonValue, fileVersion, out Tile? tile);
                return (tile is not null, tile is null ? default : new KeyValuePair<string, Tile>(GetTileDictName(tile.relativePosition), tile));
            }, out var tilesKvPair, true))
            {
                return false;
            }
            var tiles = tilesKvPair.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var totalTileNum = Constants.CHUNK_SIZE * Constants.CHUNK_SIZE;
            var allTilesExist = tiles.Count == totalTileNum;
            PACSingletons.Instance.Logger.Log(
                "Loaded chunk tiles from json",
                $"loaded tiles: {tiles.Count}/{totalTileNum} {(allTilesExist ? "" : "Remaining tiles will be regenerated")}",
                allTilesExist ? LogSeverity.INFO : LogSeverity.WARN
            );

            chunkObject = new Chunk(position, tiles, chunkRandom);
            return success;
        }
        #endregion
    }
}
