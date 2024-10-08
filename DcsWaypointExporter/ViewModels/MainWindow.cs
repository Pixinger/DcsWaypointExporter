﻿// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
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

        public Enums.DcsMaps SelectedMap
        {
            get => _selectedMap;
            set
            {
                if (SelectedMap == value)
                {
                    return;
                }

                // Save existing modifications
                if (IsModified)
                {
                    var result = MessageBox.Show(CustomResources.Language.TheMapHasUnsavedModifications,
                        CustomResources.Language.Question,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.Yes);

                    if ((result == MessageBoxResult.Yes) && (PresetsLua is not null))
                    {
                        IsModified = false;
                        if (!RequiredService<IPresetsLuaSerializer>().SerializeToFile(PresetsLua, SelectedMap))
                        {
                            System.Windows.MessageBox.Show(CustomResources.Language.UnableToSaveModifications, CustomResources.Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                // Now we can change the property.
                if (SetProperty(ref _selectedMap, value))
                {
                    PresetsLua = RequiredService<IPresetsLuaSerializer>().DeserializeFromFile(SelectedMap);
                }
            }
        }
        private Enums.DcsMaps _selectedMap = (Enums.DcsMaps)(-1); // This makes the value invalid and therefore it will respond to any setter-value from the CTOR.

        public Models.PresetsLua? PresetsLua
        {
            get => _presetsLua;
            private set
            {
                if (SetProperty(ref _presetsLua, value))
                {
                    IsModified = false;
                    UpdateAvailableMissions();
                    CommandImport.NotifyCanExecuteChanged();
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

        public Version Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }
        private Version _version = new Version();

        public string? ProductVersion
        {
            get => _productVersion;
            set => SetProperty(ref _productVersion, value);
        }
        private string? _productVersion = string.Empty;

        public bool HasUpdate
        {
            get => _hasUpdate;
            set
            {
                Debug.Assert(MainThreadInvoker?.CheckAccess() == true, "Only use on UI Thread!");
                SetProperty(ref _hasUpdate, value);
            }
        }
        private bool _hasUpdate = false;


        public MainWindow()
        {
            // Make sure we have a valid Saved-Games-Dcs folder detected.
            var settings = RequiredService<ISettingsService>();
            var folder = settings.DcsSavedGames;
            if (string.IsNullOrWhiteSpace(folder))
            {
                System.Windows.MessageBox.Show(CustomResources.Language.UnableToDetectSavedGamesFolder, CustomResources.Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            else
            {
                SelectedMap = Enums.DcsMaps.Caucasus;
            }

            #region function: static Version getAssemblyVersion()
            static Version getAssemblyVersion()
            {
                try
                {
                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    if (assembly is null)
                    {
                        return new Version();
                    }

                    var name = assembly.GetName();
                    if (name is null)
                    {
                        return new Version();
                    }

                    var version = name.Version;
                    if (version is null)
                    {
                        return new Version();
                    }

                    return (Version)version.Clone();
                }
                catch
                {
                    return new Version();
                }
            }
            #endregion
            Version = getAssemblyVersion();

            #region static string? getAssemblyProductVersion()
            static string? getAssemblyProductVersion()
            {
                var assemblyPath = Assembly.GetEntryAssembly()?.Location;
                if (assemblyPath is null)
                {
                    return null;
                }

                var filename = new FileInfo(assemblyPath).FullName;
                if (!System.IO.File.Exists(filename))
                {
                    return null;
                }

                try
                {
                    var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(filename);
                    return fileVersionInfo.ProductVersion;
                }
                catch
                {
                    return null;
                }
            }
            #endregion
            ProductVersion = getAssemblyProductVersion();

            #region RequiredService<IUpdateValidationService>().HasUpdateAsync()
            RequiredService<IUpdateValidationService>().HasUpdateAsync().ContinueWith(
                (t) =>
                {
                    MainThreadInvoker.BeginInvoke(
                        () =>
                        {
                            HasUpdate = t.Result;
                        });
                });
            #endregion
        }


        private void UpdateAvailableMissions()
        {
            AvailableMissions.Clear();

            if (PresetsLua is not null)
            {
                var missions = PresetsLua.Missions.Values.OrderBy(x => x.Name);
                foreach (var mission in missions)
                {
                    AvailableMissions.Add(mission);
                }
            }
        }

        #region public IRelayCommand CommandInfo
        public IRelayCommand CommandInfo => _commandInfo ??= new RelayCommand(
            execute: () =>
            {
                RequiredService<IInfoDialogService>().Execute(new InfoDialog(ProductVersion));
            },
            canExecute: () =>
            {
                return true;
            });
        private RelayCommand? _commandInfo;
        #endregion

        #region public IRelayCommand CommandImport
        public IRelayCommand CommandImport => _commandImport ??= new RelayCommand(
            execute: () =>
            {
                if (PresetsLua is null)
                {
                    return;
                }

                // Import dialog
                if (RequiredService<Services.Dialogs.IImportDialogService>().Execute(new ViewModels.ImportDialog(), out var importedMap, out var importedMission))
                {
                    // Checnge map if required.
                    if (importedMap is not null)
                    {
                        SelectedMap = importedMap.Value;
                    }

                    Debug.Assert(SelectedMap == PresetsLua.Map);

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

                RequiredService<IExportDialogService>().Execute(viewModel: new ViewModels.ExportDialog(map: SelectedMap, mission: mission));
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
                    System.Windows.MessageBox.Show(CustomResources.Language.UnableToDeleteMission, CustomResources.Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
                    System.Windows.MessageBox.Show(CustomResources.Language.UnableToSaveModifications, CustomResources.Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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

        #region public IRelayCommand CommandRename
        public IRelayCommand<Models.PresetsLua.Mission> CommandRename => _commandRename ??= new RelayCommand<Models.PresetsLua.Mission>(
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

                var editedName = RequiredService<Services.Dialogs.ITextEditDialog>().Execute(new ViewModels.TextEditDialog(mission.Name));
                if (string.IsNullOrWhiteSpace(editedName))
                {
                    return;
                }

                if (editedName == mission.Name)
                {
                    return;
                }

                if (!PresetsLua.Missions.Remove(mission.Name))
                {
                    System.Windows.MessageBox.Show(CustomResources.Language.UnableToRenameMission, CustomResources.Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                // Change name in case it was used before.
                #region string getFreeName(string editedName)
                string getFreeName(string editedName)
                {
                    if (!PresetsLua.Missions.ContainsKey(editedName))
                    {
                        return editedName;
                    }

                    var i = 1;
                    while (true)
                    {
                        var name = string.Format("{0}-{1}", editedName, i++);
                        if (!PresetsLua.Missions.ContainsKey(name))
                        {
                            return name;
                        }
                    }
                }
                #endregion
                editedName = getFreeName(editedName);

                mission.Name = editedName;
                PresetsLua.Missions.Add(mission.Name, mission);

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
        private RelayCommand<Models.PresetsLua.Mission>? _commandRename;
        #endregion
    }
}
