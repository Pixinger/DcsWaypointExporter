// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using DcsWaypointExporter.Models;

namespace DcsWaypointExporter.Services
{
    internal interface IPresetsLuaSerializer
    {
        string? SerializeToString(PresetsLua presets);
        PresetsLua? DeserializeFromString(string text);

        bool SerializeToFile(PresetsLua presets, Enums.DcsFiles file);
        PresetsLua? DeserializeFromFile(Enums.DcsFiles file);
    }
}
