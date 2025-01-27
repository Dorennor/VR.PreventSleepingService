using System.Windows;

namespace VR.PreventSleepingService.Wpf
{
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            var isAutoStart = true;

            //for (int i = 0; i != e.Args.Length; ++i)
            //{
            //    if (e.Args[i] == "/AutoStart")
            //    {
            //        isAutoStart = true;
            //    }
            //}

            MainWindow mainWindow = new MainWindow();

            if (isAutoStart)
            {
                mainWindow.WindowState = WindowState.Minimized;
            }

            mainWindow.OnAutoStart();
        }
    }
}