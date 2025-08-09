using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using ProgressAdventure;
using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using PAConstants = ProgressAdventure.Constants;
using PATools = PACommon.Tools;

namespace PAVisualizer
{
    public static class VisualizerTools
    {
        class DummyWindow : Window
        {
            public DummyWindow()
                :base()
            {

            }
        }
        #region Constants
        private static IStorageProvider _sp = new DummyWindow().StorageProvider;
        #endregion

        #region Config dictionaries
        /// <summary>
        /// Dictionary pairing up terrain types with their colors.
        /// </summary>
        public static readonly Dictionary<EnumValue<TerrainType>, ColorData> terrainTypeColorMap = new()
        {
            [TerrainType.FIELD] = Constants.Colors.DARK_GREEN,
            [TerrainType.OCEAN] = Constants.Colors.LIGHT_BLUE,
            [TerrainType.SHORE] = Constants.Colors.LIGHTER_BLUE,
            [TerrainType.MOUNTAIN] = Constants.Colors.LIGHT_GRAY,
        };

        /// <summary>
        /// Dictionary pairing up structure types with their colors.
        /// </summary>
        public static readonly Dictionary<EnumValue<StructureType>, ColorData> structureTypeColorMap = new()
        {
            [StructureType.NONE] = Constants.Colors.TRANSPARENT,
            [StructureType.VILLAGE] = Constants.Colors.LIGHT_BROWN,
            [StructureType.KINGDOM] = Constants.Colors.BROWN,
            [StructureType.BANDIT_CAMP] = Constants.Colors.RED,
        };

        /// <summary>
        /// Dictionary pairing up entity types with their colors.
        /// </summary>
        public static readonly Dictionary<EnumValue<EntityType>, ColorData> entityTypeColorMap = new()
        {
            [EntityType.PLAYER] = Constants.Colors.WHITE,
            [EntityType.CAVEMAN] = Constants.Colors.LIGHT_BROWN,
            [EntityType.GHOUL] = Constants.Colors.GRAY,
            [EntityType.TROLL] = Constants.Colors.DARK_GREEN,
            [EntityType.DRAGON] = Constants.Colors.RED,
            [EntityType.DEMON] = Constants.Colors.DARK_RED,
            [EntityType.DWARF] = Constants.Colors.BROWN,
            [EntityType.ELF] = Constants.Colors.GREEN,
            [EntityType.HUMAN] = Constants.Colors.SKIN,
        };
        #endregion

        #region Public functions
        #region File dialog
        /// <summary>
        /// A function to return, if the path of the file/folder, that the user selected is valid.
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
        public static void GetPathFromFileDialog(
            Action<string?> callback,
            Window? window = null,
            string? initialDirectory = null,
            Dictionary<string, List<string>>? filePickerPatterns = null,
            bool isSelectFolder = false
        )
        {
            var res = Task.Run(async () =>
            {
                var storageProvider = window?.StorageProvider ?? _sp;
                var initialUri = initialDirectory is not null ? await storageProvider.TryGetFolderFromPathAsync(initialDirectory) : null;

                if (isSelectFolder)
                {
                    var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                    {
                        AllowMultiple = false,
                        SuggestedStartLocation = initialUri,
                    });

                    if (folders.Count != 1)
                    {
                        callback(null);
                        return;
                    }

                    callback(folders[0].Path.LocalPath);
                    return;
                }

                var patterns = filePickerPatterns?.Select(p =>
                        new FilePickerFileType(p.Key)
                        {
                            Patterns = p.Value.Select(v => v).ToList().AsReadOnly()
                        }).ToList().AsReadOnly();

                var filePickerOptions = new FilePickerOpenOptions
                {
                    AllowMultiple = false,
                    SuggestedStartLocation = initialUri,
                    FileTypeFilter = patterns,
                };
                var files = await storageProvider.OpenFilePickerAsync(filePickerOptions);

                if (files.Count != 1)
                {
                    callback(null);
                    return;
                }

                callback(files[0].Path.LocalPath);
                return;
            });
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

                ShowFileDialogThread(initialDirectory, isSelectFolder);

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
            //var selectedPath = GetPathFromFileDialog(null, initialDirectory, isSelectFolder);
            //fileDialogSelectedPath = selectedPath;
            fileDialogResponseRecived = true;
        }
        #endregion

