// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using DcsWaypointExporter.Enums;
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

        bool IImportDialogService.Execute(ViewModels.ImportDialog viewModel, out DcsMaps? map, [NotNullWhen(true)] out PresetsLua.Mission? mission)
        {
            DataContext = ViewModel = viewModel;
            ViewModel.Close += OnViewModel_Close;

            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if ((ShowDialog() == true) && (ViewModel.Mission is not null))
            {
                map = viewModel.Map;
                mission = ViewModel.Mission;
                return true;
            }
            else
            {
                map = null;
                mission = null;
                return false;
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
