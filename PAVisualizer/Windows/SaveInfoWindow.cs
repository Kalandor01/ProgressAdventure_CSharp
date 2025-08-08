using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using PAVisualizer.Culture;

namespace PAVisualizer.Windows
{
    internal class SaveInfoWindow : Window
    {
        #region Constructors
        public SaveInfoWindow()
            :base()
        {
            Title = CultureManager.GetCultureString(CultureKey.SaveInfoWindowTitle);
            Width = 800;
            Height = 450;
            Content = SetupMainGrid();
        }
        #endregion

        #region SetupMethods
        private static Grid SetupMainGrid()
        {
            return new Grid()
            .Children(
                new TextBox
                {
                    IsReadOnly = true,
                    Text = VisualizerTools.GetDisplayGeneralSaveData(),
                }
            );
        }
        #endregion
    }
}
