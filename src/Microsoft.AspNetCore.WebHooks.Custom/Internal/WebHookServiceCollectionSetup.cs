using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.WebHooks.Custom.Internal
{
    /// <summary>
    /// Registrations of the services
    /// </summary>
    internal static class WebHookServiceCollectionSetup
    {
        /// <summary>
        /// Adding all webhook services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString"></param>
        /// <param name="configureOptions"></param>
        internal static void AddWebHookServices(this IServiceCollection services, Action<WebHookSettings> configureOptions)
        {
            services.Configure<WebHookSettings>(configureOptions);
            services.AddTransient<IWebHookUser, WebHookUser>();

            services.AddSingleton<IWebHookFilterProvider, WildcardWebHookFilterProvider>();
            services.AddSingleton<IWebHookFilterManager, WebHookFilterManager>();
            services.AddSingleton<IWebhookPolicyContainer, WebhookPolicyContainer>();
            services.AddSingleton<IWebHookSender, PollyWebHookSender>();
            services.AddHostedService<WebhookPolicyContainerCleanupService>();

            services.AddTransient<IWebHookManager, WebHookManager>();
            services.AddTransient<IWebHookRegistrationsManager, WebHookRegistrationsManager>();
        }
    }
}
