// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.IO;
using CommunityToolkit.Mvvm.DependencyInjection;
using DcsWaypointExporter.Services;

namespace DcsWaypointExporter.Enums
{
    public enum DcsFiles
    {
        Afghanistan,
        Caucasus,
        Kola,
        Nevada,
        MarianaIslands,
        Normandy1944,
        Normandy2,
        PersianGulf,
        Sinai,
        SouthAtlantic,
        Syria,
        TheChannel,
    }

    public static class DcsFilesTools
    {
        public static Dictionary<DcsFiles, string> FilenameDictionary => _s_FilenameDictionary ??= new Dictionary<DcsFiles, string>()
        {
            { DcsFiles.Afghanistan, @"Config\RouteToolPresets\Afghanistan.lua" },
            { DcsFiles.Caucasus, @"Config\RouteToolPresets\Caucasus.lua" },
            { DcsFiles.Kola, @"Config\RouteToolPresets\Kola.lua" },
            { DcsFiles.Nevada, @"Config\RouteToolPresets\Nevada.lua" },
            { DcsFiles.MarianaIslands, @"Config\RouteToolPresets\MarianaIslands.lua" },
            { DcsFiles.Normandy1944, @"Config\RouteToolPresets\Normandy.lua" },
            { DcsFiles.Normandy2, @"Config\RouteToolPresets\Normandy2.lua" },
            { DcsFiles.PersianGulf, @"Config\RouteToolPresets\PersianGulf.lua" },
            { DcsFiles.Sinai, @"Config\RouteToolPresets\SinaiMap.lua" },
            { DcsFiles.SouthAtlantic, @"Config\RouteToolPresets\Falkland.lua" },
            { DcsFiles.Syria, @"Config\RouteToolPresets\Syria.lua" },
            { DcsFiles.TheChannel, @"Config\RouteToolPresets\TheChannel.lua" },
        };
        private static Dictionary<DcsFiles, string>? _s_FilenameDictionary = null;

        public static Dictionary<DcsFiles, string> TextDictionary => _s_TextDictionary ??= new Dictionary<DcsFiles, string>()
        {
            { DcsFiles.Afghanistan, CustomResources.Language.Map_Afghanistan },
            { DcsFiles.Caucasus, CustomResources.Language.Map_Caucasus },
            { DcsFiles.Kola, CustomResources.Language.Map_Kola },
            { DcsFiles.Nevada, CustomResources.Language.Map_Nevada },
            { DcsFiles.MarianaIslands, CustomResources.Language.Map_MarianaIslands },
            { DcsFiles.Normandy1944, CustomResources.Language.Map_Normandy1944 },
            { DcsFiles.Normandy2, CustomResources.Language.Map_Normandy2 },
            { DcsFiles.PersianGulf, CustomResources.Language.Map_PersianGulf },
            { DcsFiles.Sinai, CustomResources.Language.Map_Sinai },
            { DcsFiles.SouthAtlantic, CustomResources.Language.Map_SouthAtlantic },
            { DcsFiles.Syria, CustomResources.Language.Map_Syria },
            { DcsFiles.TheChannel, CustomResources.Language.Map_TheChannel },
        };
        private static Dictionary<DcsFiles, string>? _s_TextDictionary = null;


        /// <summary>
        /// Get the full filename of the <paramref name="file"/>.
        /// </summary>
        /// <remarks>
        /// Warning: The file is not guaranteed to exists!
        /// </remarks>
        /// <returns>The full filename or <see langword="null"/> if it was not possible to calculate the filename.</returns>
        public static string? GetFullFilename(DcsFiles file)
        {
            if (!FilenameDictionary.TryGetValue(file, out var result))
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

        /// <summary>
        /// Property that contains the current DCS-SavedGaes folder, in respect to config and available file system.
        /// This property is initially <see langword="null"/> and will be updated on demand.
        /// </summary>
        public static string? DcsSavedGames => _s_DcsSavedGames ??= CalculateDcsSavedGames();
        private static string? _s_DcsSavedGames = null;

        private static string? CalculateDcsSavedGames()
        {
            var cfgFolder = Ioc.Default.GetRequiredService<ISettingsService>().DcsSavedGamesFolder;
            if (!string.IsNullOrWhiteSpace(cfgFolder))
            {
                if (cfgFolder.EndsWith('\\'))
                {
                    cfgFolder.Remove(cfgFolder.Length - 1, 1);
                }

                return cfgFolder;
            }

            var folder = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Saved Games"));
            if (folder.Exists)
            {
                var subFolders = folder.GetDirectories();
                foreach (var subFolder in subFolders)
                {
                    if (subFolder.Name.Contains("DCS", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return Path.Combine(folder.FullName, subFolder.Name);
                    }
                }
            }

            return null;
        }
    }
}
