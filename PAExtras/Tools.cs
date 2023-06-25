using ProgressAdventure;
using PAConstants = ProgressAdventure.Constants;

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
    }
}
