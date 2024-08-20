// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

namespace DcsWaypointExporter.Services
{
    public interface IMainThreadInvoker
    {
        void Invoke(Action callback);
        void Invoke(Action callback, CancellationToken cancellationToken);

        TResult Invoke<TResult>(Func<TResult> callback);
        TResult Invoke<TResult>(Func<TResult> callback, CancellationToken cancellationToken);

        /// <summary>
        /// This Method will throw exceptions thrown by the executed code, back to the location where the method was called.
        /// </summary>
        Task<TResult> InvokeAsync<TResult>(Func<TResult> callback);
        /// <summary>
        /// This Method will throw exceptions thrown by the executed code, back to the location where the method was called.
        /// </summary>
        Task<TResult> InvokeAsync<TResult>(Func<TResult> callback, CancellationToken cancellationToken);

        /// <summary>
        /// This Method will throw exceptions thrown by the executed code, to the internal 'UnhandledException' EventHandler of the underlying Dispatcher.
        /// </summary>
        Task BeginInvoke(Delegate method, params object[] args);

        /// <summary>
        /// Determines whether the calling thread is the thread associated with this System.Windows.Threading.Dispatcher.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the calling thread is the thread associated with this MainThreadInvoker (usually the UI thread);
        /// otherwise, <see langword="false"/>.
        /// </returns>
        bool CheckAccess();
        bool IsMainThread => CheckAccess();

        void InvokeIfRequired(Action action);
        TResult InvokeIfRequired<TResult>(Func<TResult> callback);
    }
}
