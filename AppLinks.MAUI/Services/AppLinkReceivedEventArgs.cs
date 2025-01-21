namespace AppLinks.MAUI.Services
{
    public class AppLinkReceivedEventArgs : EventArgs
    {
        public AppLinkReceivedEventArgs(Uri uri)
        {
            this.Uri = uri;
        }

        public Uri Uri { get; }
    }
}