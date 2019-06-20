using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.WebHooks.Internal
{
    /// <summary>
    /// Registrations of the services
    /// </summary>
    public static class WebHookServiceCollectionSetup
    {

        /// <summary>
        /// Adding all webhook services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString"></param>
        /// <param name="configureOptions"></param>
        public static void AddWebHookServices(IServiceCollection services, string connectionString, Action<WebHookSettings> configureOptions)
        {
            services.Configure<WebHookSettings>(configureOptions);
            services.AddDbContext<WebHookStoreContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
            services.AddTransient<IWebHookStore, SqlWebHookStore>();
            services.AddTransient<IWebHookUser, WebHookUser>();
            services.AddTransient<IWebHookFilterProvider, WildcardWebHookFilterProvider>();
            services.AddTransient<IWebHookFilterManager, WebHookFilterManager>();
            services.AddSingleton<IWebHookSender, DataflowWebHookSender>();
            services.AddTransient<IWebHookManager, WebHookManager>();
            services.AddTransient<IWebHookRegistrationsManager, WebHookRegistrationsManager>();
        }
    }
}
