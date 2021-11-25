using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.WebHooks.Internal;

/// <summary>
/// Registrations of the services
/// </summary>
internal static class WebHookServiceCollectionSetup
{
    /// <summary>
    /// Adding the webhook services
    /// </summary>
    /// <param name="services"></param>
    internal static void AddWebHookServicesApi(IServiceCollection services)
    {
        services.TryAddTransient<IWebHookIdValidator, DefaultWebHookIdValidator>();
    }
}
