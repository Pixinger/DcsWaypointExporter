// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DcsWaypointExporter.ViewModels
{
    public class InfoDialog : ObservableObject
    {
        #region nLog instance (s_log)
        private static NLog.Logger s_log { get; } = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        public event Action<bool>? Close;

        public Version Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }
        private Version _version = new Version();


        public InfoDialog()
        {
        }


        #region public IRelayCommand CommandOk
        public IRelayCommand CommandOk => _commandOk ??= new RelayCommand(
            execute: () =>
            {
                Close?.Invoke(true);
            },
            canExecute: () =>
            {
                return true;
            });
        private RelayCommand? _commandOk;
        #endregion

    }
}
