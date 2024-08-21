// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using Microsoft.Extensions.DependencyInjection;

namespace DcsWaypointExporter.Tests
{
    public class DependencyInjectionFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public DependencyInjectionFixture()
        {
            // Setup des ServiceProviders (DI-Container)
            var services = new ServiceCollection();

            // Registriere hier alle notwendigen Dienste
            ////services.AddSingleton<Services.IMainThreadInvoker>(x => new Services.MainThreadInvoker(System.Windows.Application.Current.Dispatcher));
            services.AddSingleton<Services.IMissionImportExport, Services.MissionImportExport>();
            services.AddSingleton<Services.IPresetsLuaSerializer, Services.PresetsLuaSerializer>();
            //services.AddSingleton<Services.ISettingsService, Services.SettingsService>();

            //services.AddTransient<Services.Dialogs.IImportDialogService, Views.ImportDialog>();
            //services.AddTransient<Services.Dialogs.IExportDialogService, Views.ExportDialog>();
            //services.AddTransient<Services.Dialogs.IInfoDialogService, Views.InfoDialog>();
            ServiceProvider = services.BuildServiceProvider();
        }

        public void Dispose() =>
            // Optionale Bereinigung
            (ServiceProvider as IDisposable)?.Dispose();
    }
}
