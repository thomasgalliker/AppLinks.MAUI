namespace AppLinks.MAUI.Services
{
    public interface IMainThread
    {
        /// <inheritdoc cref="MainThread.IsMainThread"/>
        public bool IsMainThread { get; }

        /// <inheritdoc cref="MainThread.BeginInvokeOnMainThread(Action)"/>
        public void BeginInvokeOnMainThread(Action action);

        /// <inheritdoc cref="MainThread.InvokeOnMainThreadAsync(Action)"/>
        public Task InvokeOnMainThreadAsync(Action action);

        /// <inheritdoc cref="MainThread.InvokeOnMainThreadAsync{T}(Func{Task{T}})"/>
        public Task<T> InvokeOnMainThreadAsync<T>(Func<T> func);

        /// <inheritdoc cref="MainThread.InvokeOnMainThreadAsync(Func{Task})"/>
        public Task InvokeOnMainThreadAsync(Func<Task> funcTask);

        /// <inheritdoc cref="MainThread.InvokeOnMainThreadAsync{T}(Func{Task{T}})"/>
        public Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask);

        /// <inheritdoc cref="MainThread.GetMainThreadSynchronizationContextAsync()"/>
        public Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync();
    }
}