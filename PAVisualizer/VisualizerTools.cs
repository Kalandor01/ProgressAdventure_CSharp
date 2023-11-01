using Microsoft.WindowsAPICodePack.Dialogs;
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
using System.Threading;
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
        #region File dialog
        /// <summary>
        /// A function to return, if the path of the file/folder, that the user selected is valid or not.
        /// </summary>
        /// <param name="rawText">The raw user input to correct.</param>
        public delegate bool FileDialogPathValidatorDelegate(string? selectedPath);

        /// <summary>
        /// Whether the user chose a file/folder from the file dialog.
        /// </summary>
        private static bool fileDialogResponseRecived = false;

        /// <summary>
        /// The path of the file/folder that the user chose.
        /// </summary>
        private static string? fileDialogSelectedPath = null;

        /// <summary>
        /// Opens a file dialog, in the saves folder, and if the user selected a file ending in the save extension, it returns the name of the file, and the path of the folder containing it.
        /// </summary>
        /// <param name="window">The window to center the dialog over.</param>
        /// <param name="initialDirectory">The initial dialog of the file dialog.</param>
        /// <param name="isSelectFolder">Whether to make the user select a folder or a file.</param>
        public static string? GetPathFromFileDialog(
            Window? window = null,
            string? initialDirectory = null,
            bool isSelectFolder = false
        )
        {
            var fileDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = isSelectFolder,
            };

            if (initialDirectory is not null)
            {
                fileDialog.InitialDirectory = initialDirectory;
            }

            CommonFileDialogResult showDialogResponse;
            if (window is null)
            {
                showDialogResponse = fileDialog.ShowDialog();
            }
            else
            {
                showDialogResponse = fileDialog.ShowDialog(window);
            }

            return showDialogResponse == CommonFileDialogResult.Ok ? fileDialog.FileName : null;
        }

        /// <summary>
        /// Opens a file dialog menu and makes the user select a file/folder, and returns the path of the selected file/folder.
        /// </summary>
        /// <param name="fileDialogPathValidator">The function to check if the returned path is valid.</param>
        /// <param name="initialDirectory">The initial dialog of the file dialog.</param>
        /// <param name="isSelectFolder">Whether to make the user select a folder or a file.</param>
        public static string? FileDialogInConsoleMode(
            FileDialogPathValidatorDelegate? fileDialogPathValidator = null,
            string? initialDirectory = null,
            bool isSelectFolder = false
        )
        {
            do
            {
                fileDialogResponseRecived = false;
                fileDialogSelectedPath = null;

                var windowThread = new Thread(() => ShowFileDialogThread(initialDirectory, isSelectFolder));
                windowThread.SetApartmentState(ApartmentState.STA);
                windowThread.Start();

                do
                {
                    Thread.Sleep(100);
                }
                while (!fileDialogResponseRecived);
            }
            while (!(fileDialogPathValidator is null || fileDialogPathValidator(fileDialogSelectedPath)));

            return fileDialogSelectedPath;
        }

        /// <summary>
        /// Shows a file dialog from a console enviorment.
        /// </summary>
        /// <param name="initialDirectory">The initial dialog of the file dialog.</param>
        /// <param name="isSelectFolder">Whether to make the user select a folder or a file.</param>
        [STAThread]
        private static void ShowFileDialogThread(
            string? initialDirectory = null,
            bool isSelectFolder = false)
        {
            Thread.CurrentThread.Name = Constants.FILE_DIALOG_THREAD_NAME;
            fileDialogSelectedPath = GetPathFromFileDialog(null, initialDirectory, isSelectFolder);
            fileDialogResponseRecived = true;
        }
        #endregion

        /// <summary>
        /// Returns a string, displaying the general data from the loaded save file.
        /// </summary>
        public static string GetDisplayGeneralSaveData()
        {
            var txt = new StringBuilder();
            txt.AppendLine($"Save name: {SaveData.Instance.saveName}");
            txt.AppendLine($"Display save name: {SaveData.Instance.displaySaveName}");
            txt.AppendLine($"Last saved: {Utils.MakeDate(SaveData.Instance.LastSave, ".")} {Utils.MakeTime(SaveData.Instance.LastSave)}");
            txt.AppendLine($"\nPlayer:\n{SaveData.Instance.player}");
            txt.AppendLine($"\nMain seed: {PATools.SerializeRandom(RandomStates.Instance.MainRandom)}");
            txt.AppendLine($"World seed: {PATools.SerializeRandom(RandomStates.Instance.WorldRandom)}");
            txt.AppendLine($"Misc seed: {PATools.SerializeRandom(RandomStates.Instance.MiscRandom)}");
            txt.AppendLine($"Chunk seed modifier: {PATools.SerializeRandom(RandomStates.Instance.WorldRandom)}");
            txt.Append($"\nTile type noise seeds:\n{string.Join("\n", RandomStates.Instance.TileTypeNoiseSeeds.Select(ttns => $"{ttns.Key.ToString().Capitalize()} seed: {ttns.Value}"))}");
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
        /// Gets the save folder name, and save folder path from a path to a folder. If it's not correct, it returns null.
        /// </summary>
        /// <param name="folderPath">The path of the save folder.</param>
        public static (string saveFolderName, string? saveFolderPath)? GetSaveFolderFromPath(string? folderPath)
        {
            if (
                folderPath is null ||
                !File.Exists(Path.Join(folderPath, $"{PAConstants.SAVE_FILE_NAME_DATA}.{PAConstants.SAVE_EXT}"))
            )
            {
                return null;
            }

            string? saveFolderPath = null;
            var folderSplit = folderPath.Split(Path.DirectorySeparatorChar);
            var saveFolderName = folderSplit.Last();
            if (folderPath.Length > 1)
            {
                saveFolderPath = string.Join(Path.DirectorySeparatorChar, folderSplit[..^1]);
            }

            return (saveFolderName, saveFolderPath);
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
