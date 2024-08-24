// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using DcsWaypointExporter.Models;

namespace DcsWaypointExporter.Services
{
    public interface IPresetsLuaSerializer
    {
        string? SerializeToString(PresetsLua presets);
        bool SerializeToFile(PresetsLua presets, Enums.DcsMaps file);
        bool SerializeToFile_XUnit(PresetsLua presets, string filename);

        PresetsLua? DeserializeFromString(Enums.DcsMaps map, string text, bool noMessageBox = false);
        PresetsLua? DeserializeFromFile(Enums.DcsMaps map, bool noMessageBox = false);
        PresetsLua? DeserializeFromFile_XUnit(Enums.DcsMaps map, string filename);
    }
}
