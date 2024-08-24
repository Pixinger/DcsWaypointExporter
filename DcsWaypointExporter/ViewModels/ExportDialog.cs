// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

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
    public class ExportDialog : ObservableObject
    {
        public event Action? Close;

        public string ExportedText { get; }
        public PresetsLua.Mission Mission { get; }
        public DcsMaps? Map
        {
            get => _map;
            set
            {
                if (SetProperty(ref _map, value))
                {
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


        public ExportDialog(DcsMaps map, PresetsLua.Mission mission)
        {
            Map = map;
            Mission = mission;

            if (!Ioc.Default.GetRequiredService<IMissionImportExport>().Export(map, mission, out var exportedText))
            {
                ExportedText = string.Empty;
                System.Windows.MessageBox.Show(CustomResources.Language.UnableToGenerateMissionExport, CustomResources.Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            Mission = mission;
            ExportedText = exportedText;
        }


        #region public IRelayCommand CommandClose
        public IRelayCommand CommandClose => _commandClose ??= new RelayCommand(
            execute: () =>
            {
                Close?.Invoke();
            },
            canExecute: () =>
            {
                return true;
            });
        private RelayCommand? _commandClose;
        #endregion

        #region public IRelayCommand CommandSaveToFile
        public IRelayCommand CommandSaveToFile => _commandSaveToFile ??= new RelayCommand(
            execute: () =>
            {
                if (string.IsNullOrWhiteSpace(ExportedText))
                {
                    return;
                }

                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Filter = CustomResources.Language.DialogFileFilterDWE;
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                // show and check for OK
                if (dialog.ShowDialog() == true)
                {
                    // Open and process file
                    var filename = dialog.FileName;

                    File.WriteAllText(filename, ExportedText, new UTF8Encoding(false));
                    Close?.Invoke();
                }
            },
            canExecute: () =>
            {
                if (string.IsNullOrWhiteSpace(ExportedText))
                {
                    return false;
                }

                return true;
            });
        private RelayCommand? _commandSaveToFile;
        #endregion

        #region public IRelayCommand CommandCopyToClipboard
        public IRelayCommand CommandCopyToClipboard => _commandCopyToClipboard ??= new RelayCommand(
            execute: () =>
            {
                if (string.IsNullOrWhiteSpace(ExportedText))
                {
                    return;
                }

                System.Windows.Clipboard.SetText(ExportedText);
                Close?.Invoke();
            },
            canExecute: () =>
            {
                if (string.IsNullOrWhiteSpace(ExportedText))
                {
                    return false;
                }

                return true;
            });
        private RelayCommand? _commandCopyToClipboard;
        #endregion
    }
}
