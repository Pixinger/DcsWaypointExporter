// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using DcsWaypointExporter.Enums;

namespace DcsWaypointExporter.Services
{
    public interface ISettingsService
    {
        /// <summary>
        /// Return the Dcs-SavedGames folder (if exists and unique or custom), otherwise it returns <see langword="null"/>.
        /// </summary>
        string? DcsSavedGames { get; }
        /// <summary>
        /// This method returns the full path of the requested map. This is usually something like:
        /// "C:\Users\{Username}\Saved Games\DCS\Config\RouteToolPresets\Caucasus.lua".
        /// </summary>
        /// <remarks>
        /// Warning: The file is not guaranteed to exists!
        /// </remarks>
        /// <returns>The full filename or <see langword="null"/> if it was not possible to calculate the filename.</returns>
        string? GetFullFilename(DcsMaps file);
    }
}
