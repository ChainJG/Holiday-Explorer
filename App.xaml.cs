using Holiday_Explorer.MVVM.Windows;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Holiday_Explorer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
