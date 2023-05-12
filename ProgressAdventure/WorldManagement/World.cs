using System.Drawing;
using System.Xml.Linq;

namespace ProgressAdventure.WorldManagement
{
    /// <summary>
    /// Object to store the currently loaded world.
    /// </summary>
    public static class World
    {
        #region Public properties
        /// <summary>
        /// The currently loaded dictionary of chunks.
        /// </summary>
        public static Dictionary<string, Chunk> Chunks {  get; private set; }
        #endregion

        #region "Constructors"
        /// <summary>
        /// <inheritdoc cref="World" path="//summary"/>
        /// </summary>
        /// <param name="chunks">The dictionary of chunks.</param>
        public static void Initialise(Dictionary<string, Chunk>? chunks = null)
        {
            Logger.Log("Generating World");
            Chunks = chunks ?? new Dictionary<string, Chunk>();
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Returns the <c>Chunk</c> if it exists, or null.
        /// </summary>
        /// <param name="position">The position of the chunk.</param>
        public static Chunk? FindChunk((long x, long y) position)
        {
            return FindChunk(GetChunkDictName(position));
        }

        /// <summary>
        /// Generates a new <c>Chunk</c> at a specific position.
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
        public static void SaveAllChunksToFiles(string? saveFolderName = null, bool clearChunks = false, string? showProgressText = null)
        {
            saveFolderName ??= SaveData.saveName;
            Dictionary<string, Chunk>? chunkData;
            if (clearChunks)
            {
                if (showProgressText is not null)
                {
                    Console.Write($"{showProgressText}COPYING...\r");
                }
                chunkData = Chunks.DeepCopy();
                Chunks.Clear();
            }
            else
            {
                chunkData = Chunks;
            }
            if (showProgressText is not null)
            {
                double chunkNum = chunkData.Count;
                Console.Write(showProgressText + "              ");
                for ( var x = 0; x < chunkNum; x++)
                {
                    chunkData.ElementAt(x).Value.SaveToFile(saveFolderName);
                    Console.Write($"\r{showProgressText}{Math.Round((x + 1) / chunkNum * 100, 1)}%");
                }
                Console.WriteLine($"\r{showProgressText}DONE!                       ");
            }
            else
            {
                foreach (var chunk in chunkData)
                {
                    chunk.Value.SaveToFile(saveFolderName);
                }
            }
            Logger.Log("Saved all chunks to file", $"save folder name: {saveFolderName}");
        }

        /// <summary>
        /// <c>Chunk.FromFile()</c>, but if it finds the chunk, it adds it to the chunks dictionary.
        /// </summary>
        /// <param name="position">The position of the chunk.</param>
        /// <param name="saveFolderName">If null, it will use the save name in <c>SaveData</c>.</param>
        public static Chunk? FindChunkInFolder((long x, long y) position, string? saveFolderName = null)
        {
            var chunk = Chunk.FromFile(position, saveFolderName);
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
        public static bool TryGetChunkFromFolder((long x, long y) position, out Chunk chunk, string? saveFolderName = null)
        {
            var chunkTemp = FindChunkInFolder(position, saveFolderName);
            chunk = chunkTemp ?? GenerateChunk(position);
            return chunkTemp is not null;
        }

        /// <summary>
        /// Loads all chunks from a save file.
        /// </summary>
        /// <param name="saveFolderName">If null, it will use the save name in <c>SaveData</c>.</param>
        /// <param name="showProgressText">If not null, it writes out a progress percentage with this string while saving.</param>
        public static void LoadAllChunksFromFolder(string? saveFolderName = null, string? showProgressText = null)
        {
            saveFolderName ??= SaveData.saveName;
            Tools.RecreateChunksFolder(saveFolderName);
            // get existing files
            var chunksFolderPath = Path.Join(Constants.SAVES_FOLDER_PATH, saveFolderName, Constants.SAVE_FOLDER_NAME_CHUNKS);
            var chunkFilePaths = Directory.GetFiles(chunksFolderPath);
            var existingChunks = new List<(long x, long y)>();
            foreach (var chunkFilePath in chunkFilePaths)
            {
                var chunkFileName = Path.GetFileName(chunkFilePath);
                if (
                    chunkFileName is not null &&
                    Path.GetExtension(chunkFileName) == $".{Constants.SAVE_EXT}" &&
                    chunkFileName.StartsWith($"{Constants.CHUNK_FILE_NAME}{Constants.CHUNK_FILE_NAME_SEP}")
                )
                {
                    var chunkPositions = Path.GetFileNameWithoutExtension(chunkFileName).Replace($"{Constants.CHUNK_FILE_NAME}{Constants.CHUNK_FILE_NAME_SEP}", "").Split(Constants.CHUNK_FILE_NAME_SEP);
                    if (
                        chunkPositions.Length == 2 &&
                        long.TryParse(chunkPositions[0], out long posX) &&
                        long.TryParse(chunkPositions[1], out long posY)
                    )
                    {
                        existingChunks.Add((posX, posY));
                        continue;
                    }
                    Logger.Log("Chunk file parse error", "chunk positions couldn' be extracted from chunk file name.", Enums.LogSeverity.WARN);
                }
                Logger.Log("Chunk file parse error", "file name is not chunk file name.", Enums.LogSeverity.WARN);
            }
            // load chunks
            if (showProgressText is not null)
            {
                double chunkNum = existingChunks.Count;
                Console.Write(showProgressText + "              ");
                for (var x = 0; x < chunkNum; x++)
                {
                    FindChunkInFolder(existingChunks[x], saveFolderName);
                    Console.Write($"\r{showProgressText}{Math.Round((x + 1) / chunkNum * 100, 1)}%");
                }
                Console.WriteLine($"\r{showProgressText}DONE!                       ");
            }
            else
            {
                foreach (var chunkPos in existingChunks)
                {
                    FindChunkInFolder(chunkPos, saveFolderName);
                }
            }
            Logger.Log("Loaded all chunks from file", $"save folder name: {saveFolderName}");
        }

        /// <summary>
        /// Returns the <c>Chunk</c>, if it exists in the the dictionary or in the file, or null.<br/>
        /// Checks in chunks dictionary and in save folder.
        /// </summary>
        /// <param name="position">The position of the chunk.</param>
        /// <param name="saveFolderName">If null, it will use the save name in <c>SaveData</c>.</param>
        public static Chunk? FindChunkAll((long x, long y) position, string? saveFolderName = null)
        {
            return FindChunk(position) ?? FindChunkInFolder(position, saveFolderName);
        }

        /// <summary>
        /// Returns the <c>Tile</c> if it, and the <c>Chunk</c> is should be in exists, or null.<br/>
        /// Checks in chunks dictionary and in save folder.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the tile.</param>
        /// <param name="saveFolderName">If null, it will use the save name in <c>SaveData</c>.</param>
        public static Tile? FindTileAll((long x, long y) absolutePosition, string? saveFolderName = null)
        {
            return FindChunkAll(absolutePosition, saveFolderName)?.FindTile(absolutePosition);
        }

        /// <summary>
        /// Tries to find a <c>Chunk</c> at a specific location, creates one, if it doesn't exist in the dictionary and the save folder, and adds the result into the chunks dictionary.<br/>
        /// Checks in chunks dictionary and in save folder.
        /// <param name="position">The position of the chunk.</param>
        /// <param name="chunk">The chunk that was fould or created.</param>
        /// <param name="saveFolderName">If null, it will use the save name in <c>SaveData</c>.</param>
        /// </summary>
        public static bool TryGetChunkAll((long x, long y) position, out Chunk chunk, string? saveFolderName = null)
        {
            var res = true;
            var chunkTemp = FindChunk(position);
            if (chunkTemp is null)
            {
                res = TryGetChunkFromFolder(position, out Chunk chunkTemp2, saveFolderName);
                chunkTemp = chunkTemp2;
            }
            chunk = chunkTemp;
            return res;
        }

        /// <summary>
        /// Generates a new <c>Tile</c>, and the <c>Chunk</c> that should contain it, if that also doesn't exist.<br/>
        /// Checks in chunks dictionary and in save folder.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the tile.</param>
        /// <param name="saveFolderName">If null, it will use the save name in <c>SaveData</c>.</param>
        public static Tile GenerateTile((long x, long y) absolutePosition, string? saveFolderName = null)
        {
            TryGetChunkAll(absolutePosition, out Chunk chunk, saveFolderName);
            var tile = chunk.GenerateTile(absolutePosition);
            return tile;
        }

        /// <summary>
        /// Tries to find a <c>Tile</c> at a specific location, creates one, if it doesn't exist in the dictionary and the save folder, and adds the result into dictionary. Generates a new <c>Tile</c>, and the <c>Chunk</c> that should contain it, if that also doesn't exist.<br/>
        /// Checks in chunks dictionary and in save folder.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the tile.</param>
        /// <param name="saveFolderName">If null, it will use the save name in <c>SaveData</c>.</param>
        public static bool TryGetTileAll((long x, long y) absolutePosition, out Tile tile, string? saveFolderName = null)
        {
            var res = TryGetChunkAll(absolutePosition, out Chunk chunk, saveFolderName);
            var res2 = chunk.TryGetTile(absolutePosition, out tile);
            return res && res2;
        }

        /// <summary>
        /// Generates ALL not yet generated tiles in ALL chunks.
        /// </summary>
        /// <param name="showProgressText">If not null, it writes out a progress percentage with this string while saving.</param>
        public static void FillAllChunks(string? showProgressText = null)
        {
            if (showProgressText is not null)
            {
                double chunkNum = Chunks.Count;
                Console.Write(showProgressText + "              ");
                for (var x = 0; x < chunkNum; x++)
                {
                    Chunks.ElementAt(x).Value.FillChunk();
                    Console.Write($"\r{showProgressText}{Math.Round((x + 1) / chunkNum * 100, 1)}%");
                }
                Console.WriteLine($"\r{showProgressText}DONE!                       ");
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
        private static (long minX, long minY, long maxX, long maxY)? GetCorners()
        {
            if (!Chunks.Any())
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
        /// <param name="saveFolderName">If null, it will use the save name in <c>SaveData</c>.</param>
        public static void MakeRectangle(string? saveFolderName = null)
        {
            var cornersTemp = GetCorners();
            if (cornersTemp is null)
            {
                return;
            }
            var (minX, minY, maxX, maxY) = cornersTemp.Value;
            for (long x = minX; x < maxX + 1; x += Constants.CHUNK_SIZE)
            {
                for (long y = minY; y < maxY + 1; y += Constants.CHUNK_SIZE)
                {
                    TryGetChunkAll((x, y), out _, saveFolderName);
                }
            }
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Converts the position of the chunk into it's dictionary key name.
        /// </summary>
        /// <param name="position">The position of the chunk.</param>
        private static string GetChunkDictName((long x, long y) position)
        {
            return $"{Utils.FloorRound(position.x, Constants.CHUNK_SIZE)}_{Utils.FloorRound(position.y, Constants.CHUNK_SIZE)}";
        }

        /// <summary>
        /// Returns the <c>Chunk</c> if it exists, or null.
        /// </summary>
        /// <param name="chunkKey">The name of the chunk in the distionary.</param>
        private static Chunk? FindChunk(string chunkKey)
        {
            Chunks.TryGetValue(chunkKey, out Chunk? chunk);
            return chunk;
        }
        #endregion
    }
}
