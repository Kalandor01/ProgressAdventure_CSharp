using System.Diagnostics.CodeAnalysis;
using PACommon.Enums;

namespace PACommon
{
    /// <summary>
    /// Abstract class containing utils to localize text.
    /// </summary>
    /// <typeparam name="TL">The <see cref="AdvancedEnum{TSelf}"/> type of the list of posible languages.</typeparam>
    /// <typeparam name="TT">The <see cref="AdvancedEnum{TSelf}"/> type of the list of posible texts.</typeparam>
    public abstract class ALocalizer<TL, TT>
        where TL : AdvancedEnum<TL>
        where TT : AdvancedEnum<TT>
    {
        #region Protected properties
        /// <summary>
        /// The default language of the <see cref="ALocalizer{TL,TT}"/>.
        /// </summary>
        protected readonly EnumValue<TL> _defaultLanguage;

        /// <summary>
        /// The currently set language.
        /// </summary>
        protected EnumValue<TL> _currentLanguage;
        
        /// <summary>
        /// The dictionary containing all localizations for all languages.
        /// </summary>
        protected abstract Dictionary<EnumValue<TL>, Dictionary<EnumValue<TT>, string>> LocalizationDictionary { get; set; }
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_currentLanguage"/>
        /// </summary>
        public EnumValue<TL> CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (!LocalizationDictionary.ContainsKey(value))
                {
                    throw new ArgumentException($"Language enum value \"{value}\" doesn't exist in the localization dictionary", nameof(CurrentLanguage));
                }
                _currentLanguage = value;
            }
        }
        #endregion
        
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="ALocalizer{TL,TT}" path="//summary"/>
        /// </summary>
        /// <param name="defaultLanguage"><inheritdoc cref="_defaultLanguage" path="//summary"/></param>
        public ALocalizer(EnumValue<TL> defaultLanguage)
        {
            _defaultLanguage = defaultLanguage;
            _currentLanguage = defaultLanguage;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Gets the list of currently available languages.
        /// </summary>
        public List<EnumValue<TL>> GetAvailableLanguages()
        {
            return LocalizationDictionary.Keys.ToList();
        }
        
        /// <summary>
        /// Gets the localized string in the currently set language, (or the default language) and returns the success.
        /// </summary>
        /// <returns>If the text existed in the current (or default) language.</returns>
        public bool TryGetLocalizedString(EnumValue<TT> textKey, [NotNullWhen(true)] out string? localizedString, bool onlyTryCurrentLanguage = false)
        {
            localizedString = null;
            return (
                       LocalizationDictionary.TryGetValue(CurrentLanguage, out var languageTexts) &&
                       languageTexts.TryGetValue(textKey, out localizedString)
                   ) ||
                   (
                       !onlyTryCurrentLanguage &&
                       LocalizationDictionary.TryGetValue(_defaultLanguage, out var defLanguageTexts) &&
                       defLanguageTexts.TryGetValue(textKey, out localizedString)
                   );
        }
        
        /// <summary>
        /// Gets the localized string in the currently set language, or the default language.
        /// </summary>
        public string GetLocalizedString(EnumValue<TT> textKey)
        {
            return TryGetLocalizedString(textKey, out var localizedStr)
                ? localizedStr
                : $"[{textKey}]";
        }
        
        /// <inheritdoc cref="GetLocalizedString(EnumValue{TT})"/>
        public string GetLocalizedString(EnumValue<TT> textKey, object arg1)
        {
            return GetLocalizedStringPrivate(textKey, locStr => string.Format(locStr, arg1), () => [arg1]);
        }
        
        /// <inheritdoc cref="GetLocalizedString(EnumValue{TT})"/>
        public string GetLocalizedString(EnumValue<TT> textKey, object arg1, object arg2)
        {
            return GetLocalizedStringPrivate(textKey, locStr => string.Format(locStr, arg1, arg2), () => [arg1, arg2]);
        }
        
        /// <inheritdoc cref="GetLocalizedString(EnumValue{TT})"/>
        public string GetLocalizedString(EnumValue<TT> textKey, object arg1, object arg2, object arg3)
        {
            return GetLocalizedStringPrivate(textKey, locStr => string.Format(locStr, arg1, arg2, arg3), () => [arg1, arg2, arg3]);
        }
        #endregion

        #region Private methods
        /// <inheritdoc cref="GetLocalizedString(EnumValue{TT})"/>
        /// <param name="formater">The formater method to format the localized text.</param>
        /// <param name="argsGetter">Gets the args passed into the formater.</param>
        private string GetLocalizedStringPrivate(EnumValue<TT> textKey, Func<string, string> formater, Func<object[]> argsGetter)
        {
            if (TryGetLocalizedString(textKey, out var localizedStr))
            {
                try
                {
                    return formater(localizedStr);
                }
                catch (FormatException fe)
                {
                    PACSingletons.Instance.Logger.Log(
                        "Localized string args number missmatch",
                        $"the resulting localized string at \"{textKey}\" key in the \"{CurrentLanguage}\" (or \"{_defaultLanguage}\") language has less arguments than expected: {argsGetter().Length}",
                        LogSeverity.ERROR
                    );
                }
            }
            
            return $"[[{textKey}] + \"{string.Join("\" + \"", argsGetter())}\"]";
        }
        #endregion
    }
}