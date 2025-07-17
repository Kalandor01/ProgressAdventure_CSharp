using ConsoleUI;
using ConsoleUI.UIElements;
using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using ProgressAdventure;
using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PAConstants = ProgressAdventure.Constants;
using PACTools = PACommon.Tools;
using Path = System.IO.Path;
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
        /// <param name="blendPopulation">Whether to blend the population layer colors.</param>
        /// <param name="image">The generated image.</param>
        /// <param name="opacityMultiplier">The opacity multiplier for the tiles.</param>
        /// <returns>The tile count for all tile types.</returns>
        public static (
                Dictionary<EnumValue<TerrainType>, long> terrainTypeCounts,
                Dictionary<EnumValue<StructureType>, long> structureTypeCounts,
                Dictionary<EnumValue<EntityType>, long> enttyTypeCounts
            ) CreateWorldLayerImage(
            VisibleTileLayer layer,
            bool blendPopulation,
            out Image image,
            double opacityMultiplier = 1
        )
        {
            (int x, int y) tileSize = (1, 1);

            var terrainTypeCounts = new Dictionary<EnumValue<TerrainType>, long>();
            var structureTypeCounts = new Dictionary<EnumValue<StructureType>, long>();
            var entityTypeCounts = new Dictionary<EnumValue<EntityType>, long>();

            var worldCorners = World.GetCorners();

            if (worldCorners is null)
            {
                image = new Image<Rgba32>(1, 1);
                return (terrainTypeCounts, structureTypeCounts, entityTypeCounts);
            }

            var (minX, minY, maxX, maxY) = worldCorners.Value;

            (long x, long y) size = ((maxX - minX + 1) * tileSize.x, (maxY - minY + 1) * tileSize.y);

            image = new Image<Rgba32>((int)size.x, (int)size.y);
            foreach (var chunk in World.Chunks.Values)
            {
                foreach (var tile in chunk.tiles.Values)
                {
                    var x = chunk.basePosition.x + tile.relativePosition.x - minX;
                    var y = chunk.basePosition.y + tile.relativePosition.y - minY;
                    var startX = x * tileSize.x;
                    var startY = size.y - y * tileSize.y - 1;
                    // find type
                    if (layer == VisibleTileLayer.Terrain)
                    {
                        var type = tile.terrain.type;
                        if (terrainTypeCounts.TryGetValue(type, out long value))
                        {
                            terrainTypeCounts[type] = ++value;
                        }
                        else
                        {
                            terrainTypeCounts[type] = 1;
                        }
                    }
                    else if (layer == VisibleTileLayer.Structure)
                    {
                        var type = tile.structure.type;
                        if (structureTypeCounts.TryGetValue(type, out long value))
                        {
                            structureTypeCounts[type] = ++value;
                        }
                        else
                        {
                            structureTypeCounts[type] = 1;
                        }
                    }
                    else if (layer == VisibleTileLayer.Population)
                    {
                        foreach (var (type, amount) in VisualizerTools.GetPopulationCounts(tile.populationManager))
                        {
                            if (entityTypeCounts.ContainsKey(type))
                            {
                                entityTypeCounts[type] += amount;
                            }
                            else
                            {
                                entityTypeCounts[type] = amount;
                            }
                        }
                    }
                    var color = VisualizerTools.GetLayerContentColor(tile, layer, blendPopulation).MultiplyOpacity(opacityMultiplier);

                    RectangularPolygon rectangle;
                    if (tileSize.x > 2 && tileSize.y > 2)
                    {
                        rectangle = new RectangularPolygon(startX + 10, startY + 10, tileSize.x, tileSize.y);
                    }
                    else
                    {
                        var actualTileSizeX = tileSize.x < 2 ? 0.5f : tileSize.x;
                        var actualTileSizeY = tileSize.y < 2 ? 0.5f : tileSize.y;
                        rectangle = new RectangularPolygon(startX, startY, actualTileSizeX, actualTileSizeY);
                    }
                    image.Mutate(x => x.Fill(color.ToImageSharpColor(), rectangle));
                }
            }
            return (terrainTypeCounts, structureTypeCounts, entityTypeCounts);
        }

        /// <summary>
        /// Creates a combined image of the world showing the provided layers.
        /// </summary>
        /// <param name="layers">The layers to show.</param>
        /// <param name="blendPopulation">Whether to blend the population layer colors.</param>
        /// <param name="image">The created image</param>
        /// <returns>The tile count for all tile types, for each layer.</returns>
        public static (
                Dictionary<EnumValue<TerrainType>, long> terrainTypeCounts,
                Dictionary<EnumValue<StructureType>, long> structureTypeCounts,
                Dictionary<EnumValue<EntityType>, long> entityTypeCounts
            ) CreateCombinedImage(
            List<VisibleTileLayer> layers,
            bool blendPopulation,
            out Image? image
        )
        {
            image = null;

            var terrainCounts = new Dictionary<EnumValue<TerrainType>, long>();
            var structureCounts = new Dictionary<EnumValue<StructureType>, long>();
            var entityCounts = new Dictionary<EnumValue<EntityType>, long>();

            foreach (var layer in Enum.GetValues<VisibleTileLayer>())
            {
                if (!layers.Contains(layer))
                {
                    continue;
                }

                var (terrainTypeCounts, structureTypeCounts, entityTypeCounts) = CreateWorldLayerImage(
                    layer,
                    blendPopulation,
                    out var layerImage
                );

                switch (layer)
                {
                    case VisibleTileLayer.Terrain:
                        terrainCounts = terrainTypeCounts;
                        break;
                    case VisibleTileLayer.Structure:
                        structureCounts = structureTypeCounts;
                        break;
                    case VisibleTileLayer.Population:
                        entityCounts = entityTypeCounts;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid layer type");
                }

                if (image is null)
                {
                    image = layerImage;
                    continue;
                }
                VisualizerTools.CombineImages(ref image, layerImage, VisualizerTools.GetLayerOpacity(layers, layer));
            }

            return (terrainCounts, structureCounts, entityCounts);
        }

        /// <summary>
        /// Creates a combined image of the world showing the provided layers, saves the image using the provided path, and displays the tile type counts for each layer.
        /// </summary>
        /// <param name="layers">The layers to use.</param>
        /// <param name="exportPath">The path to export the image to.</param>
        /// <param name="blendPopulation">Whether to blend the population layer colors.</param>
        public static void MakeImage(List<VisibleTileLayer> layers, string exportPath, bool blendPopulation)
        {
            PACSingletons.Instance.ConsoleProxy.Write("Generating image...");
            var (terrainTypeCounts, structureTypeCounts, entityCounts) = CreateCombinedImage(layers, blendPopulation, out var image);
            PACSingletons.Instance.ConsoleProxy.WriteLine("DONE!");

            if (terrainTypeCounts is null || structureTypeCounts is null || entityCounts is null || image is null)
            {
                return;
            }

            PACSingletons.Instance.ConsoleProxy.WriteLine(VisualizerTools.GetDisplayTerrainCountsData(terrainTypeCounts));
            PACSingletons.Instance.ConsoleProxy.WriteLine(VisualizerTools.GetDisplayStructureCountsData(structureTypeCounts));
            PACSingletons.Instance.ConsoleProxy.WriteLine(VisualizerTools.GetDisplayPopulationCountsData(entityCounts));

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
                SaveManager.LoadSave(saveName, false, savesFolderPath);
            }
            catch (Exception e)
            {
                PACSingletons.Instance.ConsoleProxy.PressKey($"ERROR: {e}");
                return;
            }

            // display
            var txt = new StringBuilder();
            txt.AppendLine($"---------------------------------------------------------------------------------------------------------------");
            txt.AppendLine($"EXPORTED DATA FROM \"{SaveData.Instance.SaveName}\"");
            txt.AppendLine($"Loaded {PAConstants.SAVE_FILE_NAME_DATA}.{PAConstants.SAVE_EXT}:");
            txt.AppendLine(VisualizerTools.GetDisplayGeneralSaveData());
            txt.Append("\n---------------------------------------------------------------------------------------------------------------");
            PACSingletons.Instance.ConsoleProxy.PressKey(txt.ToString());
            if (MenuManager.AskYesNoUIQuestion($"Do you want export the data from \"{SaveData.Instance.SaveName}\" into \"{Path.Join(displayVisualizedSavePath, Constants.EXPORT_DATA_FILE)}\"?"))
            {
                PACTools.RecreateFolder(Constants.VISUALIZED_SAVES_DATA_FOLDER);
                PACTools.RecreateFolder(Path.Join(Constants.VISUALIZED_SAVES_DATA_FOLDER_PATH, visualizedSaveFolderName));
                File.AppendAllText(Path.Join(visualizedSavePath, Constants.EXPORT_DATA_FILE), $"{txt}\n\n");
            }


            if (!MenuManager.AskYesNoUIQuestion($"Do you want export the world data from \"{SaveData.Instance.SaveName}\" into an image at \"{displayVisualizedSavePath}\"?"))
            {
                return;
            }

            // get chunks data
            PACTools.RecreateFolder(Constants.VISUALIZED_SAVES_DATA_FOLDER);
            PACTools.RecreateFolder(Path.Join(Constants.VISUALIZED_SAVES_DATA_FOLDER_PATH, visualizedSaveFolderName));
            World.LoadAllChunksFromFolder(out _, showProgressText: "Getting chunk data...");

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
            var layers = Enum.GetValues<VisibleTileLayer>();
            var layerElements = new List<BaseUI?>();
            foreach (var layer in layers)
            {
                layerElements.Add(new Toggle(true, $"{layer.ToString().Capitalize()}: "));
            }
            layerElements.Add(null);
            var blendPop = new Toggle(false, "Blend population colors: ", "Yes", "No");
            layerElements.Add(blendPop);
            layerElements.Add(null);
            layerElements.Add(new PAButton(UIAction.Create(
                GenerateImageCommand,
                layerElements,
                layers,
                blendPop,
                visualizedSavePath
            ), text: "Generate image"));

            new OptionsUI(
                layerElements,
                "Select the layers to export the data and image from:",
                consoleProxy: PACSingletons.Instance.ConsoleProxy
            ).Display();
        }
        #endregion

        #region Pivate fields
        private static void GenerateImageCommand(
            List<BaseUI?> layerElements,
            VisibleTileLayer[] layers,
            Toggle blendPopulationElement,
            string visualizedSavePath
        )
        {
            // get selected layers
            var selectedLayers = new List<VisibleTileLayer>();
            for (int x = 0; x < layers.Length; x++)
            {
                if (((layerElements[x] as Toggle)?.Value) ?? false)
                {
                    selectedLayers.Add(layers[x]);
                }
            }
            var blendPopulation = blendPopulationElement.Value;

            // generate image
            if (selectedLayers.Count != 0)
            {
                var imageName = string.Join("-", selectedLayers) + ".png";
                MakeImage(selectedLayers, Path.Join(visualizedSavePath, imageName), blendPopulation);
                PACSingletons.Instance.ConsoleProxy.PressKey($"Generated image as \"{imageName}\"");
            }
        }
        #endregion
    }
}
