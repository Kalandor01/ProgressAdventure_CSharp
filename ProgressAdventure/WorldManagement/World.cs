using NPrng.Generators;
using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using System.Reflection;
using System.Text.RegularExpressions;
using PACTools = PACommon.Tools;

namespace ProgressAdventure.WorldManagement
{
    /// <summary>
    /// Object to store the currently loaded world.
    /// </summary>
    public static partial class World
    {
        #region Public properties
        /// <summary>
        /// The currently loaded dictionary of chunks.
        /// </summary>
        public static Dictionary<string, Chunk> Chunks { get; private set; }
        #endregion

        #region "Constructors"
        /// <summary>
        /// <inheritdoc cref="World" path="//summary"/>
        /// </summary>
        /// <param name="chunks">The dictionary of chunks.</param>
        public static void Initialize(Dictionary<string, Chunk>? chunks = null)
        {
            PACSingletons.Instance.Logger.Log($"{(chunks is null ? "Generating" : "Loading")} world", chunks is null ? null : $"{chunks.Count} chunks");
            Chunks = chunks ?? [];
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Returns the <see cref="Chunk"/> if it exists, or null.
        /// </summary>
        /// <param name="position">The position of the chunk.</param>
        public static Chunk? FindChunk((long x, long y) position)
        {
            return FindChunk(GetChunkDictName(position));
        }

        /// <summary>
        /// Generates a new <see cref="Chunk"/> at a specific position.
        /// </summary>
        /// <param name="position">The position of the chunk.</param>
        public static Chunk GenerateChunk((long x, long y) position)
        {
            var chunkName = GetChunkDictName(position);
            var chunk = new Chunk(position);
            Chunks[chunkName] = chunk;
            return chunk;
        }

        /// <summary>
        /// Tries to find a chunk at a specific location, and creates one, if doesn't exist.
        /// </summary>
        /// <param name="position">The position of the chunk.</param>
        /// <param name="chunk">The chunk that was found or created.</param>
        public static bool TryGetChunk((long x, long y) position, out Chunk chunk)
        {
            var res = FindChunk(position);
            chunk = res ?? GenerateChunk(position);
            return res is not null;
        }

        /// <summary>
        /// Saves all chunks to the save file.
        /// </summary>
        /// <param name="saveFolderName">If null, it will use the save name in <c>SaveData</c>.</param>
        /// <param name="clearChunks">If the chunks dictionary should be cleared, after saving.</param>
        /// <param name="showProgressText">If not null, it writes out a progress percentage with this string while saving.</param>
        /// <param name="threadManager">The <see cref="ThreadManager"/> to use to cancel the task.</param>
        public static void SaveAllChunksToFiles(
            string? saveFolderName = null,
            bool clearChunks = false,
            string? showProgressText = null,
            ThreadManager? threadManager = null
        )
        {
            if (threadManager?.IsCanceled == true)
            {
                return;
            }

            saveFolderName ??= SaveData.Instance.saveName;
            Dictionary<string, Chunk> chunkData;

            // clearing chunks
            if (clearChunks)
            {
                KeyValuePair<string, Chunk>? playerRefChunkKV = null;
                (long x, long y)? playerBasePos = null;
                if (SaveData.Instance.PlayerRef.Position is not null)
                {
                    var (x, y) = SaveData.Instance.PlayerRef.Position.Value;
                    playerBasePos = (
                        Utils.FloorRound(x, Constants.CHUNK_SIZE),
                        Utils.FloorRound(y, Constants.CHUNK_SIZE)
                    );
                }

                chunkData = [];
                if (showProgressText is not null)
                {
                    var loadingText = PACTools.GetStandardLoadingText(showProgressText + "-COPYING...");
                    loadingText.Display();
                    var chunkCount = (double)Chunks.Count;
                    for (var x = 0; x < Chunks.Count; x++)
                    {
                        if (threadManager?.IsCanceled == true)
                        {
                            loadingText.StopLoadingStandard();
                            return;
                        }

                        var chunk = Chunks.ElementAt(x);
                        if (playerBasePos is not null && playerBasePos == chunk.Value.basePosition)
                        {
                            playerRefChunkKV = chunk;
                        }

                        var chunkCopy = chunk.DeepCopy();
                        chunkData.Add(chunkCopy.Key, chunkCopy.Value);
                        loadingText.Value = (x + 1) / chunkCount;
                    }
                    loadingText.StopLoadingStandard();
                }
                else
                {
                    foreach (var chunk in Chunks)
                    {
                        if (threadManager?.IsCanceled == true)
                        {
                            return;
                        }

                        if (playerBasePos is not null && playerBasePos == chunk.Value.basePosition)
                        {
                            playerRefChunkKV = chunk;
                        }

                        var chunkCopy = chunk.DeepCopy();
                        chunkData.Add(chunkCopy.Key, chunkCopy.Value);
                    }
                }

                Chunks.Clear();

                // re-add player ref Chunk
                if (playerRefChunkKV != null)
                {
                    Chunks.Add(playerRefChunkKV.Value.Key, playerRefChunkKV.Value.Value);
                }
            }
            else
            {
                chunkData = Chunks;
            }

            // saving chunks
            if (showProgressText is not null)
            {
                var chunkNum = (double)chunkData.Count;
                var loadingText = PACTools.GetStandardLoadingText(showProgressText);
                loadingText.Display();
                for (var x = 0; x < chunkNum; x++)
                {
                    if (threadManager?.IsCanceled == true)
                    {
                        loadingText.StopLoadingStandard();
                        return;
                    }
                    chunkData.ElementAt(x).Value.SaveToFile(saveFolderName);
                    loadingText.Value = (x + 1) / chunkNum;
                }
                loadingText.StopLoadingStandard();
                Console.WriteLine();
            }
            else
            {
                foreach (var chunk in chunkData)
                {
                    if (threadManager?.IsCanceled == true)
                    {
                        return;
                    }
                    chunk.Value.SaveToFile(saveFolderName);
                }
            }
            PACSingletons.Instance.Logger.Log("Saved all chunks to file", $"save folder name: \"{saveFolderName}\"");
        }

        /// <summary>
        /// <see cref="Chunk.FromFile(ValueTuple{long, long}, out Chunk?, string?, bool)"/>, but if it finds the chunk, it adds it to the chunks dictionary.
        /// </summary>
        /// <param name="position">The position of the <see cref="Chunk"/>.</param>
        /// <param name="saveFolderName">If null, it will use the save name in <see cref="SaveData"/>.</param>
        public static Chunk? FindChunkInFolder((long x, long y) position, string? saveFolderName = null)
        {
            Chunk.FromFile(position, out var chunk, saveFolderName, false);
            if (chunk is not null)
            {
                Chunks.Add(GetChunkDictName(position), chunk);
            }
            return chunk;
        }

        /// <summary>
        /// Tries to find a chunk at a specific location, creates one, if doesn't exist, and adds the result into the chunks dictionary.
        /// </summary>
        /// <param name="position">The position of the chunk.</param>
        /// <param name="chunk">The chunk that was fould or created.</param>
        /// <param name="saveFolderName">If null, it will use the save name in <c>SaveData</c>.</param>
        /// <returns>If the <see cref="Chunk"/> file was found.</returns>
        public static bool TryGetChunkFromFolder((long x, long y) position, out Chunk chunk, string? saveFolderName = null)
        {
            var chunkTemp = FindChunkInFolder(position, saveFolderName);
            chunk = chunkTemp ?? GenerateChunk(position);
            return chunkTemp is not null;
        }

        /// <summary>
        /// Gets all chunk files that have the correct syntax from a chunks folder.
        /// </summary>
        /// <param name="saveFolderName">If null, it will use the save name in <c>SaveData</c>.</param>
        /// <param name="checkOldExtension">Whether to check the pre 2.3 file extension.</param>
        /// <returns>The list of chunk positions.</returns>
        public static List<(long x , long y)> GetChunkFilesFromFolder(string? saveFolderName = null, bool checkOldExtension = true)
        {
            saveFolderName ??= SaveData.Instance.saveName;
            Tools.RecreateChunksFolder(saveFolderName);

            // get existing files
            var chunksFolderPath = Path.Join(Constants.SAVES_FOLDER_PATH, saveFolderName, Constants.SAVE_FOLDER_NAME_CHUNKS);
            var chunkFilePaths = Directory.GetFiles(chunksFolderPath);
            var existingChunks = new List<(long x, long y)>();
            foreach (var chunkFilePath in chunkFilePaths)
            {
                var chunkFileName = Path.GetFileName(chunkFilePath);
                var chunkExtension = chunkFileName is not null ? Path.GetExtension(chunkFileName) : null;
                if (!(
                    chunkFileName is not null &&
                    (chunkExtension == $".{Constants.SAVE_EXT}" || chunkExtension == $".{Constants.OLD_SAVE_EXT}") &&
                    chunkFileName.StartsWith($"{Constants.CHUNK_FILE_NAME}{Constants.CHUNK_FILE_NAME_SEP}")
                ))
                {
                    PACSingletons.Instance.Logger.Log("Chunk file parse error", $"file name is not chunk file name", LogSeverity.WARN);
                    continue;
                }

                var chunkPositions = ChunkFileNameRegex().Match(Path.GetFileNameWithoutExtension(chunkFileName)).Groups.Values.Select(group => group.Value);
                if (!(
                    chunkPositions.Count() == 3 &&
                    long.TryParse(chunkPositions.ElementAt(1), out long posX) &&
                    long.TryParse(chunkPositions.ElementAt(2), out long posY)
                ))
                {
                    PACSingletons.Instance.Logger.Log("Chunk file parse error", $"chunk positions couldn't be extracted from chunk file name: {chunkFileName}", LogSeverity.WARN);
                    continue;
                }

                existingChunks.Add((posX, posY));
            }
            return existingChunks;
        }

        /// <summary>
        /// Loads all chunks from a save file.
        /// </summary>
        /// <param name="saveFolderName">If null, it will use the save name in <see cref="SaveData"/>.</param>
        /// <param name="showProgressText">If not null, it writes out a progress percentage with this string while saving.</param>
        /// <param name="checkOldExtension">Whether to check the pre 2.3 file extension.</param>
        public static void LoadAllChunksFromFolder(string? saveFolderName = null, string? showProgressText = null, bool checkOldExtension = true)
        {
            var existingChunks = GetChunkFilesFromFolder(saveFolderName, checkOldExtension);
            // load chunks
            if (showProgressText is not null)
            {
                var loadingText = PACTools.GetStandardLoadingText(showProgressText);
                loadingText.Display();
                double chunkNum = existingChunks.Count;
                for (var x = 0; x < chunkNum; x++)
                {
                    if (!Chunks.ContainsKey(GetChunkDictName(existingChunks[x])))
                    {
                        FindChunkInFolder(existingChunks[x], saveFolderName);
                    }
                    loadingText.Value = (x + 1) / chunkNum;
                }
                loadingText.StopLoadingStandard();
            }
            else
            {
                foreach (var chunkPos in existingChunks)
                {
                    if (!Chunks.ContainsKey(GetChunkDictName(chunkPos)))
                    {
                        FindChunkInFolder(chunkPos, saveFolderName);
                    }
                }
            }
            PACSingletons.Instance.Logger.Log("Loaded all chunks from file", $"save folder name: {saveFolderName}");
        }

        /// <summary>
        /// Returns the <see cref="Chunk"/>, if it exists in the the dictionary or in the file, or null.<br/>
        /// Checks in chunks dictionary and in save folder.
        /// </summary>
        /// <param name="position">The position of the <see cref="Chunk"/>.</param>
        /// <param name="saveFolderName">If null, it will use the save name in <see cref="SaveData"/>.</param>
        public static Chunk? FindChunkAll((long x, long y) position, string? saveFolderName = null)
        {
            return FindChunk(position) ?? FindChunkInFolder(position, saveFolderName);
        }

        /// <summary>
        /// Returns the <see cref="Tile"/> if it, and the <see cref="Chunk"/> is should be in exists, or null.<br/>
        /// Checks in chunks dictionary and in save folder.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the tile.</param>
        /// <param name="saveFolderName">If null, it will use the save name in <see cref="SaveData"/>.</param>
        public static Tile? FindTileAll((long x, long y) absolutePosition, string? saveFolderName = null)
        {
            return FindChunkAll(absolutePosition, saveFolderName)?.FindTile(absolutePosition);
        }

        /// <summary>
        /// Tries to find a <see cref="Chunk"/> at a specific location, creates one, if it doesn't exist in the dictionary and the save folder, and adds the result into the chunks dictionary.<br/>
        /// Checks in chunks dictionary and in save folder.
        /// </summary>
        /// <param name="position">The position of the <see cref="Chunk"/>.</param>
        /// <param name="chunk">The <see cref="Chunk"/> that was found or created.</param>
        /// <param name="saveFolderName">If null, it will use the save name in <see cref="SaveData"/>.</param>
        /// <returns>If the <see cref="Chunk"/> didn't need to be generated.</returns>
        public static bool TryGetChunkAll((long x, long y) position, out Chunk chunk, string? saveFolderName = null)
        {
            var res = true;
            var chunkTemp = FindChunk(position);
            if (chunkTemp is null)
            {
                res = TryGetChunkFromFolder(position, out var chunkTemp2, saveFolderName);
                chunkTemp = chunkTemp2;
            }
            chunk = chunkTemp;
            return res;
        }

        /// <summary>
        /// Generates a new <see cref="Tile"/>, and the <see cref="Chunk"/> that should contain it, if that also doesn't exist.<br/>
        /// Checks in chunks dictionary and in save folder.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the tile.</param>
        /// <param name="saveFolderName">If null, it will use the save name in <see cref="SaveData"/>.</param>
        public static Tile GenerateTile((long x, long y) absolutePosition, string? saveFolderName = null)
        {
            TryGetChunkAll(absolutePosition, out var chunk, saveFolderName);
            var tile = chunk.GenerateTile(absolutePosition);
            return tile;
        }

        /// <summary>
        /// Tries to find a <see cref="Tile"/> at a specific location, creates one, if it doesn't exist in the dictionary and the save folder, and adds the result into dictionary. Generates a new <see cref="Tile"/>, and the <see cref="Chunk"/> that should contain it, if that also doesn't exist.<br/>
        /// Checks in chunks dictionary and in save folder.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the <see cref="Tile"/>.</param>
        /// <param name="saveFolderName">If null, it will use the save name in <see cref="SaveData"/>.</param>
        /// <returns>If the <see cref="Chunk"/> or the <see cref="Tile"/> didn't need to be generated.</returns>
        public static bool TryGetTileAll((long x, long y) absolutePosition, out Tile tile, string? saveFolderName = null)
        {
            var res = TryGetChunkAll(absolutePosition, out var chunk, saveFolderName);
            var res2 = chunk.TryGetTile(absolutePosition, out tile);
            return res && res2;
        }

        /// <summary>
        /// Generates ALL not yet generated tiles in ALL chunks.
        /// </summary>
        /// <param name="showProgressText">If not null, it writes out a progress percentage with this string while working.</param>
        public static void FillAllChunks(string? showProgressText = null)
        {
            if (showProgressText is not null)
            {
                double chunkNum = Chunks.Count;
                var loadingText = PACTools.GetStandardLoadingText(showProgressText);
                loadingText.Display();
                for (var x = 0; x < chunkNum; x++)
                {
                    Chunks.ElementAt(x).Value.FillChunk();
                    loadingText.Value = (x + 1) / chunkNum;
                }
                loadingText.StopLoadingStandard();
            }
            else
            {
                foreach (var chunk in Chunks)
                {
                    chunk.Value.FillChunk();
                }
            }
        }

        /// <summary>
        /// Returns the four corners of the world.
        /// </summary>
        public static (long minX, long minY, long maxX, long maxY)? GetCorners()
        {
            if (Chunks.Count == 0)
            {
                return null;
            }
            var firstChunkPos = Chunks.First().Value.basePosition;
            var minX = firstChunkPos.x;
            var minY = firstChunkPos.y;
            var maxX = firstChunkPos.x;
            var maxY = firstChunkPos.y;
            foreach (var chunk in Chunks)
            {
                var (x, y) = chunk.Value.basePosition;
                if (x > maxX)
                {
                    maxX = x;
                }
                if (y > maxY)
                {
                    maxY = y;
                }
                if (x < minX)
                {
                    minX = x;
                }
                if (y < minY)
                {
                    minY = y;
                }
            }
            maxX += Constants.CHUNK_SIZE - 1;
            maxY += Constants.CHUNK_SIZE - 1;
            return (minX, minY, maxX, maxY);
        }

        /// <summary>
        /// Generates chunks in a way that makes the world rectangle shaped.
        /// </summary>
        /// <param name="saveFolderName">If null, it will use the save name in <see cref="SaveData"/>.</param>
        /// <param name="showProgressText">If not null, it writes out a progress percentage with this string while working.</param>
        public static void MakeRectangle(string? saveFolderName = null, string? showProgressText = null)
        {
            var corners = GetCorners();
            if (corners is null)
            {
                return;
            }

            var (minX, minY, maxX, maxY) = corners.Value;

            if (showProgressText is not null)
            {
                var chunkNum = (maxX - minX) / (double)Constants.CHUNK_SIZE * ((maxY - minY) / (double)Constants.CHUNK_SIZE);
                var columnNum = (maxY - minY) / (double)Constants.CHUNK_SIZE;
                var loadingText = PACTools.GetStandardLoadingText(showProgressText);
                loadingText.Display();
                for (var x = minX; x <= maxX; x += Constants.CHUNK_SIZE)
                {
                    for (long y = minY; y <= maxY; y += Constants.CHUNK_SIZE)
                    {
                        TryGetChunkAll((x, y), out _, saveFolderName);
                        loadingText.Value = ((x - minX) / (double)Constants.CHUNK_SIZE * columnNum + (y - minY) / (double)Constants.CHUNK_SIZE) / chunkNum;
                    }
                }
                loadingText.StopLoadingStandard();
            }
            else
            {
                for (var x = minX; x <= maxX; x += Constants.CHUNK_SIZE)
                {
                    for (var y = minY; y <= maxY; y += Constants.CHUNK_SIZE)
                    {
                        TryGetChunkAll((x, y), out _, saveFolderName);
                    }
                }
            }
        }

        /// <summary>
        /// Re-calculates all chunk files in a save folder, to have the correct chunk size.
        /// </summary>
        /// <param name="saveFolderName">If null, it will use the save name in <see cref="SaveData"/>.</param>
        /// <param name="showProgressText">If not null, it writes out a progress percentage with this string while saving.</param>
        public static bool RecalculateChunkFileSizes(int oldChunkSize, int newChunkSize, string? saveFolderName = null, string? showProgressText = null)
        {
            if (oldChunkSize == newChunkSize)
            {
                return true;
            }

            // setup
            PACSingletons.Instance.Logger.Log("Recalculating chunk file sizes", $"chunk size: {oldChunkSize} -> {newChunkSize}");
            saveFolderName ??= SaveData.Instance.saveName;
            var chunkSizeChangeFolderName = $"chunk_sizes_from_{oldChunkSize}_to_{Constants.CHUNK_SIZE}";
            var saveFolderPath = Tools.GetSaveFolderPath(saveFolderName);
            var chunkSizeChangeFolderPath = Path.Join(saveFolderPath, chunkSizeChangeFolderName);
            var chunksFolderPath = Path.Join(saveFolderPath, Constants.SAVE_FOLDER_NAME_CHUNKS);
            PACTools.RecreateFolder(Path.Join(saveFolderPath, chunkSizeChangeFolderName), "chunk size change");

            // correction
            var success = true;
            var chunkPositions = GetChunkFilesFromFolder(saveFolderName);

            var versionCorrecters = typeof(Chunk)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(f => f.Name.Contains("VersionCorrecters"))?
                .GetValue(null) as List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)>;

            foreach (var chunkPosition in chunkPositions)
            {
                var chunkFileName = Chunk.GetChunkFileName(chunkPosition, oldChunkSize);

                if (
                    Tools.LoadCompressedFileExpected<Chunk>(
                        Chunk.GetChunkFilePath(chunkFileName, saveFolderName),
                        expected: true,
                        extraFileInformation: $"x: {chunkPosition.x}, y: {chunkPosition.y}"
                    ) is not JsonDictionary chunkJson
                )
                {
                    success &= false;
                    continue;
                }

                var fileVersion = SaveManager.GetSaveVersion<Chunk>(
                    chunkJson,
                    Constants.JsonKeys.Chunk.OLD_FILE_VERSION,
                    Constants.JsonKeys.Chunk.FILE_VERSION,
                    chunkFileName
                );
                if (fileVersion is null)
                {
                    PACTools.LogJsonParseError<Chunk>(Constants.JsonKeys.Chunk.FILE_VERSION, $"assuming minimum, chunk file name: {chunkFileName}");
                    fileVersion = Constants.OLDEST_SAVE_VERSION;
                }

                if (versionCorrecters is not null)
                {
                    PACSingletons.Instance.JsonDataCorrecter.CorrectJsonData<Chunk>(chunkJson, versionCorrecters, fileVersion);
                }

                success &= PACTools.TryParseJsonValue<Chunk, SplittableRandom?>(chunkJson, Constants.JsonKeys.Chunk.CHUNK_RANDOM, out var chunkRandom);
                chunkRandom ??= Chunk.GetChunkRandom(chunkPosition, newChunkSize);

                if (!PACTools.TryParseJsonListValue<Chunk, KeyValuePair<string, Tile>>(chunkJson, Constants.JsonKeys.Chunk.TILES, tileJson => {
                    if (!PACTools.TryCastAnyValueForJsonParsing<Tile, JsonDictionary>(tileJson, out var tileJsonValue, isStraigthCast: true))
                    {
                        success = false;
                        return (false, default);
                    }
                    success &= PACTools.TryFromJsonExtra(tileJsonValue, (chunkRandom, chunkPosition), fileVersion, out Tile? tile);
                    if (tile is null)
                    {
                        return (false, default);
                    }
                    var tileDictName = $"{Utils.Mod(tile.relativePosition.x, oldChunkSize)}_{Utils.Mod(tile.relativePosition.y, oldChunkSize)}";
                    return (true, new KeyValuePair<string, Tile>(tileDictName, tile));
                }, out var tilesKvPair, true))
                {
                    return false;
                }
                var tiles = tilesKvPair.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                PACSingletons.Instance.Logger.Log("Loaded chunk from file", $"{chunkFileName}.{Constants.SAVE_EXT}");
            }

            // done
            Directory.Delete(chunksFolderPath, true);
            Directory.Move(chunkSizeChangeFolderPath, chunksFolderPath);
            PACSingletons.Instance.Logger.Log("Recalculated chunk file sizes", $"chunk size: {oldChunkSize} -> {newChunkSize}");
            return success;
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Converts the position of the chunk into it's dictionary key name.
        /// </summary>
        /// <param name="position">The position of the <see cref="Chunk"/>.</param>
        private static string GetChunkDictName((long x, long y) position)
        {
            return $"{Utils.FloorRound(position.x, Constants.CHUNK_SIZE)}_{Utils.FloorRound(position.y, Constants.CHUNK_SIZE)}";
        }

        /// <summary>
        /// Returns the <see cref="Chunk"/> if it exists, or null.
        /// </summary>
        /// <param name="chunkKey">The name of the chunk in the distionary.</param>
        private static Chunk? FindChunk(string chunkKey)
        {
            Chunks.TryGetValue(chunkKey, out var chunk);
            return chunk;
        }

        [GeneratedRegex("^chunk_(-?\\d+)_(-?\\d+)$")]
        private static partial Regex ChunkFileNameRegex();
        #endregion
    }
}
