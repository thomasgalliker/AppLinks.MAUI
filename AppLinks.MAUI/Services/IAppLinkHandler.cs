namespace AppLinks.MAUI.Services
{
    public interface IAppLinkHandler
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IAppLinkHandler"/>.
        /// </summary>
        public static IAppLinkHandler Current { get; set; } = AppLinkHandler.Current;

        internal void EnqueueAppLink(Uri uri);

        event EventHandler<AppLinkReceivedEventArgs> AppLinkReceived;
    }
}