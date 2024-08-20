// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Windows.Threading;

namespace DcsWaypointExporter.Services
{
    public class MainThreadInvoker : IMainThreadInvoker
    {
        private readonly Dispatcher _dispatcher;


        public MainThreadInvoker(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }


        public Task BeginInvoke(Delegate method, params object[] args) => _dispatcher.BeginInvoke(method, args).Task;

        public bool CheckAccess() => _dispatcher.CheckAccess();

        public void Invoke(Action callback) => _dispatcher.Invoke(callback);

        public void Invoke(Action callback, CancellationToken cancellationToken) => _dispatcher.Invoke(callback, cancellationToken);

        public TResult Invoke<TResult>(Func<TResult> callback) => _dispatcher.Invoke<TResult>(callback);

        public TResult Invoke<TResult>(Func<TResult> callback, CancellationToken cancellationToken) => _dispatcher.Invoke<TResult>(callback, DispatcherPriority.Normal, cancellationToken);

        public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback) => _dispatcher.InvokeAsync(callback).Task;

        public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback, CancellationToken cancellationToken) => _dispatcher.InvokeAsync(callback, DispatcherPriority.Normal, cancellationToken).Task;

        public void InvokeIfRequired(Action action)
        {
            if (CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                _dispatcher.Invoke(action);
            }
        }

        public TResult InvokeIfRequired<TResult>(Func<TResult> callback)
        {
            if (CheckAccess())
            {
                return callback.Invoke();
            }
            else
            {
                return _dispatcher.Invoke(callback);
            }
        }
    }
}
