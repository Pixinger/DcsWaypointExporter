// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DcsWaypointExporter.ViewModels
{
    public class TextEditDialog : ObservableObject
    {
        public event Action<bool>? Close;

        public string Text
        {
            get => _text;
            set
            {
                if (SetProperty(ref _text, value))
                {
                    CommandOk.NotifyCanExecuteChanged();
                }
            }
        }
        private string _text;


        public TextEditDialog(string text)
        {
            _text = text;
        }


        #region public IRelayCommand CommandOk
        public IRelayCommand CommandOk => _commandOk ??= new RelayCommand(
            execute: () =>
            {
                Close?.Invoke(true);
            },
            canExecute: () =>
            {
                if (string.IsNullOrWhiteSpace(Text))
                {
                    return false;
                }

                return true;
            });
        private RelayCommand? _commandOk;
        #endregion

        #region public IRelayCommand CommandCancel
        public IRelayCommand CommandCancel => _commandCancel ??= new RelayCommand(
            execute: () =>
            {
                Close?.Invoke(false);
            },
            canExecute: () =>
            {
                return true;
            });
        private RelayCommand? _commandCancel;
        #endregion
    }
}
