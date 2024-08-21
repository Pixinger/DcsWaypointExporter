// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using DcsWaypointExporter.Models;

namespace DcsWaypointExporter.Services.Dialogs
{
    public interface IImportDialogService
    {
        PresetsLua.Mission? Execute(ViewModels.ImportDialog viewModel);
    }
}
