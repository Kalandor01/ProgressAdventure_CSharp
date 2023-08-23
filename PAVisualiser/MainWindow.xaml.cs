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
            [ContentType.Terrain.MOUNTAIN] = Constants.Colors.BROWN,
        };
        #endregion

        #region Public constructors
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        #endregion

        #region Commands
        public void SelectSaveCommand(object sender, RoutedEventArgs e)
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

            GenerateWorldMap();
        }

        public void CloseSaveCommand(object sender, RoutedEventArgs e)
        {
            ClearWorldGrid();
        }
        #endregion

        #region Private Methods
        private void ClearWorldGrid()
        {
            worldGrid.ColumnDefinitions.Clear();
            worldGrid.RowDefinitions.Clear();
            worldGrid.Children.Clear();
        }

        private void GenerateWorldMap()
        {
            // clear
            ClearWorldGrid();

            // corners
            var corners = World.GetCorners();

            if (corners is null)
            {
                return;
            }

            long minX = corners.Value.minX;
            long minY = corners.Value.minY;
            long maxX = corners.Value.maxX;
            long maxY = corners.Value.maxY;

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

            foreach (var chunk in World.Chunks)
            {
                foreach (var tile in chunk.Value.tiles)
                {
                    var content = new Button()
                    {
                        Background = new SolidColorBrush(contentSubtypeColorMap.TryGetValue(tile.Value.terrain.subtype, out Color color) ? color : Constants.Colors.RED),
                        Content = tile.Value.terrain.Name
                    };

                    var column = chunk.Value.basePosition.y + tile.Value.relativePosition.y - minY;
                    var row = chunk.Value.basePosition.x + tile.Value.relativePosition.x - minX;

                    Grid.SetColumn(content, (int)column);
                    Grid.SetRow(content, (int)row);
                    worldGrid.Children.Add(content);
                }
            }
        }
        #endregion
    }
}
