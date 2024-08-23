// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using DcsWaypointExporter.Enums;
using DcsWaypointExporter.Extensions;
using DcsWaypointExporter.Services.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace DcsWaypointExporter.Services
{
    public class SettingsService : ISettingsService
    {
        #region nLog instance (s_log)
        private static NLog.Logger s_log { get; } = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        #region private class JsonFile
        private class JsonFile
        {
            public string? DcsSavedGamesFolder
            {
                get => _dcsSavedGamesFolder;
                set
                {
                    if (_dcsSavedGamesFolder != value)
                    {
                        _dcsSavedGamesFolder = value;
                        UpdateConfiguration(nameof(DcsSavedGamesFolder), DcsSavedGamesFolder.ToNonNull());
                    }
                }
            }
            private string? _dcsSavedGamesFolder = null;

            private JsonFile()
            {

            }


            /// <summary>
            /// This method loads the json data from the appsettings.json file.
            /// </summary>
            public static JsonFile LoadFromJson()
            {
                try
                {
                    // Konfiguration laden
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                    var configuration = builder.Build();

                    // Zugriff auf die Konfiguration
                    var text = configuration["DcsSavedGamesFolder"];
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        text = string.Empty;
                    }

                    return new JsonFile() { DcsSavedGamesFolder = text };
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, "Unexpected exception while parsing appsettings.json");
                    return new();
                }
            }

            private bool UpdateConfiguration(string key, string newValue)
            {
                try
                {
                    // Beispiel: Änderung eines Wertes
                    var filename = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

                    // Lies die aktuelle JSON-Datei als Dictionary
                    var jsonConfiguration = new JsonConfigurationProvider(new JsonConfigurationSource { Path = filename });
                    jsonConfiguration.Load(new FileStream(filename, FileMode.Open, FileAccess.Read));

                    var configData = new Dictionary<string, string>();
                    jsonConfiguration.TryGet(key, out var _);

                    // Aktualisiere den Wert
                    configData[key] = newValue;

                    // Schreibe die geänderte Konfiguration zurück in die Datei
                    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                    var jsonString = JsonSerializer.Serialize(configData, jsonOptions);

                    File.WriteAllText(filename, jsonString);
                    return true;
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, "Unexpected exception");
                    return false;
                }
            }

        }
        #endregion
        private readonly JsonFile _jsonFile = JsonFile.LoadFromJson();

        private const string DCS = "DCS";
        private const string SAVED_GAMES = "Saved Games";

        /// <summary>
        /// Property that contains the current DCS-SavedGaes folder, in respect to config and available file system.
        /// If the property is <see langword="null"/>, the folder is invalid (e.g. does not exists, could not be found, ...).
        /// </summary>
        public string? DcsSavedGames => _dcsSavedGames ??= UpdateDcsSavedGames();
        private string? _dcsSavedGames = null;


        public SettingsService()
        {
            var _dcsSavedGames = UpdateDcsSavedGames();
            if (_dcsSavedGames is null)
            {
                // Try to solve the problem by showing a configuration dialog.
                var viewModel = new ViewModels.FolderSetupDialog(_jsonFile.DcsSavedGamesFolder);
                if (Ioc.Default.GetRequiredService<IFolderSetupService>().Execute(viewModel) == true)
                {
                    _dcsSavedGames = viewModel.Fullname;
                    Debug.Assert(Directory.Exists(_dcsSavedGames));

                    // Setting this value will automatically write the config to the harddisk.
                    _jsonFile.DcsSavedGamesFolder = _dcsSavedGames;
                }
                else
                {
                    // No soultion found.
                    _dcsSavedGames = null;
                }
            }
        }


        /// <summary>
        /// This method updates the foldername of the <see cref="DcsSavedGames"/> property.
        /// The value is calculated, by checking the appsettings.json and searching the 'saved games' folder.
        /// </summary>
        /// <returns>If existst the fullname of the DCS folder is returned. If not found or multiple possible solutions, this mehtod will return <see langword="null"/>.</returns>
        private string? UpdateDcsSavedGames()
        {
            var cfgFolder = _jsonFile?.DcsSavedGamesFolder;
            if (!string.IsNullOrWhiteSpace(cfgFolder))
            {
                if (cfgFolder.EndsWith('\\'))
                {
                    cfgFolder = cfgFolder.Remove(cfgFolder.Length - 1, 1);
                }

                s_log.Debug("appsettings.json specified a custom folder: {0}", cfgFolder);
                return cfgFolder;
            }

            var folderSavedGames = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), SAVED_GAMES));
            if (!Directory.Exists(folderSavedGames.FullName))
            {
                s_log.Error("Folder not detected: {0}", SAVED_GAMES);
                return null;
            }

            var subFolders = folderSavedGames.GetDirectories();

            var candidates = subFolders.Where(x => x.Name.Contains(DCS, StringComparison.CurrentCultureIgnoreCase));
            if (candidates is null)
            {
                s_log.Debug("No subfolders with '{0}' in it's name found.", DCS);
                return null; // no folder found
            }

            var count = candidates.Count();
            if (count > 1)
            {
                s_log.Debug("Too many subfolders with '{0}' in their name found.", DCS);
                return null; // too many folders found
            }

            if (count < 1)
            {
                Debug.Fail(string.Empty);
                return null; // should never happen
            }

            Debug.Assert(count == 1);

            var selectedFolder = candidates.First();
            if (selectedFolder is null)
            {
                Debug.Fail(string.Empty);
                return null; // should never happen
            }

            var detectedFolder = Path.Combine(folderSavedGames.FullName, selectedFolder.Name);
            s_log.Debug("Detected DCS-Folder: {0}", detectedFolder);
            return detectedFolder;
        }

        /// <inheritdoc cref="ISettingsService.GetFullFilename(DcsFiles)"/><summary>
        public string? GetFullFilename(DcsFiles file)
        {
            if (!Enums.DcsFilesTools.FilenameDictionary.TryGetValue(file, out var result))
            {
                return null;
            }

            var rootDirectory = DcsSavedGames;
            if (string.IsNullOrEmpty(rootDirectory) || !Directory.Exists(rootDirectory))
            {
                return null;
            }

            return Path.Combine(rootDirectory, result);
        }

    }
}
