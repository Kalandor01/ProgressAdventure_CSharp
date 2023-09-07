using Microsoft.Win32;
using PACommon;
using PACommon.Extensions;
using ProgressAdventure;
using ProgressAdventure.WorldManagement.Content;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using PAConstants = ProgressAdventure.Constants;
using PATools = PACommon.Tools;

namespace PAVisualizer
{
    public static class VisualizerTools
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
        public static string GetDisplayGeneralSaveData()
        {
            var txt = new StringBuilder();
            txt.AppendLine($"Save name: {SaveData.saveName}");
            txt.AppendLine($"Display save name: {SaveData.displaySaveName}");
            txt.AppendLine($"Last saved: {Utils.MakeDate(SaveData.LastSave, ".")} {Utils.MakeTime(SaveData.LastSave)}");
            txt.AppendLine($"\nPlayer:\n{SaveData.player}");
            txt.AppendLine($"\nMain seed: {PATools.SerializeRandom(RandomStates.MainRandom)}");
            txt.AppendLine($"World seed: {PATools.SerializeRandom(RandomStates.WorldRandom)}");
            txt.AppendLine($"Misc seed: {PATools.SerializeRandom(RandomStates.MiscRandom)}");
            txt.AppendLine($"Chunk seed modifier: {PATools.SerializeRandom(RandomStates.WorldRandom)}");
            txt.Append($"\nTile type noise seeds:\n{string.Join("\n", RandomStates.TileTypeNoiseSeeds.Select(ttns => $"{ttns.Key.ToString().Capitalize()} seed: {ttns.Value}"))}");
            return txt.ToString();
        }

        public static string GetDisplayTileCountsData(Dictionary<WorldLayer, Dictionary<ContentTypeID, long>> tileTypeCounts)
        {
            var txt = new StringBuilder();
            foreach (var layer in tileTypeCounts)
            {
                var total = 0L;
                txt.AppendLine($"{layer.Key} tile types:");
                foreach (var type in layer.Value)
                {
                    txt.AppendLine($"\t{type.Key.ToString()?.Split(".").Last()}: {type.Value}");
                    total += type.Value;
                }
                txt.AppendLine($"\tTOTAL: {total}\n");
            }
            return txt.ToString();
        }

        public static (string saveFolderName, string saveFolderPath)? GetSaveFolderFromFileDialog(Window window)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = PAConstants.SAVES_FOLDER_PATH,
            };

            if (openFileDialog.ShowDialog(window) != true)
            {
                return null;
            }

            var saveFolderDataPath = openFileDialog.FileName;
            if (Path.GetExtension(saveFolderDataPath) != "." + PAConstants.SAVE_EXT)
            {
                return null;
            }

            var saveFolderPath = Path.GetDirectoryName(saveFolderDataPath);
            if (saveFolderPath is null)
            {
                return null;
            }

            var savesFolderPath = Directory.GetParent(saveFolderPath);
            if (savesFolderPath is null)
            {
                return null;
            }

            var saveName = saveFolderPath.Split(Path.DirectorySeparatorChar).Last();

            return (saveName, savesFolderPath.FullName);
        }
        #endregion
    }
}
