using Newtonsoft.Json;

namespace Project_Adventure_C_sharp
{
    public class Tools
    {
        //const string ROOT_FOLDER = Directory.GetCurrentDirectory();

        const int SAVE_SEED = 87531;
        const string SAVE_EXT = "sav";
        const string ENCODING = "windows-1250";

        /// <summary>
        /// Shorthand for `encode_save` + convert from json to string.
        /// </summary>
        public static void encode_save_s(object data, string file_path, int seed = SAVE_SEED, string extension = SAVE_EXT)
        {
            // convert from json to string
            var json_data = JsonConvert.SerializeObject(data);

            encode_save(json_data, seed, file_path, extension, ENCODING);
        }

        /// <summary>
        /// Encodes the data into a save file.
        /// </summary>
        public static void encode_save(string json_data, int seed, string file_path, string extension, string ENCODING)
        {
            var path = file_path + "." + extension;
            using (StreamWriter f = File.CreateText(path))
            {
                f.WriteLine(json_data);
            }
        }

        /// <summary>
        /// Shorthand for `decode_save` + convert from string to json.
        /// `line_num` is the line, that you want go get back (starting from 0).
        /// </summary>
        public static object decode_save_s(string file_path, int line_num = 0, int seed = SAVE_SEED, string extension = SAVE_EXT)
        {
            var decoded_lines = "";
            try
            {
                decoded_lines = decode_save(seed, file_path, extension, ENCODING, line_num + 1);
            }
            //catch ()
            //{
            //    var safe_file_path = file_path.removeprefix(ROOT_FOLDER);
            //    //logger("Decode error", f"file name: {safe_file_path}.{SAVE_EXT}", Log_type.ERROR)
            //    throw;
            //}
            catch (FileNotFoundException e)
            {
                //var safe_file_path = file_path.removeprefix(ROOT_FOLDER);
                //logger("File not found", f"file name: {safe_file_path}.{SAVE_EXT}", Log_type.ERROR)
                throw;
            }
            return JsonConvert.DeserializeObject(decoded_lines);
        }

        /// <summary>
        /// Decodes the save file into data.
        /// </summary>
        public static string decode_save(int save_num, string file_path, string extension, string encoding = ENCODING, int decode_until = -1)
        {
            var path = file_path + "." + extension;
            var data = "";
            using (StreamReader sr = File.OpenText(path))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    data += s;
                }
            }
            return data;
        }
    }
}