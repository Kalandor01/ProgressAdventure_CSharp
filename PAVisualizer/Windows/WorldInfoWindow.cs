using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using PAVisualizer.Culture;

namespace PAVisualizer.Windows
{
    internal class WorldInfoWindow : Window
    {
        #region Constructors
        public WorldInfoWindow(string worldInfo)
            :base()
        {
            Title = CultureManager.GetCultureString(CultureKey.WorldInfoWindowTitle);
            Width = 800;
            Height = 450;
            Content = SetupMainGrid(worldInfo);
        }
        #endregion

        #region SetupMethods
        private static Grid SetupMainGrid(string worldInfo)
        {
            return new Grid()
            .Children(
                new TextBox
                {
                    IsReadOnly = true,
                    Text = worldInfo,
                }
            );
        }
        #endregion
    }
}
