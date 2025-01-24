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
        /// Enables URI processing using <see cref="IAppLinkProcessor"/>.
        /// Default: <c>true</c>
        /// </summary>
        public virtual bool EnableAppLinkProcessor { get; set; } = true;

        public override string ToString()
        {
            return $"[{nameof(AppLinkOptions)}: " +
                   $"{nameof(this.EnableSendOnAppLinkRequestReceived)}={this.EnableSendOnAppLinkRequestReceived}, " +
                   $"{nameof(this.EnableAppLinkProcessor)}={this.EnableAppLinkProcessor}" +
                   $"]";
        }
    }
}