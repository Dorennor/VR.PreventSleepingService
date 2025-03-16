using System.Windows;

namespace VR.PreventSleepingService.Wpf
{
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow();

            mainWindow.WindowState = WindowState.Minimized;
            mainWindow.OnAutoStart();
        }
    }
}