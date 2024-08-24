// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Windows;
using DcsWaypointExporter.Services.Dialogs;

namespace DcsWaypointExporter.Views
{
    public partial class FolderSetupDialog : Window, IFolderSetupService
    {
        public ViewModels.FolderSetupDialog? ViewModel { get; private set; } = null;


        public FolderSetupDialog()
        {
            InitializeComponent();
        }


        public bool Execute(ViewModels.FolderSetupDialog viewModel)
        {
            DataContext = ViewModel = viewModel;
            ViewModel.Close += (r) => { DialogResult = r; Close(); };

            // not yet available: Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            return ShowDialog() == true;
        }
    }
}
