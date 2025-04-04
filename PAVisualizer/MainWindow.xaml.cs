using PACommon;
using PACommon.Enums;
using ProgressAdventure;
using ProgressAdventure.Enums;
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
using PATools = PACommon.Tools;

namespace PAVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private fields
        private static readonly double WORLD_MOVE_CONSTANT = 0.2;

        private bool _selectedSave;
        private bool _isWorldVisible;
        private bool _tileCountsNeedToBeRefreshed;

        private string saveName;
        private DateTime lastWorldChange;
        private Dictionary<BaseContentType, Dictionary<EnumTreeValue<ContentType>, long>> worldTileTypeCounts;
        private Dictionary<EnumValue<EntityType>, long> entityTypeCounts;
        private string worldInfoString;

        private (double x, double y) center;
        private double worldGridScale;
        private List<VisibleTileLayer> layers;
        #endregion

        #region Public properties
        public bool SelectedSave
        {
            get => _selectedSave;
            private set
            {
                _selectedSave = value;

                closeMenuItem.IsEnabled = SelectedSave;
                createImageMenuItem.IsEnabled = SelectedSave;
                showSaveInfoMenuItem.IsEnabled = SelectedSave;
                showWorldInfoMenuItem.IsEnabled = SelectedSave;
                createSaveMenuItem.IsEnabled = SelectedSave;
            }
        }
        public bool IsWorldVisible
        {
            get => _isWorldVisible && SelectedSave;
            private set
            {
                _isWorldVisible = value;

                revealAreaButton.IsEnabled = IsWorldVisible;
            }
        }

        private bool TileCountsNeedToBeRefreshed
        {
            get => _tileCountsNeedToBeRefreshed;
            set
            {
                _tileCountsNeedToBeRefreshed = value;

                if (!value)
                {
                    worldInfoString = VisualizerTools.GetDisplayTileCountsData(worldTileTypeCounts) + "\n" + 
                        VisualizerTools.GetDisplayPopulationCountsData(entityTypeCounts);
                }
            }
        }
        #endregion

        #region Public constructors
        public MainWindow()
        {
            center = (0, 0);
            worldGridScale = 1;
            layers = [VisibleTileLayer.Terrain];
            saveName = string.Empty;
            worldTileTypeCounts = [];
            entityTypeCounts = [];
            worldInfoString = string.Empty;
            TileCountsNeedToBeRefreshed = true;

            KeyDown += new KeyEventHandler(WorldGridMoveCommand);
            MouseWheel += new MouseWheelEventHandler(WorldGridZoomCommand);

            InitializeComponent();
            DataContext = this;

            SelectedSave = false;
            IsWorldVisible = false;

            terrainLayerCheckBox.IsChecked = true;
            structureLayerCheckBox.IsChecked = true;
            populationLayerCheckBox.IsChecked = true;
            toggleLoggingWriteOut.Content = $"Toggle logging in console: {PACSingletons.Instance.Logger.DefaultWriteOut}";
        }
        #endregion

        #region Commands
        private void SelectSaveCommand(object sender, RoutedEventArgs e)
        {
            var saveStrings = VisualizerTools.GetSaveFolderFromPath(VisualizerTools.GetPathFromFileDialog(this, PAConstants.SAVES_FOLDER_PATH, true));

            if (saveStrings is null)
            {
                return;
            }

            try
            {
                SaveManager.LoadSave(saveStrings.Value.saveFolderName, false, false, saveStrings.Value.saveFolderPath);
            }
            catch (Exception ex)
            {
                if (ex is FileLoadException || ex is FileNotFoundException)
                {
                    return;
                }
                throw;
            }

            saveName = saveStrings.Value.saveFolderName;
            World.LoadAllChunksFromFolder(saveName, "Loading chunks...");

            SelectedSave = true;
            lastWorldChange = DateTime.Now;
            TileCountsNeedToBeRefreshed = true;

            center = (0, 0);
            worldGridScale = 1;
            RenderWorldArea(layers, null);
        }

        private void CloseSaveCommand(object sender, RoutedEventArgs e)
        {
            SelectedSave = false;
            ClearWorldGrid();
        }

        private void CreateSaveCommand(object sender, RoutedEventArgs e)
        {
            if (!SelectedSave)
            {
                return;
            }

            SaveManager.MakeSave(false, "Saving...");
        }

        private void CreateImageCommand(object sender, RoutedEventArgs e)
        {
            if (!SelectedSave || layers.Count == 0)
            {
                return;
            }

            var visualizedSaveFolderName = $"{saveName}_{Utils.MakeDate(lastWorldChange)}_{Utils.MakeTime(lastWorldChange, ";")}";
            var visualizedSavePath = Path.Join(Constants.VISUALIZED_SAVES_DATA_FOLDER_PATH, visualizedSaveFolderName);

            PATools.RecreateFolder(visualizedSavePath);

            var imageName = string.Join("-", layers) + ".png";
            ConsoleVisualizer.MakeImage(layers, Path.Join(visualizedSavePath, imageName));
        }

        private void ShowSaveInfoCommand(object sender, RoutedEventArgs e)
        {
            if (!SelectedSave)
            {
                return;
            }
            new SaveInfoWindow().ShowDialog();
        }

        private void ShowWorldInfoCommand(object sender, RoutedEventArgs e)
        {
            if (!SelectedSave)
            {
                return;
            }
            new WorldInfoWindow(worldInfoString).ShowDialog();
        }

        private void WorldGridZoomCommand(object sender, MouseWheelEventArgs e)
        {
            if (!SelectedSave)
            {
                return;
            }

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
            if (!SelectedSave)
            {
                return;
            }

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
            if (!SelectedSave)
            {
                return;
            }

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
            PACSingletons.Instance.Logger.DefaultWriteOut = !PACSingletons.Instance.Logger.DefaultWriteOut;
            toggleLoggingWriteOut.Content = $"Toggle logging in console: {PACSingletons.Instance.Logger.DefaultWriteOut}";
        }
        #endregion

        #region Private Methods
        private void ClearWorldGrid()
        {
            IsWorldVisible = false;
            worldGrid.ColumnDefinitions.Clear();
            worldGrid.RowDefinitions.Clear();
            worldGrid.Children.Clear();
        }

        private void RebuildLayers()
        {
            var newLayers = new List<VisibleTileLayer>();
            if (terrainLayerCheckBox.IsChecked == true)
            {
                newLayers.Add(VisibleTileLayer.Terrain);
            }
            if (structureLayerCheckBox.IsChecked == true)
            {
                newLayers.Add(VisibleTileLayer.Structure);
            }
            if (populationLayerCheckBox.IsChecked == true)
            {
                newLayers.Add(VisibleTileLayer.Population);
            }

            layers = newLayers;

            if (SelectedSave)
            {
                RenderWorldArea();
            }
        }

        private void AppendTileCounts(Tile tile)
        {
            if (
                worldTileTypeCounts.TryGetValue(BaseContentType.Terrain, out Dictionary<EnumTreeValue<ContentType>, long>? tCounts) &&
                tCounts is not null &&
                tCounts.ContainsKey(tile.terrain.subtype)
            )
            {
                worldTileTypeCounts[BaseContentType.Terrain][tile.terrain.subtype]++;
            }
            else
            {
                worldTileTypeCounts[BaseContentType.Terrain][tile.terrain.subtype] = 1;
            }

            if (
                worldTileTypeCounts.TryGetValue(BaseContentType.Structure, out Dictionary<EnumTreeValue<ContentType>, long>? sCounts) &&
                sCounts is not null &&
                sCounts.ContainsKey(tile.structure.subtype)
            )
            {
                worldTileTypeCounts[BaseContentType.Structure][tile.structure.subtype]++;
            }
            else
            {
                worldTileTypeCounts[BaseContentType.Structure][tile.structure.subtype] = 1;
            }

            var popManager = tile.populationManager;
            foreach (var (type, amount) in VisualizerTools.GetPopulationCounts(popManager))
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

        private void RenderWorldArea(List<VisibleTileLayer> layers, (long minX, long minY, long maxX, long maxY)? corners = null)
        {
            if (TileCountsNeedToBeRefreshed)
            {
                worldTileTypeCounts = new Dictionary<BaseContentType, Dictionary<EnumTreeValue<ContentType>, long>>
                {
                    [BaseContentType.Terrain] = [],
                    [BaseContentType.Structure] = [],
                };
                entityTypeCounts = [];
            }

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
            if (layers.Count == 0)
            {
                return;
            }

            foreach (var chunk in World.Chunks)
            {
                foreach (var tile in chunk.Value.tiles)
                {
                    var tileObj = tile.Value;

                    if (TileCountsNeedToBeRefreshed)
                    {
                        AppendTileCounts(tileObj);
                    }

                    var xPos = chunk.Value.basePosition.x + tileObj.relativePosition.x;
                    var yPos = chunk.Value.basePosition.y + tileObj.relativePosition.y;

                    if (
                        xPos > maxX || xPos < minX ||
                        yPos > maxY || yPos < minY ||
                        layers.Count == 0
                    )
                    {
                        continue;
                    }

                    VisibleTileLayer layer;
                    string? contentName;
                    if (layers.Contains(VisibleTileLayer.Population) && tileObj.populationManager.PopulationCount != 0)
                    {
                        layer = VisibleTileLayer.Population;
                        contentName = null;
                    }
                    else if (layers.Contains(VisibleTileLayer.Structure) && tileObj.structure.subtype != ContentType.Structure.NONE)
                    {
                        layer = VisibleTileLayer.Structure;
                        contentName = tileObj.structure.Name;
                    }
                    else if (layers.Contains(VisibleTileLayer.Terrain))
                    {
                        layer = VisibleTileLayer.Terrain;
                        contentName = tileObj.terrain.Name;
                    }
                    else
                    {
                        continue;
                    }

                    var color = VisualizerTools.GetLayerContentColor(tileObj, layer);

                    var extraTerrainData = tileObj.terrain.TryGetExtraProperty("height", out var height) ?
                        $"(height: {height})" :
                        (tileObj.terrain.TryGetExtraProperty("depth", out var depth) ? $"(depth: {depth})" : "");
                    var tooltipContent = new StackPanel()
                    {
                        Children =
                        {
                            new Label() { Content = $"Chunk seed: {PATools.SerializeRandom(chunk.Value.ChunkRandomGenerator)}" },
                            new Label() { Content = $"Terrain: {tileObj.terrain.GetSubtypeName()} {extraTerrainData}" },
                        }
                    };

                    if (tileObj.structure.subtype != ContentType.Structure.NONE)
                    {
                        var extraStructureData = tileObj.structure.TryGetExtraProperty("population", out var population) ? $"(population: {population})" : "";
                        tooltipContent.Children.Add(new Label() { Content = $"Structure: {tileObj.structure.GetSubtypeName()} {extraStructureData}" });
                    }

                    if (tileObj.populationManager.PopulationCount != 0)
                    {
                        tooltipContent.Children.Add(new Label()
                        {
                            Content = $"Population:\n\t{string.Join("\n\t", VisualizerTools.GetPopulationCounts(tileObj.populationManager).Select(e => $"{e.type}: {e.amount}"))}",
                        });
                    }

                    var content = new Label()
                    {
                        Background = new SolidColorBrush(color.ToMediaColor()),
                        Content = contentName,
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

            IsWorldVisible = true;
            TileCountsNeedToBeRefreshed = false;
        }

        private void UpdateViewTextboxes()
        {
            centerTextBox.Content = $"Center: {Math.Round(center.x, 3)}, {Math.Round(center.y, 3)}";
            diameterTextBox.Content = $"Zoom scale: {Math.Round(worldGridScale, 3)}";
        }

        private void RenderWorldArea(List<VisibleTileLayer> layers, (long x, long y) center, long extraRadius)
        {
            RenderWorldArea(layers, (center.x - extraRadius, center.y - extraRadius, center.x + extraRadius, center.y + extraRadius));
        }

        private void RenderWorldArea()
        {
            RenderWorldArea(layers, null);
        }

        private void RevealArea((long minX, long minY, long maxX, long maxY) corners)
        {
            if (!SelectedSave)
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
            lastWorldChange = DateTime.Now;
            TileCountsNeedToBeRefreshed = true;
        }
        #endregion
    }
}
