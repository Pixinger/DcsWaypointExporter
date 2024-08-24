// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

namespace DcsWaypointExporter.Enums
{
    public enum DcsMaps
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

    public static class DcsMapsTools
    {
        public static Dictionary<DcsMaps, string> FilenameDictionary => _s_FilenameDictionary ??= new Dictionary<DcsMaps, string>()
        {
            { DcsMaps.Afghanistan, @"Config\RouteToolPresets\Afghanistan.lua" },
            { DcsMaps.Caucasus, @"Config\RouteToolPresets\Caucasus.lua" },
            { DcsMaps.Kola, @"Config\RouteToolPresets\Kola.lua" },
            { DcsMaps.Nevada, @"Config\RouteToolPresets\Nevada.lua" },
            { DcsMaps.MarianaIslands, @"Config\RouteToolPresets\MarianaIslands.lua" },
            { DcsMaps.Normandy1944, @"Config\RouteToolPresets\Normandy.lua" },
            { DcsMaps.Normandy2, @"Config\RouteToolPresets\Normandy2.lua" },
            { DcsMaps.PersianGulf, @"Config\RouteToolPresets\PersianGulf.lua" },
            { DcsMaps.Sinai, @"Config\RouteToolPresets\SinaiMap.lua" },
            { DcsMaps.SouthAtlantic, @"Config\RouteToolPresets\Falkland.lua" },
            { DcsMaps.Syria, @"Config\RouteToolPresets\Syria.lua" },
            { DcsMaps.TheChannel, @"Config\RouteToolPresets\TheChannel.lua" },
        };
        private static Dictionary<DcsMaps, string>? _s_FilenameDictionary = null;

        public static Dictionary<DcsMaps, string> TextDictionary => _s_TextDictionary ??= new Dictionary<DcsMaps, string>()
        {
            { DcsMaps.Afghanistan, CustomResources.Language.Map_Afghanistan },
            { DcsMaps.Caucasus, CustomResources.Language.Map_Caucasus },
            { DcsMaps.Kola, CustomResources.Language.Map_Kola },
            { DcsMaps.Nevada, CustomResources.Language.Map_Nevada },
            { DcsMaps.MarianaIslands, CustomResources.Language.Map_MarianaIslands },
            { DcsMaps.Normandy1944, CustomResources.Language.Map_Normandy1944 },
            { DcsMaps.Normandy2, CustomResources.Language.Map_Normandy2 },
            { DcsMaps.PersianGulf, CustomResources.Language.Map_PersianGulf },
            { DcsMaps.Sinai, CustomResources.Language.Map_Sinai },
            { DcsMaps.SouthAtlantic, CustomResources.Language.Map_SouthAtlantic },
            { DcsMaps.Syria, CustomResources.Language.Map_Syria },
            { DcsMaps.TheChannel, CustomResources.Language.Map_TheChannel },
        };
        private static Dictionary<DcsMaps, string>? _s_TextDictionary = null;
    }
}
