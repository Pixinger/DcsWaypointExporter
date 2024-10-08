﻿// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Diagnostics;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DcsWaypointExporter.Enums;
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

        public DcsMaps? Map
        {
            get => _map;
            set
            {
                if (SetProperty(ref _map, value))
                {
                    CommandOk.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(MapName));
                }
            }
        }
        private DcsMaps? _map = null;
        public string MapName
        {
            get
            {
                if (Map is null)
                {
                    return string.Empty;
                }

                if (DcsMapsTools.TextDictionary.TryGetValue(Map.Value, out var text))
                {
                    return text;
                }

                return string.Empty;
            }
        }

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

                if (ImportService.Import(ImportText, out var map, out var mission))
                {
                    Map = map;
                    Mission = mission;
                    return true;
                }

                Map = null;
                Mission = null;
                return false;
            }
            catch (TaskCanceledException)
            {
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
                dialog.Filter = CustomResources.Language.DialogFileFilterDWE;
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
                            System.Windows.MessageBox.Show(string.Format(CustomResources.Language.TheFileSeemsToBeToLarge_UnableToOpenFilesLargerThan_ARG0_Mb, MAX_FILE_IMPORT_MB), CustomResources.Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            return;
                        }

                        var allText = File.ReadAllText(filename, new UTF8Encoding(false));
                        if (!ImportService.Import(allText, out var map, out var mission))
                        {
                            Map = null;
                            Mission = null;
                            System.Windows.MessageBox.Show(CustomResources.Language.TheFileContainsNoValidMissionData, CustomResources.Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            return;
                        }

                        // We need to bypass the ImportText property, as we want to have immediate update response (without debounce mechanic).
                        Debug.Assert(mission is not null, "other wise this bypass would be not allowed.");
                        OnPropertyChanging(nameof(ImportText));
                        _importText = allText;
                        Map = map;
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
                    System.Windows.MessageBox.Show(string.Format(CustomResources.Language.TheClipboardContentSeemsToBeToLarge_UnableToOpenContentLargerThan_ARG0_Mb), CustomResources.Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                if (!ImportService.Import(allText, out var map, out var mission))
                {
                    System.Windows.MessageBox.Show(CustomResources.Language.NoValidMissionDataInTheClipboardFound, CustomResources.Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    Map = null;
                    Mission = null;
                    return;
                }

                // We need to bypass the ImportText property, as we want to have immediate update response (without debounce mechanic).
                Debug.Assert(mission is not null, "other wise this bypass would be not allowed.");
                OnPropertyChanging(nameof(ImportText));
                _importText = allText;
                Map = map;
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
