using PACommon.Enums;
using ProgressAdventure.EntityManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement;
using ProgressAdventure.WorldManagement.Content;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PAVisualizer
{
    /// <summary>
    /// Interaction logic for TileInfoWindow.xaml
    /// </summary>
    public partial class TileInfoWindow : Window
    {
        private readonly List<Entity> _loadedEntities = [];

        public TileInfoWindow(Tile tile)
        {
            InitializeComponent();

            var extraTerrainData = tile.terrain.TryGetExtraProperty("height", out var height)
                ? $"(height: {height})"
                : (tile.terrain.TryGetExtraProperty("depth", out var depth) ? $"(depth: {depth})" : "");

            PositionLabel.Content = $"Position: ({tile.populationManager.absolutePosition.x}, {tile.populationManager.absolutePosition.y})";
            TerrainLabel.Content = $"Terrain: {tile.terrain.GetSubtypeName()} ({tile.terrain.Name}) {extraTerrainData}";

            if (tile.structure.subtype != ContentType.Structure.NONE)
            {
                var extraStructureData = tile.structure.TryGetExtraProperty("population", out var population) ? $"(population: {population})" : "";
                StructureLabel.Content = $"Structure: {tile.structure.GetSubtypeName()} ({tile.structure.Name}) {extraStructureData}";
            }

            if (tile.populationManager.PopulationCount == 0)
            {
                LoadedPopulationComboBox.Visibility = Visibility.Collapsed;
                return;
            }

            var popManager = tile.populationManager;
            var populationCounts = popManager.ContainedEntities.ToDictionary(k => k, eType => (tCount: popManager.GetEntityCount(eType, out var uCount), uCount));
            PopulationLabel.Content = $"Population:\n\t{string.Join("\n\t", populationCounts.Select(e => $"{e.Key}: {e.Value.tCount} ({e.Value.uCount})"))}";

            var fieldInfo = typeof(PopulationManager).GetField("loadedEntities", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var loadedEntitiesDict = (Dictionary<EnumValue<EntityType>, List<Entity>>)fieldInfo!.GetValue(tile.populationManager)!;
            _loadedEntities = loadedEntitiesDict.SelectMany(k => k.Value).ToList();

            if (_loadedEntities.Count == 0)
            {
                LoadedPopulationComboBox.Visibility = Visibility.Collapsed;
                return;
            }

            LoadedPopulationComboBox.SelectionChanged += LoadedEntitySelected;
            foreach (var loadedEntity in _loadedEntities)
            {
                LoadedPopulationComboBox.Items.Add($"{loadedEntity.FullName} ({loadedEntity.type})");
            }
        }

        private void LoadedEntitySelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var index = LoadedPopulationComboBox.SelectedIndex;
            LoadedEntityLabel.Content = _loadedEntities[index].ToString();
        }
    }
}
