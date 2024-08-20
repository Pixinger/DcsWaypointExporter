// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

namespace DcsWaypointExporter.Services
{
    public interface IMissionImportExport
    {
        Models.PresetsLua.Mission? Import(string? importedText);
        public string? Export(Models.PresetsLua.Mission mission);
    }
}
