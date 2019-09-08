using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.WebHooks.Custom.Internal;

namespace Microsoft.AspNetCore.WebHooks.Custom.SqlStorage.Internal
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
        internal static void AddWebHookServicesWithSqlBackend(IServiceCollection services, string connectionString)
        {
            services.AddDbContext<WebHookStoreContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<IWebHookStore, SqlWebHookStore>();
        }
    }
}
