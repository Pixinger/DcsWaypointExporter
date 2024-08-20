// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.Input;

namespace DcsWaypointExporter.Views
{
    public partial class MainWindow : Window
    {
        public ViewModels.MainWindow? ViewModel { get; }


        public MainWindow()
        {
            InitializeComponent();

            DataContext = ViewModel = new ViewModels.MainWindow();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (ViewModel is not null)
            {
                if (ViewModel.IsModified)
                {
                    var result = System.Windows.MessageBox.Show("Do you want to save before exit?", "Save?", System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Question, MessageBoxResult.No);
                    if (result == MessageBoxResult.Yes)
                    {
                        ViewModel.CommandSave.Execute(null);
                        if (ViewModel.IsModified)
                        {
                            // ups.. something went wrong.
                            e.Cancel = true;
                        }
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        #region public IRelayCommand CommandClose
        public IRelayCommand CommandClose => _commandClose ??= new RelayCommand(
            execute: () =>
            {
                Close();
            },
            canExecute: () =>
            {
                return true;
            });
        private RelayCommand? _commandClose;
        #endregion
    }
}
