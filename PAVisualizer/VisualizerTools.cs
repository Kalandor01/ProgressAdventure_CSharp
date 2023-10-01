using Microsoft.Win32;
using PACommon;
using PACommon.Extensions;
using ProgressAdventure;
using ProgressAdventure.WorldManagement;
using ProgressAdventure.WorldManagement.Content;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        /// <summary>
        /// Returns a string, displaying the general data from the loaded save file.
        /// </summary>
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

        /// <summary>
        /// Returns a string, displaying the tile types, and their counts.
        /// </summary>
        /// <param name="tileTypeCounts">The dictionary containing the tile subtype counts for each layer.</param>
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

        /// <summary>
        /// Opens a file dialog, in the saves folder, and if the user selected a file ending in the save extension, it returns the name of the file, and the path of the folder containing it.
        /// </summary>
        /// <param name="window">The window to center the dialog over.</param>
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

        /// <summary>
        /// Returns the opacity the current layer should have, in a way, that the higher the layer, the more transparrent.
        /// </summary>
        /// <param name="layers">The list of used layers.</param>
        /// <param name="currentLayer">The current layer to get the opacity for.</param>
        public static double GetLayerOpacity(List<WorldLayer> layers, WorldLayer currentLayer)
        {
            if (layers.Count < 2)
            {
                return 1;
            }

            var layerIndex = layers.IndexOf(currentLayer);
            if (layerIndex == -1)
            {
                return 1;
            }

            return 1.0 / (layerIndex + 1);
        }

        /// <summary>
        /// Returns the subtype of the selected layer in the tile.
        /// </summary>
        /// <param name="tile">The selected tile.</param>
        /// <param name="layer">The selected layer.</param>
        public static ContentTypeID GetLayerSubtype(Tile tile, WorldLayer layer)
        {
            if (layer == WorldLayer.Structure)
            {
                return tile.structure.subtype;
            }
            if (layer == WorldLayer.Population)
            {
                return tile.population.subtype;
            }
            return tile.terrain.subtype;
        }

        /// <summary>
        /// Returns the color associated to the subtype.
        /// </summary>
        /// <param name="subtype">The subtype.</param>
        /// <param name="opacityMultiplier">The number to multiply the opacity of the color by.</param>
        public static ColorData GetTileColor(ContentTypeID subtype, double opacityMultiplier = 1)
        {
            ColorData rawColor = contentSubtypeColorMap.TryGetValue(subtype, out ColorData rColor) ? rColor : Constants.Colors.MAGENTA;
            return new ColorData(rawColor.R, rawColor.G, rawColor.B, (byte)Math.Clamp(rawColor.A * opacityMultiplier, 0, 255));
        }

        /// <summary>
        /// Adds an image to another image.
        /// </summary>
        /// <param name="image">The first image.</param>
        /// <param name="otherImage">The image to add to the first image.</param>
        public static void CombineImages(ref Bitmap image, Bitmap otherImage)
        {
            var graphics = Graphics.FromImage(image);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(otherImage, 0, 0);
        }
        #endregion
    }
}
