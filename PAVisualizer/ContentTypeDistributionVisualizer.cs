using ConsoleUI;
using ConsoleUI.UIElements;
using PACommon.Enums;
using PACommon.Extensions;
using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement;
using ProgressAdventure.WorldManagement.Content;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using PACTools = PACommon.Tools;
using Utils = PACommon.Utils;

namespace PAVisualizer
{
    public class ContentTypeDistributionVisualizer
    {
        #region Public function
        /// <summary>
        /// Generates an image of the content type distribution graph using the provided noise types.
        /// </summary>
        /// <param name="layer">The layer to make an image from.</param>
        /// <param name="noiseTypeXAxis">The noise type to use for the X axis for the graph.</param>
        /// <param name="noiseTypeYAxis">The noise type to use for the Y axis for the graph.</param>
        /// <param name="resolution">The resolution of the graph.</param>
        /// <param name="opacityMultiplier">The opacity multiplier for the pixels.</param>
        /// <param name="blendMultiplePopulationColors">Whether to blend population colors if there are multiple entity types for a specific noise value.</param>
        public static Bitmap CreateNoiseTypeDistributionImage(
            VisibleTileLayer layer,
            TileNoiseType noiseTypeXAxis,
            TileNoiseType noiseTypeYAxis,
            uint resolution,
            double opacityMultiplier = 1,
            bool blendMultiplePopulationColors = true
        )
        {
            Func<Dictionary<TileNoiseType, double>, ColorData> colorGetterFunction;
            if (layer == VisibleTileLayer.Terrain)
            {
                var contentTypeMap = Utils.GetInternalPropertyFromStaticClass<Dictionary<EnumValue<TerrainType>, TerrainTypePropertiesDTO>>(typeof(WorldUtils), "TerrainTypeMap");

                colorGetterFunction = (noises) =>
                {
                    var contentType = WorldUtils.CalculateClosestTerrainType(noises);
                    var type = contentTypeMap.First().Key;
                    foreach (var contentSubtype in contentTypeMap)
                    {
                        if (contentSubtype.Value.matchingType == contentType)
                        {
                            type = contentSubtype.Key;
                        }
                    }
                    return VisualizerTools.GetTerrainTypeColor(type);
                };
            }
            else if (layer == VisibleTileLayer.Structure)
            {
                var contentTypeMap = Utils.GetInternalPropertyFromStaticClass<Dictionary<EnumValue<StructureType>, StructureTypePropertiesDTO>>(typeof(WorldUtils), "StructureTypeMap");

                colorGetterFunction = (noises) =>
                {
                    var contentType = WorldUtils.CalculateClosestStructureType(noises);
                    var type = contentTypeMap.First().Key;
                    foreach (var contentSubtype in contentTypeMap)
                    {
                        if (contentSubtype.Value.matchingType == contentType)
                        {
                            type = contentSubtype.Key;
                        }
                    }
                    return VisualizerTools.GetStructureTypeColor(type);
                };
            }
            else if (layer == VisibleTileLayer.Population)
            {
                colorGetterFunction = (noises) =>
                {
                    var diffs = WorldUtils.CalculatePopulationFitDifferences(noises);
                    if (diffs.Count == 0)
                    {
                        return Constants.Colors.TRANSPARENT;
                    }

                    if (!blendMultiplePopulationColors)
                    {
                        var minDiff = diffs.StableSort((n1, n2) => n1.Value > n2.Value ? 1 : (n1.Value == n2.Value ? 0 : -1)).First().Key;
                        return VisualizerTools.GetEntityTypeColor(minDiff);
                    }

                    var minDiffValue = diffs.Min(d => d.Value);
                    var maxDiffValue = diffs.Max(d => d.Value);
                    var diffDelta = maxDiffValue - minDiffValue;
                    var frequencys = diffs.Select(d => (d.Key, Value: maxDiffValue - d.Value + minDiffValue)).ToList();

                    var sumFreq = frequencys.Sum(d => d.Value);
                    ColorData? sumColor = null;
                    foreach (var (type, frequency) in frequencys)
                    {
                        var color = VisualizerTools.GetEntityTypeColor(type);
                        var percent = frequency / sumFreq;
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
                };
            }
            else
            {
                throw new InvalidOperationException("Invalid world layer type.");
            }

            (int x, int y) tileSize = (1, 1);

            var image = new Bitmap((int)resolution * tileSize.x, (int)resolution * tileSize.y);
            var drawer = Graphics.FromImage(image);

            var increment = 1.0 / resolution;
            var noiseValues = new Dictionary<TileNoiseType, double>
            {
                [noiseTypeXAxis] = 0,
                [noiseTypeYAxis] = 0,
            };

            for (var x = 0; x < resolution; x++)
            {
                for (var y = 0; y < resolution; y++)
                {
                    noiseValues[noiseTypeXAxis] = increment * x;
                    noiseValues[noiseTypeYAxis] = increment * y;

                    var startX = x * tileSize.x;
                    var startY = resolution - y * tileSize.y - 1;

                    WorldUtils.ShiftNoiseValues(noiseValues);
                    var color = colorGetterFunction(noiseValues).MultiplyOpacity(opacityMultiplier);

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
            return image;
        }

        /// <summary>
        /// Creates an image of the content type distribution graph using the provided noise types and saves the image using the provided path.
        /// </summary>
        /// <param name="layer">The layer to make an image from.</param>
        /// <param name="noiseTypeXAxis">The noise type to use for the X axis for the graph.</param>
        /// <param name="noiseTypeYAxis">The noise type to use for the Y axis for the graph.</param>
        /// <param name="resolution">The resolution of the graph.</param>
        /// <param name="exportPath">The path to export the image to.</param>
        /// <param name="blendMultiplePopulationColors">Whether to blend population colors if there are multiple entity types for a specific noise value.</param>
        public static void MakeImage(
            VisibleTileLayer layer,
            TileNoiseType noiseTypeXAxis,
            TileNoiseType noiseTypeYAxis,
            uint resolution,
            string exportPath,
            bool blendMultiplePopulationColors = true
        )
        {
            Console.Write("Generating image...");
            var image = CreateNoiseTypeDistributionImage(
                layer,
                noiseTypeXAxis,
                noiseTypeYAxis,
                resolution,
                blendMultiplePopulationColors: blendMultiplePopulationColors
            );
            Console.WriteLine("DONE!");
            image.Save(exportPath);
        }

        /// <summary>
        /// Creates an image of the content type distribution graph using the provided noise types and layer, and saves the image using the provided path.
        /// </summary>
        /// <param name="layer">The layer to use.</param>
        /// <param name="noiseTypeXAxis">The noise type to use for the X axis for the graph.</param>
        /// <param name="noiseTypeYAxis">The noise type to use for the Y axis for the graph.</param>
        /// <param name="resolution">The resolution of the graph.</param>
        /// <param name="exportPath">The path to export the image to.</param>
        /// <param name="blendMultiplePopulationColors">Whether to blend population colors if there are multiple entity types for a specific noise value.</param>
        public static void MakeImageForLayer(
            VisibleTileLayer layer,
            TileNoiseType noiseTypeXAxis,
            TileNoiseType noiseTypeYAxis,
            uint resolution,
            string exportPath,
            bool blendMultiplePopulationColors = true
        )
        {
            MakeImage(layer, noiseTypeXAxis, noiseTypeYAxis, resolution, exportPath, blendMultiplePopulationColors);
        }

        private static (TextFieldValidatorStatus status, string? message) TextValidatorDelegate(string inputValue)
        {
            return (
                uint.TryParse(inputValue, out uint value) && value > 0 ?
                    TextFieldValidatorStatus.VALID :
                    TextFieldValidatorStatus.RETRY,
                null
            );
        }

        private static bool KeyValidatorDelegate(StringBuilder currentValue, ConsoleKeyInfo? key, int cursorPos)
        {
            string newValue = PACTools.GetNewValueForKeyValidatorDelegate(currentValue, key, cursorPos);
            return uint.TryParse(newValue, out _);
        }

        /// <summary>
        /// Visualizes the content type distribution.
        /// </summary>
        public static void Visualize()
        {
            var now = DateTime.Now;
            var visualizedContentDistributionFolderName = $"{Utils.MakeDate(now)}_{Utils.MakeTime(now, ";")}";
            var visualizedContentDistributionPath = Path.Join(Constants.VISUALIZED_CONTENT_DISTRIBUTION_DATA_FOLDER_PATH, visualizedContentDistributionFolderName);
            PACTools.RecreateFolder(visualizedContentDistributionPath);

            // select layers
            var noiseTypes = Enum.GetValues<TileNoiseType>();
            var noiseTypeNames = new List<string>();
            foreach (var noiseType in noiseTypes)
            {
                noiseTypeNames.Add(noiseType.ToString().Capitalize());
            }

            var visualizeElements = new List<BaseUI?>();

            var noiseTypeXSelectionElement = new PAChoice(noiseTypeNames, 0, "X axis noise type: ");
            visualizeElements.Add(noiseTypeXSelectionElement);

            var noiseTypeYSelectionElement = new PAChoice(noiseTypeNames, 0, "Y axis noise type: ");
            visualizeElements.Add(noiseTypeYSelectionElement);
            visualizeElements.Add(null);

            var layerTypes = Enum.GetValues<VisibleTileLayer>();
            var layerSelectionElement = new PAChoice([.. layerTypes.Select(layer => layer.ToString().Capitalize())], 0, "Layer: ");
            visualizeElements.Add(layerSelectionElement);
            visualizeElements.Add(null);

            var defResolution = 100;
            var resolutionElement = new TextField(
                defResolution.ToString(), "Resolution: ",
                oldValueAsStartingValue: true,
                textValidatorFunction: TextValidatorDelegate,
                keyValidatorFunction: KeyValidatorDelegate,
                overrideDefaultKeyValidatorFunction: false
            );
            visualizeElements.Add(resolutionElement);

            var blendPopColorsElement = new Toggle(
                true,
                "Blend colors if there are multiple entity types: ",
                "Yes", "No"
            );
            visualizeElements.Add(blendPopColorsElement);
            visualizeElements.Add(null);

            var generateImageButtonElement = new PAButton(
                UIAction.Create(
                    GenerateImageCommand,
                        layerSelectionElement,
                        layerTypes,
                        noiseTypeXSelectionElement,
                        noiseTypeYSelectionElement,
                        noiseTypes,
                        resolutionElement,
                        visualizedContentDistributionPath,
                        blendPopColorsElement
                ),
                text: "Generate image"
            );
            visualizeElements.Add(generateImageButtonElement);

            var generateAllImagesButtonElement = new PAButton(
                UIAction.Create(
                    GenerateAllImagesCommand,
                    layerTypes,
                    noiseTypes,
                    resolutionElement,
                    visualizedContentDistributionPath,
                    blendPopColorsElement
                ),
                text: "Generate ALL possible images"
            );
            visualizeElements.Add(generateAllImagesButtonElement);

            new OptionsUI(visualizeElements, "Select the noise types to generate the distribution image from:").Display();
        }
        #endregion

        #region Pivate fields
        private static void GenerateImageCommand(
            PAChoice layerSelectionElement,
            VisibleTileLayer[] layers,
            PAChoice noiseTypeXSelectionElement,
            PAChoice noiseTypeYSelectionElement,
            TileNoiseType[] noiseTypes,
            TextField resolutionElement,
            string visualizedContentDistributionPath,
            Toggle blendMultiplePopulationColorsElement
        )
        {
            // get selected noise types
            var noiseTypeXAxis = noiseTypes[noiseTypeXSelectionElement.Value];
            var noiseTypeYAxis = noiseTypes[noiseTypeYSelectionElement.Value];
            var resolution = uint.Parse(resolutionElement.Value);
            var blendMultiplePopulationColors = blendMultiplePopulationColorsElement.Value;

            // get selected layer
            var layer = layers[layerSelectionElement.Value];

            // generate image
            var imageName = $"{noiseTypeXAxis}-{noiseTypeYAxis}.png";
            MakeImageForLayer(
                layer,
                noiseTypeXAxis,
                noiseTypeYAxis,
                resolution,
                Path.Join(visualizedContentDistributionPath, imageName),
                blendMultiplePopulationColors
            );
            Utils.PressKey($"Generated image as \"{imageName}\"");
        }

        private static void GenerateAllImagesCommand(
            VisibleTileLayer[] layers,
            TileNoiseType[] noiseTypes,
            TextField resolutionElement,
            string visualizedContentDistributionPath,
            Toggle blendMultiplePopulationColorsElement
        )
        {
            var resolution = uint.Parse(resolutionElement.Value);
            var blendMultiplePopulationColors = blendMultiplePopulationColorsElement.Value;

            // generate images
            foreach (var layer in layers)
            {
                foreach (var noiseTypeXAxis in noiseTypes)
                {
                    foreach (var noiseTypeYAxis in noiseTypes)
                    {
                        var imageName = $"{layer}-{noiseTypeXAxis}-{noiseTypeYAxis}.png";
                        MakeImageForLayer(
                            layer,
                            noiseTypeXAxis,
                            noiseTypeYAxis,
                            resolution,
                            Path.Join(visualizedContentDistributionPath, imageName),
                            blendMultiplePopulationColors
                        );
                        Console.WriteLine($"Generated image as \"{imageName}\"");
                    }
                }
            }
        }
        #endregion
    }
}
