using System.Windows;
using System.Windows.Controls;

namespace PAVisualizer
{
    /// <summary>
    /// Interaction logic for WorldInfoWindow.xaml
    /// </summary>
    public partial class WorldInfoWindow : Window
    {
        public WorldInfoWindow(string worldInfo)
        {
            InitializeComponent();

            worldInfoTextBox.Text = worldInfo;
            worldInfoTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        }
    }
}
