using Avalonia.Controls;

namespace PAVisualizer;

public partial class WorldInfoWindow : Window
{
    public WorldInfoWindow(string worldInfo)
    {
        InitializeComponent();

        worldInfoTextBox.Text = worldInfo;
    }
}