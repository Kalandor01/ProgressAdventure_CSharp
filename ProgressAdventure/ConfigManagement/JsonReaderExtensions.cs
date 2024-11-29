using System.Text.Json;

namespace ProgressAdventure.ConfigManagement
{
    internal static class JsonReaderExtensions
    {
        public delegate T ArrayValueConverterDelegate<T>(ref Utf8JsonReader reader, JsonSerializerOptions options);
        public delegate object? ObjectValueConverterDelegate(ref Utf8JsonReader reader, string key, JsonSerializerOptions options);

        /// <summary>
        /// Parses the next json object into a dictionary of strings.
        /// </summary>
        /// <param name="reader">The json reader.</param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        internal static Dictionary<string, string?> GetObjectDictionary(this ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var dictionary = new Dictionary<string, string?>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var key = reader.GetString() ?? throw new JsonException();
                reader.Read();
                var value = reader.GetString();

                dictionary.Add(key, value);
            }

            throw new JsonException();
        }

        /// <summary>
        /// Parses the next json object into a dictionary.
        /// </summary>
        /// <param name="reader">The json reader.</param>
        /// <param name="converterFunction">The value converter to use.</param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        internal static Dictionary<string, object?> GetObjectDictionary(
            this ref Utf8JsonReader reader,
            ObjectValueConverterDelegate converterFunction,
            JsonSerializerOptions options
        )
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var dictionary = new Dictionary<string, object?>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var key = reader.GetString() ?? throw new JsonException();
                reader.Read();
                var value = converterFunction(ref reader, key, options);

                dictionary.Add(key, value);
            }

            throw new JsonException();
        }

        /// <summary>
        /// Parses the next json array into a dictionary.
        /// </summary>
        /// <param name="reader">The json reader.</param>
        /// <param name="converterFunction">The value converter to use.</param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        internal static List<T>GetArray<T>(
            this ref Utf8JsonReader reader,
            ArrayValueConverterDelegate<T> converterFunction,
            JsonSerializerOptions options
        )
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }

            var array = new List<T>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return array;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var value = converterFunction(ref reader, options);
                array.Add(value);
            }

            throw new JsonException();
        }
    }
}
