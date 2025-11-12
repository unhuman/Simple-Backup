using System.Configuration;
using System.Data;
using System.Windows;

namespace Simple_Backup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show(
                $"An unhandled exception occurred:\n\n{e.Exception.GetType().Name}\n\n{e.Exception.Message}\n\nStackTrace:\n{e.Exception.StackTrace}",
                "Unhandled Exception",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = false;
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            System.Windows.MessageBox.Show(
                $"An unhandled domain exception occurred:\n\n{ex?.GetType().Name}\n\n{ex?.Message}\n\nStackTrace:\n{ex?.StackTrace}",
                "Unhandled Domain Exception",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
