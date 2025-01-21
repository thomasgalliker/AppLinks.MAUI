namespace AppLinks.MAUI.Services
{
    internal class EssentialsMainThread : IMainThread
    {
        private static readonly Lazy<IMainThread> Implementation = new Lazy<IMainThread>(() => new EssentialsMainThread(), LazyThreadSafetyMode.PublicationOnly);

        public static IMainThread Current => Implementation.Value;

        private EssentialsMainThread()
        {
        }

        public bool IsMainThread => MainThread.IsMainThread;

        public void BeginInvokeOnMainThread(Action action)
        {
            MainThread.BeginInvokeOnMainThread(action);
        }

        public async Task InvokeOnMainThreadAsync(Action action)
        {
            await MainThread.InvokeOnMainThreadAsync(action);
        }

        public async Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
        {
            return await MainThread.InvokeOnMainThreadAsync(func);
        }

        public async Task InvokeOnMainThreadAsync(Func<Task> funcTask)
        {
            await MainThread.InvokeOnMainThreadAsync(funcTask);
        }

        public async Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask)
        {
            return await MainThread.InvokeOnMainThreadAsync(funcTask);
        }

        public async Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync()
        {
            return await MainThread.GetMainThreadSynchronizationContextAsync();
        }
    }
}