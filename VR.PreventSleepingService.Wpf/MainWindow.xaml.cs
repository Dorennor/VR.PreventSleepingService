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

        private void SwitchAutoStart_OnToggled(object sender, RoutedEventArgs e)
        {
            const string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            var key = Registry.CurrentUser.OpenSubKey(path, true);

            if (key == null)
                return;

            key.SetValue("VR.PreventSleepingService", System.Reflection.Assembly.GetExecutingAssembly().Location + " /AutoStart");
        }

        private void SwitchAutoStart_OnUntoggled(object sender, RoutedEventArgs e)
        {
            const string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            var key = Registry.CurrentUser.OpenSubKey(path, true);

            if (key == null)
                return;

            key.DeleteValue("VR.PreventSleepingService", false);
        }

        public void OnAutoStart()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
                Hide();
            }
            else
            {
                Show();
            }

            PowerUtilities.PowerUtilities.PreventPowerSave();
        }
    }
}