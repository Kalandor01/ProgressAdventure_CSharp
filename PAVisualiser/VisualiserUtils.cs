using ProgressAdventure.WorldManagement.Content;
using System.Collections.Generic;

namespace PAVisualiser
{
    public static class VisualiserUtils
    {
        #region Config dictionaries
        /// <summary>
        /// Dictionary pairing up content types with their colors.
        /// </summary>
        public static readonly Dictionary<ContentTypeID, ColorData> contentSubtypeColorMap = new()
        {

            [ContentType.Terrain.FIELD] = Constants.Colors.DARK_GREEN,
            [ContentType.Terrain.OCEAN] = Constants.Colors.LIGHT_BLUE,
            [ContentType.Terrain.SHORE] = Constants.Colors.LIGHTER_BLUE,
            [ContentType.Terrain.MOUNTAIN] = Constants.Colors.LIGHT_GRAY,

            [ContentType.Structure.NONE] = Constants.Colors.TRANSPARENT,
            [ContentType.Structure.VILLAGE] = Constants.Colors.LIGHT_BROWN,
            [ContentType.Structure.KINGDOM] = Constants.Colors.BROWN,
            [ContentType.Structure.BANDIT_CAMP] = Constants.Colors.RED,

            [ContentType.Population.NONE] = Constants.Colors.TRANSPARENT,
            [ContentType.Population.DWARF] = Constants.Colors.BROWN,
            [ContentType.Population.HUMAN] = Constants.Colors.SKIN,
            [ContentType.Population.DEMON] = Constants.Colors.DARK_RED,
            [ContentType.Population.ELF] = Constants.Colors.DARK_GREEN,
        };
        #endregion
    }
}
