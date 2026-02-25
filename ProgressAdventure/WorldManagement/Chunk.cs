using System.Diagnostics.CodeAnalysis;
using NPrng.Generators;
using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
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
        public readonly Dictionary<(long x, long y), Tile> tiles;
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
        public Chunk((long x, long y) basePosition, Dictionary<(long x, long y), Tile>? tiles = null, SplittableRandom? chunkRandom = null)
        {
            this.basePosition = GetChunkPosition(basePosition);
            PACSingletons.Instance.Logger.Log("Creating chunk", $"baseX: {this.basePosition.x} , baseY: {this.basePosition.y}");
            ChunkRandomGenerator = chunkRandom ?? GetChunkRandom(basePosition);
            this.tiles = tiles ?? [];
            FillChunk(tiles is not null);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns the <see cref="Tile"/> if it exists, or null.
        /// </summary>
        /// <param name="position">The position of the tile.</param>
        public Tile? FindTile((long x, long y) position)
        {
            return tiles.GetValueOrDefault(GetTilePosition(position));
        }

        /// <summary>
        /// Generates a new <see cref="Tile"/> at a specific position.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the tile.</param>
        public Tile GenerateTile((long x, long y) absolutePosition)
        {
            var tilePosition = GetTilePosition(absolutePosition);
            var tile = new Tile(absolutePosition.x, absolutePosition.y, ChunkRandomGenerator);
            tiles[tilePosition] = tile;
            PACSingletons.Instance.Logger.Log(
                "Created tile",
                $"x: {tilePosition.x}, y: {tilePosition.y}, terrain: {PASingletons.Instance.Localizer.GetLocalizedString(WorldUtils.TerrainTypeMap[tile.terrain.type].displayName)}, structure: {PASingletons.Instance.Localizer.GetLocalizedString(WorldUtils.StructureTypeMap[tile.structure.type].displayName)}, population: {tile.populationManager}",
                LogSeverity.DEBUG
            );
            return tile;
        }

        /// <summary>
        /// Tries to find a tile at a specific location, and creates one, if doesn't exist.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the tile.</param>
        /// <param name="tile">The tile that was found or created.</param>
        /// <returns>If the <see cref="Tile"/> was found.</returns>
        public bool TryGetTile((long x, long y) absolutePosition, out Tile tile)
        {
            var res = FindTile(absolutePosition);
            tile = res ?? GenerateTile(absolutePosition);
            return res is not null;
        }

        /// <summary>
        /// Saves the chunk's data into a file in the save folder.
        /// </summary>
        /// <param name="saveFolderName">If null, it will use the save name in <see cref="SaveData"/>.</param>
        public void SaveToFile(string? saveFolderName = null)
        {
            saveFolderName ??= SaveData.Instance.SaveName;
            Tools.RecreateChunksFolder(saveFolderName);
            var chunkJson = ToJson();
            chunkJson.Remove(Constants.JsonKeys.Chunk.POSITION_X);
            chunkJson.Remove(Constants.JsonKeys.Chunk.POSITION_Y);
            var chunkFileName = GetChunkFileName(basePosition);
            Tools.SaveCompressedFile(chunkJson, GetChunkFilePath(chunkFileName, saveFolderName));
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
        /// Converts a position into a chunk base position.
        /// </summary>
        /// <param name="position">The position of the <see cref="Chunk"/>.</param>
        /// <param name="chunkSize">The size of a chunk in tiles.</param>
        public static (long x, long y) GetChunkPosition((long x, long y) position, int chunkSize = Constants.CHUNK_SIZE)
        {
            return (Utils.FloorRound(position.x, chunkSize), Utils.FloorRound(position.y, chunkSize));
        }
        
        /// <summary>
        /// Converts the position of a tile into its relative position.
        /// </summary>
        /// <param name="position">The position of the tile.</param>
        /// <param name="chunkSize">The size of a chunk in tiles.</param>
        public static (long x, long y) GetTilePosition((long x, long y) position, int chunkSize = Constants.CHUNK_SIZE)
        {
            return (Utils.Mod(position.x, chunkSize), Utils.Mod(position.y, chunkSize));
        }
        
        /// <summary>
        /// Tries to load a Chunk from a chunk file, and return it, if it was successfuly parsed.
        /// </summary>
        /// <param name="position">The position of the chunk.</param>
        /// <param name="isFileInvalid">If the file wasn't able to be decoded because of it's format/content.</param>
        /// <param name="chunk">The parsed <see cref="Chunk"/>.</param>
        /// <param name="saveFolderName">The name of the save folder.<br/>
        /// If null, it will make one using the save name in <see cref="SaveData"/>.</param>
        /// <param name="expected">If the chunk is expected to exist.<br/>
        /// ONLY ALTERS THE LOGS DISPLAYED, IF THE CHUNK DOESN'T EXIST.</param>
        /// <returns>If the parsing was succesfull without any warnings.</returns>
        public static bool FromFile(
            (long x, long y) position,
            out Chunk? chunk,
            out bool isFileInvalid,
            string? saveFolderName = null,
            bool expected = true
        )
        {
            saveFolderName ??= SaveData.Instance.SaveName;
            var chunkPosition = GetChunkPosition(position);
            var chunkFileName = GetChunkFileName(chunkPosition);
            chunk = null;
            
            var chunkJson = Tools.LoadFileExpected<Chunk>(
                GetChunkFilePath(chunkFileName, saveFolderName),
                out isFileInvalid,
                expected: expected,
                extraFileInformation: $"x: {chunkPosition.x}, y: {chunkPosition.y}"
            );

            if (chunkJson is null)
            {
                return false;
            }

            var fileVersion = SaveManager.GetSaveVersion<Chunk>(
                chunkJson,
                Constants.JsonKeys.Chunk.OLD_FILE_VERSION,
                Constants.JsonKeys.Chunk.FILE_VERSION,
                chunkFileName
            );
            if (fileVersion is null)
            {
                PACTools.LogJsonParseError(Constants.JsonKeys.Chunk.FILE_VERSION, $"assuming minimum, chunk file name: {chunkFileName}");
                fileVersion = Constants.OLDEST_SAVE_VERSION;
            }

            chunkJson.Add(Constants.JsonKeys.Chunk.POSITION_X, position.x);
            chunkJson.Add(Constants.JsonKeys.Chunk.POSITION_Y, position.y);

            var success = PACTools.TryFromJson(chunkJson, fileVersion, out chunk);
            PACSingletons.Instance.Logger.Log("Loaded chunk from file", $"{chunkFileName}.{Constants.SAVE_EXT}");
            return success;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Generates ALL not yet generated tiles.
        /// </summary>
        /// <param name="checkExisting">If it should check, if tile already exists before creating it.</param>
        private void FillChunk(bool checkExisting)
        {
            for (var x = 0; x < Constants.CHUNK_SIZE; x++)
            {
                for (var y = 0; y < Constants.CHUNK_SIZE; y++)
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
        public static string GetChunkFileName((long x, long y) absolutePosition, int chunkSize = Constants.CHUNK_SIZE)
        {
            var chunkPosition = GetChunkPosition(absolutePosition, chunkSize);
            return $"{Constants.CHUNK_FILE_NAME}{Constants.CHUNK_FILE_NAME_SEP}{chunkPosition.x}{Constants.CHUNK_FILE_NAME_SEP}{chunkPosition.y}";
        }
        
        /// <summary>
        /// Generates the chunk random genrator for a chunk.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the chunk.</param>
        /// <param name="chunkSize">The size of a chunk.</param>
        public static SplittableRandom GetChunkRandom((long x, long y) absolutePosition, int chunkSize = Constants.CHUNK_SIZE)
        {
            var chunkPosition = GetChunkPosition(absolutePosition, chunkSize);
            var noiseValues = WorldUtils.GetNoiseValues(chunkPosition.x, chunkPosition.y);
            var noiseNum = noiseValues.Count;
            const double seedNumSize = 19.0;
            var noiseTenMulti = (int)Math.Floor(seedNumSize / noiseNum);
            var noiseMulti = Math.Pow(10, noiseTenMulti);
            var seed = noiseValues.Values
                .Aggregate<double, ulong>(1, (current, noiseVal) => current * (ulong)(noiseVal * noiseMulti));
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
                JsonDataCorrecterUtils.RenameKeyIfExists(oldJson, "chunkRandom", "chunk_random");
            }, "2.2"),
        ];

        public JsonDictionary ToJson()
        {
            var tilesJson = tiles
                .Select(tile => tile.Value.ToJson())
                .Cast<JsonObject?>()
                .ToList();
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
                PACTools.TryParseJsonValue<long>(chunkJson, Constants.JsonKeys.Chunk.POSITION_X, out var posX, isCritical: true) &&
                PACTools.TryParseJsonValue<long>(chunkJson, Constants.JsonKeys.Chunk.POSITION_Y, out var posY, isCritical: true)
            ))
            {
                return false;
            }
            (long x, long y) position = (posX, posY);

            success &= PACTools.TryParseJsonValue<SplittableRandom?>(chunkJson, Constants.JsonKeys.Chunk.CHUNK_RANDOM, out var chunkRandom);
            chunkRandom ??= GetChunkRandom(position);
            
            var chunkPos = GetChunkPosition(position);
            if (!PACTools.TryParseJsonListValue(chunkJson, Constants.JsonKeys.Chunk.TILES, tileJson =>
                {
                    if (!PACTools.TryCastAnyValueForJsonParsing<Tile, JsonDictionary>(tileJson, out var tileJsonValue, isStraigthCast: true))
                    {
                        success = false;
                        return (false, default);
                    }
                    success &= PACTools.TryFromJsonExtra(tileJsonValue, (chunkRandom, chunkPos), fileVersion, out Tile? tile);
                    return (
                        tile is not null,
                        tile is null ? default : new KeyValuePair<(long x, long y), Tile>(GetTilePosition(tile.relativePosition), tile)
                    );
                }, out var tilesKvPair, true)
            )
            {
                return false;
            }
            var tiles = tilesKvPair.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            const int totalTileNum = Constants.CHUNK_SIZE * Constants.CHUNK_SIZE;
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
