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
        public static Bitmap CreateNoiseTypeDistributionImage(
            VisibleTileLayer layer,
            TileNoiseType noiseTypeXAxis,
            TileNoiseType noiseTypeYAxis,
            uint resolution,
            double opacityMultiplier = 1
        )
        {
            Func<Dictionary<TileNoiseType, double>, ColorData> colorGetterFunction;
            if (layer == VisibleTileLayer.Population)
            {
                colorGetterFunction = (noises) =>
                {
                    var diffs = WorldUtils.CalculatePopulationFitDifferences(noises);
                    if (diffs.Count == 0)
                    {
                        return Constants.Colors.MAGENTA;
                    }

                    var minDiff = diffs.StableSort((n1, n2) => n1.Value > n2.Value ? 1 : (n1.Value == n2.Value ? 0 : -1)).First().Key;
                    return VisualizerTools.GetEntityTypeColor(minDiff);
                };
            }
            else
            {
                var contentTypeMap = Utils.GetInternalFieldFromStaticClass<Dictionary<Type, Dictionary<EnumTreeValue<ContentType>, Type>>>(typeof(WorldUtils), "contentTypeMap");
                var isTerrain = layer == VisibleTileLayer.Terrain;
                var contentSubtypeMap = contentTypeMap[isTerrain ? typeof(TerrainContent) : typeof(StructureContent)];

                colorGetterFunction = (noises) =>
                {
                    var contentType = isTerrain
                        ? WorldUtils.CalculateClosestContentType<TerrainContent>(noises)
                        : WorldUtils.CalculateClosestContentType<StructureContent>(noises);
                    var subtype = contentSubtypeMap.First().Key;
                    foreach (var contentSubtype in contentSubtypeMap)
                    {
                        if (contentSubtype.Value == contentType)
                        {
                            subtype = contentSubtype.Key;
                        }
                    }
                    return VisualizerTools.GetContentColor(subtype);
                };
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
        public static void MakeImage(
            VisibleTileLayer layer,
            TileNoiseType noiseTypeXAxis,
            TileNoiseType noiseTypeYAxis,
            uint resolution,
            string exportPath
        )
        {
            Console.Write("Generating image...");
            var image = CreateNoiseTypeDistributionImage(layer, noiseTypeXAxis, noiseTypeYAxis, resolution);
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
        public static void MakeImageForLayer(BaseContentType layer, TileNoiseType noiseTypeXAxis, TileNoiseType noiseTypeYAxis, uint resolution, string exportPath)
        {
            if (layer == BaseContentType.Terrain)
            {
                MakeImage(VisibleTileLayer.Terrain, noiseTypeXAxis, noiseTypeYAxis, resolution, exportPath);
            }
            else if (layer == BaseContentType.Structure)
            {
                MakeImage(VisibleTileLayer.Structure, noiseTypeXAxis, noiseTypeYAxis, resolution, exportPath);
            }
            else
            {
                MakeImage(VisibleTileLayer.Population, noiseTypeXAxis, noiseTypeYAxis, resolution, exportPath);
            }
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

            var noiseTypeElements = new List<BaseUI?>();

            var noiseTypeXSelectionElement = new PAChoice(noiseTypeNames, 0, "X axis noise type: ");
            noiseTypeElements.Add(noiseTypeXSelectionElement);

            var noiseTypeYSelectionElement = new PAChoice(noiseTypeNames, 0, "Y axis noise type: ");
            noiseTypeElements.Add(noiseTypeYSelectionElement);
            noiseTypeElements.Add(null);

            var layerTypes = Enum.GetValues<BaseContentType>();
            var layerSelectionElement = new PAChoice(layerTypes.Select(layer => layer.ToString().Capitalize()).ToList(), 0, "Layer: ");
            noiseTypeElements.Add(layerSelectionElement);
            noiseTypeElements.Add(null);

            var defResolution = 100;
            var resolutionElement = new TextField(
                defResolution.ToString(), "Resolution: ",
                oldValueAsStartingValue: true,
                textValidatorFunction: TextValidatorDelegate,
                keyValidatorFunction: KeyValidatorDelegate,
                overrideDefaultKeyValidatorFunction: false
            );
            noiseTypeElements.Add(resolutionElement);
            noiseTypeElements.Add(null);

            var generateImageButtonElement = new PAButton(
                new UIAction(
                    GenerateImageCommand,
                        layerSelectionElement,
                        layerTypes,
                        noiseTypeXSelectionElement,
                        noiseTypeYSelectionElement,
                        noiseTypes,
                        resolutionElement,
                        visualizedContentDistributionPath
                ),
                text: "Generate image"
            );
            noiseTypeElements.Add(generateImageButtonElement);

            var generateAllImagesButtonElement = new PAButton(
                new UIAction(
                    GenerateAllImagesCommand,
                        layerTypes,
                        noiseTypes,
                        resolutionElement,
                        visualizedContentDistributionPath
                ),
                text: "Generate ALL possible images"
            );
            noiseTypeElements.Add(generateAllImagesButtonElement);

            new OptionsUI(noiseTypeElements, "Select the noise types to generate the distribution image from:").Display();
        }
        #endregion

        #region Pivate fields
        private static void GenerateImageCommand(
            PAChoice layerSelectionElement,
            BaseContentType[] layers,
            PAChoice noiseTypeXSelectionElement,
            PAChoice noiseTypeYSelectionElement,
            TileNoiseType[] noiseTypes,
            TextField resolutionElement,
            string visualizedContentDistributionPath
        )
        {
            // get selected noise types
            var noiseTypeXAxis = noiseTypes[noiseTypeXSelectionElement.Value];
            var noiseTypeYAxis = noiseTypes[noiseTypeYSelectionElement.Value];
            var resolution = uint.Parse(resolutionElement.Value);

            // get selected layer
            var layer = layers[layerSelectionElement.Value];

            // generate image
            var imageName = $"{noiseTypeXAxis}-{noiseTypeYAxis}.png";
            MakeImageForLayer(layer, noiseTypeXAxis, noiseTypeYAxis, resolution, Path.Join(visualizedContentDistributionPath, imageName));
            Utils.PressKey($"Generated image as \"{imageName}\"");
        }

        private static void GenerateAllImagesCommand(
            BaseContentType[] layers,
            TileNoiseType[] noiseTypes,
            TextField resolutionElement,
            string visualizedContentDistributionPath
        )
        {
            var resolution = uint.Parse(resolutionElement.Value);

            // generate images
            foreach (var layer in layers)
            {
                foreach (var noiseTypeXAxis in noiseTypes)
                {
                    foreach (var noiseTypeYAxis in noiseTypes)
                    {
                        var imageName = $"{layer}-{noiseTypeXAxis}-{noiseTypeYAxis}.png";
                        MakeImageForLayer(layer, noiseTypeXAxis, noiseTypeYAxis, resolution, Path.Join(visualizedContentDistributionPath, imageName));
                        Console.WriteLine($"Generated image as \"{imageName}\"");
                    }
                }
            }
        }
        #endregion
    }
}
