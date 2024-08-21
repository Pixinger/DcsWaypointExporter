// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Windows;
using DcsWaypointExporter.Services.Dialogs;

namespace DcsWaypointExporter.Views
{
    public partial class ExportDialog : Window, IExportDialogService
    {
        public ViewModels.ExportDialog? ViewModel { get; private set; } = null;


        public ExportDialog()
        {
            InitializeComponent();
        }


        public bool Execute(ViewModels.ExportDialog viewModel)
        {
            DataContext = ViewModel = viewModel;
            ViewModel.Close += OnViewModel_Close;

            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            return ShowDialog() == true;
        }

        private void OnViewModel_Close() => Close();
    }
}
