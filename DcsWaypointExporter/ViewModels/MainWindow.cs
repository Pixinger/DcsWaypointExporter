// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DcsWaypointExporter.Enums;
using DcsWaypointExporter.Models;
using DcsWaypointExporter.Services;
using DcsWaypointExporter.Services.Dialogs;

namespace DcsWaypointExporter.ViewModels
{
    public class MainWindow : ObservableObject
    {
        #region nLog instance (s_log)
        private static NLog.Logger s_log { get; } = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        #region Lazycut's: Service() / RequiredService()
        /// <summary>
        /// I'm just lazy to write ...
        /// </summary>
        private static T? Service<T>() where T : class => Ioc.Default.GetService<T>();
        /// <summary>
        /// I'm just lazy to write ...
        /// </summary>
        private static T RequiredService<T>() where T : class => Ioc.Default.GetRequiredService<T>();
        #endregion

        #region IMainThreadInvoker? MainThreadInvoker { get; }
        private IMainThreadInvoker? MainThreadInvoker { get; } = RequiredService<IMainThreadInvoker>();
        #endregion

        public Enums.DcsFiles SelectedMap
        {
            get => _selectedMap;
            set
            {
                if (SetProperty(ref _selectedMap, value))
                {
                    PresetsLua = RequiredService<IPresetsLuaSerializer>().DeserializeFromFile(SelectedMap);
                }
            }
        }
        private Enums.DcsFiles _selectedMap = (Enums.DcsFiles)(-1); // This makes the value invalid and therefore it will respond to any setter-value from the CTOR.

        public Models.PresetsLua? PresetsLua
        {
            get => _presetsLua;
            private set
            {
                if (SetProperty(ref _presetsLua, value))
                {
                    UpdateAvailableMissions();
                }
            }
        }
        private Models.PresetsLua? _presetsLua = null;

        public ObservableCollection<Models.PresetsLua.Mission> AvailableMissions { get; } = [];
        public Models.PresetsLua.Mission? SelectedMission
        {
            get => _selectedMission;
            private set
            {
                if (SetProperty(ref _selectedMission, value))
                {
                }
            }
        }
        private Models.PresetsLua.Mission? _selectedMission = null;

        public bool IsModified
        {
            get => _isModified;
            set
            {
                if (SetProperty(ref _isModified, value))
                {
                    CommandSave.NotifyCanExecuteChanged();
                }
            }
        }
        private bool _isModified = false;


        public MainWindow()
        {
            // Make sure we have a valid Saved-Games-Dcs folder detected.
            string? folder = DcsFilesTools.DcsSavedGames;
            if (string.IsNullOrWhiteSpace(folder))
            {
                System.Windows.MessageBox.Show("Unable to autodetect the SavedGames-Dcs folder. Please specify it manually in the appsettings.json.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            else
            {
                SelectedMap = Enums.DcsFiles.Caucasus;
            }
        }


        private void UpdateAvailableMissions()
        {
            AvailableMissions.Clear();

            if (PresetsLua is not null)
            {
                foreach (var missionPair in PresetsLua.Missions)
                {
                    AvailableMissions.Add(missionPair.Value);
                }
            }
        }

        #region public IRelayCommand CommandImport
        public IRelayCommand CommandImport => _commandImport ??= new RelayCommand(
            execute: () =>
            {
                if (PresetsLua is null)
                {
                    return;
                }

                // Import dialog
                var importedMission = RequiredService<Services.Dialogs.IImportDialogService>().Execute(new ViewModels.ImportDialog());
                if (importedMission is not null)
                {
                    // Change name in case it was used before.
                    #region string getFreeName()
                    string getFreeName()
                    {
                        var baseName = importedMission.Name;
                        if (!PresetsLua.Missions.ContainsKey(baseName))
                        {
                            return baseName;
                        }

                        var i = 1;
                        while (true)
                        {
                            var name = string.Format("{0}-{1}", baseName, i++);
                            if (!PresetsLua.Missions.ContainsKey(name))
                            {
                                return name;
                            }
                        }
                    }
                    #endregion
                    var name = getFreeName();
                    importedMission.Name = name;

                    // Add mission to presets-lua.
                    PresetsLua.Missions.Add(name, importedMission);

                    // Update UI
                    UpdateAvailableMissions();
                    IsModified = true;
                }
            },
            canExecute: () =>
            {
                if (PresetsLua is null)
                {
                    return false;
                }

                return true;
            });
        private RelayCommand? _commandImport;
        #endregion

        #region public IRelayCommand CommandExport
        public IRelayCommand<Models.PresetsLua.Mission> CommandExport => _commandExport ??= new RelayCommand<Models.PresetsLua.Mission>(
            execute: (mission) =>
            {
                if (PresetsLua is null)
                {
                    return;
                }

                if (mission is null)
                {
                    return;
                }

                RequiredService<IExportDialogService>().Execute(viewModel: new ViewModels.ExportDialog(mission));
            },
            canExecute: (mission) =>
            {
                if (PresetsLua is null)
                {
                    return false;
                }

                if (mission is null)
                {
                    return false;
                }

                return true;
            });
        private RelayCommand<Models.PresetsLua.Mission>? _commandExport;
        #endregion

        #region public IRelayCommand CommandDelete
        public IRelayCommand<Models.PresetsLua.Mission> CommandDelete => _commandDelete ??= new RelayCommand<Models.PresetsLua.Mission>(
            execute: (mission) =>
            {
                if (PresetsLua is null)
                {
                    return;
                }

                if (mission is null)
                {
                    return;
                }

                if (!PresetsLua.Missions.Remove(mission.Name))
                {
                    System.Windows.MessageBox.Show("Unable to delete selected mission.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                // Update UI
                UpdateAvailableMissions();
                IsModified = true;
            },
            canExecute: (mission) =>
            {
                if (PresetsLua is null)
                {
                    return false;
                }

                if (mission is null)
                {
                    return false;
                }

                return true;
            });
        private RelayCommand<Models.PresetsLua.Mission>? _commandDelete;
        #endregion

        #region public IRelayCommand CommandSave
        public IRelayCommand CommandSave => _commandSave ??= new RelayCommand(
            execute: () =>
            {
                if (!IsModified)
                {
                    return;
                }

                if (PresetsLua is null)
                {
                    return;
                }

                if (!RequiredService<IPresetsLuaSerializer>().SerializeToFile(PresetsLua, SelectedMap))
                {
                    System.Windows.MessageBox.Show("Unable to save modifications.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                IsModified = false;
            },
            canExecute: () =>
            {
                if (!IsModified)
                {
                    return false;
                }

                if (PresetsLua is null)
                {
                    return false;
                }

                return true;
            });
        private RelayCommand? _commandSave;
        #endregion
    }
}