        /// <summary>
        /// Gets the color ascociated with the content subtype, or <see cref="Constants.Colors.MAGENTA"/>.
        /// </summary>
        /// <param name="contentSubtype">The content subtype.</param>
        public static ColorData GetTerrainTypeColor(EnumValue<TerrainType> contentSubtype)
        {
            return terrainTypeColorMap.TryGetValue(contentSubtype, out var cColor) ? cColor : Constants.Colors.MAGENTA;
        }

        /// <summary>
        /// Gets the color ascociated with the content subtype, or <see cref="Constants.Colors.MAGENTA"/>.
        /// </summary>
        /// <param name="contentSubtype">The content subtype.</param>
        public static ColorData GetStructureTypeColor(EnumValue<StructureType> contentSubtype)
        {
            return structureTypeColorMap.TryGetValue(contentSubtype, out var cColor) ? cColor : Constants.Colors.MAGENTA;
        }

        /// <summary>
        /// Gets the color ascociated with the entity type, or <see cref="Constants.Colors.MAGENTA"/>.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        public static ColorData GetEntityTypeColor(EnumValue<EntityType> entityType)
        {
            return entityTypeColorMap.TryGetValue(entityType, out var cColor) ? cColor : Constants.Colors.MAGENTA;
        }

        public static List<(EnumValue<EntityType> type, long amount)> GetPopulationCounts(PopulationManager populationManager)
        {
            return [.. populationManager.ContainedEntities.Select(eType => (eType, populationManager.GetEntityCount(eType, out _)))];
        }

        /// <summary>
        /// Gets the color ascociated with the entity with the highest population, or <see cref="Constants.Colors.MAGENTA"/>.
        /// </summary>
        /// <param name="populationManager">The <see cref="PopulationManager"/>.</param>
        /// <param name="blendPopulation">Whether to blend the entity type colors.</param>
        public static ColorData GetPopulationManagerColor(PopulationManager populationManager, bool blendPopulation)
        {
            if (populationManager.PopulationCount == 0)
            {
                return Constants.Colors.MAGENTA;
            }

            var entityCounts = GetPopulationCounts(populationManager);
            if (!blendPopulation)
            {
                var mostAmountType = entityCounts
                    .StableSort((n1, n2) => n1.amount > n2.amount ? -1 : (n1.amount == n2.amount ? 0 : 1))
                    .First().type;

                return GetEntityTypeColor(mostAmountType);
            }

            var minAmount = entityCounts.Min(e => e.amount);
            var maxAmount = entityCounts.Max(e => e.amount);

            var sumAmount = (double)entityCounts.Sum(e => e.amount);
            ColorData? sumColor = null;
            foreach (var (type, amount) in entityCounts)
            {
                var color = GetEntityTypeColor(type);
                var percent = amount / sumAmount;
                var newColor = color.MultiplyOpacity(percent);
                if (sumColor is null)
                {
                    sumColor = newColor;
                }
                else
                {
                    sumColor = sumColor.Value.Blend(newColor);
                }
            }
            return (ColorData)sumColor!;
        }

        /// <summary>
        /// Gets the color ascociated with a layer of a tile, or <see cref="Constants.Colors.MAGENTA"/>.
        /// </summary>
        /// <param name="tile">The tile to get the color from.</param>
        /// <param name="layer">The layer to get the color from.</param>
        /// <param name="blendPopulation">Whether to blend the population layer colors.</param>
        public static ColorData GetLayerContentColor(Tile tile, VisibleTileLayer layer, bool blendPopulation)
        {
            return layer switch
            {
                VisibleTileLayer.Terrain => GetTerrainTypeColor(tile.terrain.type),
                VisibleTileLayer.Structure => GetStructureTypeColor(tile.structure.type),
                VisibleTileLayer.Population => GetPopulationManagerColor(tile.populationManager, blendPopulation),
                _ => Constants.Colors.MAGENTA
            };
        }

