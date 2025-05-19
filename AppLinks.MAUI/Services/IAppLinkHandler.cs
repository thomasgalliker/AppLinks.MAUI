namespace AppLinks.MAUI.Services
{
    public interface IAppLinkHandler
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IAppLinkHandler"/>.
        /// </summary>
        public static IAppLinkHandler Current => AppLinkHandler.Current;

        internal void EnqueueAppLink(Uri uri);

        /// <summary>
        /// Raises app link data to subscribers.
        /// </summary>
        event EventHandler<AppLinkReceivedEventArgs> AppLinkReceived;

        /// <summary>
        /// Clear any cached app link data.
        /// </summary>
        void ResetCache();
    }
}