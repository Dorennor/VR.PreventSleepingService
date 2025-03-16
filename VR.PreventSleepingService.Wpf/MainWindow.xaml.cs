using System.Windows;

using Microsoft.Win32;

namespace VR.PreventSleepingService.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void OnAutoStart()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
                Hide();
            }

            PowerUtilities.PowerUtilities.PreventPowerSave();
        }
    }
}