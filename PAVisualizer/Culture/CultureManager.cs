using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PAVisualizer.Culture
{
    internal class CultureManager
    {
        enum Culture
        {
            EN,
            HUN,
        }

        private static readonly Dictionary<CultureKey, string> _cultureStringsEN = new()
        {
            [CultureKey.False] = "False",
            [CultureKey.True] = "True",

            [CultureKey.MainWindowTitle] = "Save world viewer",
            [CultureKey.SaveInfoWindowTitle] = "",
            [CultureKey.WorldInfoWindowTitle] = "",
            [CultureKey.TileInfoWindowTitle] = "",

            [CultureKey.FileHeader] = "File",
            [CultureKey.SelectMenuItemHeader] = "Select",
            [CultureKey.CloseMenuItemHeader] = "Close",
            [CultureKey.CreateImageMenuItemHeader] = "Create image based on seleced layers",
            [CultureKey.ShowSaveInfoMenuItemHeader] = "Save info",
            [CultureKey.ShowWorldInfoMenuItemHeader] = "World info",
            [CultureKey.CreateSaveMenuItemHeader] = "Save",

            [CultureKey.RevalAreaButtonText] = "Reveal visible area",
            [CultureKey.ToggleLoggingText] = "Toggle logging in console: ",
            [CultureKey.BlendPopulationColors] = "Blend population colors: ",
            [CultureKey.TerrainLayerCheckBoxText] = "Terrain layer",
            [CultureKey.StructureLayerCheckBoxText] = "Structure layer",
            [CultureKey.PopulationLayerCheckBoxText] = "Population layer",
        };

        private static readonly Dictionary<CultureKey, string> _cultureStringsHUN = new()
        {
            [CultureKey.False] = "Hamis",
            [CultureKey.True] = "Igaz",

            [CultureKey.MainWindowTitle] = "Mentés világ nézegető",
        };

        private static readonly Dictionary<Culture, Dictionary<CultureKey, string>> _cultureStringsMap = new()
        {
            [Culture.EN] = _cultureStringsEN,
            [Culture.HUN] = _cultureStringsHUN,
        };

        private static Dictionary<CultureKey, string> _cultureStrings = _cultureStringsEN;

        public static bool TryGetCultureString(
            CultureKey cultureKey,
            [NotNullWhen(true)] out string? cultureString,
            bool onlyCurrentCulture = true
        )
        {
            cultureString = null;
            if (_cultureStrings.TryGetValue(cultureKey, out var cultureStringValue))
            {
                cultureString = cultureStringValue;
                return true;
            }

            if (!onlyCurrentCulture && _cultureStringsEN.TryGetValue(cultureKey, out var cultureStringEng))
            {
                cultureString = cultureStringEng;
                return true;
            }

            return false;
        }

        public static string GetCultureString(CultureKey cultureKey)
        {
            return TryGetCultureString(cultureKey, out var cultureString, false)
                ? cultureString
                : $"[{cultureKey}]";
        }

        public static void SetCultureEnglish()
        {
            SetCulture(Culture.EN);
        }

        public static void SetCultureHungarian()
        {
            SetCulture(Culture.HUN);
        }

        private static void SetCulture(Culture culture)
        {
            if (_cultureStringsMap.TryGetValue(culture, out var cultureStrings))
            {
                _cultureStrings = cultureStrings;
            }
        }
    }
}
