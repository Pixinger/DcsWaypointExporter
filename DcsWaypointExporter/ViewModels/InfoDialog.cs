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

        public string? Version { get; }


        public InfoDialog(string? version)
        {
            Version = version;
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
