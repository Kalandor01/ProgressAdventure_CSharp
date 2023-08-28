using Microsoft.Win32;
using ProgressAdventure;
using ProgressAdventure.WorldManagement;
using ProgressAdventure.WorldManagement.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PAConstants = ProgressAdventure.Constants;

namespace PAVisualiser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private fields
        private static readonly double WORLD_MOVE_CONSTANT = 0.2;

        private bool selectedSave = false;
        private (double x, double y) center;
        private double worldGridScale;
        private List<WorldLayer> layers;
        #endregion

        #region Public constructors
        public MainWindow()
        {
            center = (0, 0);
            worldGridScale = 1;
            layers = new List<WorldLayer> { WorldLayer.Terrain };

            KeyDown += new KeyEventHandler(WorldGridMoveCommand);
            MouseWheel += new MouseWheelEventHandler(WorldGridZoomCommand);

            InitializeComponent();
            DataContext = this;

            terrainLayerCheckBox.IsChecked = true;
            structureLayerCheckBox.IsChecked = true;
            populationLayerCheckBox.IsChecked = true;
            toggleLoggingWriteOut.Content = $"Toggle logging in console: {Logger.DefaultWriteOut}";
        }
        #endregion

        #region Commands
        private void SelectSaveCommand(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
            };

            if (openFileDialog.ShowDialog(this) != true)
            {
                return;
            }

            var saveFolderDataPath = openFileDialog.FileName;
            if (Path.GetExtension(saveFolderDataPath) != "." + PAConstants.SAVE_EXT)
            {
                return;
            }

            var saveFolderPath = Path.GetDirectoryName(saveFolderDataPath);
            if (saveFolderPath is null)
            {
                return;
            }

            var savesFolderPath = Directory.GetParent(saveFolderPath);
            if (savesFolderPath is null)
            {
                return;
            }

            var saveName = saveFolderPath.Split(Path.DirectorySeparatorChar).Last();

            try
            {
                SaveManager.LoadSave(saveName, false, false, savesFolderPath.FullName);
            }
            catch (Exception ex)
            {
                if (ex is FileLoadException || ex is FileNotFoundException)
                {
                    return;
                }
            }

            World.LoadAllChunksFromFolder(saveName);

            selectedSave = true;

            center = (0, 0);
            worldGridScale = 1;
            RenderWorldArea(layers, null);
        }

        private void CloseSaveCommand(object sender, RoutedEventArgs e)
        {
            selectedSave = false;
            ClearWorldGrid();
        }

        private void WorldGridZoomCommand(object sender, MouseWheelEventArgs e)
        {
            var scrollUp = e.Delta > 0;
            worldGridScale *= scrollUp ? 1.1 : 0.9;
            worldGrid.RenderTransform = new ScaleTransform(worldGridScale, worldGridScale, center.x, center.y);

            UpdateViewTextboxes();
        }

        private void RebuildLayersCommand(object sender, RoutedEventArgs e)
        {
            RebuildLayers();
        }

        private void WorldGridMoveCommand(object sender, KeyEventArgs e)
        {
            var key = e.Key;

            var newCenter = center;

            var scaleModifierMultiplier = (worldGridScale > 1 ? 1.0 : -1.0) / (Math.Abs(worldGridScale - 1) + 1);
            var scaleModifier = WORLD_MOVE_CONSTANT * scaleModifierMultiplier;

            switch (key)
            {
                case Key.W:
                    newCenter.y -= scaleModifier;
                    break;
                case Key.S:
                    newCenter.y += scaleModifier;
                    break;
                case Key.A:
                    newCenter.x -= scaleModifier;
                    break;
                case Key.D:
                    newCenter.x += scaleModifier;
                    break;
                default:
                    return;
            }

            center = newCenter;
            worldGrid.RenderTransform = new ScaleTransform(worldGridScale, worldGridScale);
            worldGrid.RenderTransformOrigin = new Point(center.x, center.y);

            UpdateViewTextboxes();
        }

        private void RevealAreaCommand(object sender, RoutedEventArgs e)
        {
            var corners = World.GetCorners();

            if (corners is null)
            {
                return;
            }

            var worldWidth = corners.Value.maxX - corners.Value.minX;
            var totalTorenderedWorldSize = 1 / worldGridScale;
            var revealAreaWidth = worldWidth * totalTorenderedWorldSize;

            RevealArea((
                (long)(revealAreaWidth + (-1 * center.x) * worldWidth),
                (long)(revealAreaWidth + (center.y - 1) * worldWidth),
                (long)(revealAreaWidth + (-1 * center.x + 1) * worldWidth),
                (long)(revealAreaWidth + center.y * worldWidth)
            ));
            RenderWorldArea();
        }

        private void ToggleLoggingWriteOutCommand(object sender, RoutedEventArgs e)
        {
            Logger.DefaultWriteOut = !Logger.DefaultWriteOut;
            toggleLoggingWriteOut.Content = $"Toggle logging in console: {Logger.DefaultWriteOut}";
        }
        #endregion

        #region Private Methods
        private void ClearWorldGrid()
        {
            revealAreaButton.IsEnabled = false;
            worldGrid.ColumnDefinitions.Clear();
            worldGrid.RowDefinitions.Clear();
            worldGrid.Children.Clear();
        }

        private void RebuildLayers()
        {
            var newLayers = new List<WorldLayer>();
            if (terrainLayerCheckBox.IsChecked == true)
            {
                newLayers.Add(WorldLayer.Terrain);
            }
            if (structureLayerCheckBox.IsChecked == true)
            {
                newLayers.Add(WorldLayer.Structure);
            }
            if (populationLayerCheckBox.IsChecked == true)
            {
                newLayers.Add(WorldLayer.Population);
            }

            layers = newLayers;

            if (selectedSave)
            {
                RenderWorldArea();
            }
        }

        private void RenderWorldArea(List<WorldLayer> layers, (long minX, long minY, long maxX, long maxY)? corners = null)
        {
            UpdateViewTextboxes();

            corners ??= World.GetCorners();

            if (corners is null)
            {
                return;
            }

            (long minX, long minY, long maxX, long maxY) = corners.Value;

            // grid
            if (
                maxX - minX + 1 != worldGrid.ColumnDefinitions.Count ||
                maxY - minY + 1 != worldGrid.ColumnDefinitions.Count
            )
            {
                // clear
                ClearWorldGrid();

                // columns
                for (long x = minX; x < maxX + 1; x++)
                {
                    var cd = new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Star)
                    };
                    worldGrid.ColumnDefinitions.Add(cd);
                }

                // rows
                for (long y = minY; y < maxY + 1; y++)
                {
                    var rd = new RowDefinition
                    {
                        Height = new GridLength(1, GridUnitType.Star)
                    };
                    worldGrid.RowDefinitions.Add(rd);
                }
            }
            else
            {
                worldGrid.Children.Clear();
            }

            // world
            if (!layers.Any())
            {
                return;
            }

            foreach (var chunk in World.Chunks)
            {
                foreach (var tile in chunk.Value.tiles)
                {
                    var tileObj = tile.Value;
                    var xPos = chunk.Value.basePosition.x + tileObj.relativePosition.x;
                    var yPos = chunk.Value.basePosition.y + tileObj.relativePosition.y;

                    if (
                        xPos > maxX || xPos < minX ||
                        yPos > maxY || yPos < minY
                    )
                    {
                        continue;
                    }

                    BaseContent tileContent;

                    if (layers.Contains(WorldLayer.Population) && tileObj.population.subtype != ContentType.Population.NONE)
                    {
                        tileContent = tileObj.population;
                    }
                    else if (layers.Contains(WorldLayer.Structure) && tileObj.structure.subtype != ContentType.Structure.NONE)
                    {
                        tileContent = tileObj.structure;
                    }
                    else if (layers.Contains(WorldLayer.Terrain))
                    {
                        tileContent = tileObj.terrain;
                    }
                    else
                    {
                        continue;
                    }

                    var color = VisualiserTools.contentSubtypeColorMap.TryGetValue(tileContent.subtype, out ColorData colorD) ? colorD : Constants.Colors.MAGENTA;

                    var extraTerrainData = tileObj.terrain.TryGetExtraProperty("height", out object? height) ?
                        $"(height: {height})" :
                        (tileObj.terrain.TryGetExtraProperty("depth", out object? depth) ? $"(depth: {depth})" : "");
                    var tooltipContent = new StackPanel()
                    {
                        Children =
                        {
                            new Label() { Content = $"Chunk seed: {Tools.SerializeRandom(chunk.Value.ChunkRandomGenerator)}" },
                            new Label() { Content = $"Terrain: {tileObj.terrain.GetSubtypeName()} {extraTerrainData}" },
                        }
                    };

                    if (tileObj.structure.subtype != ContentType.Structure.NONE)
                    {
                        var extraStructureData = tileObj.structure.TryGetExtraProperty("population", out object? population) ? $"(population: {population})" : "";
                        tooltipContent.Children.Add(new Label() { Content = $"Structure: {tileObj.structure.GetSubtypeName()} {extraStructureData}" });
                    }
                    if (tileObj.population.subtype != ContentType.Population.NONE)
                    {
                        tooltipContent.Children.Add(new Label() { Content = $"Population: {tileObj.population.GetSubtypeName()} ({tileObj.population.amount})" });
                    }

                    var content = new Label()
                    {
                        Background = new SolidColorBrush(color.ToMediaColor()),
                        Content = tileContent.Name,
                        ToolTip = new ToolTip()
                        {
                            Content = tooltipContent,
                        }
                    };

                    var column = xPos - minX;
                    var row = worldGrid.RowDefinitions.Count - (yPos - minY);

                    Grid.SetColumn(content, (int)column);
                    Grid.SetRow(content, (int)row);
                    worldGrid.Children.Add(content);
                }
            }

            revealAreaButton.IsEnabled = true;
        }

        private void UpdateViewTextboxes()
        {
            centerTextBox.Content = $"Center: {Math.Round(center.x, 3)}, {Math.Round(center.y, 3)}";
            diameterTextBox.Content = $"Zoom scale: {Math.Round(worldGridScale, 3)}";
        }

        private void RenderWorldArea(List<WorldLayer> layers, (long x, long y) center, long extraRadius)
        {
            RenderWorldArea(layers, (center.x - extraRadius, center.y - extraRadius, center.x + extraRadius, center.y + extraRadius));
        }

        private void RenderWorldArea()
        {
            RenderWorldArea(layers, null);
        }

        private void RevealArea((long minX, long minY, long maxX, long maxY) corners)
        {
            if (!selectedSave)
            {
                return;
            }

            for (long x = corners.minX; x < corners.maxX + 1; x++)
            {
                for (long y = corners.minY; y < corners.maxY + 1; y++)
                {
                    World.TryGetChunk((x, y), out Chunk chunk);
                    chunk.TryGetTile((x, y), out _);
                }
            }
        }
        #endregion
    }
}
