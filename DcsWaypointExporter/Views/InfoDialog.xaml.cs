// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Diagnostics;
using System.Windows;
using DcsWaypointExporter.Services.Dialogs;

namespace DcsWaypointExporter.Views
{
    public partial class InfoDialog : Window, IInfoDialogService
    {
        public ViewModels.InfoDialog? ViewModel { get; private set; } = null;


        public InfoDialog()
        {
            InitializeComponent();
        }


        bool IInfoDialogService.Execute(ViewModels.InfoDialog viewModel)
        {
            DataContext = ViewModel = viewModel;
            ViewModel.Close += OnViewModel_Close;

            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            return ShowDialog() == true;
        }

        private void OnViewModel_Close(bool ok)
        {
            if (ok)
            {
                DialogResult = true;
            }

            Close();
        }

        private void OnHyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
