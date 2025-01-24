using System.Diagnostics;

namespace AppLinks.MAUI.Services
{
    [DebuggerDisplay("{Uri}")]
    public class AppLinkReceivedEventArgs : EventArgs
    {
        public AppLinkReceivedEventArgs(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            this.Uri = uri;
        }

        public Uri Uri { get; }
    }
}