// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.IO;
using Microsoft.Extensions.Configuration;

namespace DcsWaypointExporter.Services
{
    public class SettingsService : ISettingsService
    {
        public string DcsSavedGamesFolder { get; }


        public SettingsService()
        {
            // Konfiguration laden
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var _configuration = builder.Build();

            // Zugriff auf die Konfiguration
            var text = _configuration["DcsSavedGamesFolder"];
            if (string.IsNullOrWhiteSpace(text))
            {
                DcsSavedGamesFolder = string.Empty;
            }
            else
            {
                DcsSavedGamesFolder = text;
            }
        }
    }
}
