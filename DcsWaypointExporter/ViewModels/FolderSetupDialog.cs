// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DcsWaypointExporter.ViewModels
{
    public class FolderSetupDialog : ObservableObject
    {
        #region nLog instance (s_log)
        private static NLog.Logger s_log { get; } = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        private const string DCS = "DCS";
        private const string SAVED_GAMES = "Saved Games";

        public event Action<bool>? Close;

        public string? Fullname
        {
            get => _fullname;
            set
            {
                if (SetProperty(ref _fullname, value))
                {
                    CommandOk.NotifyCanExecuteChanged();
                }
            }
        }
        private string? _fullname;

        public ObservableCollection<string> PossibleCandidates { get; } = new();


        public FolderSetupDialog(string? customDcsSavedGamesFolder)
        {
            Fullname = customDcsSavedGamesFolder;

            #region function: updatePossibleCandidates
            void updatePossibleCandidates()
            {
                var folderSavedGames = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), SAVED_GAMES));
                if (!Directory.Exists(folderSavedGames.FullName))
                {
                    s_log.Error("Folder not detected: {0}", SAVED_GAMES);
                    return;
                }

                var subFolders = folderSavedGames.GetDirectories();

                var candidates = subFolders.Where(x => x.Name.Contains(DCS, StringComparison.CurrentCultureIgnoreCase));
                if (candidates is null)
                {
                    s_log.Debug("No subfolders with '{0}' in it's name found.", DCS);
                    return; // no folder found
                }

                foreach (var candidate in candidates)
                {
                    var detectedFolder = Path.Combine(folderSavedGames.FullName, candidate.Name);
                    if (Directory.Exists(detectedFolder))
                    {
                        PossibleCandidates.Add(detectedFolder);
                    }
                    else
                    {
                        s_log.Warn("Folder calculated but not found: {0}", detectedFolder);
                    }
                }
            }
            #endregion
            updatePossibleCandidates();
        }


        #region public IRelayCommand CommandOk
        public IRelayCommand CommandOk => _commandOk ??= new RelayCommand(
            execute: () =>
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(Fullname));
                Debug.Assert(Directory.Exists(Fullname));

                Close?.Invoke(true);
            },
            canExecute: () =>
            {
                if (string.IsNullOrWhiteSpace(Fullname))
                {
                    return false;
                }

                if (!Directory.Exists(Fullname))
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
