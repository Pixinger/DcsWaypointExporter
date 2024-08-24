// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Windows;
using DcsWaypointExporter.Services.Dialogs;

namespace DcsWaypointExporter.Views
{
    /// <summary>
    /// Interaktionslogik für TextEditDialog.xaml
    /// </summary>
    public partial class TextEditDialog : Window, ITextEditDialog
    {
        public ViewModels.TextEditDialog? ViewModel { get; private set; } = null;


        public TextEditDialog()
        {
            InitializeComponent();
        }


        string? ITextEditDialog.Execute(ViewModels.TextEditDialog viewModel)
        {
            DataContext = ViewModel = viewModel;
            ViewModel.Close += (r) =>
            {
                DialogResult = r;
                Close();
            };

            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if ((ShowDialog() == true) && (!string.IsNullOrWhiteSpace(ViewModel.Text)))
            {
                return ViewModel.Text;
            }
            else
            {
                return null;
            }
        }
    }
}
