// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Windows;
using DcsWaypointExporter.Models;
using DcsWaypointExporter.Services.Dialogs;

namespace DcsWaypointExporter.Views
{
    public partial class ImportDialog : Window, IImportDialogService
    {
        public ViewModels.ImportDialog? ViewModel { get; private set; } = null;


        public ImportDialog()
        {
            InitializeComponent();
        }


        PresetsLua.Mission? IImportDialogService.Execute(ViewModels.ImportDialog viewModel)
        {
            DataContext = ViewModel = viewModel;
            ViewModel.Close += OnViewModel_Close;

            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (ShowDialog() == true)
            {
                return ViewModel.Mission;
            }
            else
            {
                return null;
            }
        }

        private void OnViewModel_Close(bool ok)
        {
            if (ok)
            {
                DialogResult = true;
            }

            Close();
        }
    }
}
