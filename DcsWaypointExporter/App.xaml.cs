// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DcsWaypointExporter
{
    public partial class App : Application
    {
        #region nLog instance (s_log)
        private static NLog.Logger s_log { get; } = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        #region public enum UnhandledExceptionSources
        public enum UnhandledExceptionSources
        {
            UnhandledException,
            DispatcherUnhandledException,
            UnobservedTaskException
        }
        #endregion

        /// <summary>
        /// Diese Eigenschaft legt fest, wie die Anwendung mit nicht aufgefangenen Exception verfährt.
        /// Wenn sie auf <see langword="true"/> gesetzt ist, wird versucht das Beenden der Anwendung zu verhindern.
        /// Wird sie auf <see langword="false"/> gesetzt, wird die Anwendung beendet (default).
        /// </summary>
        public bool PreventTerminationOnUnhandledException { get; set; } = false;


        public App()
            : base()
        {
            #region Register extended Exception Handling
            // Occurs when an exception is not caught.
            AppDomain.CurrentDomain.UnhandledException += (s, e) => CatchedUnhandledException((Exception)e.ExceptionObject, UnhandledExceptionSources.UnhandledException);

            // Occurs when an exception is thrown by an application and not caught.
            DispatcherUnhandledException += (s, e) =>
            {
                CatchedUnhandledException(e.Exception, UnhandledExceptionSources.DispatcherUnhandledException);
                if (PreventTerminationOnUnhandledException)
                {
                    e.Handled = true;
                }
            };

            // Occurs when an faulted task's unobserved exception is about to trigger exception escalation policy (which by default would terminate the process).
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                CatchedUnhandledException(e.Exception, UnhandledExceptionSources.UnobservedTaskException);
                if (PreventTerminationOnUnhandledException)
                {
                    e.SetObserved();
                }
            };
            #endregion
        }


        /// <summary>
        /// Diese Methode wird aufgerufen, wenn in der Anwendung nicht abgefangene Ausnahmen ausgelöst wurden.
        /// Im Standard wird die Methode die Exception über NLOG protokollieren.
        /// </summary>
        protected virtual void OnCatchedUnhandledException(Exception exception, UnhandledExceptionSources source)
        {
            s_log.Fatal(exception, "Application throwed an exception that was unhandled. PreventTerminationOnUnhandledException = {0}", PreventTerminationOnUnhandledException);
            System.Diagnostics.Debug.Fail(string.Empty);
        }
        private void CatchedUnhandledException(Exception exception, UnhandledExceptionSources source)
        {
            try
            {
                OnCatchedUnhandledException(exception, source);
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Exception while handling '{0}'.", nameof(OnCatchedUnhandledException));
            }
        }

        /// <summary>
        /// Diese Methode wird in der <see cref="OnStartup(StartupEventArgs)"/> Methode aufgerufen und fordert zur Registrierung der benötigten Services auf.
        /// In der Basis Implementierung wird hier der <see cref="SST.Common.Threads.IMainThreadInvoker"/> registriert (als Applicatin.Dispatcher).
        /// </summary>
        protected virtual void OnConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            services.AddSingleton<Services.IMainThreadInvoker>(x => new Services.MainThreadInvoker(Dispatcher));
            services.AddSingleton<Services.IMissionImportExport, Services.MissionImportExport>();
            services.AddSingleton<Services.IPresetsLuaSerializer, Services.PresetsLuaSerializer>();
            services.AddSingleton<Services.ISettingsService, Services.SettingsService>();
            services.AddSingleton<Services.IUpdateValidationService, Services.UpdateValidationService>();

            services.AddTransient<Services.Dialogs.IImportDialogService, Views.ImportDialog>();
            services.AddTransient<Services.Dialogs.IExportDialogService, Views.ExportDialog>();
            services.AddTransient<Services.Dialogs.IInfoDialogService, Views.InfoDialog>();
            services.AddTransient<Services.Dialogs.IFolderSetupService, Views.FolderSetupDialog>();
            services.AddTransient<Services.Dialogs.ITextEditDialog, Views.TextEditDialog>();
        }

        /// <summary>
        /// In dieser Überladung wird der <see cref="Ioc.Default"/> Service-Provider initialisiert.
        /// Dazu wird die Methode <see cref="OnConfigureServices(IServiceCollection)"/> bereitgestellt.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var services = new ServiceCollection();
                OnConfigureServices(services);
                Ioc.Default.ConfigureServices(services.BuildServiceProvider());

                ShutdownMode = ShutdownMode.OnMainWindowClose;
                MainWindow = new Views.MainWindow();
                MainWindow.Show();
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Unexpected exception while executing '{0}'.", nameof(OnConfigureServices));
            }
        }
    }
}
