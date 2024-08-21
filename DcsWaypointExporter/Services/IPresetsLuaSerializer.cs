// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using DcsWaypointExporter.Models;

namespace DcsWaypointExporter.Services
{
    public interface IPresetsLuaSerializer
    {
        string? SerializeToString(PresetsLua presets);
        bool SerializeToFile(PresetsLua presets, Enums.DcsFiles file);

        PresetsLua? DeserializeFromString(string text);
        PresetsLua? DeserializeFromFile(Enums.DcsFiles file);

        PresetsLua? DeserializeFromFile_XUnit(string filename);
        bool SerializeToFile_XUnit(PresetsLua presets, string filename);
    }
}