        /// <summary>
        /// Returns a string, displaying the general data from the loaded save file.
        /// </summary>
        public static string GetDisplayGeneralSaveData()
        {
            var txt = new StringBuilder();
            txt.AppendLine($"Save name: {SaveData.Instance.SaveName}");
            txt.AppendLine($"Display save name: {SaveData.Instance.DisplaySaveName}");
            txt.AppendLine($"Last saved: {Utils.MakeDate(SaveData.Instance.LastSave, ".")} {Utils.MakeTime(SaveData.Instance.LastSave)}");
            txt.AppendLine($"\nPlayer:\n{SaveData.Instance.PlayerRef}");
            txt.AppendLine($"\nMain seed: {PATools.SerializeRandom(RandomStates.Instance.MainRandom)}");
            txt.AppendLine($"World seed: {PATools.SerializeRandom(RandomStates.Instance.WorldRandom)}");
            txt.AppendLine($"Misc seed: {PATools.SerializeRandom(RandomStates.Instance.MiscRandom)}");
            txt.AppendLine($"Chunk seed modifier: {PATools.SerializeRandom(RandomStates.Instance.WorldRandom)}");
            txt.Append($"\nTile type noise seeds:\n{string.Join("\n", RandomStates.Instance.TileTypeNoiseSeeds.Select(ttns => $"{ttns.Key.ToString().Capitalize()} seed: {ttns.Value}"))}");
            return txt.ToString();
        }

        /// <summary>
        /// Returns a string, displaying the tile terrain types, and their counts.
        /// </summary>
        /// <param name="terrainTypeCounts">The dictionary containing the tile terrain type counts for each layer.</param>
        public static string GetDisplayTerrainCountsData(Dictionary<EnumValue<TerrainType>, long> terrainTypeCounts)
        {
            var txt = new StringBuilder();
            var total = 0L;
            txt.AppendLine($"Terrain content types:");
            foreach (var terrainTypeCount in terrainTypeCounts)
            {
                txt.AppendLine($"\t{terrainTypeCount.Key}: {terrainTypeCount.Value}");
                total += terrainTypeCount.Value;
            }
            txt.AppendLine($"\tTOTAL: {total}\n");
            return txt.ToString();
        }

        /// <summary>
        /// Returns a string, displaying the tile structure types, and their counts.
        /// </summary>
        /// <param name="structureTypeCounts">The dictionary containing the tile structure type counts for each layer.</param>
        public static string GetDisplayStructureCountsData(Dictionary<EnumValue<StructureType>, long> structureTypeCounts)
        {
            var txt = new StringBuilder();
            var total = 0L;
            txt.AppendLine($"Structure content types:");
            foreach (var structureTypeCount in structureTypeCounts)
            {
                txt.AppendLine($"\t{structureTypeCount.Key}: {structureTypeCount.Value}");
                total += structureTypeCount.Value;
            }
            txt.AppendLine($"\tTOTAL: {total}\n");
            return txt.ToString();
        }

        /// <summary>
        /// Returns a string, displaying the entities, and their counts.
        /// </summary>
        /// <param name="entityTypeCounts">The dictionary containing the entity type counts.</param>
        public static string GetDisplayPopulationCountsData(Dictionary<EnumValue<EntityType>, long> entityTypeCounts)
        {
            var txt = new StringBuilder();
            var total = 0L;
            txt.AppendLine($"Entity types:");
            foreach (var entityTypeCount in entityTypeCounts)
            {
                txt.AppendLine($"\t{entityTypeCount.Key}: {entityTypeCount.Value}");
                total += entityTypeCount.Value;
            }
            txt.AppendLine($"\tTOTAL: {total}\n");
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
                (
                    !File.Exists(Path.Join(folderPath, $"{PAConstants.SAVE_FILE_NAME_DATA}.{PAConstants.SAVE_EXT}")) &&
                    !File.Exists(Path.Join(folderPath, $"{PAConstants.SAVE_FILE_NAME_DATA}.{PAConstants.OLD_SAVE_EXT}"))
                )
            )
            {
                return null;
            }

            folderPath = folderPath.TrimEnd(Path.DirectorySeparatorChar);

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
        public static double GetLayerOpacity(List<VisibleTileLayer> layers, VisibleTileLayer currentLayer)
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
        /// Adds an image to another image.
        /// </summary>
        /// <param name="image">The first image.</param>
        /// <param name="otherImage">The image to add to the first image.</param>
        /// <param name="opacity">The opacity of the second image.</param>
        public static void CombineImages(ref Image image, Image otherImage, double opacity)
        {
            image.Mutate(x => x.DrawImage(otherImage, (float)opacity));
        }
        #endregion
    }
}
