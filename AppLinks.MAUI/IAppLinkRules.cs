namespace AppLinks.MAUI
{
    /// <summary>
    /// Implement this interface to get app link rules configured automatically.
    /// Register this interface in DI as follows:
    /// <c>builder.Services.AddSingleton&lt;IAppLinkRules, CustomAppLinkRules&gt;();</c>
    /// </summary>
    public interface IAppLinkRules
    {
        IEnumerable<AppLinkRule> Get();
    }
}