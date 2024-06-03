using PACommon.Extensions;
using ProgressAdventure;
using ProgressAdventure.WorldManagement;
using ProgressAdventure.WorldManagement.Content;
using SaveFileManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using PAConstants = ProgressAdventure.Constants;
using PACTools = PACommon.Tools;
using Utils = PACommon.Utils;

namespace PAVisualizer
{
    public static class ConsoleVisualizer
    {
        #region Public function
        /// <summary>
        /// Genarates an image, representing the different types of tiles, and their placements in the world.
        /// </summary>
        /// <param name="layer">Sets which layer to export.</param>
        /// <param name="image">The generated image.</param>
        /// <param name="opacityMultiplier">The opacity multiplier for the tiles.</param>
        /// <returns>The tile count for all tile types.</returns>
        public static Dictionary<ContentTypeID, long> CreateWorldLayerImage(WorldLayer layer, out Bitmap image, double opacityMultiplier = 1)
        {
            (int x, int y) tileSize = (1, 1);


            var tileTypeCounts = new Dictionary<ContentTypeID, long>();

            var worldCorners = World.GetCorners();

            if (worldCorners is null)
            {
                image = new Bitmap(1, 1);
                return tileTypeCounts;
            }

            var (minX, minY, maxX, maxY) = worldCorners.Value;

            (long x, long y) size = ((maxX - minX + 1) * tileSize.x, (maxY - minY + 1) * tileSize.y);

            image = new Bitmap((int)size.x, (int)size.y);
            var drawer = Graphics.FromImage(image);

            foreach (var chunk in World.Chunks.Values)
            {
                foreach (var tile in chunk.tiles.Values)
                {
                    var x = chunk.basePosition.x + tile.relativePosition.x - minX;
                    var y = chunk.basePosition.y + tile.relativePosition.y - minY;
                    var startX = x * tileSize.x;
                    var startY = size.y - y * tileSize.y - 1;
                    // find type
                    var subtype = VisualizerTools.GetLayerSubtype(tile, layer);
                    if (tileTypeCounts.TryGetValue(subtype, out long value))
                    {
                        tileTypeCounts[subtype] = value + 1;
                    }
                    else
                    {
                        tileTypeCounts[subtype] = 1;
                    }
                    var color = VisualizerTools.GetTileColor(subtype, opacityMultiplier);

                    if (tileSize.x > 2 && tileSize.y > 2)
                    {
                        drawer.FillRectangle(new SolidBrush(color.ToDrawingColor()), startX + 10, startY + 10, tileSize.x, tileSize.y);
                    }
                    else
                    {
                        (float x, float y) actualTileSize = (tileSize.x < 2 ? 0.5f : tileSize.x, tileSize.y < 2 ? 0.5f : tileSize.y);
                        drawer.DrawRectangle(new Pen(color.ToDrawingColor()), startX, startY, actualTileSize.x, actualTileSize.y);
                    }
                }
            }
            return tileTypeCounts;
        }

        /// <summary>
        /// Creates a combined image of the world showing the provided layers.
        /// </summary>
        /// <param name="layers">The layers to show.</param>
        /// <param name="image">The created image</param>
        /// <returns>The tile count for all tile types, for each layer.</returns>
        public static Dictionary<WorldLayer, Dictionary<ContentTypeID, long>> CreateCombinedImage(List<WorldLayer> layers, out Bitmap? image)
        {
            image = null;

            var layerCounts = new Dictionary<WorldLayer, Dictionary<ContentTypeID, long>>();

            foreach (var layer in Enum.GetValues<WorldLayer>())
            {
                if (!layers.Contains(layer))
                {
                    continue;
                }

                var layerTileTypeCounts = CreateWorldLayerImage(layer, out Bitmap layerImage, VisualizerTools.GetLayerOpacity(layers, layer));
                layerCounts.Add(layer, layerTileTypeCounts);

                if (image is null)
                {
                    image = layerImage;
                    continue;
                }
                VisualizerTools.CombineImages(ref image, layerImage);
            }

            return layerCounts;
        }

        /// <summary>
        /// Creates a combined image of the world showing the provided layers, saves the image using the provided path, and displays the tile type counts for each layer.
        /// </summary>
        /// <param name="layers">The layers to use.</param>
        /// <param name="exportPath">The path to export the image to.</param>
        public static void MakeImage(List<WorldLayer> layers, string exportPath)
        {
            Console.Write("Generating image...");
            var tileTypeCounts = CreateCombinedImage(layers, out Bitmap? image);
            Console.WriteLine("DONE!");

            if (tileTypeCounts is null || image is null)
            {
                return;
            }

            Console.WriteLine(VisualizerTools.GetDisplayTileCountsData(tileTypeCounts));

            image.Save(exportPath);
        }

