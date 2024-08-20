// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Diagnostics;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DcsWaypointExporter.Models;
using DcsWaypointExporter.Services;

namespace DcsWaypointExporter.ViewModels
{
    public class ImportDialog : ObservableObject
    {
        #region nLog instance (s_log)
        private static NLog.Logger s_log { get; } = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        private const int MAX_FILE_IMPORT_MB = 5;
        private const int DEBOUNCE_TIME_MS = 1000;

        public event Action<bool>? Close;

        private IMissionImportExport ImportService { get; } = Ioc.Default.GetRequiredService<IMissionImportExport>();

        public string? ImportText
        {
            get => _importText;
            set
            {
                if (SetProperty(ref _importText, value))
                {
                    _ = SerializeDebouncedAsync();
                }
            }
        }
        private string? _importText = string.Empty;

        public PresetsLua.Mission? Mission
        {
            get => _mission;
            set
            {
                if (SetProperty(ref _mission, value))
                {
                    CommandOk.NotifyCanExecuteChanged();
                }
            }
        }
        private PresetsLua.Mission? _mission = null;

        private CancellationTokenSource? _debounceTokenSource;


        public ImportDialog()
        {
        }


        private async Task<bool> SerializeDebouncedAsync()
        {
            // Cancel the last running task. We will start a new one, with a new timer.
            _debounceTokenSource?.Cancel();

            // Create a new token
            _debounceTokenSource = new CancellationTokenSource();
            var token = _debounceTokenSource.Token;

            try
            {
                await Task.Delay(DEBOUNCE_TIME_MS, token);

                Mission = ImportService.Import(ImportText);
                s_log.Debug("Checked");
                return true; // Executed
            }
            catch (TaskCanceledException)
            {
                s_log.Debug("Aborted");
                return false; // not executed
            }
        }

        #region public IRelayCommand CommandOk
        public IRelayCommand CommandOk => _commandOk ??= new RelayCommand(
            execute: () =>
            {
                if (Mission is null)
                {
                    return;
                }

                Close?.Invoke(true);
            },
            canExecute: () =>
            {
                if (Mission is null)
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
                Mission = null; // just to be sure.
                Close?.Invoke(false);
            },
            canExecute: () =>
            {
                return true;
            });
        private RelayCommand? _commandCancel;
        #endregion

        #region public IRelayCommand CommandLoadFromFile
        public IRelayCommand CommandLoadFromFile => _commandLoadFromFile ??= new RelayCommand(
            execute: () =>
            {
                Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.Filter = "DCS-Waypoint-Export file (*.dwe)|*.dwe|All files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                // show and check for OK
                if (dialog.ShowDialog() == true)
                {
                    // Open and process file
                    var filename = dialog.FileName;

                    if (File.Exists(filename))
                    {
                        FileInfo fileInfo = new FileInfo(filename);
                        if (fileInfo.Length / (1024 * 1024) > MAX_FILE_IMPORT_MB)
                        {
                            System.Windows.MessageBox.Show($"The file seems to be to large. Unable to open files larger than {MAX_FILE_IMPORT_MB}MB.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            return;
                        }

                        var allText = File.ReadAllText(filename, new UTF8Encoding(false));
                        var mission = ImportService.Import(allText);
                        if (mission is null)
                        {
                            System.Windows.MessageBox.Show("The file contains no valid mission data.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            return;
                        }

                        // We need to bypass the ImportText property, as we want to have immediate update response (without debounce mechanic).
                        Debug.Assert(mission is not null, "other wise this bypass would be not allowed.");
                        OnPropertyChanging(nameof(ImportText));
                        _importText = allText;
                        Mission = mission;
                        OnPropertyChanged(nameof(ImportText));
                    }
                }
            },
            canExecute: () =>
            {
                return true;
            });
        private RelayCommand? _commandLoadFromFile;
        #endregion

        #region public IRelayCommand CommandLoadFromClipboard
        public IRelayCommand CommandLoadFromClipboard => _commandLoadFromClipboard ??= new RelayCommand(
            execute: () =>
            {
                var allText = System.Windows.Clipboard.GetText();

                if (allText.Length / (1024 * 1024) > MAX_FILE_IMPORT_MB)
                {
                    System.Windows.MessageBox.Show($"The clipboard content seems to be to large. Unable to open content larger than {MAX_FILE_IMPORT_MB}MB.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                var mission = ImportService.Import(allText);
                if (mission is null)
                {
                    System.Windows.MessageBox.Show("No valid mission data in the clipboard found.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                // We need to bypass the ImportText property, as we want to have immediate update response (without debounce mechanic).
                Debug.Assert(mission is not null, "other wise this bypass would be not allowed.");
                OnPropertyChanging(nameof(ImportText));
                _importText = allText;
                Mission = mission;
                OnPropertyChanged(nameof(ImportText));
            },
            canExecute: () =>
            {
                return true;
            });
        private RelayCommand? _commandLoadFromClipboard;
        #endregion
    }
}
