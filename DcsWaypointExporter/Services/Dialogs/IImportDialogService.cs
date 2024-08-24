// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Diagnostics.CodeAnalysis;
using DcsWaypointExporter.Enums;
using DcsWaypointExporter.Models;

namespace DcsWaypointExporter.Services.Dialogs
{
    public interface IImportDialogService
    {
        bool Execute(ViewModels.ImportDialog viewModel, out DcsMaps? map, [NotNullWhen(true)] out PresetsLua.Mission? mission);
    }
}
