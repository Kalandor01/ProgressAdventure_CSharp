using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using PACommon.Enums;
using PAVisualizer.Culture;
using ProgressAdventure.EntityManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement;
using System.Collections.Generic;
using System.Linq;

namespace PAVisualizer.Windows
{
    internal class TileInfoWindow : Window
    {
        #region Control fields
        private Label _structureLabel;
        private Label _populationLabel;
        private ComboBox _loadedPopulationComboBox;
        private Label _loadedEntityLabel;
        #endregion

        #region Private fields
        private readonly List<Entity> _loadedEntities = [];
        #endregion

        #region Constructors
        public TileInfoWindow(Tile tile)
            :base()
        {
            Title = CultureManager.GetCultureString(CultureKey.MainWindowTitle);
            Width = 800;
            Height = 450;
            Content = SetupMainGrid(tile);

            if (tile.structure.type != StructureType.NONE)
            {
                var extraStructureData = tile.structure.TryGetExtraProperty("population", out var population) ? $"(population: {population})" : "";
                _structureLabel.Content = $"Structure: {tile.structure.GetTypeName()} ({tile.structure.Name}) {extraStructureData}";
            }

            if (tile.populationManager.PopulationCount == 0)
            {
                _loadedPopulationComboBox.IsVisible = false;
                return;
            }

            var popManager = tile.populationManager;
            var populationCounts = popManager.ContainedEntities.ToDictionary(k => k, eType => (tCount: popManager.GetEntityCount(eType, out var uCount), uCount));
            _populationLabel.Content = $"Population:\n\t{string.Join("\n\t", populationCounts.Select(e => $"{e.Key}: {e.Value.tCount} ({e.Value.uCount})"))}";

            var fieldInfo = typeof(PopulationManager).GetField("loadedEntities", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var loadedEntitiesDict = (Dictionary<EnumValue<EntityType>, List<Entity>>)fieldInfo!.GetValue(tile.populationManager)!;
            _loadedEntities = [.. loadedEntitiesDict.SelectMany(k => k.Value)];

            if (_loadedEntities.Count == 0)
            {
                _loadedPopulationComboBox.IsVisible = false;
                return;
            }

            _loadedPopulationComboBox.SelectionChanged += LoadedEntitySelected;
            foreach (var loadedEntity in _loadedEntities)
            {
                _loadedPopulationComboBox.Items.Add($"{loadedEntity.FullName} ({loadedEntity.type})");
            }
        }
        #endregion

        #region SetupMethods
        private StackPanel SetupStackPanel(Tile tile)
        {
            var extraTerrainData = tile.terrain.TryGetExtraProperty("height", out var height)
                ? $"(height: {height})"
                : (tile.terrain.TryGetExtraProperty("depth", out var depth) ? $"(depth: {depth})" : "");

            return new StackPanel()
            .Row(0)
            .Children(
                new Label
                {
                    Content = $"Position: ({tile.populationManager.absolutePosition.x}, {tile.populationManager.absolutePosition.y})"
                },
                new Label
                {
                    Content = $"Terrain: {tile.terrain.GetTypeName()} ({tile.terrain.Name}) {extraTerrainData}"
                },
                new Label().SplitInline(out _structureLabel),
                new Label().SplitInline(out _populationLabel),
                new ComboBox().SplitInline(out _loadedPopulationComboBox)
            );
        }

        //<Label x:Name="LoadedEntityLabel"/>
        private ScrollViewer SetupScrollViewer()
        {
            return new ScrollViewer
            {
                Content = new Label().SplitInline(out _loadedEntityLabel)
            }
            .Row(1);
        }

        private Grid SetupMainGrid(Tile tile)
        {
            return new Grid
            {
                RowDefinitions = [
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Star),
                ],
            }
            .Children(
                SetupStackPanel(tile),
                SetupScrollViewer()
            );
        }
        #endregion

        private void LoadedEntitySelected(object? sender, SelectionChangedEventArgs e)
        {
            var index = _loadedPopulationComboBox.SelectedIndex;
            _loadedEntityLabel.Content = _loadedEntities[index].ToString();
        }
    }
}
