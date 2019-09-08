using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.WebHooks.Internal
{
    /// <summary>
    /// Registrations of the services
    /// </summary>
    public static class WebHookServiceCollectionSetup
    {
        /// <summary>
        /// Adding the webhook services
        /// </summary>
        /// <param name="services"></param>
        public static void AddWebHookServices(IServiceCollection services)
        {
            services.TryAddTransient<IWebHookIdValidator, DefaultWebHookIdValidator>();
        }
    }
}
