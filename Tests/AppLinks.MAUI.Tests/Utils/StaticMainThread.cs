namespace AppLinks.MAUI.Tests.Utils
{
    /// <summary>
    /// For testing purposes only!
    /// </summary>
    internal class StaticMainThread : AppLinks.MAUI.Services.IMainThread
    {
        public bool IsMainThread { get; }

        public void BeginInvokeOnMainThread(Action action)
        {
            action();
        }

        public Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync()
        {
            throw new NotSupportedException();
        }

        public Task InvokeOnMainThreadAsync(Action action)
        {
            action();
            return Task.CompletedTask;
        }

        public Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
        {
            return Task.FromResult(func());
        }

        public async Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask)
        {
            return await funcTask();
        }

        public async Task InvokeOnMainThreadAsync(Func<Task> funcTask)
        {
            await funcTask();
        }
    }
}