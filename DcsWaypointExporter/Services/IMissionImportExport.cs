// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Diagnostics.CodeAnalysis;
using DcsWaypointExporter.Enums;

namespace DcsWaypointExporter.Services
{
    public interface IMissionImportExport
    {
        bool Import(string? importedText, out DcsMaps? map, [NotNullWhen(true)] out Models.PresetsLua.Mission? mission);
        bool Export(DcsMaps map, Models.PresetsLua.Mission mission, [NotNullWhen(true)] out string? exportedText);
    }
}
