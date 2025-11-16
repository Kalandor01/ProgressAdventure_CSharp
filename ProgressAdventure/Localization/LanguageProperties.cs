using System.Text.Json.Serialization;

namespace ProgressAdventure.Localization
{
    /// <summary>
    /// Class for storing the properties for a language.
    /// </summary>
    public class LanguageProperties
    {
        /// <summary>
        /// The name of the language in the default language.
        /// </summary>
        [JsonPropertyName("default_name")]
        public readonly string defaultName;
        
        /// <summary>
        /// <inheritdoc cref="LanguageProperties"/>
        /// </summary>
        /// <param name="defaultName"><inheritdoc cref="defaultName"/></param>
        [JsonConstructor]
        public LanguageProperties(string defaultName)
        {
            this.defaultName = defaultName;
        }
    }
}