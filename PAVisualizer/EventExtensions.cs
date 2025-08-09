using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PAVisualizer
{
    public static class EventExtensions
    {
        public static CheckBox SetIsCheckedChangedEvent(this CheckBox control, EventHandler<RoutedEventArgs> handler)
        {
            control.IsCheckedChanged += handler;
            return control;
        }
    }
}
