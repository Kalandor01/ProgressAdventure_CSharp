using ProgressAdventure;
using ProgressAdventure.Extensions;
using ProgressAdventure.WorldManagement.Content;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PATools = ProgressAdventure.Tools;
using PAUtils = ProgressAdventure.Utils;

namespace PAVisualiser
{
    public static class VisualiserTools
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

        #region Public functions
        public static string GetGeneralSaveData()
        {
            var txt = new StringBuilder();
            txt.AppendLine($"Save name: {SaveData.saveName}");
            txt.AppendLine($"Display save name: {SaveData.displaySaveName}");
            txt.AppendLine($"Last saved: {PAUtils.MakeDate(SaveData.LastSave, ".")} {PAUtils.MakeTime(SaveData.LastSave)}");
            txt.AppendLine($"\nPlayer:\n{SaveData.player}");
            txt.AppendLine($"\nMain seed: {PATools.SerializeRandom(RandomStates.MainRandom)}");
            txt.AppendLine($"World seed: {PATools.SerializeRandom(RandomStates.WorldRandom)}");
            txt.AppendLine($"Misc seed: {PATools.SerializeRandom(RandomStates.MiscRandom)}");
            txt.AppendLine($"Chunk seed modifier: {PATools.SerializeRandom(RandomStates.WorldRandom)}");
            txt.Append($"\nTile type noise seeds:\n{string.Join("\n", RandomStates.TileTypeNoiseSeeds.Select(ttns => $"{ttns.Key.ToString().Capitalize()} seed: {ttns.Value}"))}");
            return txt.ToString();
        }
        #endregion
    }
}
