using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace VR.KeepAwake.Views;

internal sealed partial class SettingsWindow : FluentWindow
{
    public SettingsWindow(AppController controller)
    {
        DataContext = controller;
        InitializeComponent();

        Loaded += (_, _) => SystemThemeWatcher.Watch(this);
        Closing += (_, _) => SystemThemeWatcher.UnWatch(this);
    }
}
