// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

namespace DcsWaypointExporter.Services.Dialogs
{
    public interface IExportDialogService
    {
        bool Execute(ViewModels.ExportDialog viewModel);
    }
}
