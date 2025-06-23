using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PACommon;
using PACommon.Enums;
using ProgressAdventure;
using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PAConstants = ProgressAdventure.Constants;
using PATools = PACommon.Tools;
using Tools = PACommon.Tools;

namespace PAVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private fields
        private const double WORLD_ZOOM_IN_CONSTANT = 1.1;
        private const double WORLD_ZOOM_OUT_CONSTANT = 1 / WORLD_ZOOM_IN_CONSTANT;

        private string saveName;
        private DateTime lastWorldChange;
        private Dictionary<EnumValue<TerrainType>, long> terrainTypeCounts;
        private Dictionary<EnumValue<StructureType>, long> structureTypeCounts;
        private Dictionary<EnumValue<EntityType>, long> entityTypeCounts;
        private string worldInfoString;

        private (double x, double y) center;
        private double worldGridScale;
        private List<VisibleTileLayer> layers;
        private bool blendPopulationColors;
        #endregion

        #region Public properties
        public bool SelectedSave
        {
            get;
            private set
            {
                field = value;

                closeMenuItem.IsEnabled = SelectedSave;
                createImageMenuItem.IsEnabled = SelectedSave;
                showSaveInfoMenuItem.IsEnabled = SelectedSave;
                showWorldInfoMenuItem.IsEnabled = SelectedSave;
                createSaveMenuItem.IsEnabled = SelectedSave;
            }
        }
        public bool IsWorldVisible
        {
            get => field && SelectedSave;
            private set
            {
                field = value;

                revealAreaButton.IsEnabled = IsWorldVisible;
            }
        }

        private bool TileCountsNeedToBeRefreshed
        {
            get;
            set
            {
                field = value;

                if (!value)
                {
                    worldInfoString = VisualizerTools.GetDisplayTerrainCountsData(terrainTypeCounts) + "\n" +
                        VisualizerTools.GetDisplayStructureCountsData(structureTypeCounts) + "\n" +
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
            blendPopulationColors = true;
            saveName = string.Empty;
            terrainTypeCounts = [];
            structureTypeCounts = [];
            entityTypeCounts = [];
            worldInfoString = string.Empty;
            TileCountsNeedToBeRefreshed = true;

            KeyDown += WorldGridMoveCommand;
            PointerWheelChanged += WorldGridZoomCommand;

            InitializeComponent();
            DataContext = this;

            SelectedSave = false;
            IsWorldVisible = false;

            terrainLayerCheckBox.IsChecked = true;
            structureLayerCheckBox.IsChecked = true;
            populationLayerCheckBox.IsChecked = true;
            toggleLoggingWriteOut.Content = $"Toggle logging in console: {PACSingletons.Instance.Logger.DefaultWriteOut}";

            selectMenuItem.Command = new Command(SelectSaveCommand);
            closeMenuItem.Command = new Command(CloseSaveCommand);
            createImageMenuItem.Command = new Command(CreateImageCommand);
            showSaveInfoMenuItem.Command = new Command(ShowSaveInfoCommand);
            showWorldInfoMenuItem.Command = new Command(ShowWorldInfoCommand);
            createSaveMenuItem.Command = new Command(CreateSaveCommand);
            revealAreaButton.Command = new Command(RevealAreaCommand);
            toggleLoggingWriteOut.Command = new Command(ToggleLoggingWriteOutCommand);
            toggleBlendPopulationColors.Command = new Command(ToggleBlendPopulationColorsCommand);
            terrainLayerCheckBox.IsCheckedChanged += TerrainLayerCheckBoxIsCheckedChanged;
            structureLayerCheckBox.IsCheckedChanged += StructureLayerCheckBoxIsCheckedChanged;
            populationLayerCheckBox.IsCheckedChanged += PopulationLayerCheckBoxIsCheckedChanged;
        }
        #endregion

        #region Commands
        private void SelectSaveCommand()
        {
            var pathTask = VisualizerTools.GetPathFromFileDialog(this, PAConstants.SAVES_FOLDER_PATH, true);
            pathTask.Wait();
            var path = pathTask.Result;
            var saveStrings = VisualizerTools.GetSaveFolderFromPath(path);

            if (saveStrings is null)
            {
                return;
            }

            try
            {
                SaveManager.LoadSave(saveStrings.Value.saveFolderName, false, saveStrings.Value.saveFolderPath);
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
            World.LoadAllChunksFromFolder(out _, saveName, "Loading chunks...");

            SelectedSave = true;
            lastWorldChange = DateTime.Now;
            TileCountsNeedToBeRefreshed = true;

            center = (0, 0);
            worldGridScale = 1;
            RenderWorldArea(layers, null);
        }

        private void CloseSaveCommand()
        {
            SelectedSave = false;
            ClearWorldGrid();
        }

        private void CreateSaveCommand( )
        {
            if (!SelectedSave)
            {
                return;
            }

            SaveManager.MakeSave(false, "Saving...");
        }

        private void CreateImageCommand()
        {
            if (!SelectedSave || layers.Count == 0)
            {
                return;
            }

            var visualizedSaveFolderName = $"{saveName}_{Utils.MakeDate(lastWorldChange)}_{Utils.MakeTime(lastWorldChange, ";")}";
            var visualizedSavePath = Path.Join(Constants.VISUALIZED_SAVES_DATA_FOLDER_PATH, visualizedSaveFolderName);

            PATools.RecreateFolder(visualizedSavePath);

            var imageName = string.Join("-", layers) + ".png";
            ConsoleVisualizer.MakeImage(layers, Path.Join(visualizedSavePath, imageName), blendPopulationColors);
        }

        private void ShowSaveInfoCommand()
        {
            if (!SelectedSave)
            {
                return;
            }
            new SaveInfoWindow().ShowDialog(this);
        }

        private void ShowWorldInfoCommand()
        {
            if (!SelectedSave)
            {
                return;
            }
            new WorldInfoWindow(worldInfoString).ShowDialog(this);
        }

        private void WorldGridZoomCommand(object? sender, PointerWheelEventArgs args)
        {
            if (!SelectedSave)
            {
                return;
            }

            var scrollUp = args.Delta.X > 0;
            worldGridScale *= scrollUp ? WORLD_ZOOM_IN_CONSTANT : WORLD_ZOOM_OUT_CONSTANT;

            var transformMatrix = Matrix.CreateTranslation(center.x, center.y)
                .Append(new ScaleTransform(worldGridScale, worldGridScale).Value);
            worldGrid.RenderTransform = new MatrixTransform(transformMatrix);
            UpdateViewTextboxes();
        }

        private void RebuildLayersCommand()
        {
            RebuildLayers();
        }
        
        private void WorldGridMoveCommand(object? sender, KeyEventArgs args)
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

            var worldWidth = corners.Value.maxX - corners.Value.minX + 1;
            var worldHeight = corners.Value.maxY - corners.Value.minY + 1;

            var key = args.Key;
            var newCenter = center;

            var moveModifierX = worldGrid.Width / worldWidth * -1;
            var moveModifierY = worldGrid.Height / worldHeight * -1;

            switch (key)
            {
                case Key.W:
                    newCenter.y -= moveModifierY;
                    break;
                case Key.S:
                    newCenter.y += moveModifierY;
                    break;
                case Key.A:
                    newCenter.x -= moveModifierX;
                    break;
                case Key.D:
                    newCenter.x += moveModifierX;
                    break;
                default:
                    return;
            }

            center = newCenter;

            var transformMatrix = Matrix.CreateTranslation(center.x, center.y)
                .Append(new ScaleTransform(worldGridScale, worldGridScale).Value);
            worldGrid.RenderTransform = new MatrixTransform(transformMatrix);
            UpdateViewTextboxes();
        }

        private void RevealAreaCommand()
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

            var worldWidth = corners.Value.maxX - corners.Value.minX + 1;
            var worldHeight = corners.Value.maxY - corners.Value.minY + 1;
            var worldMoveAmountX = worldGrid.Width / worldWidth * -1;
            var worldMoveAmountY = worldGrid.Height / worldHeight;
            var xOffset = center.x / worldMoveAmountX;
            var yOffset = center.y / worldMoveAmountY;

            var totalTorenderedWorldSize = 1 / worldGridScale;
            var revealAreaWidth = worldWidth * totalTorenderedWorldSize;
            var revealAreaHeight = worldHeight * totalTorenderedWorldSize;

            var revealRadiusX = revealAreaWidth / 2;
            var revealRadiusY = revealAreaHeight / 2;
            var trueCenterX = corners.Value.minX + (worldWidth / 2);
            var trueCenterY = corners.Value.minY + (worldHeight / 2);

            RevealArea((
                (long)(trueCenterX + xOffset - revealRadiusX),
                (long)(trueCenterY + yOffset - revealRadiusY),
                (long)(trueCenterX + xOffset + revealRadiusX - 1),
                (long)(trueCenterY + yOffset + revealRadiusY - 1)
            ));
            RenderWorldArea();
        }

        private void ToggleLoggingWriteOutCommand()
        {
            PACSingletons.Instance.Logger.DefaultWriteOut = !PACSingletons.Instance.Logger.DefaultWriteOut;
            toggleLoggingWriteOut.Content = $"Toggle logging in console: {PACSingletons.Instance.Logger.DefaultWriteOut}";
        }

        private void ToggleBlendPopulationColorsCommand()
        {
            blendPopulationColors = !blendPopulationColors;
            toggleBlendPopulationColors.Content = $"Blend population colors: {blendPopulationColors}";

            if (SelectedSave)
            {
                RenderWorldArea();
            }
        }

        private void TerrainLayerCheckBoxIsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            RebuildLayersCommand();
        }

        private void StructureLayerCheckBoxIsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            RebuildLayersCommand();
        }
        
        private void PopulationLayerCheckBoxIsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            RebuildLayersCommand();
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
            terrainTypeCounts[tile.terrain.type] = terrainTypeCounts.TryGetValue(tile.terrain.type, out var value1) ? ++value1 : 1;
            structureTypeCounts[tile.structure.type] = structureTypeCounts.TryGetValue(tile.structure.type, out var value2) ? ++value2 : 1;

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
                terrainTypeCounts = [];
                structureTypeCounts = [];
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
                for (var x = minX; x <= maxX; x++)
                {
                    var cd = new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Star)
                    };
                    worldGrid.ColumnDefinitions.Add(cd);
                }

                // rows
                for (var y = minY; y <= maxY; y++)
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

            var loadingText = Tools.GetStandardLoadingText("Rendering...");
            loadingText.Display();
            var sumProgress = (double)World.Chunks.Count * PAConstants.CHUNK_SIZE * PAConstants.CHUNK_SIZE;
            var progress = 0;
            foreach (var chunk in World.Chunks)
            {
                foreach (var tile in chunk.Value.tiles)
                {
                    progress++;
                    loadingText.Value = progress / sumProgress;

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
                    else if (layers.Contains(VisibleTileLayer.Structure) && tileObj.structure.type != StructureType.NONE)
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

                    var color = VisualizerTools.GetLayerContentColor(tileObj, layer, blendPopulationColors);

                    var extraTerrainData = tileObj.terrain.TryGetExtraProperty("height", out var height) ?
                        $"(height: {height})" :
                        (tileObj.terrain.TryGetExtraProperty("depth", out var depth) ? $"(depth: {depth})" : "");
                    var tooltipContent = new StackPanel()
                    {
                        Children =
                        {
                            new Label() { Content = $"Position: ({chunk.Value.basePosition.x + tileObj.relativePosition.x}, {chunk.Value.basePosition.y + tileObj.relativePosition.y})" },
                            new Label() { Content = $"Chunk seed: {PATools.SerializeRandom(chunk.Value.ChunkRandomGenerator)}" },
                            new Label() { Content = $"Terrain: {tileObj.terrain.GetTypeName()} ({tileObj.terrain.Name}) {extraTerrainData}" },
                        }
                    };

                    if (tileObj.structure.type != StructureType.NONE)
                    {
                        var extraStructureData = tileObj.structure.TryGetExtraProperty("population", out var population) ? $"(population: {population})" : "";
                        tooltipContent.Children.Add(new Label() { Content = $"Structure: {tileObj.structure.GetTypeName()} ({tileObj.structure.Name}) {extraStructureData}" });
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
                        Background = new SolidColorBrush(color.ToAvaloniaColor()),
                        Content = contentName,
                        //ToolTip = new ToolTip()
                        //{
                        //    Content = tooltipContent,
                        //}
                    };
                    content.Tapped += (s, e) => OnWorldTileClick(tileObj);

                    var column = xPos - minX;
                    var row = worldGrid.RowDefinitions.Count - 1 - (yPos - minY);

                    Grid.SetColumn(content, (int)column);
                    Grid.SetRow(content, (int)row);
                    worldGrid.Children.Add(content);
                }
            }
            loadingText.StopLoadingStandard();

            IsWorldVisible = true;
            TileCountsNeedToBeRefreshed = false;
        }

        private void OnWorldTileClick(Tile tile)
        {
            var tileInfo = new TileInfoWindow(tile);
            tileInfo.ShowDialog(this);
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

            for (var x = corners.minX; x <= corners.maxX; x++)
            {
                for (var y = corners.minY; y <= corners.maxY; y++)
                {
                    World.TryGetChunk((x, y), out var chunk);
                    chunk.TryGetTile((x, y), out _);
                }
            }
            lastWorldChange = DateTime.Now;
            TileCountsNeedToBeRefreshed = true;
        }
        #endregion
    }
}