        /// <summary>
        /// Visualizes the data in a save file.
        /// </summary>
        /// <param name="saveName">The name of the save to read.</param>
        /// <param name="savesFolderPath">The path to the saves folder.</param>
        public static void SaveVisualizer(string saveName, string? savesFolderPath = null)
        {
            var now = DateTime.Now;
            var visualizedSaveFolderName = $"{saveName}_{Utils.MakeDate(now)}_{Utils.MakeTime(now, ";")}";
            var displayVisualizedSavePath = Path.Join(Constants.VISUALIZED_SAVES_DATA_FOLDER, visualizedSaveFolderName);
            var visualizedSavePath = Path.Join(Constants.VISUALIZED_SAVES_DATA_FOLDER_PATH, visualizedSaveFolderName);

            // load
            try
            {
                SaveManager.LoadSave(saveName, false, false, savesFolderPath);
            }
            catch (Exception e)
            {
                Utils.PressKey($"ERROR: {e}");
                return;
            }

            // display
            var txt = new StringBuilder();
            txt.AppendLine($"---------------------------------------------------------------------------------------------------------------");
            txt.AppendLine($"EXPORTED DATA FROM \"{SaveData.Instance.saveName}\"");
            txt.AppendLine($"Loaded {PAConstants.SAVE_FILE_NAME_DATA}.{PAConstants.SAVE_EXT}:");
            txt.AppendLine(VisualizerTools.GetDisplayGeneralSaveData());
            txt.Append("\n---------------------------------------------------------------------------------------------------------------");
            Utils.PressKey(txt.ToString());
            if (MenuManager.AskYesNoUIQuestion($"Do you want export the data from \"{SaveData.Instance.saveName}\" into \"{Path.Join(displayVisualizedSavePath, Constants.EXPORT_DATA_FILE)}\"?"))
            {
                PACTools.RecreateFolder(Constants.VISUALIZED_SAVES_DATA_FOLDER);
                PACTools.RecreateFolder(visualizedSaveFolderName, Constants.VISUALIZED_SAVES_DATA_FOLDER_PATH);
                File.AppendAllText(Path.Join(visualizedSavePath, Constants.EXPORT_DATA_FILE), $"{txt}\n\n");
            }


            if (!MenuManager.AskYesNoUIQuestion($"Do you want export the world data from \"{SaveData.Instance.saveName}\" into an image at \"{displayVisualizedSavePath}\"?"))
            {
                return;
            }

            // get chunks data
            PACTools.RecreateFolder(Constants.VISUALIZED_SAVES_DATA_FOLDER);
            PACTools.RecreateFolder(visualizedSaveFolderName, Constants.VISUALIZED_SAVES_DATA_FOLDER_PATH);
            World.LoadAllChunksFromFolder(showProgressText: "Getting chunk data...");

            // make rectangle
            if (MenuManager.AskYesNoUIQuestion($"Do you want to generates the rest of the chunks in a way that makes the world rectangle shaped?", false))
            {
                World.MakeRectangle(null, "Generating chunks...");
            }

            // fill
            if (MenuManager.AskYesNoUIQuestion($"Do you want to fill in ALL tiles in ALL generated chunks?", false))
            {
                World.FillAllChunks("Filling chunks...");
            }

            // select layers
            var layers = Enum.GetValues<WorldLayer>();
            var layerElements = new List<BaseUI?>();
            foreach (var layer in layers)
            {
                layerElements.Add(new Toggle(true, $"{layer.ToString().Capitalize()}: "));
            }
            layerElements.Add(null);
            layerElements.Add(new PAButton(new UIAction(GenerateImageCommand, layerElements, layers, visualizedSavePath ), text: "Generate image"));

            new OptionsUI(layerElements, "Select the layers to export the data and image from:").Display();
        }
        #endregion

        #region Pivate fields
        private static void GenerateImageCommand(List<BaseUI?> layerElements, WorldLayer[] layers, string visualizedSavePath)
        {
            // get selected layers
            var selectedLayers = new List<WorldLayer>();
            for (int x = 0; x < layers.Length; x++)
            {
                if (((layerElements[x] as Toggle)?.Value) ?? false)
                {
                    selectedLayers.Add(layers[x]);
                }
            }

            // generate image
            if (selectedLayers.Count != 0)
            {
                var imageName = string.Join("-", selectedLayers) + ".png";
                MakeImage(selectedLayers, Path.Join(visualizedSavePath, imageName));
                Utils.PressKey($"Generated image as \"{imageName}\"");
            }
        }
        #endregion
    }
}
