using System.Windows;
using System.Windows.Controls;

namespace PAVisualizer
{
    /// <summary>
    /// Interaction logic for SaveInfoWindow.xaml
    /// </summary>
    public partial class SaveInfoWindow : Window
    {
        #region Public constructors
        public SaveInfoWindow()
        {
            InitializeComponent();

            saveInfoTextBox.Text = VisualizerTools.GetDisplayGeneralSaveData();
            saveInfoTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        }
        #endregion
    }
}
