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
using System.Windows.Media;
using PAConstants = ProgressAdventure.Constants;

namespace PAVisualiser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Config dictionaries
        private static readonly Dictionary<ContentTypeID, Color> contentSubtypeColorMap = new()
        {
            [ContentType.Terrain.FIELD] = Constants.Colors.GREEN,
            [ContentType.Terrain.OCEAN] = Constants.Colors.LIGHT_BLUE,
            [ContentType.Terrain.SHORE] = Constants.Colors.LIGHTER_BLUE,
            [ContentType.Terrain.MOUNTAIN] = Constants.Colors.LIGHT_GRAY,

            [ContentType.Structure.VILLAGE] = Constants.Colors.LIGHT_BROWN,
            [ContentType.Structure.KINGDOM] = Constants.Colors.BROWN,
            [ContentType.Structure.BANDIT_CAMP] = Constants.Colors.DARK_RED,

            [ContentType.Population.DWARF] = Constants.Colors.LIGHT_GRAY,
            [ContentType.Population.HUMAN] = Constants.Colors.SKIN,
            [ContentType.Population.DEMON] = Constants.Colors.DARK_RED,
            [ContentType.Population.ELF] = Constants.Colors.DARK_GREEN,
        };
        #endregion

        #region Private fields
        private bool selectedSave = false;
        private (long x, long y) center;
        private long radius;
        private List<WorldLayers> layers;
        #endregion

        #region Public constructors
        public MainWindow()
        {
            center = (0, 0);
            radius = 3;
            layers = new List<WorldLayers> { WorldLayers.Terrain };

            KeyDown += new System.Windows.Input.KeyEventHandler(WorldGridMoveCommand);
            MouseWheel += new System.Windows.Input.MouseWheelEventHandler(WorldGridZoomCommand);

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
            radius = 3;
            RenderWorldArea();
        }

        private void CloseSaveCommand(object sender, RoutedEventArgs e)
        {
            selectedSave = false;
            ClearWorldGrid();
        }

        private void WorldGridZoomCommand(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scrollUp = e.Delta > 0;

            var newRadius = Math.Clamp(radius + (scrollUp ? -1 : 1), 1, long.MaxValue);

            if (newRadius !=  radius)
            {
                radius = newRadius;
                RenderWorldArea();
            }
        }

        private void RebuildLayersCommand(object sender, RoutedEventArgs e)
        {
            RebuildLayers();
        }

        private void WorldGridMoveCommand(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var key = e.Key;

            var newCenter = center;

            switch (key)
            {
                case System.Windows.Input.Key.Up:
                    newCenter.y++;
                    break;
                case System.Windows.Input.Key.Down:
                    newCenter.y--;
                    break;
                case System.Windows.Input.Key.Left:
                    newCenter.x--;
                    break;
                case System.Windows.Input.Key.Right:
                    newCenter.x++;
                    break;
                default:
                    return;
            }

            center = newCenter;
            RenderWorldArea();
        }

        private void RevealAreaCommand(object sender, RoutedEventArgs e)
        {
            RevealArea((center.x - radius, center.y - radius, center.x + radius, center.y + radius));
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
            var newLayers = new List<WorldLayers>();
            if (terrainLayerCheckBox.IsChecked == true)
            {
                newLayers.Add(WorldLayers.Terrain);
            }
            if (structureLayerCheckBox.IsChecked == true)
            {
                newLayers.Add(WorldLayers.Structure);
            }
            if (populationLayerCheckBox.IsChecked == true)
            {
                newLayers.Add(WorldLayers.Population);
            }

            layers = newLayers;

            if (selectedSave)
            {
                RenderWorldArea();
            }
        }

        private void RenderWorldArea(List<WorldLayers> layers, (long minX, long minY, long maxX, long maxY)? corners = null)
        {
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
                for (long y = minY; y < maxY + 1; y++)
                {
                    var cd = new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Star)
                    };
                    worldGrid.ColumnDefinitions.Add(cd);
                }

                // rows
                for (long x = minX; x < maxX + 1; x++)
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
            foreach (var chunk in World.Chunks)
            {
                foreach (var tile in chunk.Value.tiles)
                {
                    var xPos = chunk.Value.basePosition.x + tile.Value.relativePosition.x;
                    var yPos = chunk.Value.basePosition.y + tile.Value.relativePosition.y;

                    if (
                        xPos > maxX || xPos < minX ||
                        yPos > maxY || yPos < minY
                    )
                    {
                        continue;
                    }

                    ContentTypeID contentSubtype;

                    if (layers.Contains(WorldLayers.Population) && tile.Value.population.subtype != ContentType.Population.NONE)
                    {
                        contentSubtype = tile.Value.population.subtype;
                    }
                    else if (layers.Contains(WorldLayers.Structure) && tile.Value.structure.subtype != ContentType.Structure.NONE)
                    {
                        contentSubtype = tile.Value.structure.subtype;
                    }
                    else if (layers.Contains(WorldLayers.Terrain))
                    {
                        contentSubtype = tile.Value.terrain.subtype;
                    }
                    else
                    {
                        continue;
                    }

                    Color color = Constants.Colors.RED;
                    contentSubtypeColorMap.TryGetValue(contentSubtype, out color);

                    var content = new Canvas()
                    {
                        Background = new SolidColorBrush(color),
                    };

                    var column = xPos - minX;
                    var row = worldGrid.RowDefinitions.Count - 1 - (yPos - minY);

                    Grid.SetColumn(content, (int)column);
                    Grid.SetRow(content, (int)row);
                    worldGrid.Children.Add(content);
                }
            }

            revealAreaButton.IsEnabled = true;
        }

        private void UpdateViewTextboxes()
        {
            centerTextBox.Content = $"Center: {center.x}, {center.y}";
            diameterTextBox.Content = $"Zoom diameter: {radius * 2 + 1}";
        }

        private void RenderWorldArea(List<WorldLayers> layers, (long x, long y) center, long extraRadius)
        {
            UpdateViewTextboxes();
            RenderWorldArea(layers, (center.x - extraRadius, center.y - extraRadius, center.x + extraRadius, center.y + extraRadius));
        }

        private void RenderWorldArea()
        {
            RenderWorldArea(layers, center, radius);
        }

        private void RevealArea((long minX, long minY, long maxX, long maxY) corners)
        {
            for (long x = corners.minX; x < corners.maxX + 1; x++)
            {
                for (long y = corners.minX; y < corners.maxX + 1; y++)
                {
                    World.TryGetChunk((x, y), out Chunk chunk);
                    chunk.TryGetTile((x, y), out _);
                }
            }
        }
        #endregion
    }
}
