// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using DcsWaypointExporter.ViewModels;

namespace DcsWaypointExporter.Services.Dialogs
{
    public interface IInfoDialogService
    {
        bool Execute(InfoDialog viewModel);
    }
}
