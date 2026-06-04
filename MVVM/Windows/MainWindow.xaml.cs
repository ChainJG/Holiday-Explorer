using Holiday_Explorer.Core;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Holiday_Explorer.MVVM.Windows
{
    public partial class MainWindow : Window
    {
        #region Application Functions
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWind, int wMsg, int wParam, int lParam);
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void BtnMinimiseWindow_Click(object sender, RoutedEventArgs e) =>
            this.WindowState = WindowState.Minimized;

        private void BtnExitApplication_Click(object sender, RoutedEventArgs e) =>
            HolidayExplorerServices.Shutdown();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}