using System.Configuration;
using System.Data;
using System.Windows;
using WellSoft.Services;
using WellSoft.ViewModels;

namespace WellSoft
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IFileDialogService fileDialogService = new FileDialogService();
            IWellDataService wellDataService = new WellDataService();
            var mainViewModel = new MainViewModel(wellDataService, fileDialogService);

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            mainWindow.Show();
        }
    }
}