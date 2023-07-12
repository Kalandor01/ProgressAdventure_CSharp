using ProgressAdventure;
using ProgressAdventure.WorldManagement;
using System.Drawing;
using System;
using PAConstants = ProgressAdventure.Constants;
using PAUtils = ProgressAdventure.Utils;

namespace PAExtras
{
    /// <summary>
    /// Contains project specific useful functions.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Decodes a save file into a json format, and returns if it succeded.
        /// </summary>
        /// <param name="saveFolderName"></param>
        /// <param name="savesFolderPath"></param>
        /// <param name="saveSeed"></param>
        /// <param name="saveExtension"></param>
        public static bool DecodeSaveFile(string saveFolderName, string? savesFolderPath = null, long? saveSeed = null, string? saveExtension = null)
        {
            savesFolderPath ??= PAConstants.SAVES_FOLDER_PATH;
            saveSeed ??= PAConstants.SAVE_SEED;
            saveExtension ??= PAConstants.SAVE_EXT;

            List<string> saveData;
            try
            {
                saveData = SaveFileManager.FileConversion.DecodeFile((long)saveSeed, Path.Join(savesFolderPath, saveFolderName), saveExtension);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"decode save file: FILE {saveFolderName} NOT FOUND!");
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"decode save file: DIRECTORY {saveFolderName} NOT FOUND!");
                return false;
            }

            using var f = File.CreateText(Path.Join(savesFolderPath, saveFolderName) + ".decoded.json");
            foreach (var line in saveData)
            {
                f.WriteLine(line);
            }
            f.Close();
            return true;
        }

        /// <summary>
        /// Encodes a json file into a .savc format, and returns if it succeded.
        /// </summary>
        /// <param name="saveFolderName"></param>
        /// <param name="savesFolderPath"></param>
        /// <param name="saveSeed"></param>
        /// <param name="saveExtension"></param>
        public static bool EncodeSaveFile(string saveFolderName, string? savesFolderPath = null, long? saveSeed = null, string? saveExtension = null)
        {
            savesFolderPath ??= PAConstants.SAVES_FOLDER_PATH;
            saveSeed ??= PAConstants.SAVE_SEED;
            saveExtension ??= PAConstants.SAVE_EXT;

            List<string> saveData;
            try
            {
                using var f = File.OpenText(Path.Join(savesFolderPath, saveFolderName) + ".decoded.json");
                saveData = f.ReadToEnd().Split("\n").ToList();
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"encode save file: FILE {saveFolderName} NOT FOUND!");
                return false;
            }

            SaveFileManager.FileConversion.EncodeFile(saveData, (long)saveSeed, Path.Join(savesFolderPath, saveFolderName), saveExtension, PAConstants.FILE_ENCODING_VERSION);
            return true;
        }

        /// <summary>
        /// Recompiles a save file to a different name/seed, and returns if it succeded.
        /// </summary>
        /// <param name="saveFolderName"></param>
        /// <param name="newSaveFolderName"></param>
        /// <param name="savesFolderPath"></param>
        /// <param name="newSavesFolderPath"></param>
        /// <param name="saveSeed"></param>
        /// <param name="newSaveSeed"></param>
        /// <param name="saveExtension"></param>
        /// <param name="newSaveExtension"></param>
        public static bool RecompileSaveFile(string saveFolderName, string? newSaveFolderName = null, string? savesFolderPath = null, string? newSavesFolderPath = null, long? saveSeed = null, long? newSaveSeed = null, string? saveExtension = null, string? newSaveExtension = null)
        {
            savesFolderPath ??= PAConstants.SAVES_FOLDER_PATH;
            saveSeed ??= PAConstants.SAVE_SEED;
            saveExtension ??= PAConstants.SAVE_EXT;

            newSaveFolderName ??= saveFolderName;
            newSavesFolderPath ??= savesFolderPath;
            newSaveSeed ??= saveSeed;
            newSaveExtension ??= saveExtension;

            List<string> saveData;
            try
            {
                saveData = SaveFileManager.FileConversion.DecodeFile((long)saveSeed, Path.Join(savesFolderPath, saveFolderName), saveExtension);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"recompile save file: FILE {saveFolderName} NOT FOUND!");
                return false;
            }

            SaveFileManager.FileConversion.EncodeFile(saveData, (long)newSaveSeed, Path.Join(newSavesFolderPath, newSaveFolderName), newSaveExtension, PAConstants.FILE_ENCODING_VERSION);
            return true;
        }

        /// <summary>
        /// Generates a rectangle saped area of chunks.
        /// </summary>
        /// <param name="corners">The coordinates of the corners.</param>
        /// <param name="numberOfSaves">How many times to save the world, while generating the chunks.</param>
        /// <param name="saveFolderName">The name of the save folder.</param>
        /// <param name="fillChunks">If the function should fill all chunks it goes through. All chunks should be filled by default.</param>
        public static void FillWorldAreaSegmented((long minX, long minY, long maxX, long maxY) corners, int numberOfSaves = 1, string? saveFolderName = null, bool fillChunks = false)
        {
            var (minX, minY, maxX, maxY) = ChunkAlignRectangleCoordinates(corners);
            var chunkNumX = (maxX - minX) / PAConstants.CHUNK_SIZE;
            var chunkNumY = (maxY - minY) / PAConstants.CHUNK_SIZE;
            var chunkNum = chunkNumX * chunkNumY;
            var saveNum = 0;
            for (var x = minX; x < maxX; x += PAConstants.CHUNK_SIZE)
            {
                for (var y = minY; y < maxY; y += PAConstants.CHUNK_SIZE)
                {
                    World.TryGetChunkAll((x, y), out Chunk chunk);
                    if (fillChunks)
                    {
                        chunk.FillChunk();
                    }
                    Console.Write($"\r({saveNum}/{numberOfSaves})Filling chunks...{Math.Round(  ((double)(((x - minX) / PAConstants.CHUNK_SIZE) + ((y - minY) / PAConstants.CHUNK_SIZE) + 1) % (chunkNum / numberOfSaves)) / (chunkNum / numberOfSaves) * 100, 1)}%");
                    if (PAUtils.Mod(((x - minX) / PAConstants.CHUNK_SIZE) + ((y - minY) / PAConstants.CHUNK_SIZE) + 1, chunkNum / numberOfSaves) == 0)
                    {
                        saveNum++;
                        Console.WriteLine($"\r({saveNum}/{numberOfSaves})Filling chunks...DONE!           ");
                        World.SaveAllChunksToFiles(saveFolderName, true, $"({saveNum}/{numberOfSaves})Saving...");
                    }
                }
            }
            World.SaveAllChunksToFiles(saveFolderName, true, "(FINAL)Saving...");
            Console.WriteLine("DONE!");
        }

        /// <summary>
        /// Generates chunks in a way that makes the world rectangle shaped.
        /// </summary>
        /// <param name="corners">The coordinates of the corners.</param>
        /// <param name="saveFolderName">The name of the save folder.</param>
        public static void FillWorldSimple((long minX, long minY, long maxX, long maxY) corners, string? saveFolderName = null)
        {
            World.TryGetTileAll((corners.minX, corners.minY), out _, saveFolderName);
            World.TryGetTileAll((corners.maxX, corners.maxY), out _, saveFolderName);
            Console.Write("Generating chunks...");
            World.MakeRectangle(saveFolderName);
            Console.WriteLine("DONE!");
            World.FillAllChunks("Filling chunks...");
        }

        /// <summary>
        /// Returns the chunk border aligned equivalent of the original coordinates.
        /// </summary>
        /// <param name="corners">The coordinates of the corners.</param>
        public static (long minX, long minY, long maxX, long maxY) ChunkAlignRectangleCoordinates((long minX, long minY, long maxX, long maxY) corners)
        {
            return (ChunkAlign(corners.minX), ChunkAlign(corners.minY), ChunkAlign(corners.maxX, false), ChunkAlign(corners.maxY, false));
        }

        /// <summary>
        /// Returns the chunk border aligned equivalent of the original coordinates.
        /// </summary>
        /// <param name="coordinate">The coordinates to align.</param>
        /// <param name="alignToMin">Whether to align to the minimum or the maximum border.</param>
        public static long ChunkAlign(long coordinate, bool alignToMin = true)
        {
            return alignToMin ? PAUtils.FloorRound(coordinate, PAConstants.CHUNK_SIZE) : PAUtils.CeilRound(coordinate, PAConstants.CHUNK_SIZE);
        }
    }
}
