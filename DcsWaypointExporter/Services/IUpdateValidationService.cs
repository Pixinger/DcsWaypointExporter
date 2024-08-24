// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/


namespace DcsWaypointExporter.Services
{
    public interface IUpdateValidationService
    {
        Task<bool> HasUpdateAsync();
    }
}
