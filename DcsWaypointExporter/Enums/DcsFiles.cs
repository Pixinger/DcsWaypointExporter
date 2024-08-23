// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

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
    }
}
