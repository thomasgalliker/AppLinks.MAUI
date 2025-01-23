namespace AppLinks.MAUI
{
    public class AppLinkOptions
    {
        /// <summary>
        /// Automatically calls Application.OnAppLinkRequestReceived(Uri) if an app link is received.
        /// Default: <c>true</c>
        /// </summary>
        public virtual bool EnableSendOnAppLinkRequestReceived { get; set; } = true;

        /// <summary>
        /// Enables URI processing using <see cref="IUriProcessor"/>.
        /// Default: <c>true</c>
        /// </summary>
        public virtual bool EnableUriProcessor { get; set; } = true;

        public override string ToString()
        {
            return $"[{nameof(AppLinkOptions)}: " +
                   $"{nameof(this.EnableSendOnAppLinkRequestReceived)}={this.EnableSendOnAppLinkRequestReceived}, " +
                   $"{nameof(this.EnableUriProcessor)}={this.EnableUriProcessor}" +
                   $"]";
        }
    }
}