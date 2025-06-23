using Avalonia.Controls;

namespace PAVisualizer;

public partial class SaveInfoWindow : Window
{
    public SaveInfoWindow()
    {
        InitializeComponent();

        saveInfoTextBox.Text = VisualizerTools.GetDisplayGeneralSaveData();
    }
}