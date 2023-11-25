using NPrng;
using NPrng.Generators;
using NPrng.Serializers;
using PACommon.Enums;
using PACommon.JsonUtils;
using SaveFileManager;
using System.Collections;

namespace PACommon
{
    /// <summary>
    /// Contains project specific useful functions.
    /// </summary>
    public static class Tools
    {
        #region Public functions
        #region Short
        /// <param name="data">The list of data to write to the file, where each element of the list is a line.</param>
        /// <inheritdoc cref="EncodeSaveShort(IEnumerable{IDictionary}, string, long, string)"/>
        public static void EncodeSaveShort(IDictionary data, string filePath, long seed, string extension)
        {
            EncodeSaveShort(new List<IDictionary> { data }, filePath, seed, extension);
        }

        /// <summary>
        /// Shorthand for <c>EncodeFile</c> + convert from json to string.
        /// </summary>
        /// <param name="dataList">The data to write to the file.</param>
        /// <param name="filePath">The path and the name of the file without the extension, that will be created.<br/>
        /// If the path contains a *, it will be replaced with the seed.</param>
        /// <param name="seed">The seed for encoding the file.</param>
        /// <param name="extension">The extension of the file that will be created.</param>
        public static void EncodeSaveShort(IEnumerable<IDictionary> dataList, string filePath, long seed, string extension)
        {
            var JsonDataList = dataList.Select(JsonSerializer.SerializeJson);
            FileConversion.EncodeFile(JsonDataList, seed, filePath, extension, Constants.FILE_ENCODING_VERSION, Constants.ENCODING);
        }

        /// <summary>
        /// Shorthand for <c>DecodeFile</c> + convert from string to json.
        /// </summary>
        /// <param name="filePath">The path and the name of the file without the extension, that will be decoded.<br/>
        /// If the path contains a *, it will be replaced with the seed.</param>
        /// <param name="lineNum">The line, that you want go get back (starting from 0).</param>
        /// <param name="seed">The seed for decoding the file.</param>
        /// <param name="extension">The extension of the file that will be decoded.</param>
        /// <param name="expected">If the file is expected to exist.<br/>
        /// ONLY ALTERS THE LOGS DISPLAYED, IF THE FILE/FOLDER DOESN'T EXIST.</param>
        /// <exception cref="FormatException">Exeption thrown, if the file couldn't be decode.</exception>
        /// <exception cref="FileNotFoundException">Exeption thrown, if the file couldn't be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Exeption thrown, if the directory containing the file couldn't be found.</exception>
        public static Dictionary<string, object?>? DecodeSaveShort(string filePath, long seed, string extension, int lineNum = 0, bool expected = true)
        {
            var safeFilePath = Path.GetRelativePath(Constants.ROOT_FOLDER, filePath);

            string decodedLine;
            try
            {
                decodedLine = FileConversion.DecodeFile((long)seed, filePath, extension, lineNum + 1, Constants.ENCODING).Last();
            }
            catch (FormatException)
            {
                PACSingletons.Instance.Logger.Log("Decode error", $"file name: {safeFilePath}.{extension}", LogSeverity.ERROR);
                throw;
            }
            catch (FileNotFoundException)
            {
                PACSingletons.Instance.Logger.Log("File not found", $"{(expected ? "" : "(but it was expected) ")}file name: {safeFilePath}.{extension}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
                throw;
            }
            catch (DirectoryNotFoundException)
            {
                PACSingletons.Instance.Logger.Log("Folder containing file not found", $"{(expected ? "" : "(but it was expected) ")}file name: {safeFilePath}.{extension}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
                throw;
            }

            try
            {
                return JsonSerializer.DeserializeJson(decodedLine);
            }
            catch (Exception)
            {
                PACSingletons.Instance.Logger.Log("Json decode error", $"file name: {safeFilePath}.{extension}", LogSeverity.ERROR);
                throw;
            }
        }
        #endregion

        #region Recreate folder
        /// <summary>
        /// Recreates the folder, if it doesn't exist.
        /// </summary>
        /// <param name="folderName">The name of the folder to check.</param>
        /// <param name="parentFolderPath">The path to the parrent folder, where the folder should be located.</param>
        /// <param name="displayName">The display name of the folder, for the logger, if it needs to be recreated.</param>
        /// <returns>If the folder needed to be recreated.</returns>
        public static bool RecreateFolder(string folderName, string? parentFolderPath = null, string? displayName = null)
        {
            parentFolderPath ??= Constants.ROOT_FOLDER;
            displayName ??= folderName.ToLower();

            var folderPath = Path.Join(parentFolderPath, folderName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                PACSingletons.Instance.Logger.Log($"Recreating {displayName} folder");
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Random
        /// <summary>
        /// Turns a Splittable random into its string representation.
        /// </summary>
        /// <param name="randomGenerator">The random generator.</param>
        public static string SerializeRandom(SplittableRandom randomGenerator)
        {
            return new SplittableRandomSerializer().WriteToString(randomGenerator);
        }

        /// <summary>
        /// Turns the string representation of a Splittable random into an object.
        /// </summary>
        /// <param name="randomString">The random generator's string representation.</param>
        public static SplittableRandom? DeserializeRandom(string? randomString)
        {
            if (randomString is null)
            {
                PACSingletons.Instance.Logger.Log("Random parse error", "random seed is null", LogSeverity.WARN);
                return null;
            }
            try
            {
                return (SplittableRandom)new SplittableRandomSerializer().ReadFromString(randomString);
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is FormatException)
                {
                    PACSingletons.Instance.Logger.Log("Random parse error", "cannot parse random generator from seed string", LogSeverity.WARN);
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Tries to turn the string representation of a Splittable random into an object, and returns the success.
        /// </summary>
        /// <param name="randomString">The random generator's string representation.</param>
        /// <param name="random">The random generator, that got deserialised.</param>
        public static bool TryDeserializeRandom(string? randomString, out SplittableRandom? random)
        {
            random = DeserializeRandom(randomString);
            return random is not null;
        }

        /// <summary>
        /// Returns a new <c>SplittableRandom</c> from another <c>SplittableRandom</c>.
        /// </summary>
        /// <param name="parrentRandom">The random generator to use, to generate the other generator.</param>
        public static SplittableRandom MakeRandomGenerator(SplittableRandom parrentRandom)
        {
            return parrentRandom.Split();
        }
        #endregion

        #region FromJson
        /// <summary>
        /// Tries to convert the json representation of the object to an object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        public static bool TryFromJson<T>(IDictionary<string, object?>? objectJson, string fileVersion, out T? convertedObject)
            where T : IJsonConvertable<T>
        {
            return T.FromJson(objectJson, fileVersion, out convertedObject);
        }

        /// <summary>
        /// Converts the json representation of the object to an object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        public static T? FromJson<T>(IDictionary<string, object?>? objectJson, string fileVersion)
            where T : IJsonConvertable<T>
        {
            T.FromJson(objectJson, fileVersion, out T? convertedObject);
            return convertedObject;
        }
        #endregion
        #endregion
    }
}